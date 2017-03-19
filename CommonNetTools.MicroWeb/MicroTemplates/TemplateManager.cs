using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CommonNetTools.Scripting.MicroWebServer;

namespace CommonNetTools.Scripting.MicroTemplates
{
    public class TemplateManager
    {
        private readonly IResourceLoader _loader;
        private readonly ITemplateParser _parser;
        private readonly Dictionary<string, Template> _cache = new Dictionary<string, Template>(StringComparer.InvariantCultureIgnoreCase); 

        public TemplateManager(IResourceLoader loader, ITemplateParser parser)
        {
            _loader = loader;
            _parser = parser;
        }

        protected Template DoLoad(string path)
        {
            if (_cache.ContainsKey(path))
                return _cache[path];

            var str = string.Intern("__templatecache_" + path);
            lock (str)
            {
                if (_cache.ContainsKey(path))
                    return _cache[path];

                var data = Encoding.UTF8.GetString(_loader.Load(path));
                var template = new Template(data, path, _parser);
                template.Compile();
                _cache[path] = template;

                return template;
            }
        }

        public Template Load(string path)
        {
            return DoLoad(NormalizePath(null, path));
        }

        public Template Load(string current, string relativePath)
        {
            return DoLoad(NormalizePath(current, relativePath));
        }

        protected string NormalizePath(string current, string path)
        {
            current = Wash(current);
            path = Wash(path);

            var currentList = path.StartsWith("/") ? new List<string>() : SplitPath(current, true);
            var relativeList = SplitPath(path, false);

            foreach (var item in relativeList)
            {
                switch (item)
                {
                    case ".":
                        continue;
                    case "..":
                        if (currentList.Any())
                            currentList.RemoveAt(currentList.Count - 1);
                        else
                            throw new FileNotFoundException("File not found: " + path);
                        break;
                    default:
                        currentList.Add(item);
                        break;
                }
            }

            return string.Join("/", currentList);
        }

        public string Process(string path, object data = null)
        {
            var template = Load(path);
            var output = template.Run(data);
            var c = 0;

            while (!string.IsNullOrEmpty(output.ExtendsTemplate))
            {
                c++;
                if (c > 6)
                    throw new FileLoadException("Template extend depth is too large.");

                template = Load(template.Filename, output.ExtendsTemplate);
                var master = template.Run();
                output = TemplateOutput.Merge(master, output);
            }

            return output.GetResult();
        }

        protected List<string> SplitPath(string path, bool removeLast)
        {
            var list = path
                .Replace('\\', '/')
                .Split('/')
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            if (removeLast && list.Any())
                list.RemoveAt(list.Count - 1);

            return list;
        }

        protected string Wash(string path)
        {
            return (path ?? "").Trim().Replace("\\", "/");
        }
    }
}
