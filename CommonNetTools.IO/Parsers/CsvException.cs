using System;

namespace CommonNetTools.IO.Parsers
{
    public class CsvException : Exception
    {
        public CsvException(string message) : base(message)
        {
        }
    }
}
