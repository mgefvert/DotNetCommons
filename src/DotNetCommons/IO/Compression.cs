using System.IO.Compression;
using System.Text;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.IO;

public static class Compression
{
    /// Compress a byte buffer using the Deflate algorithm.
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

    /// Compress a string using the Deflate algorithm.
    public static byte[] PackString(string data, Encoding? encoding = null)
    {
        return Pack((encoding ?? Encoding.UTF8).GetBytes(data));
    }

    /// Decompress a byte buffer using the Deflate algorithm.
    public static byte[] Unpack(byte[] data)
    {
        using var packed = new MemoryStream(data);
        using var plain = new MemoryStream();

        using (var compress = new DeflateStream(packed, CompressionMode.Decompress, true))
            compress.CopyTo(plain);

        return plain.ToArray();
    }

    /// Decompress a string using the Deflate algorithm.
    public static string UnpackString(byte[] data, Encoding? encoding = null)
    {
        return (encoding ?? Encoding.UTF8).GetString(Unpack(data));
    }
}