using System.Drawing.Imaging;
using System.Runtime.Serialization;
using System.Text;

// ReSharper disable MemberCanBePrivate.Global

namespace DotNetCommons.WinForms.Graphics;

public static class ExifTags
{
    public const int Orientation = 274;
    public const int XpTitle = 0x9C9B;
    public const int XpComment = 0x9C9C;
    public const int XpKeywords = 0x9C9E;
    public const int XpSubject = 0x9C9F;
    public const int Rating = 0x4746;
}

public class ExifImage
{
    public Image Image { get; }

    public string? Comments
    {
        get => ReadString(ExifTags.XpComment, Encoding.Unicode);
        set
        {
            if (value == null)
                Remove(ExifTags.XpComment);
            else
                Write(ExifTags.XpComment, value, Encoding.Unicode);
        }
    }

    public short? Rating
    {
        get => ReadInt16(ExifTags.Rating);
        set
        {
            if (value == null)
                Remove(ExifTags.Rating);
            else
                Write(ExifTags.Rating, value.Value);
        }
    }

    public string? Subject
    {
        get => ReadString(ExifTags.XpSubject, Encoding.Unicode);
        set
        {
            if (value == null)
                Remove(ExifTags.XpSubject);
            else
                Write(ExifTags.XpSubject, value, Encoding.Unicode);
        }
    }

    public string[] Tags
    {
        get => ReadStrings(ExifTags.XpKeywords, Encoding.Unicode);
        set
        {
            if (value.Length == 0)
                Remove(ExifTags.XpKeywords);
            else
                Write(ExifTags.XpKeywords, value, Encoding.Unicode);
        }
    }

    public string TagAsText => string.Join("; ", Tags);

    public string? Title
    {
        get => ReadString(ExifTags.XpTitle, Encoding.Unicode);
        set
        {
            if (value == null)
                Remove(ExifTags.XpTitle);
            else
                Write(ExifTags.XpTitle, value, Encoding.Unicode);
        }
    }

    public ExifImage(Stream stream)
    {
        Image = Image.FromStream(stream);
    }

    public void AdjustForOrientation()
    {
        var orientation = ReadUInt8(ExifTags.Orientation);
        switch (orientation)
        {
            case 2: Image.RotateFlip(RotateFlipType.RotateNoneFlipX); break;
            case 3: Image.RotateFlip(RotateFlipType.Rotate180FlipNone); break;
            case 4: Image.RotateFlip(RotateFlipType.Rotate180FlipX); break;
            case 5: Image.RotateFlip(RotateFlipType.Rotate90FlipX); break;
            case 6: Image.RotateFlip(RotateFlipType.Rotate90FlipNone); break;
            case 7: Image.RotateFlip(RotateFlipType.Rotate270FlipX); break;
            case 8: Image.RotateFlip(RotateFlipType.Rotate270FlipNone); break;
        }

        Image.RemovePropertyItem(ExifTags.Orientation);
    }

    public bool Exists(int id)
    {
        return Image.PropertyIdList.Contains(id);
    }

    public bool TryRead(int id, out byte[] value)
    {
        value = null!;
        if (!Image.PropertyIdList.Contains(id))
            return false;

        var item = Image.GetPropertyItem(id);
        if (item == null)
            return false;

        value = item.Value!;
        return true;
    }

    public sbyte? ReadInt8(int id) => TryRead(id, out var value) ? (sbyte?)value[0] : null;
    public short? ReadInt16(int id) => TryRead(id, out var value) ? BitConverter.ToInt16(value, 0) : null;
    public int? ReadInt32(int id) => TryRead(id, out var value) ? BitConverter.ToInt32(value, 0) : null;
    public long? ReadInt64(int id) => TryRead(id, out var value) ? BitConverter.ToInt64(value, 0) : null;
    public byte? ReadUInt8(int id) => TryRead(id, out var value) ? value[0] : null;
    public ushort? ReadUInt16(int id) => TryRead(id, out var value) ? BitConverter.ToUInt16(value, 0) : null;
    public uint? ReadUInt32(int id) => TryRead(id, out var value) ? BitConverter.ToUInt32(value, 0) : null;
    public ulong? ReadUInt64(int id) => TryRead(id, out var value) ? BitConverter.ToUInt64(value, 0) : null;
    public string? ReadString(int id, Encoding encoding) => TryRead(id, out var value) ? encoding.GetString(value).TrimEnd('\0') : null;

    public string[] ReadStrings(int id, Encoding encoding)
    {
        return (ReadString(id, encoding) ?? "")
            .Split(';')
            .Select(x => x.TrimEnd('\0'))
            .Where(x => !string.IsNullOrEmpty(x))
            .ToArray();
    }

    public void Save(Stream target)
    {
        Image.Save(target, ImageFormat.Jpeg);
    }

    public void Remove(int id)
    {
        Image.RemovePropertyItem(id);
    }

    public void Write(int id, byte[] data)
    {
        // Because PropertyItem doesn't have a public constructor, we need to use FormatterServices.GetUninitializedObject
        #pragma warning disable SYSLIB0050

        var prop = Image.PropertyIdList.Contains(id)
            ? Image.GetPropertyItem(id)
            : (PropertyItem)FormatterServices.GetUninitializedObject(typeof(PropertyItem));
        
        #pragma warning restore SYSLIB0050

        prop!.Id = id;
        prop.Type = 1;
        prop.Value = data;
        prop.Len = data.Length;

        Image.SetPropertyItem(prop);
    }

    public void Write(int id, sbyte value) => Write(id, [(byte)value]);
    public void Write(int id, byte value) => Write(id, [value]);
    public void Write(int id, ushort value) => Write(id, BitConverter.GetBytes(value));
    public void Write(int id, short value) => Write(id, BitConverter.GetBytes(value));
    public void Write(int id, int value) => Write(id, BitConverter.GetBytes(value));
    public void Write(int id, uint value) => Write(id, BitConverter.GetBytes(value));
    public void Write(int id, ulong value) => Write(id, BitConverter.GetBytes(value));
    public void Write(int id, long value) => Write(id, BitConverter.GetBytes(value));
    public void Write(int id, string value, Encoding encoding) => Write(id, encoding.GetBytes(value + "\0"));
    public void Write(int id, IEnumerable<string> values, Encoding encoding) => Write(id, string.Join(";", values), encoding);
}