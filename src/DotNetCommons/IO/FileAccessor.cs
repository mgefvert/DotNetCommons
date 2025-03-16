using System.Text;

namespace DotNetCommons.IO;

public class FileAccessor : IFileAccessor
{
    /// <inheritdoc/>
    public string CurrentDirectory => Directory.GetCurrentDirectory();

    /// <inheritdoc/>
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    /// <inheritdoc/>
    public void ChangeDirectory(string path)
    {
        Environment.CurrentDirectory = Path.Combine(CurrentDirectory, path);
    }

    /// <inheritdoc/>
    public void CopyFile(string sourceName, string targetName, bool overwrite = false)
    {
        File.Copy(sourceName, targetName, overwrite);
    }

    /// <inheritdoc/>
    public void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path);
    }

    /// <inheritdoc/>
    public Stream CreateFile(string fileName, FileAccess access)
    {
        return new FileStream(fileName, FileMode.Create, access);
    }

    /// <inheritdoc/>
    public void DeleteDirectory(string path, bool recursive)
    {
        Directory.Delete(path, recursive);
    }

    /// <inheritdoc/>
    public void DeleteFile(string fileName)
    {
        File.Delete(fileName);
    }

    /// <inheritdoc/>
    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    /// <inheritdoc/>
    public bool FileExists(string fileName)
    {
        return File.Exists(fileName);
    }

    /// <inheritdoc/>
    public IEnumerable<string> GetFiles(string path, string searchPattern, bool recursive)
    {
        var files = Directory.EnumerateFiles(path, searchPattern, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        if (!Path.EndsInDirectorySeparator(path))
            path += Path.PathSeparator;

        foreach (var file in files)
        {
            var s = file;
            if (s.StartsWith(path))
                s = s.Mid(path.Length);
            yield return s;
        }
    }

    /// <inheritdoc/>
    public DateTime GetFileTime(string fileName)
    {
        return File.GetLastWriteTime(fileName);
    }

    /// <inheritdoc/>
    public long GetFileSize(string fileName)
    {
        return new FileInfo(fileName).Length;
    }

    /// <inheritdoc/>
    public void MoveFile(string sourceName, string targetName, bool overwrite)
    {
        File.Move(sourceName, targetName, overwrite);
    }
    
    /// <inheritdoc/>
    public Stream OpenFile(string fileName, bool canCreate, FileAccess access)
    {
        return new FileStream(fileName, canCreate ? FileMode.OpenOrCreate : FileMode.Open, access);
    }

    /// <inheritdoc/>
    public byte[] ReadAllBytes(string fileName)
    {
        return File.ReadAllBytes(fileName);
    }

    /// <inheritdoc/>
    public Task<byte[]> ReadAllBytesAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return File.ReadAllBytesAsync(filePath, cancellationToken);
    }

    /// <inheritdoc/>
    public string ReadAllText(string fileName)
    {
        return File.ReadAllText(fileName, Encoding);
    }

    /// <inheritdoc/>
    public Task<string> ReadAllTextAsync(string fileName, CancellationToken cancellationToken = default)
    {
        return File.ReadAllTextAsync(fileName, Encoding, cancellationToken);
    }

    /// <inheritdoc/>
    public void SetFileTime(string fileName, DateTime time)
    {
        File.SetLastWriteTime(fileName, time);
    }

    /// <inheritdoc/>
    public void Touch(string fileName)
    {
        if (File.Exists(fileName))
        {
            var now = DateTime.Now;
            File.SetLastWriteTime(fileName, now);
            File.SetLastAccessTime(fileName, now);
        }
        else
        {
            using (File.Create(fileName)) { }
        }
    }
    
    /// <inheritdoc/>
    public void WriteAllBytes(string fileName, byte[] content)
    {
        File.WriteAllBytes(fileName, content);
    }

    /// <inheritdoc/>
    public Task WriteAllBytesAsync(string fileName, byte[] content, CancellationToken cancellationToken = default)
    {
        return File.WriteAllBytesAsync(fileName, content, cancellationToken);
    }

    /// <inheritdoc/>
    public void WriteAllText(string fileName, string content)
    {
        File.WriteAllText(fileName, content, Encoding);
    }

    /// <inheritdoc/>
    public Task WriteAllTextAsync(string fileName, string content, CancellationToken cancellationToken = default)
    {
        return File.WriteAllTextAsync(fileName, content, Encoding, cancellationToken);
    }
}
