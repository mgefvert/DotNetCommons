using System;

namespace DotNetCommons.IO.Parsers
{
    public class CsvException : Exception
    {
        public CsvException(string message) : base(message)
        {
        }
    }
}
