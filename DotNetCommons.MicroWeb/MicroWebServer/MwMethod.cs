using System;

namespace DotNetCommons.MicroWeb.MicroWebServer
{
    public class MwMethod
    {
        public string Path { get; }
        public MwRequestHandler Handler { get; }
        public Type ControllerType { get; }

        public MwMethod(string path, MwRequestHandler handler)
        {
            Path = path;
            Handler = handler;
        }

        public MwMethod(string path, Type controllerType)
        {
            if (!controllerType.IsSubclassOf(typeof(MwController)))
                throw new InvalidCastException("Controller must inherit from MwController");

            Path = path;
            ControllerType = controllerType;
        }
    }
}
