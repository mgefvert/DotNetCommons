using System;
using System.Text;

namespace CommonNetTools.IO.Csv
{
    public class CsvOptions
    {
        public string Delimiter { get; set; }
        public Encoding Encoding { get; set; }
        public string EndOfLine { get; set; }

        public CsvOptions()
        {
            Encoding = Encoding.UTF8;
        }
    }
}
