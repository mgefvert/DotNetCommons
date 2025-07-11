﻿using System.Text;
using System.Text.RegularExpressions;
using DotNetCommons.Temporal;
using DotNetCommons.Text;

namespace DotNetCommons.IO;

/// <summary>
/// Implements IFileAccessor in an internal, memory-driven, Unix-like file system. All files are stored in a root ("/")
/// with subdirectories and files. Implemented using MemoryBlock as the individual file storage.
/// </summary>
public class InMemoryFileAccessor : IFileAccessor
{
    /// <summary>
    /// Wrapper around a MemoryBlock; disposing it ensures that we update the file time, but we don't clear the contents.
    /// </summary>
    internal class NonDisposableStreamWrapper : Stream
    {
        private readonly MemoryBlock _data;
        private readonly File _entry;
        private readonly FileAccess _access;
        private readonly IClock _clock;
        private bool _disposed;

        public NonDisposableStreamWrapper(File entry, FileAccess access, IClock clock)
        {
            _access = access;
            _clock  = clock;
            _entry  = entry;
            _data   = entry.Data;
        }

        protected override void Dispose(bool disposing)
        {
            Close();
        }

        public override void Close()
        {
            if (_access.HasFlag(FileAccess.Write))
                _entry.Time = _clock.Now;
            
            _disposed = true;
        }

        public override bool CanRead => _access.HasFlag(FileAccess.Read);
        public override bool CanSeek => true;
        public override bool CanWrite => _access.HasFlag(FileAccess.Write);
        public override long Length => (long)_data.Length;
        public override long Position { get; set; }

        private void CheckDisposed()
        {
            if (_disposed)
                throw new IOException("Cannot access a disposed stream.");
        }

        public override void Flush()
        {
            CheckDisposed();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            CheckDisposed();
            var len = _data.Read(buffer, (ulong)Position, count);
            Position += len;
            return len;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            CheckDisposed();
            Position = origin switch
            {
                SeekOrigin.Begin   => offset,
                SeekOrigin.Current => Position + offset,
                SeekOrigin.End     => (long)_data.Length + offset,
                _                  => throw new ArgumentOutOfRangeException(nameof(origin), origin, null)
            };
            
            return Position;
        }

        public override void SetLength(long value)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            
            CheckDisposed();
            _data.Length = (ulong)value;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            CheckDisposed();

            byte[] area;
            if (offset == 0)
                area = buffer;
            else
            {
                area = new byte[count];
                Array.Copy(buffer, offset, area, 0, count);
            }

