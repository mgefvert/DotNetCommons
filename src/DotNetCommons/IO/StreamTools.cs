using System;
using System.IO;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.IO
{
    public enum StreamMode
    {
        FromCurrent,
        FromStart
    }

    public static class StreamTools
    {
        public static MemoryStream LoadFileIntoMemory(string filename)
        {
            var result = new MemoryStream();
            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                fs.CopyTo(result);

            result.Position = 0;
            return result;
        }

        public static byte[] ReadToEnd(Stream stream)
        {
            var result = new MemoryStream();
            var buffer = new byte[16384];

            for (;;)
            {
                var len = stream.Read(buffer, 0, 16384);
                if (len == 0)
                    return result.ToArray();

                result.Write(buffer, 0, len);
            }
        }

        public static void SaveStreamToFile(Stream stream, string filename, StreamMode mode)
        {
            if (mode == StreamMode.FromStart)
                stream.Position = 0;

            using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
                stream.CopyTo(fs);
        }
    }
}
