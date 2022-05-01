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
        Data
    }

    private static readonly StringTokenizer<LogToken> Tokenizer = new(
        new Characters<LogToken>(TokenMode.Any, LogToken.Data, false),
        new Characters<LogToken>(TokenMode.Whitespace, LogToken.Whitespace, true),
        new Section<LogToken>(LogToken.Data, "\"", "\"", true, false),
        new Section<LogToken>(LogToken.Data, "[", "]", false, false)
    );

    public static ApacheLogEntry Parse(string line)
    {
        var fields = Tokenizer.Tokenize(line);

        if (fields.Count < 9)
            return null;

        if (fields.Any(x => string.IsNullOrEmpty(x.InsideText)))
            return null;

        return new ApacheLogEntry
        {
            IP = IPAddress.Parse(fields[0].InsideText!),
            UserName = fields[2].InsideText,
            Time = DateTime.ParseExact(fields[3].InsideText!, "dd/MMM/yyyy:HH:mm:ss zzz", CultureInfo.InvariantCulture),
            Method = fields[4].Section[0].InsideText,
            Url = fields[4].Section[1].InsideText,
            Protocol = fields[4].Section[2].InsideText,
            ResponseCode = int.Parse(fields[5].InsideText!),
            ResponseLength = int.Parse(fields[6].InsideText!),
            Referer = fields[7].InsideText!,
            UserAgent = fields[8].InsideText!
        };
    }
}