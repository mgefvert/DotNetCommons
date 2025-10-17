﻿using System.Reflection;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Sys;

public class CommandLineDefinition
{
    public bool BooleanValue { get; }
    public string? Description { get; set; }
    public string[]? LongOptions { get; set; }
    public PropertyInfo Property { get; }
    public string[]? ShortOptions { get; set; }
    public bool Required { get; set; }

    internal bool Remainder { get; set; }
    internal int Position { get; set; }

    public CommandLineDefinition(PropertyInfo property)
    {
        Property = property;
        BooleanValue = property.PropertyType == typeof(bool);
    }

    internal bool HasInfo => AnyShortOptions() || AnyLongOptions() || Position != -1 || Remainder || Description != null;
    internal bool IsAttribute => AnyShortOptions() || AnyLongOptions();

    public string OptionString
    {
        get
        {
            var result = new List<string>();

            if (AnyShortOptions())
                result.Add("-" + ShortOptions!.First());

            if (AnyLongOptions())
                result.Add("--" + LongOptions!.First());

            return string.Join(", ", result);
        }
    }

    public bool AnyLongOptions() => LongOptions != null && LongOptions.Length > 0;
    public bool AnyShortOptions() => ShortOptions != null && ShortOptions.Length > 0;

    public bool MatchesLongOption(string arg) => AnyLongOptions() && LongOptions!.Any(o => o == arg);
    public bool MatchesShortOption(string arg) => AnyShortOptions() && ShortOptions!.Any(o => o == arg);
}