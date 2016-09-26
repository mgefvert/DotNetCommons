using System;
using System.Collections.Generic;
using System.Reflection;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT

namespace CommonNetTools
{
    public class CommandLineDefinition
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
}