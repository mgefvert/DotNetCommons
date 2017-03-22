using System;
using CommonNetTools.IO.Parsers;

namespace CommonNetTools.IO.Test.Parsers
{
    public class Name
    {
        [CsvField("Id", false)]
        public int Id { get; set; }

        [CsvField("GivenName", true)]
        public string FirstName { get; set; }

        [CsvField("SurName", true)]
        public string LastName { get; set; }

        [CsvField("City", true)]
        public string City { get; set; }

        [CsvField("TelephoneNumber", true)]
        public string PhoneNumber { get; set; }

        [CsvField("Birthday", true)]
        public DateTime? Birthday { get; set; }

        [CsvField("GUID", true)]
        public Guid Guid { get; set; }
    }
}
