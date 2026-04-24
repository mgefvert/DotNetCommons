using System.Drawing.Imaging;
using System.Runtime.Serialization;
using System.Text;

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

public class ExifImage : IDisposable
{
    private MemoryStream _data;
    private Stream _source;
    private Image _image;

    public string Comments
    {
        get => ReadString(ExifTags.XpComment, Encoding.Unicode);
        set => Write(ExifTags.XpComment, value, Encoding.Unicode);
    }

    public short? Rating
    {
        get => ReadInt16(ExifTags.Rating);
        set => Write(ExifTags.Rating, value);
    }

    public string Subject
    {
        get => ReadString(ExifTags.XpSubject, Encoding.Unicode);
        set => Write(ExifTags.XpSubject, value, Encoding.Unicode);
    }

    public string[] Tags
    {
        get => ReadStrings(ExifTags.XpKeywords, Encoding.Unicode);
        set => Write(ExifTags.XpKeywords, value, Encoding.Unicode);
    }

    public string TagAsText => string.Join("; ", Tags);

    public string Title
    {
        get => ReadString(ExifTags.XpTitle, Encoding.Unicode);
        set => Write(ExifTags.XpTitle, value, Encoding.Unicode);
    }

    public ExifImage(string filename) : this(new FileStream(filename, FileMode.Open, FileAccess.ReadWrite))
    {
    }

    public ExifImage(Stream stream)
    {
        _source = stream;
        LoadExifData();
    }

    ~ExifImage()
    {
        Dispose();
    }

    public void Dispose()
    {
        _source?.Dispose();
        _source = null;
    }

    private void LoadExifData()
    {
        _data = new MemoryStream();
        _source.CopyTo(_data);

        _data.Position = 0;
        _image = Image.FromStream(_data);
    }

    public Image GetImage(bool adjustForOrientation)
    {
        _data.Position = 0;
        var img = Image.FromStream(_data);

        if (adjustForOrientation && img.PropertyIdList.Contains(ExifTags.Orientation))
        {
            var orientation = (int)img.GetPropertyItem(ExifTags.Orientation).Value[0];
            switch (orientation)
            {
                case 2: img.RotateFlip(RotateFlipType.RotateNoneFlipX); break;
                case 3: img.RotateFlip(RotateFlipType.Rotate180FlipNone); break;
                case 4: img.RotateFlip(RotateFlipType.Rotate180FlipX); break;
                case 5: img.RotateFlip(RotateFlipType.Rotate90FlipX); break;
                case 6: img.RotateFlip(RotateFlipType.Rotate90FlipNone); break;
                case 7: img.RotateFlip(RotateFlipType.Rotate270FlipX); break;
                case 8: img.RotateFlip(RotateFlipType.Rotate270FlipNone); break;
            }

            img.RemovePropertyItem(ExifTags.Orientation);
        }

        return img;
    }

    public bool Exists(int id)
    {
        return _image.PropertyIdList.Contains(id);
    }

    public byte[] Read(int id)
    {
        return _image.GetPropertyItem(id).Value;
    }

    public sbyte? ReadInt8(int id)
    {
        return Exists(id) ? (sbyte)Read(id)[0] : null;
    }

    public short? ReadInt16(int id)
    {
        return Exists(id) ? BitConverter.ToInt16(Read(id), 0) : null;
    }

    public int? ReadInt32(int id)
    {
        return Exists(id) ? BitConverter.ToInt32(Read(id), 0) : null;
    }

    public long? ReadInt64(int id)
    {
        return Exists(id) ? BitConverter.ToInt64(Read(id), 0) : null;
    }

    public byte? ReadUInt8(int id)
    {
        return Exists(id) ? Read(id)[0] : null;
    }

    public ushort? ReadUInt16(int id)
    {
        return Exists(id) ? BitConverter.ToUInt16(Read(id), 0) : null;
    }

    public uint? ReadUInt32(int id)
    {
        return Exists(id) ? BitConverter.ToUInt32(Read(id), 0) : null;
    }

    public ulong? ReadUInt64(int id)
    {
        return Exists(id) ? BitConverter.ToUInt64(Read(id), 0) : null;
    }

    public string ReadString(int id, Encoding encoding)
    {
        return Exists(id)
            ? encoding.GetString(_image.GetPropertyItem(id).Value).TrimEnd('\0')
            : null;
    }

    public string[] ReadStrings(int id, Encoding encoding)
    {
        return (ReadString(id, encoding) ?? "")
            .Split(';')
            .Select(x => x.TrimEnd('\0'))
            .Where(x => !string.IsNullOrEmpty(x))
            .ToArray();
    }

    public void Save()
    {
        // Verify file sizes - must be at least 80% of original
        if (_data.Length < _source.Length * 0.8)
            throw new Exception($"File save resulted in an unexpectedly small file ({_data.Length} bytes compared to original {_source.Length} bytes)");

        _source.Position = 0;
        _source.SetLength(0);
        Save(_source);
    }

    public void Save(string filename)
    {
        using var fs = new FileStream(filename, FileMode.Create);
        Save(fs);
    }

    public void Save(Stream target)
    {
        _image.Save(target, ImageFormat.Jpeg);
    }

    public void Write(int id, byte[] data)
    {
        var exists = _image.PropertyIdList.Contains(id);

        if (data == null)
        {
            if (exists)
                _image.RemovePropertyItem(id);
            return;
        }

        // Disable warning for use of FormatterServices... yes, I know what I'm doing
        #pragma warning disable SYSLIB0050
        
        var prop = exists
            ? _image.GetPropertyItem(id)
            : (PropertyItem)FormatterServices.GetUninitializedObject(typeof(PropertyItem));
        
        #pragma warning restore SYSLIB0050

        prop!.Id = id;
        prop.Type = 1;
        prop.Value = data;
        prop.Len = data.Length;

        _image.SetPropertyItem(prop);
    }

    public void Write(int id, sbyte? value)
    {
        Write(id, value != null ? new[] { (byte)value.Value } : null);
    }

    public void Write(int id, byte? value)
    {
        Write(id, value != null ? new[] { value.Value } : null);
    }

    public void Write(int id, ushort? value)
    {
        Write(id, value != null ? BitConverter.GetBytes(value.Value) : null);
    }

    public void Write(int id, short? value)
    {
        Write(id, value != null ? BitConverter.GetBytes(value.Value) : null);
    }

    public void Write(int id, int? value)
    {
        Write(id, value != null ? BitConverter.GetBytes(value.Value) : null);
    }

    public void Write(int id, uint? value)
    {
        Write(id, value != null ? BitConverter.GetBytes(value.Value) : null);
    }

    public void Write(int id, ulong? value)
    {
        Write(id, value != null ? BitConverter.GetBytes(value.Value) : null);
    }

    public void Write(int id, long? value)
    {
        Write(id, value != null ? BitConverter.GetBytes(value.Value) : null);
    }

    public void Write(int id, string value, Encoding encoding)
    {
        Write(id, value == null ? null : encoding.GetBytes(value + "\0"));
    }

    public void Write(int id, IEnumerable<string> values, Encoding encoding)
    {
        Write(id, string.Join(";", values), encoding);
    }
}