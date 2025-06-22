namespace DotNetCommons.IO;

public abstract class BaseFileItem : IFileItem
{
    protected readonly IFileAccessor Accessor;

    public abstract string Name { get; }
    public abstract string FullName { get; }
    public abstract bool Directory { get; }
    public abstract long Size { get; }
    public abstract DateTime LastWriteTime { get; }
    public abstract IFileItem? Parent { get; }

    public abstract Stream Open(FileAccess access);

    protected BaseFileItem(IFileAccessor accessor)
    {
        Accessor = accessor;
    }

    public IEnumerable<IFileItem> ListFiles() => Accessor.ListFiles(FullName);
    public byte[] ReadAllBytes() => Accessor.ReadAllBytes(FullName);
    public Task<byte[]> ReadAllBytesAsync(CancellationToken token) => Accessor.ReadAllBytesAsync(FullName, token);
    public string ReadAllText() => Accessor.ReadAllText(FullName);
    public Task<string> ReadAllTextAsync(CancellationToken token) => Accessor.ReadAllTextAsync(FullName, token);
    public void WriteAllBytes(byte[] content) => Accessor.WriteAllBytes(FullName, content);
    public Task WriteAllBytesAsync(byte[] content, CancellationToken token) => Accessor.WriteAllBytesAsync(FullName, content, token);
    public void WriteAllText(string content) => Accessor.WriteAllText(FullName, content);
    public Task WriteAllTextAsync(string content, CancellationToken token) => Accessor.WriteAllTextAsync(FullName, content, token);
}