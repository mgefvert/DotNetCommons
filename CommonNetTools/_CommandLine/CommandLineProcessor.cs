using System;
using System.Collections.Generic;
using System.Linq;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT

namespace CommonNetTools
{
    internal class CommandLineProcessor<T>
    {
        public List<string> Arguments;
        public List<CommandLineDefinition> Definitions;
        public T Result;
        private int _position;

        public void Process()
        {
            while (Arguments.Any())
            {
                var arg = Arguments.ExtractFirst();

                if (IsOption(ref arg))
                    ProcessOption(arg);
                else
                    ProcessPosition(arg);
            }
        }

        private void ProcessOption(string arg)
        {
            string value = null;

            if (arg.Contains("="))
            {
                var items = arg.Split(new[] { '=' }, 2);
                arg = items[0];
                value = items[1];
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
                if (IsOption(ref value))
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
            if (definition != null)
            {

                SetParameter(definition, arg);
                return;
            }

            throw new CommandLineException("Unrecognized text on command line: " + arg);
        }

        private CommandLineDefinition FindDefinition(string key)
        {
            return Definitions.FirstOrDefault(x => x.ShortOption.ToString() == key || x.LongOption == key);
        }

        private CommandLineDefinition FindDefinition(int position)
        {
            return Definitions.FirstOrDefault(x => x.Position == position);
        }

        private CommandLineDefinition FindRemainder()
        {
            return Definitions.FirstOrDefault(x => x.Remainder);
        }

        private bool IsOption(ref string arg)
        {
            if (string.IsNullOrEmpty(arg))
                return false;

            if (arg.StartsWith("--") && arg.Length > 2)
            {
                arg = arg.Substring(2);
                return true;
            }

            if ((arg.StartsWith("/") || arg.StartsWith("-")) && arg.Length > 1)
            {
                arg = arg.Substring(1);
                return true;
            }

            return false;
        }

        private void SetParameter(CommandLineDefinition definition, object value)
        {
            if (definition.Property.PropertyType != value.GetType())
                value = Convert.ChangeType(value, definition.Property.PropertyType);

            definition.Property.SetValue(Result, value);
        }
    }
}
