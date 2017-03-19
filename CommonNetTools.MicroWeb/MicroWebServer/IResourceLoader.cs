using System;

namespace CommonNetTools.Scripting.MicroWebServer
{
    public interface IResourceLoader
    {
        byte[] Load(string filespec);
    }
}
