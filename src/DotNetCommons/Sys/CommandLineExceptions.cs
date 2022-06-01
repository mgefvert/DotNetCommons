using System;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Sys;

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
        return error switch
        {
            CommandLineParameterError.BooleanParameterDoesNotTakeValue => "Parameter does not take a value",
            CommandLineParameterError.UndefinedParameter => "Undefined parameter",
            CommandLineParameterError.ValueRequired => "Parameter requires a value",
            _ => "Error"
        };
    }
}

public class CommandLineNoParameters : CommandLineException
{
    public CommandLineNoParameters() : base("No parameters specified.")
    {
    }
}