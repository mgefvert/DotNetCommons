using System;

namespace DotNetCommons.Core.Text.Parsers
{
    public class CsvException : Exception
    {
        public CsvException(string message) : base(message)
        {
        }
    }
}
