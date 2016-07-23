using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT

namespace Gefvert.Tools.Common
{
  // --- Exceptions ---

  public class CommandLineException : Exception
  {
    public CommandLineException(string message) : base(message)
    {
    }

    public CommandLineException(string message, Exception innerException) : base(message, innerException)
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

  // --- Attributes ---

  [AttributeUsage(AttributeTargets.Property)]
  public class CommandLineOptionAttribute : Attribute
  {
    public char ShortOption { get; }
    public string LongOption { get; }
    public string Description { get; }

    public CommandLineOptionAttribute(string longOption, string description = null)
    {
      ShortOption = '\0';
      LongOption = longOption;
      Description = description;
    }

    public CommandLineOptionAttribute(char shortOption, string longOption = null, string description = null)
    {
      ShortOption = shortOption;
      LongOption = longOption;
      Description = description;
    }
  }

  [AttributeUsage(AttributeTargets.Property)]
  public class CommandLinePositionAttribute : Attribute
  {
    public int Position { get; }

    public CommandLinePositionAttribute(int position)
    {
      Position = position;
    }
  }

  [AttributeUsage(AttributeTargets.Property)]
  public class CommandLineRemainingAttribute : Attribute
  {
  }

  // --- CommandLine ---

  public static class CommandLine<T> where T : class, new()
  {
    public class Definition
    {
      public PropertyInfo Property { get; set; }
      public char ShortOption { get; set; } = '\0';
      public string LongOption { get; set; }
      public string Description { get; set; }
      public bool BooleanValue { get; set; }

      internal bool Remainder { get; set; }
      internal int Position { get; set; }
      internal bool HasInfo => ShortOption != '\x0' || LongOption != null || Position != -1 || Remainder || Description != null;
      internal bool IsAttribute => ShortOption != '\x0' || LongOption != null;

      public string OptionString
      {
        get
        {
          var result = new List<string>();

          if (ShortOption != '\0')
            result.Add("-" + ShortOption);

          if (!string.IsNullOrEmpty(LongOption))
            result.Add("--" + LongOption);

          return string.Join(", ", result);
        }
      }
    }

    private class Processor
    {
      public List<string> Arguments;
      public List<Definition> Definitions;
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

      private Definition FindDefinition(string key)
      {
        return Definitions.FirstOrDefault(x => x.ShortOption.ToString() == key || x.LongOption == key);
      }

      private Definition FindDefinition(int position)
      {
        return Definitions.FirstOrDefault(x => x.Position == position);
      }

      private Definition FindRemainder()
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

      private void SetParameter(Definition definition, object value)
      {
        if (definition.Property.PropertyType != value.GetType())
          value = Convert.ChangeType(value, definition.Property.PropertyType);

        definition.Property.SetValue(Result, value);
      }
    }

    public static List<Definition> GetParameters()
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
      var processor = new Processor
      {
        Arguments = args.ToList(),
        Definitions = GetDefinitionList(),
        Result = new T()
      };

      processor.Process();

      return processor.Result;
    }

    private static List<Definition> GetDefinitionList()
    {
      try
      {
        var result = new List<Definition>();
        foreach (var property in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
          var definition = new Definition();

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
