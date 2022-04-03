#nullable disable
using DotNetCommons.Text.Tokenizer;
using System;
using System.Globalization;
using System.Linq;
using System.Net;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Text.Parsers;

public class ApacheLogEntry
{
    public IPAddress IP { get; set; }
    public DateTime? Time { get; set; }
    public string Method { get; set; }
    public string Url { get; set; }
    public string Protocol { get; set; }
    public int? ResponseCode { get; set; }
    public int? ResponseLength { get; set; }
    public string UserAgent { get; set; }
    public string UserName { get; set; }
    public string Referer { get; set; }
}

public class ApacheLogParser
{
    private enum LogToken
    {
        Whitespace,
        Data,
        Quotation
    }

    private static readonly StringTokenizer<LogToken> Tokenizer = new(
        new Characters<LogToken>(TokenMode.Any, LogToken.Data, false),
        new Characters<LogToken>(TokenMode.Whitespace, LogToken.Whitespace, false),
        new Section<LogToken>("\"", "\"", true, LogToken.Quotation, false),
        new Section<LogToken>("[", "]", false, LogToken.Quotation, false)
    );

    public static ApacheLogEntry Parse(string line)
    {
        var fields = Tokenizer.Tokenize(line);
        fields.RemoveValues(LogToken.Whitespace);
        fields[4].Section.RemoveValues(LogToken.Whitespace);

        if (fields.Count < 9 || fields[4].Section.Count < 3)
            return null;

        if (fields.Any(x => string.IsNullOrEmpty(x.Text)))
            return null;

        return new ApacheLogEntry
        {
            IP = IPAddress.Parse(fields[0].Text!),
            UserName = fields[2].Text,
            Time = DateTime.ParseExact(fields[3].Text!, "dd/MMM/yyyy:HH:mm:ss zzz", CultureInfo.InvariantCulture),
            Method = fields[4].Section[0].Text,
            Url = fields[4].Section[1].Text,
            Protocol = fields[4].Section[2].Text,
            ResponseCode = int.Parse(fields[5].Text!),
            ResponseLength = int.Parse(fields[6].Text!),
            Referer = fields[7].Section.ToString(),
            UserAgent = fields[8].Section.ToString()
        };
    }
}