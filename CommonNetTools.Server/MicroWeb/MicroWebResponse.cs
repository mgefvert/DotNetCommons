using System;
using System.IO;
using System.Net;
using System.Text;

namespace CommonNetTools.Server.MicroWeb
{
    public abstract class MicroWebResponse
    {
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public string ContentType { get; set; } = "text/html";

        public abstract long ContentLength { get; }
        public abstract void WriteToStream(Stream response);

        public static MicroWebResponse Error(HttpStatusCode statusCode, string message = null)
        {
            var status = new StringBuilder();
            var text = statusCode.ToString();
            var first = true;
            foreach (var c in text)
            {
                if (!first && char.IsUpper(c))
                    status.Append(" ");
                status.Append(c);
                first = false;
            }

            return new MicroWebResponseContent($"<h1>{(int)statusCode} {status}</h1><p>{message}</p>")
            {
                StatusCode = statusCode,
            };
        }

        public static MicroWebResponse InternalServerError()
        {
            return Error(HttpStatusCode.InternalServerError, "An undefined exception occurred while processing this request.");
        }

        public static MicroWebResponse NotFound()
        {
            return Error(HttpStatusCode.NotFound, "The URL requested was not found.");
        }

        public static MicroWebResponse Empty()
        {
            return new MicroWebResponseContent("");
        }
    }

    public class MicroWebResponseContent : MicroWebResponse
    {
        public string Data { get; set; }
        public override long ContentLength => Buffer.LongLength;
        protected byte[] Buffer;

        public MicroWebResponseContent(string data) : this(data, Encoding.UTF8)
        {
        }

        public MicroWebResponseContent(string data, Encoding encoding)
        {
            Buffer = encoding.GetBytes(data);
        }

        public override void WriteToStream(Stream response)
        {
            response.Write(Buffer, 0, Buffer.Length);
        }
    }

    public class MicroWebResponseStream : MicroWebResponse
    {
        public Stream Stream { get; }
        public override long ContentLength => Stream.Length - Stream.Position;

        public MicroWebResponseStream(Stream stream, string contentType)
        {
            ContentType = contentType;
            Stream = stream;
        }

        public override void WriteToStream(Stream response)
        {
            Stream.CopyTo(response);
        }
    }
}
