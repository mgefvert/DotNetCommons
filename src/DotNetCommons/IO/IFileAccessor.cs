using System.Text;

namespace DotNetCommons.IO;

public interface IFileAccessor
{
    /// <summary>
    /// The current directory in the file system.
    /// </summary>
    string CurrentDirectory { get; }
    
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
    void CopyFile(string sourceName, string targetName, bool overwrite);
    
    /// <summary>
    /// Create a directory; can create several subdirectories in a single call if the whole parent chain needs to be created.
    /// </summary>
    /// <param name="path"></param>
    void CreateDirectory(string path);
    
    /// <summary>
    /// Create a file for opening.
    /// </summary>
    Stream CreateFile(string fileName, FileAccess access);
    
    /// <summary>
    /// Delete a directory. If recursive, will also delete all the contents including any subdirectories.
    /// </summary>
    void DeleteDirectory(string path, bool recursive);
    
    /// <summary>
    /// Delete a file. If the file doesn't exist, the call will ignore the operation and return.
    /// </summary>
    void DeleteFile(string fileName);
    
    /// <summary>
    /// Check to see whether a given directory exists or not, relative or absolute paths.
    /// </summary>
    bool DirectoryExists(string path);
    
    /// <summary>
    /// Check to see whether a given file exists or not, relative or absolute paths.
    /// </summary>
    bool FileExists(string fileName);
    
    /// <summary>
    /// List files in a given directory, according to a search pattern (i.e. "*.txt" or similar) and optionally searches recursively.
    /// Does not enumerate directories.
    /// </summary>
    IEnumerable<string> GetFiles(string path, string searchPattern, bool recursive);
    
    /// <summary>
    /// Get the file size of a given file.
    /// </summary>
    long GetFileSize(string fileName);
    
    /// <summary>
    /// Get the last written time of a given file.
    /// </summary>
    DateTime GetFileTime(string fileName);
    
    /// <summary>
    /// Move a file across the file system. If overwrite is selected, it will overwrite the target file if it already exists; if overwrite
    /// is deselected, it will throw an exception. Both sourceName and targetName are relative to the current directory, if no absolute
    /// paths are given.
    /// </summary>
    void MoveFile(string sourceName, string targetName, bool overwrite);
    
    /// <summary>
    /// Open a file for reading or writing, optionally creating a new file if it doesn't exist.
    /// </summary>
    Stream OpenFile(string fileName, bool canCreate, FileAccess access);
    
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
    /// <param name="fileName"></param>
    void Touch(string fileName);
    
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
