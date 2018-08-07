using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;

namespace DotNetCommons.Net.Cache
{
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
            using (var deflate = new DeflateStream(stream, CompressionMode.Decompress, true))
            using (var reader = new BinaryReader(deflate, Encoding.UTF8, true))
            {
                var records = reader.ReadInt32();
                while (records-- > 0)
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
                    for (int i = 0; i < count; i++)
                    {
                        var key = reader.ReadString();
                        var value = reader.ReadString();
                        result.Headers[key] = value;
                    }

                    count = reader.ReadInt32();
                    result.Data = reader.ReadBytes(count);

                    _store[uri] = new CacheItem
                    {
                        Uri = uri,
                        Timestamp = new DateTime(ticks, DateTimeKind.Utc),
                        Result = result
                    };
                }
            }
        }

        private string NullIfEmpty(string s)
        {
            return string.IsNullOrEmpty(s) ? null : s;
        }

        public void Save(string filename)
        {
            using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
                Save(fs);
        }

        public void Save(Stream stream)
        {
            using (var deflate = new DeflateStream(stream, CompressionMode.Compress, true))
            using (var writer = new BinaryWriter(deflate, Encoding.UTF8, true))
            {
                writer.Write(_store.Count);
                foreach(var record in _store.Values)
                {
                    writer.Write(record.Uri);
                    writer.Write(record.Timestamp.Ticks);
                    writer.Write(record.Result.Success);
                    writer.Write((int)record.Result.StatusCode);
                    writer.Write(record.Result.StatusDescription ?? "");
                    writer.Write(record.Result.ContentEncoding ?? "");
                    writer.Write(record.Result.CharacterSet ?? "");
                    writer.Write(record.Result.ContentType ?? "");

                    writer.Write(record.Result.Headers.Count);
                    foreach(var header in record.Result.Headers)
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

        public void SaveIfChanged(string filename)
        {
            if (Changed)
                Save(filename);
        }
    }
}
