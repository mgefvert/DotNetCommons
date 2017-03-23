using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using DotNetCommons.MicroWeb.MicroTemplates;

namespace DotNetCommons.MicroWeb.MicroWebServer
{
    public delegate MwResponse MwRequestHandler(MwRequest request);
    public delegate void PreprocessRequest(object sender, MwRequest args);

    public class MwServer : IDisposable
    {
        private HttpListener _listener;
        public List<string> Prefixes { get; } = new List<string>();
        public List<MwMethod> Methods { get; } = new List<MwMethod>();
        public TemplateManager TemplateManager { get; set; }

        public event PreprocessRequest PreprocessRequest;

        public MwServer()
        {
            if (!HttpListener.IsSupported)
                throw new NotSupportedException();
        }

        public void Dispose()
        {
            Stop();
        }

        private void HandleRequest(object c)
        {
            var ctx = (HttpListenerContext)c;
            try
            {
                var request = new MwRequest(ctx);
                OnPreprocessRequest(request);

                var path = request.HttpRequest.Url.AbsolutePath;
                var method = Methods.FirstOrDefault(m => m.Path == path) ??
                             Methods.FirstOrDefault(m => path.StartsWith(m.Path) && m.ControllerType != null);

                if (method == null)
                    SendResponse(ctx, MwResponse.NotFound());
                else if (method.Handler != null)
                    SendResponse(ctx, method.Handler(request));
                else if (method.ControllerType != null)
                {
                    using (var controller = (MwController) Activator.CreateInstance(method.ControllerType))
                    {
                        controller.InitializeRequest();
                        var response = controller.ResolveRequest(request, method.Path);
                        controller.FinalizeRequest();

                        SendResponse(ctx, response);
                    }
                }
            }
            catch (MwHandlerException ex)
            {
                SendResponse(ctx, ex.Response);
            }
            catch(Exception ex)
            {
                Logger.Err("Error in web server method handler:");
                Logger.Err(ex);
                SendResponse(ctx, MwResponse.InternalServerError());
            }
            finally
            {
                ctx.Response.OutputStream.Close();
            }
        }

        private void ListenerProc()
        {
            Logger.Notice("Starting web server...");
            try
            {
                _listener.Prefixes.Clear();
                foreach (var prefix in Prefixes)
                    _listener.Prefixes.Add(prefix);

                _listener.IgnoreWriteExceptions = true;
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

        protected virtual void OnPreprocessRequest(MwRequest request)
        {
            PreprocessRequest?.Invoke(this, request);
        }

        public void RegisterController<T>(string path) where T : MwController
        {
            if (!path.EndsWith("/"))
                path = path + "/";

            Methods.Add(new MwMethod(path, typeof(T)));
        }

        public void RegisterHandler(string path, MwRequestHandler handler)
        {
            Methods.Add(new MwMethod(path, handler));
        }

        public void RegisterPrefix(string prefix)
        {
            Prefixes.Add(prefix);
        }

        private void SendResponse(HttpListenerContext ctx, MwResponse result)
        {
            ctx.Response.StatusCode = (int)result.StatusCode;
            ctx.Response.ContentType = result.ContentType;
            ctx.Response.ContentLength64 = result.ContentLength;

            if (!result.Cache)
            {
                ctx.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, post-check=0, pre-check=0";
                ctx.Response.Headers["Expires"] = "Thu, 19 Nov 1981 08:52:00 GMT";
            }

            result.WriteToStream(ctx.Response.OutputStream);
        }

        public void Start()
        {
            if (_listener != null)
                return;

            _listener = new HttpListener();
            ThreadPool.QueueUserWorkItem(o => ListenerProc());
        }

        public void Stop()
        {
            if (_listener == null)
                return;

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

            _listener = null;
        }
    }
}
