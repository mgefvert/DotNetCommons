using System.Text;

namespace DotNetCommons.IO;

public interface IFileAccessor
{
    /// <summary>
    /// The current directory in the file system.
    /// </summary>
    string CurrentDirectory { get; }
    
    /// <summary>
    /// The current directory as a FileItem.
    /// </summary>
    IFileItem CurrentItem { get; }
    
    /// <summary>
    /// The character used to separate paths.
    /// </summary>
    char DirectorySeparator { get; }

    /// <summary>
    /// Encoding to use for string-based text-to-byte conversions; UTF8 by default.
    /// </summary>
    Encoding Encoding { get; set; }

    /// <summary>
    /// Change directory using an absolute or relative path.
    /// </summary>
    /// <param name="path"></param>
    void ChangeDirectory(string path);
    
    /// <summary>
    /// Copy a file across the file system. If overwrite is selected, it will overwrite the target file if it already exists; if overwrite
    /// is deselected, it will throw an exception. Both sourceName and targetName are relative to the current directory, if no absolute
    /// paths are given.
    /// </summary>
    IFileItem CopyFile(string sourceName, string targetName, bool overwrite);
    
    /// <summary>
    /// Delete a directory. If recursive, will also delete all the contents including any subdirectories.
    /// </summary>
    bool DeleteDirectory(string path, bool recursive);
    
    /// <summary>
    /// Delete a file. If the file doesn't exist, the call will ignore the operation and return.
    /// </summary>
    bool DeleteFile(string fileName);

    /// <summary>
    /// Shortcut to GetDirectoryIfExists != null
    /// </summary>
    bool DirectoryExists(string path);
    
    /// <summary>
    /// Shortcut to GetFileIfExists != null
    /// </summary>
    bool FileExists(string fileName);
    
    /// <summary>
    /// Check to see whether a given directory exists or not, relative or absolute paths.
    /// </summary>
    IFileItem? GetDirectory(string path, bool canCreate);
    
    /// <summary>
    /// Open a file for reading or writing, optionally creating a new file if it doesn't exist.
    /// </summary>
    IFileItem? GetFile(string fileName, bool canCreate);
    
    /// <summary>
    /// List files in a given directory, according to a search pattern (i.e. "*.txt" or similar) and optionally searches recursively.
    /// Does not enumerate directories.
    /// </summary>
    IEnumerable<IFileItem> GetFiles(string path, string searchPattern, bool recursive);
    
    /// <summary>
    /// Get the file size of a given file.
    /// </summary>
    long GetFileSize(string fileName);
    
    /// <summary>
    /// Get the last written time of a given file.
    /// </summary>
    DateTime GetFileTime(string fileName);

    /// <summary>
    /// List files and directories in the current directory, or optionally another directory.
    /// </summary>
    IEnumerable<IFileItem> ListFiles(string? directory = null);
    
    /// <summary>
    /// Move a file across the file system. If overwrite is selected, it will overwrite the target file if it already exists; if overwrite
    /// is deselected, it will throw an exception. Both sourceName and targetName are relative to the current directory, if no absolute
    /// paths are given.
    /// </summary>
    IFileItem MoveFile(string sourceName, string targetName, bool overwrite);

    /// <summary>
    /// Create or open a file for reading/writing. 
    /// </summary>
    Stream OpenFile(string fileName, FileMode mode, FileAccess access);
    
    /// <summary>
    /// Read all bytes from a given file.
    /// </summary>
    byte[] ReadAllBytes(string fileName);

    /// <summary>
    /// Read all bytes asynchronously from a given file.
    /// </summary>
    Task<byte[]> ReadAllBytesAsync(string fileName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Read all text from a given file, using <see cref="Encoding"/> as the byte converter.
    /// </summary>
    string ReadAllText(string fileName);

    /// <summary>
    /// Read all text asynchronously from a given file, using <see cref="Encoding"/> as the byte converter.
    /// </summary>
    Task<string> ReadAllTextAsync(string fileName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Set the last write file time for a given file.
    /// </summary>
    void SetFileTime(string fileName, DateTime time);
    
    /// <summary>
    /// Touch a given file, updating the last write time to the current time. If the file does not exist, create it.
    /// </summary>
    IFileItem Touch(string fileName);
    
    /// <summary>
    /// Write contents to a given file, replacing the file if it already exists.
    /// </summary>
    void WriteAllBytes(string fileName, byte[] content);

    /// <summary>
    /// Write contents asynchronously to a given file, replacing the file if it already exists.
    /// </summary>
    Task WriteAllBytesAsync(string fileName, byte[] content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write text contents to a given file, replacing the file if it already exists and using <see cref="Encoding"/> as the text to
    /// byte converter.
    /// </summary>
    void WriteAllText(string fileName, string content);

    /// <summary>
    /// Write text contents asynchronously to a given file, replacing the file if it already exists and using <see cref="Encoding"/> as
    /// the text to byte converter.
    /// </summary>
    Task WriteAllTextAsync(string fileName, string content, CancellationToken cancellationToken = default);
}
