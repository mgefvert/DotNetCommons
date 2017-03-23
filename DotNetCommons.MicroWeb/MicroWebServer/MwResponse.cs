using System;
using System.IO;
using System.Net;
using System.Text;

namespace DotNetCommons.MicroWeb.MicroWebServer
{
    public abstract class MwResponse
    {
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public string ContentType { get; set; } = "text/html";
        public bool Cache { get; set; }

        public abstract long ContentLength { get; }
        public abstract void WriteToStream(Stream response);

        public static MwResponse BadRequest()
        {
            return Error(HttpStatusCode.BadRequest, "The request was malformed or did not contain the required parameters.");
        }

        public static MwResponse Empty()
        {
            return new MwResponseContent("");
        }

        public static MwResponse Error(HttpStatusCode statusCode, string message = null)
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

            return new MwResponseContent($"<h1>{(int)statusCode} {status}</h1><p>{message}</p>")
            {
                StatusCode = statusCode,
            };
        }

        public static MwResponse InternalServerError()
        {
            return Error(HttpStatusCode.InternalServerError, "An undefined exception occurred while processing this request.");
        }

        public static MwResponse InternalServerError(string message)
        {
            return Error(HttpStatusCode.InternalServerError, message);
        }

        public static MwResponse NotFound()
        {
            return Error(HttpStatusCode.NotFound, "The URL requested was not found.");
        }
    }

    public class MwResponseContent : MwResponse
    {
        public string Data { get; set; }
        public override long ContentLength => Buffer.LongLength;
        protected byte[] Buffer;

        public MwResponseContent(string data) : this(data, Encoding.UTF8)
        {
        }

        public MwResponseContent(string data, Encoding encoding)
        {
            Buffer = encoding.GetBytes(data);
        }

        public override void WriteToStream(Stream response)
        {
            response.Write(Buffer, 0, Buffer.Length);
        }
    }

    public class MwResponseJson : MwResponseContent
    {
        public MwResponseJson(string data) : base(data, Encoding.UTF8)
        {
            ContentType = "application/json";
        }
    }

    public class MwResponseStream : MwResponse
    {
        public Stream Stream { get; }
        public override long ContentLength => Stream.Length - Stream.Position;

        public MwResponseStream(Stream stream, string contentType)
        {
            ContentType = contentType;
            Stream = stream;
            Cache = true;
        }

        public override void WriteToStream(Stream response)
        {
            Stream.CopyTo(response);
        }
    }
}
