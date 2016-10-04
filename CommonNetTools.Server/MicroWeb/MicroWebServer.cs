using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

namespace CommonNetTools.Server.MicroWeb
{
    public delegate MicroWebResponse MicroWebRequestHandler(MicroWebRequest request);
    public delegate void PreprocessRequest(object sender, MicroWebRequest args);

    public class MicroWebServer : IDisposable
    {
        private readonly HttpListener _listener = new HttpListener();
        public List<string> Prefixes { get; } = new List<string>();
        public List<MicroWebMethod> Methods { get; } = new List<MicroWebMethod>();

        public event PreprocessRequest PreprocessRequest;

        public MicroWebServer()
        {
            if (!HttpListener.IsSupported)
                throw new NotSupportedException();
        }

        public void Dispose()
        {
            Stop();
        }

        private void Run()
        {
            Logger.Notice("Starting web server...");
            try
            {
                _listener.Prefixes.Clear();
                foreach (var prefix in Prefixes)
                    _listener.Prefixes.Add(prefix);

                _listener.Start();
                while (_listener.IsListening)
                {
                    ThreadPool.QueueUserWorkItem(HandleRequest, _listener.GetContext());
                }
            }
            catch(Exception ex)
            {
                Logger.Err("Fatal error in web server:");
                Logger.Err(ex);
            }
        }

        private void HandleRequest(object c)
        {
            var ctx = (HttpListenerContext)c;
            try
            {
                var request = new MicroWebRequest(ctx);
                OnPreprocessRequest(request);

                var path = request.HttpRequest.Url.AbsolutePath;
                var method = Methods.FirstOrDefault(m => m.Path == path)?.Handler;

                SendResponse(ctx, method == null ? MicroWebResponse.NotFound() : method(request));
            }
            catch (MicroWebHandlerException ex)
            {
                SendResponse(ctx, ex.Response);
            }
            catch(Exception ex)
            {
                Logger.Err("Error in web server method handler:");
                Logger.Err(ex);
                SendResponse(ctx, MicroWebResponse.InternalServerError());
            }
            finally
            {
                ctx.Response.OutputStream.Close();
            }
        }

        protected virtual void OnPreprocessRequest(MicroWebRequest request)
        {
            PreprocessRequest?.Invoke(this, request);
        }

        private void SendResponse(HttpListenerContext ctx, MicroWebResponse result)
        {
            ctx.Response.StatusCode = (int)result.StatusCode;
            ctx.Response.ContentType = result.ContentType;
            ctx.Response.ContentLength64 = result.ContentLength;
            result.WriteToStream(ctx.Response.OutputStream);
        }

        public void Start()
        {
            ThreadPool.QueueUserWorkItem(o => Run());
        }

        public void Stop()
        {
            Logger.Notice("Shutting down web server");

            try
            {
                _listener.Stop();
            }
            catch (Exception ex)
            {
                Logger.Err(ex);
            }

            try
            {
                _listener.Close();
            }
            catch (Exception ex)
            {
                Logger.Err(ex);
            }
        }
    }
}
