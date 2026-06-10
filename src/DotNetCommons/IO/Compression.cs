using System.IO.Compression;
using System.Text;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.IO;

public static class Compression
{
    /// Compress a byte buffer using the Deflate algorithm.
    public static byte[] Compress(byte[] data, CompressionMethod method = CompressionMethod.Deflate,
        CompressionLevel level = CompressionLevel.Optimal)
    {
        using var compressedStream = new MemoryStream();

        using (var compressionStream = GetCompressionStream(compressedStream, level, method))
            compressionStream.Write(data, 0, data.Length);

        return compressedStream.ToArray();
    }

    /// Compress a string using the Deflate algorithm.
    public static byte[] CompressString(string data, CompressionMethod method = CompressionMethod.Deflate,
        CompressionLevel level = CompressionLevel.Optimal, Encoding? encoding = null)
    {
        return Compress((encoding ?? Encoding.UTF8).GetBytes(data), method, level);
    }

    /// Decompress a byte buffer using the Deflate algorithm.
    public static byte[] Decompress(byte[] compressedData, CompressionMethod method = CompressionMethod.Deflate)
    {
        using var compressedStream = new MemoryStream(compressedData);
        using var plaintextStream  = new MemoryStream();

        using (var decompressionStream = GetDecompressionStream(compressedStream, method))
            decompressionStream.CopyTo(plaintextStream);

        return plaintextStream.ToArray();
    }

    /// Decompress a string using the Deflate algorithm.
    public static string DecompressString(byte[] compressedData,
        CompressionMethod method = CompressionMethod.Deflate, Encoding? encoding = null)
    {
        return (encoding ?? Encoding.UTF8).GetString(Decompress(compressedData, method));
    }

    public static Stream GetCompressionStream(Stream output, CompressionLevel level, CompressionMethod method)
    {
        return method switch
        {
            CompressionMethod.Deflate => new DeflateStream(output, level, true),
            CompressionMethod.GZip    => new GZipStream(output, level, true),
            CompressionMethod.Brotli  => new BrotliStream(output, level, true),
            _                         => throw new ArgumentOutOfRangeException(nameof(method), method, null)
        };
    }

    public static Stream GetDecompressionStream(Stream input, CompressionMethod method)
    {
        return method switch
        {
            CompressionMethod.Deflate => new DeflateStream(input, CompressionMode.Decompress),
            CompressionMethod.GZip    => new GZipStream(input, CompressionMode.Decompress),
            CompressionMethod.Brotli  => new BrotliStream(input, CompressionMode.Decompress),
            _                         => throw new ArgumentOutOfRangeException(nameof(method), method, null)
        };
    }
}