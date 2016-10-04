using System;
using System.Net;

namespace CommonNetTools.Server.MicroWeb
{
    public class MicroWebHandlerException : Exception
    {
        public MicroWebResponse Response { get; }

        public MicroWebHandlerException(MicroWebResponse error)
        {
            Response = error;
        }

        public MicroWebHandlerException(HttpStatusCode statusCode, string message = null)
        {
            Response = MicroWebResponse.Error(statusCode, message);
        }
    }
}
