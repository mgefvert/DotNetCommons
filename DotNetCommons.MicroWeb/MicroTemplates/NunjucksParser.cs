using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotNetCommons.MicroWeb.MicroTemplates
{
    public class NunjucksParser : ITemplateParser
    {
        public string Parse(string source)
        {
            var result = new List<string>();

            var sourceList = source.Split('\n').ToList();
            for (var i = 0; i < sourceList.Count; i++)
                result.AddRange(ProcessLine(i, sourceList[i].Trim()));

            return string.Join("\r\n", result);
        }

        private IEnumerable<string> ProcessLine(int i, string line)
        {
            yield return "";
            yield return "LineNo = " + (i + 1) + ";";
            do
            {
                var match = Regex.Match(line, "(?<text>.*?)({{(?<var>.*?)}}|{%(?<code>.*?)%})(?<rest>.*)", RegexOptions.Singleline);
                if (!match.Success)
                {
                    yield return $"Output.Add({Quote(line)});";
                    yield break;
                }

                if (!string.IsNullOrEmpty(match.Groups["text"].Value))
                    yield return "Output.Add(@" + Quote(match.Groups["text"].Value) + ");";

                var variable = match.Groups["var"].Value.Trim();
                var code = match.Groups["code"].Value.Trim();

                if (!string.IsNullOrEmpty(variable))
                    yield return $"Output.Add({variable});";
                else if (!string.IsNullOrEmpty(code))
                {
                    if (code.StartsWith("extends "))
                        yield return $"Output.ExtendsTemplate = {Quote(StripQuotes(code.Substring(8).Trim()))};";
                    else if (code.StartsWith("block "))
                        yield return $"Output.BlockStart({Quote(code.Substring(6).Trim())});";
                    else if (code == "endblock")
                        yield return "Output.BlockEnd();";
                    else
                        yield return code;
                }

                line = match.Groups["rest"].Value;
            } while (!string.IsNullOrWhiteSpace(line));
        }

        private string StripQuotes(string text)
        {
            text = (text ?? "").Trim();

            if (text.StartsWith("'") || text.StartsWith("\""))
                text = text.Substring(1);

            if (text.EndsWith("'") || text.EndsWith("\""))
                text = text.Substring(0, text.Length - 1);

            return text;
        }

        private string Quote(string text)
        {
            return '"' + text.Replace("\"", "\\\"") + '"';
        }
    }
}
