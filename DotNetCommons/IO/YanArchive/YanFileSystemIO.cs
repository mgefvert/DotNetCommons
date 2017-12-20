using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DotNetCommons.IO.YanArchive
{
    public static class YanFileSystemIO
    {
        internal static MemoryStream BlockRead(Stream stream, Guid id, YanFileFlags flags, int position, int size, byte[] password)
        {
            var buffer = new byte[size];
            stream.Position = position;
            stream.Read(buffer, 0, size);

            var streams = new Stack<Stream>();
            streams.Push(new MemoryStream(buffer));

            if (password != null)
                streams.Push(new CryptoStream(streams.Peek(), GetCryptoProvider(password, id).CreateDecryptor(), CryptoStreamMode.Read));

            if (flags.HasFlag(YanFileFlags.GZip))
                streams.Push(new GZipStream(streams.Peek(), CompressionMode.Decompress));

            var result = new MemoryStream();
            streams.Peek().CopyTo(result);

            while (streams.Any())
                streams.Pop().Dispose();

            result.Position = 0;
            return result;
        }

        internal static int BlockWrite(Stream stream, Guid id, YanFileFlags flags, int position, byte[] password, Stream data)
        {
            var streams = new Stack<Stream>();
            streams.Push(new MemoryStream());

            if (password != null)
                streams.Push(new CryptoStream(streams.Peek(), GetCryptoProvider(password, id).CreateEncryptor(), CryptoStreamMode.Write));

            if (flags.HasFlag(YanFileFlags.GZip))
                streams.Push(new GZipStream(streams.Peek(), CompressionMode.Compress, true));

            data.CopyTo(streams.Peek());

            while (streams.Count > 1)
                streams.Pop().Dispose();

            var buffer = ((MemoryStream) streams.Peek()).ToArray();
            stream.Position = position;
            stream.Write(buffer, 0, buffer.Length);

            streams.Pop().Dispose();

            return buffer.Length;
        }

        internal static void Create(string filename, YanHeaderFlags flags, byte[] password = null)
        {
            if (File.Exists(filename))
                throw new IOException($"File {filename} already exists.");

            using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                HeaderWrite(fs, new YanHeader { Flags = flags });
            }
        }

        internal static YanHeader HeaderRead(Stream stream)
        {
            var buffer = new byte[YanHeader.Length];
            stream.Position = 0;
            if (stream.Read(buffer, 0, YanHeader.Length) != YanHeader.Length)
                throw new IOException("Unable to read header.");

            var result = new YanHeader();

            var magic = Encoding.ASCII.GetString(buffer, 0, 3);
            if (magic != "YAN")
                throw new IOException("Invalid file header.");

            result.Version = buffer[3];
            if (result.Version != 1)
                throw new IOException("Unhandled file version " + result.Version);

            result.Flags = (YanHeaderFlags)BitConverter.ToInt32(buffer, 4);
            result.IndexPosition = BitConverter.ToInt32(buffer, 8);

            return result;
        }

        internal static void HeaderWrite(Stream stream, YanHeader header)
        {
            var buffer = new byte[YanHeader.Length];
            buffer[0] = (byte)'Y';
            buffer[1] = (byte)'A';
            buffer[2] = (byte)'N';
            buffer[3] = header.Version;
            Buffer.BlockCopy(BitConverter.GetBytes((int)header.Flags), 0, buffer, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(header.IndexPosition), 0, buffer, 8, 4);

            stream.Position = 0;
            stream.Write(buffer, 0, YanHeader.Length);
        }

        internal static List<YanFile> IndexRead(Stream stream, int position, byte[] password)
        {
            var result = new List<YanFile>();

            using (var block = BlockRead(stream, Guid.Empty, YanFileFlags.GZip, position, (int)stream.Length - position, password))
            using (var reader = new BinaryReader(block, Encoding.UTF8))
            {
                var len = reader.ReadInt32();
                for (var i = 0; i < len; i++)
                    result.Add(new YanFile
                    {
                        Id = new Guid(reader.ReadBytes(16)),
                        Name = reader.ReadString(),
                        Size = reader.ReadInt32(),
                        SizeOnDisk = reader.ReadInt32(),
                        Position = reader.ReadInt32(),
                        Flags = (YanFileFlags) reader.ReadInt32()
                    });
            }

            return result;
        }

        internal static int IndexWrite(Stream stream, int position, byte[] password, List<YanFile> index)
        {
            using (var mem = new MemoryStream())
            using (var writer = new BinaryWriter(mem, Encoding.UTF8))
            {
                writer.Write(index.Count);
                foreach(var f in index)
                {
                    writer.Write(f.Id.ToByteArray());
                    writer.Write(f.Name);
                    writer.Write(f.Size);
                    writer.Write(f.SizeOnDisk);
                    writer.Write(f.Position);
                    writer.Write((int)f.Flags);
                }

                mem.Position = 0;
                stream.Position = position;
                return BlockWrite(stream, Guid.Empty, YanFileFlags.GZip, position, password, mem);
            }
        }

        private static AesManaged GetCryptoProvider(byte[] password, Guid iv)
        {
            return new AesManaged
            {
                BlockSize = 128,
                KeySize = 256,
                Mode = CipherMode.CBC,
                IV = iv.ToByteArray(),
                Key = new SHA256Managed().ComputeHash(password)
            };
        }
    }
}
