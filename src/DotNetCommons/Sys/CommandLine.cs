using System.ComponentModel.DataAnnotations;
using DotNetCommons.Text;
using System.Reflection;
using System.Text;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Sys;

/// <summary>
/// Simple command line parsing class, using an argument object to define properties.
/// </summary>
public static class CommandLine
{
    /// <summary>
    /// Handle -aop as -a, -o, and -p.
    /// </summary>
    public static bool MultipleShortOptions { get; set; } = true;

    /// <summary>
    /// If no parameters are submitted at all, display help.
    /// </summary>
    public static bool DisplayHelpOnEmpty { get; set; } = true;

    public static List<CommandLineDefinition> GetParameters(Type type)
    {
        return GetDefinitionList(type).Where(x => x.IsAttribute).ToList();
    }

    public record OptionHelp(bool Required, string Option, string? Description);

    public static List<OptionHelp> GetHelpText(Type type)
    {
        return GetParameters(type)
            .Select(x => new OptionHelp(x.Required, x.OptionString, x.Description))
            .ToList();
    }

    public static string GetFormattedHelpText(Type type)
    {
        var result = new StringBuilder();

        var help = GetHelpText(type);
        var keyLength = help.Max(x => x.Option.Length);

        int consoleWidth;
        try
        {
            consoleWidth = Console.WindowWidth;
        }
        catch (Exception)
        {
            consoleWidth = 80;
        }

        var (required, optional) = help.Toss(x => x.Required);

        if (required.IsAtLeastOne())
        {
            result.AppendLine("Required arguments:");
            FormatOptions(required);
        }

        if (required.IsAtLeastOne() && optional.IsAtLeastOne())
            result.AppendLine();

        if (optional.IsAtLeastOne())
        {
            result.AppendLine("Optional arguments:");
            FormatOptions(optional);
        }

        return result.ToString();

        void FormatOptions(List<OptionHelp> options)
        {
            if (keyLength > 20)
            {
                foreach (var item in options)
                {
                    result.AppendLine(item.Option);
                    foreach (var line in TextTools.WordWrap(item.Description, consoleWidth - 5))
                        result.AppendLine("   " + line);
                    result.AppendLine("");
                }
            }
            else
            {
                foreach (var item in options)
                {
                    var key = item.Option;
                    foreach (var line in TextTools.WordWrap(item.Description, consoleWidth - keyLength - 5))
                    {
                        result.AppendLine(key.PadRight(keyLength) + "   " + line);
                        key = "";
                    }
                }
            }
        }
    }

    /// <summary>
    /// Parse the default arguments on the command line as a given object.
    /// </summary>
    public static T Parse<T>() where T : class, new()
    {
        return Parse<T>(Environment.GetCommandLineArgs().Skip(1).ToArray());
    }

    /// <summary>
    /// Parse specific command line arguments as a given object.
    /// </summary>
    public static T Parse<T>(params string[] args) where T : class, new()
    {
        return (T)Parse(typeof(T), args);
    }

    /// <summary>
    /// Parse specific command line arguments as a given object.
    /// </summary>
    public static object Parse(Type resultType, params string[] args)
    {
        if (args.Length == 0 && DisplayHelpOnEmpty)
            throw new CommandLineDisplayHelpException(resultType);

        var processor = new CommandLineProcessor(resultType, args.ToList(), GetDefinitionList(resultType));
        processor.Process();
        processor.Validate();
        return processor.Result;
    }

    private static List<CommandLineDefinition> GetDefinitionList(Type type)
    {
        try
        {
            var result = new List<CommandLineDefinition>();
            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var definition = new CommandLineDefinition(property);

                var option = property.GetCustomAttribute<CommandLineOptionAttribute>();
                if (option != null)
                {
                    definition.ShortOptions = option.ShortOptions;
                    definition.LongOptions = option.LongOptions;
                    definition.Description = option.Description;
                }

                var position = property.GetCustomAttribute<CommandLinePositionAttribute>();
                if (position != null)
                    definition.Position = position.Position;

                var remaining = property.GetCustomAttribute<CommandLineRemainingAttribute>();
                if (remaining != null)
                    definition.Remainder = true;

                var requried = property.GetCustomAttribute<RequiredAttribute>();
                if (requried != null)
                    definition.Required = true;

                if (!definition.HasInfo)
                    continue;

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