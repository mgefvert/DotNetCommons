using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT

namespace CommonNetTools
{
    public static class CommandLine<T> where T : class, new()
    {
        public static List<CommandLineDefinition> GetParameters()
        {
            return GetDefinitionList().Where(x => x.IsAttribute).ToList();
        }

        public static List<KeyValuePair<string, string>> GetHelpText()
        {
            return GetParameters()
              .Select(x => new KeyValuePair<string, string>(x.OptionString, x.Description))
              .ToList();
        }

        public static string GetFormattedHelpText()
        {
            var result = new StringBuilder();

            var help = GetHelpText();
            var keylen = help.Max(x => x.Key.Length);

            int consoleWidth;
            try
            {
                consoleWidth = Console.WindowWidth;
            }
            catch (Exception)
            {
                consoleWidth = 80;
            }

            if (keylen > 20)
            {
                foreach (var item in help)
                {
                    result.Append(item.Key);
                    foreach (var line in TextTools.WordWrap(item.Value, consoleWidth - 5))
                        result.Append("   " + line);
                    result.Append("");
                }
            }
            else
            {
                foreach (var item in help)
                {
                    var key = item.Key;
                    foreach (var line in TextTools.WordWrap(item.Value, consoleWidth - keylen - 5))
                    {
                        result.Append(key.PadRight(keylen) + "   " + line);
                        key = "";
                    }
                }
            }

            return string.Join("\r\n", result);
        }

        public static T Parse()
        {
            return Parse(Environment.GetCommandLineArgs());
        }

        public static T Parse(string[] args)
        {
            var processor = new CommandLineProcessor<T>
            {
                Arguments = args.ToList(),
                Definitions = GetDefinitionList(),
                Result = new T()
            };

            processor.Process();

            return processor.Result;
        }

        private static List<CommandLineDefinition> GetDefinitionList()
        {
            try
            {
                var result = new List<CommandLineDefinition>();
                foreach (var property in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    var definition = new CommandLineDefinition();

                    var option = property.GetCustomAttribute<CommandLineOptionAttribute>();
                    if (option != null)
                    {
                        definition.ShortOption = option.ShortOption;
                        definition.LongOption = option.LongOption;
                        definition.Description = option.Description;
                    }

                    var position = property.GetCustomAttribute<CommandLinePositionAttribute>();
                    if (position != null)
                        definition.Position = position.Position;

                    var remaining = property.GetCustomAttribute<CommandLineRemainingAttribute>();
                    if (remaining != null)
                        definition.Remainder = true;

                    if (!definition.HasInfo)
                        continue;

                    definition.Property = property;
                    definition.BooleanValue = property.PropertyType == typeof(bool);
                    result.Add(definition);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new CommandLineException("CommandLine: " + ex.Message, ex);
            }
        }
    }
}
