using System;
using System.Net;

namespace DotNetCommons.Logger
{
    public class LogEntryOptions
    {
        public LogEntryOptions(IPAddress address = null, string user = null)
        {
            Address = address;
            User = user;
        }

        /// <summary>Optional duration of the event</summary>
        public TimeSpan? Duration { get; set; }

        /// <summary>Optional address involved (IP address)</summary>
        public IPAddress Address { get; set; }

        /// <summary>Optional user name</summary>
        public string User { get; set; }
    }
}
