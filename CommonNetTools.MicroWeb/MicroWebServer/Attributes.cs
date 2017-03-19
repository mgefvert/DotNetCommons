using System;

namespace CommonNetTools.Scripting.MicroWebServer
{
    [Flags]
    public enum HttpVerb
    {
        Any = 65535,
        Get = 1,
        Post = 2,
        Put = 4,
        Patch = 8,
        Delete = 16,
        Options = 32,
        Head = 64,
        Trace = 128,
        Connect = 256,
        Unknown = 512
    }

    public class MwMethodAttribute : Attribute
    {
        public HttpVerb Verb { get; }

        public MwMethodAttribute(HttpVerb verb)
        {
            Verb = verb;
        }
    }
}
