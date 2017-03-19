using System;
using System.IO;

namespace CommonNetTools
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

        public static void SaveStreamToFile(Stream stream, string filename, StreamMode mode)
        {
            if (mode == StreamMode.FromStart)
                stream.Position = 0;

            using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
                stream.CopyTo(fs);
        }
    }
}
