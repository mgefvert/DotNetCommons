// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Sys;

/// <summary>
/// Define short and long options for a given property. Long options are expressed as "--long", short
/// options as "-l".
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class CommandLineOptionAttribute : Attribute
{
    public string[] ShortOptions { get; }
    public string[] LongOptions { get; }
    public string? Description { get; }

    public CommandLineOptionAttribute(string longOption, string? description = null)
    {
        ShortOptions = Array.Empty<string>();
        LongOptions = new[] { longOption };
        Description = description;
    }

    public CommandLineOptionAttribute(char shortOption, string? longOption = null, string? description = null)
    {
        ShortOptions = new[] { shortOption.ToString() };
        LongOptions = longOption != null ? new[] { longOption } : Array.Empty<string>();
        Description = description;
    }

    public CommandLineOptionAttribute(char[] shortOptions, string[]? longOption = null, string? description = null)
    {
        ShortOptions = shortOptions.Select(x => x.ToString()).ToArray();
        LongOptions = longOption ?? Array.Empty<string>();
        Description = description;
    }
}

/// <summary>
/// Define an option representing a position on the command line, e.g. CommandLinePosition(2) would
/// capture the second textual element after the program name. Thus "program one two" would have "two"
/// as the second element.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class CommandLinePositionAttribute : Attribute
{
    public int Position { get; }

    public CommandLinePositionAttribute(int position)
    {
        Position = position;
    }
}

/// <summary>
/// Define an option capturing any remaining elements not captured by other positional options.
/// Must be a List of type string.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class CommandLineRemainingAttribute : Attribute
{
}