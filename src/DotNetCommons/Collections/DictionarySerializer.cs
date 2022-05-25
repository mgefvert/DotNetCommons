using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Text;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Collections;

/// <summary>
/// Class that serializes a dictionary to a binary stream in a custom format. Handles only trivial
/// data types.
/// </summary>
public class DictionarySerializer
{
    /// <summary>
    /// Selects the encoding to use for string values.
    /// </summary>
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    /// <summary>
    /// Load a dictionary from a file.
    /// </summary>
    public Dictionary<TKey, TValue> Load<TKey, TValue>(string filename) where TKey : notnull
    {
        if (!File.Exists(filename))
            return new Dictionary<TKey, TValue>();

        using var fs = new FileStream(filename, FileMode.Open);
        return Load<TKey, TValue>(fs);
    }

    /// <summary>
    /// Load a dictionary from a stream.
    /// </summary>
    public Dictionary<TKey, TValue> Load<TKey, TValue>(Stream stream) where TKey : notnull
    {
        using var zip = new DeflateStream(stream, CompressionMode.Decompress, true);
        using var reader = new BinaryReader(zip, Encoding, true);
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

        throw new SerializationException($"{typeof(T).Name} is a complex object and cannot be serialized");
    }

    /// <summary>
    /// Save a dictionary to a file name.
    /// </summary>
    public void Save<TKey, TValue>(Dictionary<TKey, TValue> dictionary, string filename) where TKey : notnull
    {
        using var fs = new FileStream(filename, FileMode.Create);
        Save(dictionary, fs);
    }

    /// <summary>
    /// Save a dictionary to a stream.
    /// </summary>
    public void Save<TKey, TValue>(Dictionary<TKey, TValue> dictionary, Stream stream) where TKey : notnull
    {
        using var zip = new DeflateStream(stream, CompressionMode.Compress, true);
        using var writer = new BinaryWriter(zip, Encoding, true);
        writer.Write(dictionary.Count);

        foreach (var item in dictionary)
            if (item.Value != null)
            {
                SaveValue(writer, item.Key);
                SaveValue(writer, item.Value);
            }
    }

    /// <summary>
    /// Write a single value to a BinaryWriter.
    /// </summary>
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
            var data = (char[])value;

            writer.Write(data.Length);
            writer.Write(data);
        }
        else if (type == typeof(byte[]))
        {
            var data = (byte[])value;

            writer.Write(data.Length);
            writer.Write(data);
        }
        else
            throw new SerializationException($"{type.Name} is a complex object and cannot be serialized");
    }
}