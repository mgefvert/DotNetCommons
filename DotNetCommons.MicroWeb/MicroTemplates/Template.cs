using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace DotNetCommons.MicroWeb.MicroTemplates
{
    public class Globals
    {
        public int LineNo;
        public dynamic Model;
        public TemplateOutput Output;
    }

    public class Template
    {
        public string Filename { get; }

        private readonly ITemplateParser _parser;
        private readonly string _source;
        private Microsoft.CodeAnalysis.Scripting.Script<string> _script;

        public Template(string source, string filename)
        {
            _source = source;
            Filename = filename;
        }

        public Template(string source, string filename, ITemplateParser parser) : this(source, filename)
        {
            _parser = parser;
        }

        public void Compile()
        {
            try
            {
                var source = _parser != null
                    ? string.Join("\r\n", _parser.Parse(_source))
                    : _source;

                var options = ScriptOptions.Default
                  .WithReferences("System", "System.Code", "System.Core", "System.Dynamic", "Microsoft.CSharp")
                  .WithImports("System");

                _script = CSharpScript.Create<string>(source, options, typeof(Globals));
                _script.Compile();
            }
            catch (CompilationErrorException ex)
            {
                throw new Exception(string.Join(Environment.NewLine, ex.Diagnostics), ex);
            }
        }

        public TemplateOutput Run()
        {
            return Run(null);
        }

        public TemplateOutput Run(object parameters)
        {
            if (_script == null)
                Compile();

            if (_script == null)
                throw new NullReferenceException("Script did not compile.");

            var globals = new Globals
            {
                Model = ToDynamic(parameters),
                Output = new TemplateOutput(Filename),
                LineNo = -1
            };

            var result = _script.RunAsync(globals);
            result.Wait();

            return globals.Output;
        }

        public static dynamic ToDynamic(object value)
        {
            if (value == null)
                return null;

            IDictionary<string, object> result = new ExpandoObject();
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType()))
                result.Add(property.Name, property.GetValue(value));

            return (ExpandoObject)result;
        }
    }
}
