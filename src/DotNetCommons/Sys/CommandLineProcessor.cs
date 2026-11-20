using System.Collections;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Sys;

internal class CommandLineProcessor
{
    private enum OptionType
    {
        None,
        Short,
        Long
    }

    public List<string> Arguments { get; }
    public List<CommandLineDefinition> Definitions { get; }
    public object Result { get; }

    private readonly HashSet<CommandLineDefinition> _assignedParameters = new();
    private readonly bool _multipleShortOptions;
    private int _position;

    public CommandLineProcessor(Type resultType, List<string> args, List<CommandLineDefinition> definitions, bool multipleShortOptions)
    {
        Arguments   = args;
        Definitions = definitions;
        _multipleShortOptions = multipleShortOptions;
        Result      = Activator.CreateInstance(resultType) 
                      ?? throw new CommandLineException($"Unable to create instance of type {resultType}");
    }

    public void Process()
    {
        var parseOptions = true;
        while (Arguments.Any())
        {
            var arg = Arguments.ExtractFirst();
            if (parseOptions && arg == "--")
            {
                parseOptions = false;
                continue;
            }

            var type = parseOptions ? FindOptionType(ref arg) : OptionType.None;

            if (type == OptionType.None)
                ProcessPosition(arg);
            else
                ProcessOption(arg, type);
        }
    }

    private void ProcessOption(string arg, OptionType type)
    {
        string? value = null;

        if (arg.Contains('='))
        {
            var items = arg.Split(['='], 2);
            arg = items[0];
            value = items[1].Trim('"');
        }
        else if (arg.Length >= 2 && type == OptionType.Short)
        {
            // More characters than we expected for a short option.
            // This can be either like "-xzf" for multiple options at once, or
            // "-uroot" for MySQL-like options; depending on MultipleShortOptions.

            if (_multipleShortOptions)
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

            SetParameter(definition, Arguments.ExtractFirst());
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

        if (definition.Property.GetValue(Result) is not ICollection<string> remainder)
            throw new CommandLineException("No CommandLineRemainder property found, or the designated property is not an ICollection<string>.");

        remainder.Add(arg);
    }

    private CommandLineDefinition? FindDefinition(string key)
    {
        return Definitions.FirstOrDefault(x => x.MatchesShortOption(key) || x.MatchesLongOption(key));
    }

    private CommandLineDefinition? FindDefinition(int position)
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

    private CommandLineDefinition? FindRemainder()
    {
        return Definitions.FirstOrDefault(x => x.Remainder);
    }

    private void SetParameter(CommandLineDefinition definition, object value)
    {
        var type = definition.Property.PropertyType;
        var elementType = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)
            ? type.GetGenericArguments().Single()
            : null;
        var valueType = elementType ?? type;
        var enumType = Nullable.GetUnderlyingType(valueType) ?? valueType;
        if (value is string text && enumType.IsEnum)
            value = ParseEnum(definition, enumType, text);

        if (elementType != null)
        {
            // Add the value onto a list instead of just setting the property
            if (definition.Property.GetValue(Result) is not IList list)
                throw new CommandLineException("Property " + definition.Property.Name + " has not been initialized.");

            if (!enumType.IsEnum && elementType != value.GetType())
                value = Convert.ChangeType(value, elementType);

            list.Add(value);
            return;
        }

        definition.Property.SetPropertyValue(Result, value);
        if (definition.Required && type.IsValueType)
            _assignedParameters.Add(definition);
    }

    private static object ParseEnum(CommandLineDefinition definition, Type enumType, string value)
    {
        var text = value.Trim();
        // Keep the sign on numeric values while allowing hyphenated enum names.
        if (text.Length > 0 && (char.IsLetter(text[0]) || text[0] == '_'))
            text = text.Replace("-", "");

        if (Enum.TryParse(enumType, text, ignoreCase: true, out var result))
            return result;

        var parameter = definition.IsAttribute ? definition.OptionString : definition.Property.Name;
        var choices = string.Join(", ", Enum.GetNames(enumType));
        throw new CommandLineParameterException(parameter, CommandLineParameterError.InvalidValue,
            $"Invalid value '{value}' for {parameter}. Expected one of {choices}.");
    }

    public void Validate()
    {
        foreach (var definition in Definitions.Where(x => x.Required))
        {
            var missingValueOption = definition.IsAttribute && definition.Property.PropertyType.IsValueType
                && !_assignedParameters.Contains(definition);
            if (missingValueOption || definition.Property.GetValue(Result) == null)
                throw new CommandLineParameterException(definition.OptionString, CommandLineParameterError.ValueRequired);
        }
    }
}
