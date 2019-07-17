using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotNetCommons.Collections;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Sys
{
    internal class CommandLineProcessor<T>
    {
        private enum OptionType
        {
            None,
            Short,
            Long
        }

        public List<string> Arguments;
        public List<CommandLineDefinition> Definitions;
        public T Result;
        private int _position;

        public void Process()
        {
            while (Arguments.Any())
            {
                var arg = Arguments.ExtractFirst();
                var type = FindOptionType(ref arg);

                if (type == OptionType.None)
                    ProcessPosition(arg);
                else
                    ProcessOption(arg, type);
            }
        }

        private void ProcessOption(string arg, OptionType type)
        {
            string value = null;

            if (arg.Contains("="))
            {
                var items = arg.Split(new[] { '=' }, 2);
                arg = items[0];
                value = items[1].Trim('"');
            }
            else if (arg.Length >= 2 && type == OptionType.Short)
            {
                // More characters than we expected for a short option.
                // This can be either like "-xzf" for multiple options at once, or
                // "-uroot" for MySQL-like options; depending on MultipleShortOptions.

                if (CommandLine.MultipleShortOptions)
                {
                    foreach (var ch in arg)
                        ProcessOption(ch.ToString(), OptionType.Short);
                    return;
                }

                // MySQL style
                value = arg.Substring(1);
                arg = arg.Substring(0, 1);
            }

            var definition = FindDefinition(arg);
            if (definition == null)
                throw new CommandLineParameterException(arg, CommandLineParameterError.UndefinedParameter);

            if (definition.BooleanValue)
            {
                if (value != null)
                    throw new CommandLineParameterException(arg, CommandLineParameterError.BooleanParameterDoesNotTakeValue);

                SetParameter(definition, true);
            }
            else if (value != null)
            {
                SetParameter(definition, value);
            }
            else
            {
                if (!Arguments.Any())
                    throw new CommandLineParameterException(arg, CommandLineParameterError.ValueRequired);

                value = Arguments.ExtractFirst();
                if (FindOptionType(ref value) != OptionType.None)
                    throw new CommandLineParameterException(arg, CommandLineParameterError.ValueRequired);

                SetParameter(definition, value);
            }
        }

        private void ProcessPosition(string arg)
        {
            _position++;

            var definition = FindDefinition(_position);
            if (definition != null)
            {
                SetParameter(definition, arg);
                return;
            }

            definition = FindRemainder();
            if (definition == null) 
                throw new CommandLineException("Unrecognized text on command line: " + arg);

            if (!(definition.Property.GetValue(Result) is ICollection<string> remainder))
                throw new CommandLineException("No CommandLineRemainder property found, or the designated property is not an ICollection<string>.");

            remainder.Add(arg);
        }

        private CommandLineDefinition FindDefinition(string key)
        {
            return Definitions.FirstOrDefault(x => x.MatchesShortOption(key) || x.MatchesLongOption(key));
        }

        private CommandLineDefinition FindDefinition(int position)
        {
            return Definitions.FirstOrDefault(x => x.Position == position);
        }

        private OptionType FindOptionType(ref string arg)
        {
            if (string.IsNullOrEmpty(arg))
                return OptionType.None;

            if (arg.StartsWith("--") && arg.Length > 2)
            {
                arg = arg.Substring(2);
                return OptionType.Long;
            }

            if ((arg.StartsWith("/") || arg.StartsWith("-")) && arg.Length > 1)
            {
                arg = arg.Substring(1);
                return OptionType.Short;
            }

            return OptionType.None;
        }

        private CommandLineDefinition FindRemainder()
        {
            return Definitions.FirstOrDefault(x => x.Remainder);
        }

        private void SetParameter(CommandLineDefinition definition, object value)
        {
            var type = definition.Property.PropertyType;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                // Add the value onto a list instead of just setting the property
                if (!(definition.Property.GetValue(Result) is IList list))
                    throw new CommandLineException("Property " + definition.Property.Name + " has not been initialized.");

                var subtype = type.GetGenericArguments().Single();
                if (subtype != value.GetType())
                    value = Convert.ChangeType(value, subtype);

                list.Add(value);
                return;
            }

            if (definition.Property.PropertyType != value.GetType())
                value = Convert.ChangeType(value, definition.Property.PropertyType);

            definition.Property.SetValue(Result, value);
        }
    }
}
