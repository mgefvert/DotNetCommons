using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;

namespace DotNetCommons.Logger
{
    public static class LogPacker
    {
        private static readonly BinaryFormatter Formatter = new BinaryFormatter();

        public static byte[] Pack(List<LogEntry> entries)
        {
            using (var mem = new MemoryStream())
            {
                using (var gz = new GZipStream(mem, CompressionLevel.Fastest, true))
                    Formatter.Serialize(gz, entries);

                return mem.ToArray();
            }
        }

        public static List<LogEntry> Unpack(byte[] data)
        {
            using (var mem = new MemoryStream(data))
            using (var gz = new GZipStream(mem, CompressionMode.Decompress, true))
            {
                return (List<LogEntry>)Formatter.Deserialize(gz);
            }
        }
    }
}
