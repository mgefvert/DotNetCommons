using System;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Core.Sys
{
    public class CommandLineException : Exception
    {
        public CommandLineException(string message) : base(message)
        {
        }

        public CommandLineException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class CommandLineDisplayHelpException : CommandLineException
    {
        public CommandLineDisplayHelpException(Type type) : base(CommandLine.GetFormattedHelpText(type))
        {
        }
    }

    public enum CommandLineParameterError
    {
        UndefinedParameter,
        ValueRequired,
        BooleanParameterDoesNotTakeValue
    }

    public class CommandLineParameterException : CommandLineException
    {
        public string Parameter { get; }
        public CommandLineParameterError Error { get; }

        public CommandLineParameterException(string parameter, CommandLineParameterError error)
          : base(GetMessage(error) + ": " + parameter)
        {
            Parameter = parameter;
            Error = error;
        }

        private static string GetMessage(CommandLineParameterError error)
        {
            switch (error)
            {
                case CommandLineParameterError.BooleanParameterDoesNotTakeValue:
                    return "Parameter does not take a value";

                case CommandLineParameterError.UndefinedParameter:
                    return "Undefined parameter";

                case CommandLineParameterError.ValueRequired:
                    return "Parameter requires a value";

                default:
                    return "Error";
            }
        }
    }

    public class CommandLineNoParameters : CommandLineException
    {
        public CommandLineNoParameters() : base("No parameters specified.")
        {
        }
    }
}
