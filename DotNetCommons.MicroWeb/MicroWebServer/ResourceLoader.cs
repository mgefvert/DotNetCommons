using System;
using System.IO;
using System.Reflection;

namespace DotNetCommons.MicroWeb.MicroWebServer
{
    public class ResourceLoader : IResourceLoader
    {
        private readonly string _root;
        private readonly Assembly _assembly;

        public ResourceLoader(Assembly assembly, string root)
        {
            _root = root.Trim('.');
            _assembly = assembly;
        }

        public byte[] Load(string filespec)
        {
            filespec = filespec.Replace('\\', '.').Replace('/', '.');

            using (var stream = _assembly.GetManifestResourceStream(_root + '.' + filespec))
            {
                if (stream == null)
                    throw new FileNotFoundException("Unable to find file " + filespec, filespec);

                var result = new byte[stream.Length];
                stream.Read(result, 0, result.Length);

                return result;
            }
        }
    }
}
