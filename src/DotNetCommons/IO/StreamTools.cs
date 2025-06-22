// ReSharper disable UnusedMember.Global

namespace DotNetCommons.IO;

public enum StreamMode
{
    FromCurrent,
    FromStart
}

public static class StreamTools
{
    /// <summary>
    /// Load a file into a MemoryStream.
    /// </summary>
    public static MemoryStream LoadFileIntoMemory(string filename)
    {
        var result = new MemoryStream();
        using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            fs.CopyTo(result);

        result.Position = 0;
        return result;
    }

    /// <summary>
    /// Read a number of bytes into a buffer. Will attempt to fill the entire
    /// buffer. Returns the number of bytes read.
    /// </summary>
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

    /// <summary>
    /// Read a stream to the end, using a fixed buffer size of 16K.
    /// </summary>
    public static byte[] ReadToEnd(Stream stream) => ReadToEnd(stream, 16384);

    /// <summary>
    /// Read a stream to the end, return a byte array of the bytes read.
    /// </summary>
    public static byte[] ReadToEnd(Stream stream, int bufferSize)
    {
        var result = new MemoryStream();
        var buffer = new byte[bufferSize];

        for (; ; )
        {
            var len = stream.Read(buffer, 0, bufferSize);
            if (len == 0)
                return result.ToArray();

            result.Write(buffer, 0, len);
        }
    }

    /// <summary>
    /// Save a stream to a file, using either the beginning or current position of the stream.
    /// </summary>
    public static void SaveStreamToFile(Stream stream, string filename, StreamMode mode)
    {
        if (mode == StreamMode.FromStart)
            stream.Position = 0;

        using var fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
        stream.CopyTo(fs);
    }
}