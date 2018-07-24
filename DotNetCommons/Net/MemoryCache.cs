using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;

namespace DotNetCommons.Net
{
    public class MemoryCache : IWebCache
    {
        private readonly Dictionary<string, CommonWebResult> _store = new Dictionary<string, CommonWebResult>();

        public bool Exists(string uri)
        {
            return _store.ContainsKey(uri);
        }

        public CommonWebResult Fetch(string uri)
        {
            return _store.TryGetValue(uri, out var result) ? result : null;
        }

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

                    _store[uri] = result;
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
                foreach(var record in _store)
                {
                    writer.Write(record.Key);
                    writer.Write(record.Value.Success);
                    writer.Write((int)record.Value.StatusCode);
                    writer.Write(record.Value.StatusDescription ?? "");
                    writer.Write(record.Value.ContentEncoding ?? "");
                    writer.Write(record.Value.CharacterSet ?? "");
                    writer.Write(record.Value.ContentType ?? "");

                    writer.Write(record.Value.Headers.Count);
                    foreach(var header in record.Value.Headers)
                    {
                        writer.Write(header.Key);
                        writer.Write(header.Value ?? "");
                    }

                    writer.Write(record.Value.Data?.Length ?? 0);
                    if (record.Value.Data != null)
                        writer.Write(record.Value.Data);
                }
            }
        }

        public void Store(string uri, CommonWebResult result)
        {
            _store[uri] = result;
        }

        public bool TryFetch(string uri, out CommonWebResult result)
        {
            return _store.TryGetValue(uri, out result);
        }
    }
}
