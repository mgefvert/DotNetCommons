using System;
using System.Globalization;
using System.Net;
using DotNetCommons.Text.Tokenizer;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Text.Parsers
{
    public class ApacheLogEntry
    {
        public IPAddress IP { get; set; }
        public DateTime Time { get; set; }
        public string Method { get; set; }
        public string Url { get; set; }
        public string Protocol { get; set; }
        public int ResponseCode { get; set; }
        public int ResponseLength { get; set; }
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

        private static readonly TokenDefinition[] Definitions = {
            new TokenCharacterModeDefinition(TokenMode.Any, (int)LogToken.Data, false),
            new TokenCharacterModeDefinition(TokenMode.Whitespace, (int)LogToken.Whitespace, false),
            new TokenSectionDefinition("\"", "\"", true, (int)LogToken.Quotation, false),
            new TokenSectionDefinition("[", "]", false, (int)LogToken.Quotation, false)
        };

        private static readonly StringTokenizer Tokenizer = new StringTokenizer(Definitions);

        public static ApacheLogEntry Parse(string line)
        {
            var fields = Tokenizer.Tokenize(line);
            fields.RemoveValues((int)LogToken.Whitespace);
            fields[4].Section.RemoveValues((int)LogToken.Whitespace);

            if (fields.Count < 9 || fields[4].Section.Count < 3)
                return null;

            return new ApacheLogEntry
            {
                IP = IPAddress.Parse(fields[0].Text),
                UserName = fields[2].Text,
                Time = DateTime.ParseExact(fields[3].Text, "dd/MMM/yyyy:HH:mm:ss zzz", CultureInfo.InvariantCulture),
                Method = fields[4].Section[0].Text,
                Url = fields[4].Section[1].Text,
                Protocol = fields[4].Section[2].Text,
                ResponseCode = int.Parse(fields[5].Text),
                ResponseLength = int.Parse(fields[6].Text),
                Referer = fields[7].Section.ToString(),
                UserAgent = fields[8].Section.ToString()
            };
        }
    }
}
