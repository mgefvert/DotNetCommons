using System;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT

namespace DotNetCommons.Sys
{
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
}
