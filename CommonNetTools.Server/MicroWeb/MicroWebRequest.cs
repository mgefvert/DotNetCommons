using System;
using System.Net;

namespace CommonNetTools.Server.MicroWeb
{
    public class MicroWebRequest : EventArgs
    {
        public HttpListenerContext Context { get; }
        public HttpListenerRequest HttpRequest { get; }

        public MicroWebRequest(HttpListenerContext context)
        {
            Context = context;
            HttpRequest = context.Request;
        }
    }
}
