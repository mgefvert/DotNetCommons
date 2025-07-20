using System.Text;
using DotNetCommons.Security;

namespace DotNetCommons.IO;

public class FileSystemAccessor : IFileAccessor
{
    private Exception DirectoryNotFound(string path) => new DirectoryNotFoundException($"Directory not found: {path}");
    
    /// <inheritdoc/>
    public string CurrentDirectory => Directory.GetCurrentDirectory();

    /// <inheritdoc/>
    public IFileItem CurrentItem => new FileSystemItem(new DirectoryInfo(Directory.GetCurrentDirectory()), this);

    /// <inheritdoc/>
    public char DirectorySeparator => Path.DirectorySeparatorChar;

    /// <inheritdoc/>
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    /// <inheritdoc/>
    public void ChangeDirectory(string path)
    {
        Environment.CurrentDirectory = Path.Combine(CurrentDirectory, path);
    }

    /// <inheritdoc/>
    public IFileItem CopyFile(string sourceName, string targetName, bool overwrite = false)
    {
        File.Copy(sourceName, targetName, overwrite);
        return new FileSystemItem(new FileInfo(targetName), this);
    }

    /// <inheritdoc/>
    public bool DeleteDirectory(string path, bool recursive)
    {
        if (!Directory.Exists(path))
            return false;
        
        Directory.Delete(path, recursive);
        return true;
    }

    /// <inheritdoc/>
    public bool DeleteFile(string fileName)
    {
        if (!File.Exists(fileName))
            return false;
        
        File.Delete(fileName);
        return true;
    }

    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    public bool FileExists(string fileName)
    {
        return File.Exists(fileName);
    }

    /// <inheritdoc/>
    public IFileItem? GetDirectory(string path, bool canCreate)
    {
        var dir = new DirectoryInfo(path);
        if (!dir.Exists && canCreate)
            dir.Create();
        
        return dir.Exists ? new FileSystemItem(dir, this) : null;
    }

    /// <inheritdoc/>
    public IFileItem? GetFile(string fileName, bool canCreate)
    {
        var file = new FileInfo(fileName);
        if (file.Exists)
            return new FileSystemItem(file, this);

        if (!canCreate)
            return null;
        
        using (file.Create()) {}
        return new FileSystemItem(file, this);
    }

    /// <inheritdoc/>
    public IEnumerable<IFileItem> GetFiles(string path, string searchPattern, bool recursive)
    {
        var dir = new DirectoryInfo(path);
        if (!dir.Exists)
            throw new DirectoryNotFoundException();
        
        var files = dir.EnumerateFiles(searchPattern, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        foreach (var file in files)
            yield return new FileSystemItem(file, this);
    }

    /// <inheritdoc/>
    public DateTime GetFileTime(string fileName)
    {
        return File.GetLastWriteTime(fileName);
    }

    /// <inheritdoc/>
    public IFileItem GetTempDirectory(string baseName)
    {
        baseName = WhiteWash.FileName(baseName);
        var fullName = Path.Combine(Path.GetTempPath(), $"{baseName}_{Guid.NewGuid():N}");

        return GetDirectory(fullName, true) ?? throw new IOException($"Cannot create temporary directory: {fullName}");
    }

    /// <inheritdoc/>
    public IEnumerable<IFileItem> ListFiles(string? directory = null)
    {
        var dir = new DirectoryInfo(directory ?? Environment.CurrentDirectory);
        if (!dir.Exists)
            throw DirectoryNotFound(directory ?? Environment.CurrentDirectory);

        foreach (var item in dir.EnumerateDirectories("*", SearchOption.TopDirectoryOnly).OrderBy(x => x.Name))
            yield return new FileSystemItem(item, this);

        foreach (var item in dir.EnumerateFiles("*", SearchOption.TopDirectoryOnly).OrderBy(x => x.Name))
            yield return new FileSystemItem(item, this);
    }

    /// <inheritdoc/>
    public long GetFileSize(string fileName)
    {
        return new FileInfo(fileName).Length;
    }

    /// <inheritdoc/>
    public IFileItem MoveFile(string sourceName, string targetName, bool overwrite)
    {
        File.Move(sourceName, targetName, overwrite);
        return new FileSystemItem(new FileInfo(targetName), this);
    }

    /// <inheritdoc/>
    public Stream OpenFile(string fileName, FileMode mode, FileAccess access)
    {
        return new FileStream(fileName, mode, access);
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
    public IFileItem Touch(string fileName)
    {
        var file = new FileInfo(fileName);
        
        if (file.Exists)
        {
            var now = DateTime.Now;
            File.SetLastWriteTime(fileName, now);
            File.SetLastAccessTime(fileName, now);
        }
        else
        {
            using (File.Create(fileName)) { }
            file.Refresh();
        }

        return new FileSystemItem(file, this);
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
