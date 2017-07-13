using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace DotNetCommons.Configuration
{
    public class LocalConfigFile : Dictionary<string, string>
    {
        public LocalConfigFile(string machineName)
            : base(StringComparer.InvariantCultureIgnoreCase)
        {
            Load("local-" + machineName + ".xml");
        }

        protected void Load(string filename)
        {
            if (!File.Exists(filename))
                return;

            var doc = XDocument.Load(filename);
            if (doc.Root == null)
                return;

            foreach (var node in doc.Root.Elements())
                this[node.Name.LocalName] = node.Value;
        }
    }
}