            _data.Write(area, (ulong)Position, count);
            Position += count;
        }
    }

    /// <summary>
    /// Base class for either a directory or a file.
    /// </summary>
    internal abstract class Entry(Directory? parent, string? name, DateTime time)
    {
        public string? Name { get; set; } = name;
        public Directory? Parent { get; set; } = parent;
        public DateTime Time { get; set; } = time;
       
        public List<Entry> Elements()
        {
            var result = new List<Entry>();
            var entry  = this;
            while (entry != null)
            {
                result.Insert(0, entry);
                entry = entry.Parent;
            }
            
            return result;
        }

        public List<string> Names() => Elements().Select(x => x.Name ?? "").ToList();

        public string FullName() => Parent == null ? "/" : string.Join("/", Names());

        public abstract void Remove();
    }

    /// <summary>
    /// Represents a file in the virtual file system.
    /// </summary>
    internal class File : Entry
    {
        internal MemoryBlock Data { get; set; } = new();

        public File(Directory parent, string name, DateTime time, string? data, Encoding? encoding = null)
            : base(parent, name, time)
        {
            if (!data.IsSet()) 
                return;
            
            var buffer = (encoding ?? Encoding.UTF8).GetBytes(data);
            Data.Write(buffer, 0, buffer.Length);
        }

        public override void Remove()
        {
            Parent?.Files.Remove(this);
        }
    }

    /// <summary>
    /// Represents a directory in the virtual file system.
    /// </summary>
    internal class Directory : Entry
    {
        public List<Directory> Directories { get; } = [];
        public List<File> Files { get; } = [];

        public Directory(Directory? parent, string? name, DateTime time) : base(parent, name, time)
        {
        }

        public override void Remove()
        {
            Parent?.Directories.Remove(this);
        }
    }

    internal readonly IClock Clock;
    private readonly Directory _root;
    private Directory _currentDirectory;

    /// <inheritdoc/>
    public string CurrentDirectory => _currentDirectory.FullName();

    /// <inheritdoc/>
    public IFileItem CurrentItem => new InMemoryItem(_currentDirectory, this);

    /// <inheritdoc/>
    public char DirectorySeparator => '/';

    /// <inheritdoc/>
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    public InMemoryFileAccessor(IClock clock)
    {
        Clock             = clock;
        _root             = new Directory(null, null, Clock.Now);
        _currentDirectory = _root;
    }

    private Exception DirectoryNotFound(string path) => new DirectoryNotFoundException($"Directory not found: {GetAbsolutePath(path)}");
    private Exception FileNotFound(string fileName) => new FileNotFoundException($"File not found: {GetAbsolutePath(fileName)}");

    private string GetAbsolutePath(string path)
    {
        path = path.Replace("\\", "/");
        while (path.Contains("//"))
            path = path.Replace("//", "/");

        var isRooted = path.StartsWith("/");
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var current  = isRooted 
            ? [] 
            : _currentDirectory.Elements().Select(x => x.Name).NotNulls().ToList();

        foreach (var segment in segments)
        {
            switch (segment)
            {
                case ".":
                    continue;
                case ".." when current.Count == 0:
                    throw new IOException($"Invalid path: {path}");
                case "..":
                    current.RemoveAt(current.Count - 1);
                    break;
                default:
                    current.Add(segment);
                    break;
            }
        }

        return "/" + string.Join("/", current);
    }

    private Directory? FindDirectory(string path, bool create)
    {
        var segments = GetAbsolutePath(path).Split('/', StringSplitOptions.RemoveEmptyEntries);
        var dir      = _root;

        foreach (var segment in segments)
        {
            var subdir = dir.Directories.FirstOrDefault(x => x.Name == segment);
            if (subdir == null)
            {
                if (create)
                {
                    subdir = new Directory(dir, segment, Clock.Now);
                    dir.Directories.Add(subdir);
                }
                else
                    return null;
            }

            dir = subdir;
        }

        return dir;
    }

    private File? FindFile(string path)
    {
        var segments = GetAbsolutePath(path).Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
        if (segments.Count == 0)
            return null;

        var fileName = segments.ExtractLast();
        var dir  = _root;

        foreach (var segment in segments)
        {
            dir = dir.Directories.FirstOrDefault(x => x.Name == segment);
            if (dir == null)
                return null;
        }

        return dir.Files.FirstOrDefault(x => x.Name == fileName);
    }

    private IEnumerable<IFileItem> InternalEnumerateFiles(Directory directory, Regex regex, bool recursive)
    {
        foreach (var file in directory.Files.Where(x => regex.IsMatch(x.Name!)))
            yield return new InMemoryItem(file, this);

        if (recursive)
        {
            foreach (var subdir in directory.Directories)
                foreach (var x in InternalEnumerateFiles(subdir, regex, recursive))
                    yield return x;
        }
    }

    private (string path, string file) SplitPath(string absolutePathAndFileName)
    {
        var n = absolutePathAndFileName.LastIndexOf('/');
        return (absolutePathAndFileName.Left(n), absolutePathAndFileName.Mid(n + 1));
    }

    /// <inheritdoc/>
    public void ChangeDirectory(string path)
    {
        _currentDirectory = FindDirectory(path, false) ?? throw DirectoryNotFound(path);
    }

    /// <inheritdoc/>
    public IFileItem CopyFile(string sourceName, string targetName, bool overwrite)
    {
        var source = FindFile(sourceName) ?? throw FileNotFound(sourceName);
        var target = FindFile(targetName);

        if (!overwrite && target != null)
            throw new IOException($"File already exists: {target.FullName()}");

        target?.Remove();

        var file = CreateFileInternal(targetName);
        file.Data = source.Data.Clone();
        file.Time = source.Time;

        return new InMemoryItem(file, this);
    }

    private File CreateFileInternal(string fileName)
    {
        var result = FindFile(fileName);
        if (result == null)
        {
            var (path, file) = SplitPath(GetAbsolutePath(fileName));
            var dir = FindDirectory(path, false) ?? throw DirectoryNotFound(path);
            result = new File(dir, file, Clock.Now, null);
            dir.Files.Add(result);
        }

        result.Data.Clear();
        result.Time = Clock.Now;
        
        return result;
    }

    /// <inheritdoc/>
    public bool DeleteDirectory(string path, bool recursive)
    {
        var directory = FindDirectory(path, false);
        if (directory == null)
            return false;
        
        if (!recursive)
        {
            if (directory.Directories.Any() || directory.Files.Any())
                throw new IOException($"Directory not empty: {directory.FullName()}");
        }
        
        directory.Remove();
        return true;
    }

    /// <inheritdoc/>
    public bool DeleteFile(string fileName)
    {
        var file = FindFile(fileName);
        file?.Remove();

        return file != null;
    }

    /// <inheritdoc/>
    public bool DirectoryExists(string path)
    {
        return FindDirectory(path, false) != null;
    }

    /// <inheritdoc/>
    public bool FileExists(string fileName)
    {
        return FindFile(fileName) != null;
    }

    /// <inheritdoc/>
    public IFileItem? GetDirectory(string path, bool canCreate)
    {
        var dir = FindDirectory(path, canCreate);
        return dir != null ? new InMemoryItem(dir, this) : null;
    }

    /// <inheritdoc/>
    public IFileItem? GetFile(string fileName, bool canCreate)
    {
        var file = FindFile(fileName);
        if (file == null && canCreate)
            file = CreateFileInternal(fileName);
        
        return file != null ? new InMemoryItem(file, this) : null;
    }

    /// <inheritdoc/>
    public IEnumerable<IFileItem> GetFiles(string path, string searchPattern, bool recursive)
    {
        var dir   = FindDirectory(path, false) ?? throw DirectoryNotFound(path);
        var regex = Wildcards.ToRegex(searchPattern, true);

        return InternalEnumerateFiles(dir, regex, recursive);
    }

    /// <inheritdoc/>
    public long GetFileSize(string fileName)
    {
        var file = FindFile(fileName) ?? throw FileNotFound(fileName);
        return (long)file.Data.Length;
    }

    /// <inheritdoc/>
    public DateTime GetFileTime(string fileName)
    {
        var file = FindFile(fileName) ?? throw FileNotFound(fileName);
        return file.Time;
    }

    /// <inheritdoc/>
    public IEnumerable<IFileItem> ListFiles(string? directory = null)
    {
        var dir = FindDirectory(directory ?? ".", false) ?? throw DirectoryNotFound(directory ?? ".");

        foreach (var item in dir.Directories.OrderBy(x => x.Name))
            yield return new InMemoryItem(item, this);

        foreach (var item in dir.Files.OrderBy(x => x.Name))
            yield return new InMemoryItem(item, this);
    }

    /// <inheritdoc/>
    public IFileItem MoveFile(string sourceName, string targetName, bool overwrite)
    {
        var source = FindFile(sourceName) ?? throw FileNotFound(sourceName);
        var target = FindFile(targetName);

        if (!overwrite && target != null)
            throw new IOException($"File already exists: {target.FullName()}");

        target?.Remove();

        var (path, name) = SplitPath(GetAbsolutePath(targetName));
        var newParent = FindDirectory(path, false) ?? throw DirectoryNotFound(path);

        source.Remove();
        source.Parent = newParent;
        source.Name   = name;
        newParent.Files.Add(source);

        return new InMemoryItem(source, this);
    }

    /// <inheritdoc/>
    public Stream OpenFile(string fileName, FileMode mode, FileAccess access)
    {
        var file = FindFile(fileName);

        switch (mode)
        {
            case FileMode.CreateNew:
                if (file != null)
                    throw new IOException($"File {fileName} already exists.");
                file = CreateFileInternal(fileName);
                return new NonDisposableStreamWrapper(file, access, Clock);
            
            case FileMode.Create:
                file = CreateFileInternal(fileName);
                return new NonDisposableStreamWrapper(file, access, Clock);
            
            case FileMode.Open:
                if (file == null)
                    throw FileNotFound(fileName);
                return new NonDisposableStreamWrapper(file, access, Clock);
            
            case FileMode.OpenOrCreate:
                file ??= CreateFileInternal(fileName);
                return new NonDisposableStreamWrapper(file, access, Clock);
            
            case FileMode.Truncate:
                if (file == null)
                    throw FileNotFound(fileName);
                file.Data.Clear();
                return new NonDisposableStreamWrapper(file, access, Clock);
            
            case FileMode.Append:
                file ??= CreateFileInternal(fileName);
                var stream = new NonDisposableStreamWrapper(file, access, Clock);
                stream.Position = stream.Length;
                return stream;
            
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }

    /// <inheritdoc/>
    public byte[] ReadAllBytes(string fileName)
    {
        var file   = FindFile(fileName) ?? throw FileNotFound(fileName);
        var result = new byte[file.Data.Length];
        file.Data.Read(result, 0, result.Length);
        return result;
    }

    /// <inheritdoc/>
    public Task<byte[]> ReadAllBytesAsync(string fileName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ReadAllBytes(fileName));
    }

    /// <inheritdoc/>
    public string ReadAllText(string fileName)
    {
        var bytes = ReadAllBytes(fileName);
        return Encoding.GetString(bytes);
    }

    /// <inheritdoc/>
    public Task<string> ReadAllTextAsync(string fileName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ReadAllText(fileName));
    }

    /// <inheritdoc/>
    public void SetFileTime(string fileName, DateTime time)
    {
        var file = FindFile(fileName) ?? throw FileNotFound(fileName);
        file.Time = time;
    }

    /// <inheritdoc/>
    public IFileItem Touch(string fileName)
    {
        var file = FindFile(fileName) ?? CreateFileInternal(fileName);
        file.Time = Clock.Now;
        return new InMemoryItem(file, this);
    }

    /// <inheritdoc/>
    public void WriteAllBytes(string fileName, byte[] content)
    {
        var file = CreateFileInternal(fileName);
        file.Data.Clear();
        file.Data.Write(content, 0);
    }

    /// <inheritdoc/>
    public Task WriteAllBytesAsync(string fileName, byte[] content, CancellationToken cancellationToken = default)
    {
        WriteAllBytes(fileName, content);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void WriteAllText(string fileName, string content)
    {
        var buffer = Encoding.GetBytes(content);
        WriteAllBytes(fileName, buffer);
    }

    /// <inheritdoc/>
    public Task WriteAllTextAsync(string fileName, string content, CancellationToken cancellationToken = default)
    {
        var buffer = Encoding.GetBytes(content);
        WriteAllBytes(fileName, buffer);
        return Task.CompletedTask;
    }
}
