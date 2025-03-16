using System.Security.Cryptography;

namespace DotNetCommons.IO;

/// <summary>
/// Class that encapsulates a 64-bit read/writable memory block. Grows exponentially with new block allocations. Can be used as a
/// backing store for memory-based streams, etc.
/// </summary>
public class MemoryBlock : IDisposable
{
    private const int MaxBlockSize = 64 * 1048576;

    private ulong _capacity;
    private ulong _length;
    private byte[]? _sha256;
    private int _initialSize;
    private readonly List<byte[]> _blocks = new();
    private readonly ReaderWriterLockSlim _lock = new();

    /// <summary>
    /// Allocated capacity at this moment.
    /// </summary>
    public ulong Capacity => _capacity;

    /// <summary>
    /// Length of file (may differ from the capacity). Set this value to contract or expand the file as necessary.
    /// </summary>
    public ulong Length
    {
        get => _length;
        set => UpdateLength(value);
    }

    /// <summary>
    /// Initialize a new MemoryBlock with a suggested initial block size of 1024 bytes. Blocks grow with * 3 everytime capacity
    /// is exhausted. 
    /// </summary>
    public MemoryBlock(int initialSize = 1024)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(initialSize);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(initialSize, MaxBlockSize);
       
        _initialSize = initialSize;
    }
    
    /// <summary>
    /// Initialize a new MemoryBlock using a given buffer as the first, initial block. Block sizes will grow with * 3 from the initial
    /// block size. Note that the buffer is considered writable.
    /// </summary>
    public MemoryBlock(byte[] buffer)
    {
        if (buffer.Length > 0)
        {
            _blocks.Add(buffer);
            _initialSize = buffer.Length;
            _capacity = _length = (ulong)buffer.Length;
        }
        else
            _initialSize = 1024;
    }

    /// <summary>
    /// Initialize a new MemoryBlock by making a new byte buffer from the ReadOnlySpan.
    /// </summary>
    public MemoryBlock(ReadOnlySpan<byte> buffer) : this(buffer.ToArray())
    {
    }

    private void InternalAddBlock()
    {
        var size = _blocks.Count == 0 ? _initialSize : Math.Min(_blocks.Last().Length * 3, MaxBlockSize);
        _blocks.Add(new byte[size]);
        _capacity += (ulong)size;
    }

    internal (int block, uint pos) InternalMapPosition(ulong position)
    {
        for (var i = 0; i < _blocks.Count; i++)
        {
            if (position < (uint)_blocks[i].Length)
                return (i, (uint)position);

            position -= (uint)_blocks[i].Length;
        }

        return (-1, 0);
    }

    /// <summary>
    /// Clear the file contents by calling Clear().
    /// </summary>
    public void Dispose()
    {
        Clear();
    }

    /// <summary>
    /// Clear the file contents. Resets the allocated data back to zero.
    /// </summary>
    public void Clear()
    {
        _lock.EnterWriteLock();
        try
        {
            _sha256 = null;
            _blocks.Clear();
            _length   = 0;
            _capacity = 0;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Make a clone of the MemoryBlock, with a deep copy of all the memory blocks.
    /// </summary>
    public MemoryBlock Clone()
    {
        _lock.EnterReadLock();
        try
        {
            var result = new MemoryBlock(_initialSize);
            result._capacity = _capacity;
            result._length   = _length;
            result._sha256  = _sha256;

            foreach (var block in _blocks)
            {
                var buffer = new byte[block.Length];
                Array.Copy(block, buffer, buffer.Length);
                result._blocks.Add(buffer);
            }

            return result;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Calculate the SHA256 from the contents.
    /// </summary>
    public byte[] GetSha256()
    {
        if (_sha256 != null)
            return _sha256;

        var len  = _length;
        var hash = SHA256.Create();
        foreach (var block in _blocks)
        {
            var copyLen = (int)Math.Min(len, (ulong)block.Length);
            hash.TransformBlock(block, 0, copyLen, block, 0);
            len -= (ulong)copyLen;
            if (len <= 0)
                break;
        }

        hash.TransformFinalBlock([], 0, 0);
        return _sha256 = hash.Hash!;
    }
    
    /// <summary>
    /// Read contents from a stream and write to the MemoryBlock.
    /// </summary>
    /// <param name="stream">Source stream to read from.</param>
    /// <param name="append">Whether writing will start at zero (append = false), or at the end of the data (append = true).
    ///   Note that starting at zero does not erase previous content.</param>
    public void LoadFrom(Stream stream, bool append)
    {
        var pos    = append ? _length : 0;
        var buffer = new byte[1048576];

        if (stream.CanSeek)
            _initialSize = (int)Math.Min(stream.Length, MaxBlockSize);

        for(;;)
        {
            var len = stream.Read(buffer, 0, buffer.Length);
            if (len == 0)
                return;
                
            Write(buffer, pos, len);
            pos += (ulong)len;
        }
    }
    
    /// <summary>
    /// Read a section from the contents.
    /// </summary>
    /// <returns>The number of bytes actually read. Will always equal the request size unless it's greater than the content.</returns>
    public int Read(byte[] result, ulong position, int length)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, result.Length);
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        _lock.EnterReadLock();
        try
        {
            if (position >= _length)
                return 0;

            if (position + (ulong)length > _length)
                length = (int)(_length - position);
            
            var index  = 0;
            var (block, blockPos) = InternalMapPosition(position);

            while (length > 0)
            {
                var blockLen = _blocks[block].Length - blockPos;
                if (blockLen > length)
                    blockLen = length;

                Array.Copy(_blocks[block], blockPos, result, index, blockLen);
                length -= (int)blockLen;
                index  += (int)blockLen;
                block++;
                blockPos = 0;
            }

            return index;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Save the entire contents to a stream.
    /// </summary>
    /// <param name="stream"></param>
    public void SaveTo(Stream stream)
    {
        var len = _length;
        foreach (var block in _blocks)
        {
            var copyLen = (int)Math.Min(len, (ulong)block.Length);
            stream.Write(block, 0, copyLen);
            len -= (ulong)copyLen;
        }
    }
    
    private void UpdateLength(ulong length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        if (_length == length)
            return;
        
        _lock.EnterWriteLock();
        try
        {
            _sha256 = null;

            if (length < _length)
            {
                var (block, _) = InternalMapPosition(length);
                var targetBlockCount = block + 1;

                while (_blocks.Count > targetBlockCount)
                {
                    var removal = _blocks.ExtractLast();
                    _capacity -= (ulong)removal.Length;
                }
            }
            else
            {
                while (length > Capacity)
                    InternalAddBlock();
            }

            _length = length;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Write a buffer in its entirety to a specified position in the MemoryBlock contents. If the position is larger than the current
    /// file contents, the file will be padded with zero bytes to that position if necessary.
    /// </summary>
    public void Write(byte[] buffer, ulong position)
    {
        Write(buffer, position, buffer.Length);
    }

    /// <summary>
    /// Write a buffer to a specified position in the MemoryBlock contents, giving the actual length of data to write. If the position
    /// is larger than the current file contents, the file will be padded with zero bytes to that position if necessary.
    /// </summary>
    public void Write(byte[] buffer, ulong position, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, buffer.Length);

        _lock.EnterWriteLock();
        try
        {
            _sha256 = null;
            var finalPosition = position + (uint)length;
            while (finalPosition > Capacity)
                InternalAddBlock();

            var index = 0;
            var (block, blockPos) = InternalMapPosition(position);

            while (length > 0)
            {
                var blockLen = _blocks[block].Length - blockPos;
                if (blockLen > length)
                    blockLen = length;

                Array.Copy(buffer, index, _blocks[block], blockPos, blockLen);
                length -= (int)blockLen;
                index  += (int)blockLen;
                block++;
                blockPos = 0;
            }

            _length = Math.Max(_length, finalPosition);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Return the entire contents as a byte array. Only works if the contents is less or equal to 2 GB.
    /// </summary>
    public byte[] ToArray()
    {
        if (_length > int.MaxValue)
            throw new InvalidOperationException("Memory block is larger than 2GB.");

        var result = new byte[(int)_length];
        var pos    = 0;
        var len    = (int)_length;
        foreach (var block in _blocks)
        {
            var copyLen = Math.Min(len, block.Length);
            Array.Copy(block, 0, result, pos, copyLen);
            pos += copyLen;
            len -= copyLen;
        }

        return result;
    }
}
