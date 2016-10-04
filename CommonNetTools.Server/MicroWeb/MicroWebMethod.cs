using System;

namespace CommonNetTools.Server.MicroWeb
{
    public class MicroWebMethod
    {
        public string Path { get; }
        public MicroWebRequestHandler Handler { get; }

        public MicroWebMethod(string path, MicroWebRequestHandler handler)
        {
            Path = path;
            Handler = handler;
        }
    }
}
