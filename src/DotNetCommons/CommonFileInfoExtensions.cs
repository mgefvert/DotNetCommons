using System.Text;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons;

public static class CommonFileInfoExtensions
{
    /// <summary>
    /// Create a new file on disk from a DirectoryInfo with a given filename and contents.
    /// </summary>
    public static FileInfo CreateFile(this DirectoryInfo directory, string fileName, ReadOnlySpan<byte> contents)
    {
        var newFileName = Path.Combine(directory.FullName, fileName);
        using (var file = new FileStream(newFileName, FileMode.Create, FileAccess.Write))
            file.Write(contents);

        return new FileInfo(newFileName);
    }

    /// <summary>
    /// Create a new file on disk from a DirectoryInfo with a given filename and string content.
    /// </summary>
    public static FileInfo CreateFile(this DirectoryInfo directory, string fileName, string contents, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        return CreateFile(directory, fileName, encoding.GetBytes(contents));
    }

    /// <summary>
    /// Create a new DirectoryInfo for a given subdirectory in the directory.
    /// </summary>
    public static DirectoryInfo ForDirectory(this DirectoryInfo directory, string directoryName)
    {
        return new DirectoryInfo(Path.Combine(directory.FullName, directoryName));
    }

    /// <summary>
    /// Create a new FileInfo for a given file in the directory.
    /// </summary>
    public static FileInfo ForFile(this DirectoryInfo directory, string fileName)
    {
        return new FileInfo(Path.Combine(directory.FullName, fileName));
    }

    /// <summary>
    /// Read the contents of file.
    /// </summary>
    public static byte[] ReadBinary(this FileInfo file)
    {
        using var stream = file.Open(FileMode.Open, FileAccess.Read);
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }

    /// <summary>
    /// Read the contents of file.
    /// </summary>
    public static string ReadText(this FileInfo file)
    {
        using var reader = file.OpenText();
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Create a new file, or update the last written timestamp of it.
    /// </summary>
    /// <param name="file"></param>
    public static void Touch(this FileInfo file)
    {
        if (file.Exists)
            using (file.AppendText()) {}
        else
            using (file.Create()) {}
    }

    /// <summary>
    /// Write the contents of file.
    /// </summary>
    public static void WriteBinary(this FileInfo file, ReadOnlySpan<byte> contents)
    {
        using (var stream = new FileStream(file.FullName, FileMode.Create, FileAccess.Write))
            stream.Write(contents);
        file.Refresh();
    }

    /// <summary>
    /// Write the contents of file.
    /// </summary>
    public static void WriteText(this FileInfo file, string contents, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        WriteBinary(file, encoding.GetBytes(contents));
    }
}