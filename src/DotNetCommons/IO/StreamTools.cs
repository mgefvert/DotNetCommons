using System;
using System.IO;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.IO;

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

        public static int ReadIntoBuffer(Stream stream, byte[] buffer)
        {
            var span = buffer.AsSpan();
            var totalRead = 0;

            while (totalRead < buffer.Length)
            {
                var len = stream.Read(span[totalRead..]);
                if (len == 0)
                    return totalRead;

                totalRead += len;
            }

            return totalRead;
        }

        public static byte[] ReadToEnd(Stream stream) => ReadToEnd(stream, 16384);

        public static byte[] ReadToEnd(Stream stream, int bufferSize)
    {
        var result = new MemoryStream();
            var buffer = new byte[bufferSize];

            for (;;)
        {
                var len = stream.Read(buffer, 0, bufferSize);
            if (len == 0)
                return result.ToArray();

            result.Write(buffer, 0, len);
        }
    }

    public static void SaveStreamToFile(Stream stream, string filename, StreamMode mode)
    {
        if (mode == StreamMode.FromStart)
            stream.Position = 0;

        using var fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
        stream.CopyTo(fs);
    }
}