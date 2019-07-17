using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Core.Net.Cache
{
    [Flags]
    public enum PersistedMemoryFlags
    {
        None = 0,
        Compressed = 1
    }

    public class PersistedMemoryCache : MemoryCache
    {
        public void Load(string filename)
        {
            if (!File.Exists(filename))
                return;

            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                Load(fs);
        }

        public void Load(Stream stream)
        {
            Stream deflate = null;
            BinaryReader reader = null;

            try
            {
                reader = new BinaryReader(stream, Encoding.UTF8, true);
                var records = reader.ReadInt32();
                var flags = (PersistedMemoryFlags)reader.ReadInt32();

                if (flags.HasFlag(PersistedMemoryFlags.Compressed))
                {
                    deflate = new DeflateStream(stream, CompressionMode.Decompress, true);
                    reader = new BinaryReader(deflate, Encoding.UTF8, true);
                }

                while (records-- > 0)
                {
                    var item = LoadRecord(reader);
                    InternalStore[item.Uri] = item;
                }
            }
            finally
            {
                reader?.Dispose();
                deflate?.Dispose();
            }
        }

        private static CacheItem LoadRecord(BinaryReader reader)
        {
            var uri = reader.ReadString();
            var ticks = reader.ReadInt64();

            var result = new CommonWebResult
            {
                Success = reader.ReadBoolean(),
                StatusCode = (HttpStatusCode) reader.ReadInt32(),
                StatusDescription = NullIfEmpty(reader.ReadString()),
                ContentEncoding = NullIfEmpty(reader.ReadString()),
                CharacterSet = NullIfEmpty(reader.ReadString()),
                ContentType = NullIfEmpty(reader.ReadString())
            };

            var count = reader.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                var key = reader.ReadString();
                var value = reader.ReadString();
                result.Headers[key] = value;
            }

            count = reader.ReadInt32();
            result.Data = reader.ReadBytes(count);

            return new CacheItem
            {
                Uri = uri,
                Timestamp = new DateTime(ticks, DateTimeKind.Utc),
                Result = result
            };
        }

        private static string NullIfEmpty(string s)
        {
            return string.IsNullOrEmpty(s) ? null : s;
        }

        public void Save(string filename, PersistedMemoryFlags flags)
        {
            using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
                Save(fs, flags);
        }

        public void Save(Stream stream, PersistedMemoryFlags flags)
        {
            Stream deflate = null;
            BinaryWriter writer = null;

            try
            {
                writer = new BinaryWriter(stream, Encoding.UTF8, true);
                {
                    writer.Write(InternalStore.Count);
                    writer.Write((int) flags);
                }

                if (flags.HasFlag(PersistedMemoryFlags.Compressed))
                {
                    deflate = new DeflateStream(stream, CompressionMode.Compress, true);
                    writer.Flush();
                    writer = new BinaryWriter(deflate, Encoding.UTF8, true);
                }

                foreach (var record in InternalStore.Values)
                    SaveRecord(writer, record);
            }
            finally
            {
                writer?.Dispose();
                deflate?.Dispose();
                stream.Flush();
            }
        }

        public void SaveIfChanged(string filename, PersistedMemoryFlags flags)
        {
            if (Changed)
                Save(filename, flags);
        }

        private static void SaveRecord(BinaryWriter writer, CacheItem record)
        {
            writer.Write(record.Uri);
            writer.Write(record.Timestamp.Ticks);
            writer.Write(record.Result.Success);
            writer.Write((int) record.Result.StatusCode);
            writer.Write(record.Result.StatusDescription ?? "");
            writer.Write(record.Result.ContentEncoding ?? "");
            writer.Write(record.Result.CharacterSet ?? "");
            writer.Write(record.Result.ContentType ?? "");

            writer.Write(record.Result.Headers.Count);
            foreach (var header in record.Result.Headers)
            {
                writer.Write(header.Key);
                writer.Write(header.Value ?? "");
            }

            writer.Write(record.Result.Data?.Length ?? 0);
            if (record.Result.Data != null)
                writer.Write(record.Result.Data);
        }
    }
}
