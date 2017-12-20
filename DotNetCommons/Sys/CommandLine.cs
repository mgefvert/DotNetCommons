using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DotNetCommons.Text;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT

namespace DotNetCommons.Sys
{
    public static class CommandLine
    {
        public static bool MultipleShortOptions { get; set; } = true;
        public static bool DisplayHelpOnEmpty { get; set; } = true;

        public static List<CommandLineDefinition> GetParameters(Type type)
        {
            return GetDefinitionList(type).Where(x => x.IsAttribute).ToList();
        }

        public static List<KeyValuePair<string, string>> GetHelpText(Type type)
        {
            return GetParameters(type)
              .Select(x => new KeyValuePair<string, string>(x.OptionString, x.Description))
              .ToList();
        }

        public static string GetFormattedHelpText(Type type)
        {
            var result = new StringBuilder();

            var help = GetHelpText(type);
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
                    result.AppendLine(item.Key);
                    foreach (var line in TextTools.WordWrap(item.Value, consoleWidth - 5))
                        result.AppendLine("   " + line);
                    result.AppendLine("");
                }
            }
            else
            {
                foreach (var item in help)
                {
                    var key = item.Key;
                    foreach (var line in TextTools.WordWrap(item.Value, consoleWidth - keylen - 5))
                    {
                        result.AppendLine(key.PadRight(keylen) + "   " + line);
                        key = "";
                    }
                }
            }

            return string.Join("\r\n", result);
        }

        public static T Parse<T>() where T : class, new()
        {
            return Parse<T>(Environment.GetCommandLineArgs().Skip(1).ToArray());
        }

        public static T Parse<T>(string[] args) where T : class, new()
        {
            if (args.Length == 0 && DisplayHelpOnEmpty)
                throw new CommandLineDisplayHelpException(typeof(T));    

            var processor = new CommandLineProcessor<T>
            {
                Arguments = args.ToList(),
                Definitions = GetDefinitionList(typeof(T)),
                Result = new T()
            };

            processor.Process();

            return processor.Result;
        }

        private static List<CommandLineDefinition> GetDefinitionList(Type type)
        {
            try
            {
                var result = new List<CommandLineDefinition>();
                foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
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
