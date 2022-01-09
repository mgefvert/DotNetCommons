using System;

namespace DotNetCommons.Text;

public class StringTokenizerException : Exception
{
    public string SourceText { get; set; }
    public int? Position { get; set; }

    public StringTokenizerException(string message) : base(message)
    {
    }

    public StringTokenizerException(string message, int position, string sourceText) : base(message)
    {
        Position = position;
        SourceText = sourceText;
    }
}