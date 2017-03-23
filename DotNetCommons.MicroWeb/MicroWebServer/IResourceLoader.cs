using System;

namespace DotNetCommons.MicroWeb.MicroWebServer
{
    public interface IResourceLoader
    {
        byte[] Load(string filespec);
    }
}
