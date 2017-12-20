using System;

namespace DotNetCommons.Text.Parsers
{
    public class CsvException : Exception
    {
        public CsvException(string message) : base(message)
        {
        }
    }
}
