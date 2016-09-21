using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace CommonNetTools.Scripting
{
  public class ScriptException : Exception
  {
    public ScriptException(string message, Exception innerException) : base(message, innerException)
    {
    }
  }

  public class Script<T>
  {
    public T Globals { get; set; }

    public List<string> References { get; } = new List<string>
    {
      "System",
      "System.Code",
      "System.Core",
      "System.Dynamic",
      "System.IO",
      "System.Net",
      "System.Net.Http",
      "System.Xml",
      "System.Xml.Linq",
      "Microsoft.CSharp"
    };

    public List<string> Usings { get; } = new List<string>
    {
      "System",
      "System.IO",
      "System.Collections.Generic",
      "System.Dynamic",
      "System.Linq",
      "System.Linq.Expressions",
      "System.Text",
      "System.Threading",
      "System.Threading.Tasks"
    };

    public object Run(string script)
    {
      try
      {
        var options = ScriptOptions.Default
          .WithReferences(References)
          .WithImports(Usings);

        var result = CSharpScript.EvaluateAsync(script, options, Globals);
        return result.Result;
      }
      catch (CompilationErrorException ex)
      {
        throw new ScriptException(string.Join(Environment.NewLine, ex.Diagnostics), ex);
      }
      catch (Exception ex)
      {
        throw new ScriptException(ex.Message, ex);
      }
    }
  }

  public class Script : Script<object>
  {
  }
}
