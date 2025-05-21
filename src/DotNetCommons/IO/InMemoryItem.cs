using DotNetCommons.Temporal;

namespace DotNetCommons.IO;

public class InMemoryItem : BaseFileItem
{
    private readonly InMemoryFileAccessor.Entry _entry;
    private readonly IClock _clock;

    public override string Name => _entry.Name!;
    public override string FullName => _entry.FullName();
    public override bool Directory => _entry is InMemoryFileAccessor.Directory;
    public override long Size => _entry is InMemoryFileAccessor.File file ? (long)file.Data.Length : 0;
    public override DateTime LastWriteTime => _entry.Time;

    public override IFileItem? Parent => _entry.Parent != null
        ? new InMemoryItem(_entry.Parent, (InMemoryFileAccessor)Accessor)
        : null;

    internal InMemoryItem(InMemoryFileAccessor.Entry entry, InMemoryFileAccessor accessor) : base(accessor)
    {
        _entry = entry;
        _clock = accessor.Clock;
    }

    public override Stream Open(FileAccess access)
    {
        return _entry is InMemoryFileAccessor.File file
            ? new InMemoryFileAccessor.NonDisposableStreamWrapper(file, access, _clock)
            : throw new IOException("Cannot open a directory.");
    }
}
