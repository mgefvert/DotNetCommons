using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommonNetTools.Scripting.MicroTemplates;

namespace CommonNetTools.Scripting.MicroWebServer
{
    public class MwController : IDisposable
    {
        private class ActionMapEntry
        {
            public string Name;
            public MethodInfo Method;
            public HttpVerb Verb;
        }

        private static readonly Dictionary<string, ILookup<string, ActionMapEntry>> ActionMap = new Dictionary<string, ILookup<string,ActionMapEntry>>();
        private readonly ILookup<string, ActionMapEntry> _map;
        public TemplateManager TemplateManager { get; set; }

        public MwController()
        {
            BuildMap();
            _map = ActionMap[GetType().Name];
        }

        private void BuildMap()
        {
            var classname = GetType().Name;
            if (ActionMap.ContainsKey(classname))
                return;

            var map = new List<ActionMapEntry>();
            foreach (var method in GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                var verb = method.GetCustomAttribute<MwMethodAttribute>();
                if (verb != null)
                    map.Add(new ActionMapEntry
                    {
                        Name = method.Name,
                        Method = method,
                        Verb = verb.Verb
                    });
            }

            ActionMap[classname] = map.ToLookup(x => x.Name, StringComparer.InvariantCultureIgnoreCase);
        }

        public virtual void Dispose()
        {
        }

        public virtual void FinalizeRequest()
        {
        }

        public virtual void InitializeRequest()
        {
        }

        public MwResponse ResolveRequest(MwRequest request, string root)
        {
            HttpVerb verb;
            if (!Enum.TryParse(request.HttpRequest.HttpMethod, true, out verb))
                verb = HttpVerb.Unknown;

            var action = request.HttpRequest.Url.AbsolutePath.Substring(root.Length).Trim('/');
            if (action.Contains("/"))
                return MwResponse.NotFound();

            var method = _map[action].Where(x => x.Verb == HttpVerb.Any || x.Verb.HasFlag(verb)).ToList();
            if (method.Count == 0)
                return MwResponse.NotFound();
            if (method.Count != 1)
                return MwResponse.InternalServerError("Unable to find a single controller action for this request.");

            var methodInfo = method.Single().Method;
            return (MwResponse)methodInfo.Invoke(this, new object[] { request });
        }
    }
}
