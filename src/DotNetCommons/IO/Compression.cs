using System.IO.Compression;
using System.Text;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.IO;

public static class Compression
{
    /// <summary>
    /// Compress a byte buffer using the Deflate algorithm.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static byte[] Pack(byte[] data)
    {
        using var plain = new MemoryStream(data);
        using var packed = new MemoryStream();

        using (var compress = new DeflateStream(packed, CompressionMode.Compress, true))
        {
            plain.CopyTo(compress);
        }

        return packed.ToArray();
    }

    /// <summary>
    /// Compress a string using the Deflate algorithm.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static byte[] PackString(string data, Encoding? encoding = null)
    {
        return Pack((encoding ?? Encoding.UTF8).GetBytes(data));
    }

    /// <summary>
    /// Decompress a byte buffer using the Deflate algorithm.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static byte[] Unpack(byte[] data)
    {
        using var packed = new MemoryStream(data);
        using var plain = new MemoryStream();

        using (var compress = new DeflateStream(packed, CompressionMode.Decompress, true))
            compress.CopyTo(plain);

        return plain.ToArray();
    }

    /// <summary>
    /// Decompress a string using the Deflate algorithm.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static string UnpackString(byte[] data, Encoding? encoding = null)
    {
        return (encoding ?? Encoding.UTF8).GetString(Unpack(data));
    }
}