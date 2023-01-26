using System;
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

        var keyReader = GetReader<TKey>();
        var valueReader = GetReader<TValue>();

        var count = reader.ReadInt32();
        while (count-- > 0)
        {
            var key = (TKey)keyReader(reader);
            var value = (TValue)valueReader(reader);

            result[key] = value;
        }

        return result;
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

        var keyWriter = GetWriter<TKey>();
        var valueWriter = GetWriter<TValue>();

        foreach (var item in dictionary)
            if (item.Value != null)
            {
                keyWriter(writer, item.Key);
                valueWriter(writer, item.Value);
            }
    }

    private static Func<BinaryReader, object> GetReader<T>()
    {
        return typeof(T).Name switch
        {
            nameof(Int16) => r => r.ReadInt16(),
            nameof(Int32) => r => r.ReadInt32(),
            nameof(Int64) => r => r.ReadInt64(),
            nameof(UInt16) => r => r.ReadInt16(),
            nameof(UInt32) => r => r.ReadInt32(),
            nameof(UInt64) => r => r.ReadInt64(),
            nameof(Boolean) => r => r.ReadBoolean(),
            nameof(Byte) => r => r.ReadByte(),
            nameof(SByte) => r => r.ReadSByte(),
            nameof(Char) => r => r.ReadChar(),
            nameof(Decimal) => r => r.ReadDecimal(),
            nameof(Single) => r => r.ReadSingle(),
            nameof(Double) => r => r.ReadDouble(),
            nameof(String) => r => r.ReadString(),
            "Char[]" => r =>
            {
                var count = r.ReadInt32();
                return r.ReadChars(count);
            },
            "Byte[]" => r =>
            {
                var count = r.ReadInt32();
                return r.ReadBytes(count);
            },
            _ => throw new SerializationException($"{typeof(T).Name} is a complex object and cannot be serialized")
        };
    }

    private static Action<BinaryWriter, object> GetWriter<T>()
    {
        return typeof(T).Name switch
        {
            nameof(Int16) => (w, v) => w.Write((short)v),
            nameof(Int32) => (w, v) => w.Write((int)v),
            nameof(Int64) => (w, v) => w.Write((long)v),
            nameof(UInt16) => (w, v) => w.Write((short)v),
            nameof(UInt32) => (w, v) => w.Write((int)v),
            nameof(UInt64) => (w, v) => w.Write((long)v),
            nameof(Boolean) => (w, v) => w.Write((bool)v),
            nameof(Byte) => (w, v) => w.Write((byte)v),
            nameof(SByte) => (w, v) => w.Write((sbyte)v),
            nameof(Char) => (w, v) => w.Write((char)v),
            nameof(Decimal) => (w, v) => w.Write((decimal)v),
            nameof(Single) => (w, v) => w.Write((float)v),
            nameof(Double) => (w, v) => w.Write((double)v),
            nameof(String) => (w, v) => w.Write((string)v),
            "Char[]" => (w, v) =>
            {
                var data = (char[])v;
                w.Write(data.Length);
                w.Write(data);
            },
            "Byte[]" => (w, v) =>
            {
                var data = (byte[])v;
                w.Write(data.Length);
                w.Write(data);
            },
            _ => throw new SerializationException($"{typeof(T).Name} is a complex object and cannot be serialized")
        };
    }
}