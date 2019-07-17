using System;
using System.Linq;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Core.Sys
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CommandLineOptionAttribute : Attribute
    {
        public string[] ShortOptions { get; }
        public string[] LongOptions { get; }
        public string Description { get; }

        public CommandLineOptionAttribute(string longOption, string description = null)
        {
            ShortOptions = new string[0];
            LongOptions = new[] { longOption };
            Description = description;
        }

        public CommandLineOptionAttribute(char shortOption, string longOption = null, string description = null)
        {
            ShortOptions = new[] { shortOption.ToString() };
            LongOptions = new[] { longOption };
            Description = description;
        }

        public CommandLineOptionAttribute(char[] shortOptions, string[] longOption = null, string description = null)
        {
            ShortOptions = shortOptions?.Select(x => x.ToString()).ToArray() ?? new string[0];
            LongOptions = longOption ?? new string[0];
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
}
