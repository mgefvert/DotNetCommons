namespace DotNetCommons.IO;

public interface IFileItem
{
    public string Name { get; }
    public string FullName { get; }
    public bool Directory { get; }
    public long Size { get; }
    public DateTime LastWriteTime { get; }

    /// <summary>
    /// Get a reference to the parent object - null if there is none.
    /// </summary>
    IFileItem? Parent { get; }

    /// <summary>
    /// List files in current directory.
    /// </summary>
    IEnumerable<IFileItem> ListFiles();
    
    /// <summary>
    /// Open the file with a certain access.
    /// </summary>
    Stream Open(FileAccess access);
    
    /// <summary>
    /// Read all bytes from a given file.
    /// </summary>
    byte[] ReadAllBytes();

    /// <summary>
    /// Read all bytes asynchronously from a given file.
    /// </summary>
    Task<byte[]> ReadAllBytesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Read all text from a given file.
    /// </summary>
    string ReadAllText();

    /// <summary>
    /// Read all text asynchronously from a given file.
    /// </summary>
    Task<string> ReadAllTextAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Write contents to a given file, replacing the file if it already exists.
    /// </summary>
    void WriteAllBytes(byte[] content);

    /// <summary>
    /// Write contents asynchronously to a given file, replacing the file if it already exists.
    /// </summary>
    Task WriteAllBytesAsync(byte[] content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write text contents to a given file, replacing the file if it already exists.
    /// </summary>
    void WriteAllText(string content);

    /// <summary>
    /// Write text contents asynchronously to a given file, replacing the file if it already exists.
    /// </summary>
    Task WriteAllTextAsync(string content, CancellationToken cancellationToken = default);
}