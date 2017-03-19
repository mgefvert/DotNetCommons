using System;

namespace CommonNetTools
{
    public class StringTokenizerException : Exception
    {
        public string SourceText { get; set; }
        public int Position { get; set; }

        public StringTokenizerException(string message, int position, string sourceText) : base(message)
        {
            Position = position;
            SourceText = sourceText;
        }
    }
}
