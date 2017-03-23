using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace DotNetCommons.IO
{
    public class KeyValueStore
    {
        public Encoding Encoding { get; set; } = Encoding.UTF8;
        private readonly BinaryFormatter _fmt = new BinaryFormatter();

        public Dictionary<TKey, TValue> Load<TKey, TValue>(string filename)
        {
            if (!File.Exists(filename))
                return new Dictionary<TKey, TValue>();

            using (var fs = new FileStream(filename, FileMode.Open))
                return Load<TKey, TValue>(fs);
        }

        public Dictionary<TKey, TValue> Load<TKey, TValue>(Stream stream)
        {
            using (var zip = new DeflateStream(stream, CompressionMode.Decompress, true))
            using (var reader = new BinaryReader(zip, Encoding, true))
            {
                var result = new Dictionary<TKey, TValue>();

                var count = reader.ReadInt32();
                while (count-- > 0)
                {
                    var key = (TKey)ReadValue<TKey>(reader);
                    var value = (TValue)ReadValue<TValue>(reader);

                    result[key] = value;
                }

                return result;
            }
        }

        private object ReadValue<T>(BinaryReader reader)
        {
            var type = typeof(T);

            if (type == typeof(short))
                return reader.ReadInt16();
            if (type == typeof(int))
                return reader.ReadInt32();
            if (type == typeof(long))
                return reader.ReadInt64();
            if (type == typeof(ushort))
                return reader.ReadUInt16();
            if (type == typeof(uint))
                return reader.ReadUInt32();
            if (type == typeof(ulong))
                return reader.ReadUInt64();
            if (type == typeof(bool))
                return reader.ReadBoolean();
            if (type == typeof(byte))
                return reader.ReadByte();
            if (type == typeof(sbyte))
                return reader.ReadSByte();
            if (type == typeof(char))
                return reader.ReadChar();
            if (type == typeof(decimal))
                return reader.ReadDecimal();
            if (type == typeof(double))
                return reader.ReadDouble();
            if (type == typeof(float))
                return reader.ReadSingle();
            if (type == typeof(string))
                return reader.ReadString();
            if (type == typeof(char[]))
            {
                var count = reader.ReadInt32();
                return reader.ReadChars(count);
            }
            if (type == typeof(byte[]))
            {
                var count = reader.ReadInt32();
                return reader.ReadBytes(count);
            }

            // Generic object
            {
                var count = reader.ReadInt32();
                var data = reader.ReadBytes(count);

                using (var mem = new MemoryStream(data))
                {
                    var obj = _fmt.Deserialize(mem);
                    if (obj is T)
                        return obj;
                }
            }

            return default(T);
        }

        public void Save<TKey, TValue>(Dictionary<TKey, TValue> dictionary, string filename)
        {
            using (var fs = new FileStream(filename, FileMode.Create))
                Save(dictionary, fs);
        }

        public void Save<TKey, TValue>(Dictionary<TKey, TValue> dictionary, Stream stream)
        {
            using (var zip = new DeflateStream(stream, CompressionMode.Compress, true))
            using (var writer = new BinaryWriter(zip, Encoding, true))
            {
                writer.Write(dictionary.Count);

                foreach (var item in dictionary)
                { 
                    SaveValue(writer, item.Key);
                    SaveValue(writer, item.Value);
                }
            }
        }

        public void SaveValue(BinaryWriter writer, object value)
        {
            var type = value.GetType();

            if (type == typeof(short))
                writer.Write((short)value);
            else if (type == typeof(int))
                writer.Write((int)value);
            else if (type == typeof(long))
                writer.Write((long)value);
            else if (type == typeof(ushort))
                writer.Write((ushort)value);
            else if (type == typeof(uint))
                writer.Write((uint)value);
            else if (type == typeof(ulong))
                writer.Write((ulong)value);
            else if (type == typeof(bool))
                writer.Write((bool)value);
            else if (type == typeof(byte))
                writer.Write((byte)value);
            else if (type == typeof(sbyte))
                writer.Write((sbyte)value);
            else if (type == typeof(char))
                writer.Write((char)value);
            else if (type == typeof(decimal))
                writer.Write((decimal)value);
            else if (type == typeof(double))
                writer.Write((double)value);
            else if (type == typeof(float))
                writer.Write((float)value);
            else if (type == typeof(string))
                writer.Write((string)value);
            else if (type == typeof(char[]))
            {
                var data = (char[]) value;

                writer.Write(data.Length);
                writer.Write(data);
            }
            else if (type == typeof(byte[]))
            {
                var data = (byte[]) value;

                writer.Write(data.Length);
                writer.Write(data);
            }

            // Generic object
            else
            {
                using (var mem = new MemoryStream())
                {
                    _fmt.Serialize(mem, value);

                    writer.Write((int)mem.Length);
                    writer.Write(mem.ToArray());
                }
            }
        }
    }
}
