using System;
using System.Globalization;
using System.Net;

namespace DotNetCommons.IO.Parsers
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

            var result = new ApacheLogEntry();
            result.IP = IPAddress.Parse(fields[0].Text);
            result.UserName = fields[2].Text;
            result.Time = DateTime.ParseExact(fields[3].Text, "dd/MMM/yyyy:HH:mm:ss zzz", CultureInfo.InvariantCulture);
            result.Method = fields[4].Section[0].Text;
            result.Url = fields[4].Section[1].Text;
            result.Protocol = fields[4].Section[2].Text;
            result.ResponseCode = int.Parse(fields[5].Text);
            result.ResponseLength = int.Parse(fields[6].Text);
            result.Referer = fields[7].Section.ToString();
            result.UserAgent = fields[8].Section.ToString();

            return result;
        }
    }
}
