namespace DotNetCommons.IO;

public class FileSystemItem : BaseFileItem
{
    private readonly DirectoryInfo? _directoryInfo;
    private readonly FileInfo? _fileInfo;

    public override string Name => _fileInfo?.Name ?? _directoryInfo?.Name ?? throw new InvalidDataException();
    public override string FullName => _fileInfo?.FullName ?? _directoryInfo?.FullName ?? throw new InvalidDataException();
    public override bool Directory => _directoryInfo != null;
    public override long Size => _fileInfo?.Length ?? 0;
    public override DateTime LastWriteTime => _fileInfo?.LastWriteTime ?? _directoryInfo?.LastWriteTime ?? throw new InvalidDataException();

    public override IFileItem? Parent =>
        (_fileInfo?.Directory != null ? new FileSystemItem(_fileInfo.Directory, Accessor) : null) ??
        (_directoryInfo?.Parent != null ? new FileSystemItem(_directoryInfo.Parent, Accessor) : null);

    public FileSystemItem(FileInfo fileInfo, IFileAccessor accessor) : base(accessor)
    {
        _fileInfo = fileInfo;
    }

    public FileSystemItem(DirectoryInfo directoryInfo, IFileAccessor accessor) : base(accessor)
    {
        _directoryInfo = directoryInfo;
    }

    public override Stream Open(FileAccess access)
    {
        if (_fileInfo == null)
            throw new IOException("Cannot open a directory.");

        return access switch
        {
            FileAccess.Read      => new FileStream(_fileInfo.FullName, FileMode.Open, access),
            FileAccess.Write     => new FileStream(_fileInfo.FullName, FileMode.OpenOrCreate, access),
            FileAccess.ReadWrite => new FileStream(_fileInfo.FullName, FileMode.OpenOrCreate, access),
            _                    => throw new ArgumentOutOfRangeException(nameof(access), access, null)
        };
    }
}
