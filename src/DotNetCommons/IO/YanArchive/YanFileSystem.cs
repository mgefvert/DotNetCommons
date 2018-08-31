using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace DotNetCommons.IO.YanArchive
{
    public class YanFileSystem : IDisposable
    {
        private readonly ReaderWriterLock _lock = new ReaderWriterLock();

        private const string ErrorArchiveIsReadonly = "Archive is readonly.";
        private const string ErrorAlreadyDisposed = "File stream is already disposed.";

        private readonly string _filename;
        private YanHeader _header;
        private List<YanFile> _index;
        private readonly byte[] _password;
        private readonly bool _readOnly;
        private volatile FileStream _stream;

        public IReadOnlyList<YanFile> Files => _index.AsReadOnly();
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

        public YanFileSystem(string filename, bool readOnly, byte[] password = null)
        {
            _filename = filename;
            _password = password;
            _readOnly = readOnly;

            _lock.AcquireWriterLock(Timeout);
            try
            {
                if (!File.Exists(filename))
                {
                    if (_readOnly)
                        throw new IOException("Archive does not exist.");

                    var flags = password != null ? YanHeaderFlags.Encrypted : YanHeaderFlags.None;
                    YanFileSystemIO.Create(filename, flags, password);
                }

                OpenStream();
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
        }

        public YanFileSystem(string filename, byte[] password = null) : this(filename, false, password)
        {
        }

        public void Dispose()
        {
            _lock.AcquireWriterLock(Timeout);
            try
            {
                _stream?.Dispose();
                _stream = null;
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
        }

        public void Add(string filename, byte[] data, YanFileFlags flags = YanFileFlags.None)
        {
            using (var mem = new MemoryStream(data))
                Add(filename, mem, flags);
        }

        public void Add(string filename, Stream data, YanFileFlags flags = YanFileFlags.None)
        {
            if (_stream == null)
                throw new ObjectDisposedException(ErrorAlreadyDisposed);
            if (_readOnly)
                throw new IOException(ErrorArchiveIsReadonly);

            _lock.AcquireWriterLock(Timeout);
            try
            {
                Delete(filename);

                var record = new YanFile
                {
                    Id = Guid.NewGuid(),
                    Name = filename,
                    Flags = flags,
                    Position = _index.InsertPosition(),
                    Size = (int)(data.Length - data.Position)
                };

                record.SizeOnDisk = YanFileSystemIO.BlockWrite(_stream, record.Id, flags, record.Position, _password, data);
                _index.Add(record);

                FlushIndex();
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
        }

        public void Delete(Guid id)
        {
            if (_stream == null)
                throw new ObjectDisposedException(ErrorAlreadyDisposed);
            if (_readOnly)
                throw new IOException(ErrorArchiveIsReadonly);

            _lock.AcquireWriterLock(Timeout);
            try
            {
                var deleted = false;
                foreach (var f in _index.FindAll(id))
                {
                    deleted = true;
                    f.Flags |= YanFileFlags.Deleted;
                }

                if (deleted)
                    FlushIndex();
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
        }

        public void Delete(string filename)
        {
            if (_stream == null)
                throw new ObjectDisposedException(ErrorAlreadyDisposed);
            if (_readOnly)
                throw new IOException(ErrorArchiveIsReadonly);

            _lock.AcquireWriterLock(Timeout);
            try
            {
                var deleted = false;
                foreach (var f in _index.FindAll(filename))
                {
                    deleted = true;
                    f.Flags |= YanFileFlags.Deleted;
                }

                if (deleted)
                    FlushIndex();
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
        }

        public void DeleteArchive()
        {
            _lock.AcquireWriterLock(Timeout);
            try
            {
                Dispose();
                File.Delete(_filename);
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
        }

        public bool Exists(Guid id)
        {
            _lock.AcquireReaderLock(Timeout);
            try
            {
                return _index.Find(id) != null;
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }
        }

        public bool Exists(string filename)
        {
            _lock.AcquireReaderLock(Timeout);
            try
            {
                return _index.Find(filename) != null;
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }
        }

        protected void FlushIndex()
        {
            if (_readOnly)
                throw new IOException(ErrorArchiveIsReadonly);

            FlushIndex(_stream, _header, _index, _password);
        }

        protected static void FlushIndex(FileStream stream, YanHeader header, List<YanFile> index, byte[] password)
        {
            if (stream == null)
                throw new ObjectDisposedException(ErrorAlreadyDisposed);

            index.SortByPosition();
            header.IndexPosition = index.InsertPosition();
            var len = YanFileSystemIO.IndexWrite(stream, header.IndexPosition, password, index);
            YanFileSystemIO.HeaderWrite(stream, header);

            stream.SetLength(header.IndexPosition + len);
        }

        public MemoryStream Load(Guid id)
        {
            _lock.AcquireReaderLock(Timeout);
            try
            {
                if (_stream == null)
                    throw new ObjectDisposedException(ErrorAlreadyDisposed);

                var f = _index.Find(id) ?? throw new IOException($"File {id} does not exist in archive.");
                return YanFileSystemIO.BlockRead(_stream, id, f.Flags, f.Position, f.Size, _password);
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }
        }

        public MemoryStream Load(string filename)
        {
            _lock.AcquireReaderLock(Timeout);
            try
            {
                if (_stream == null)
                    throw new ObjectDisposedException(ErrorAlreadyDisposed);

                var f = _index.Find(filename) ?? throw new IOException($"File {filename} does not exist in archive.");
                return YanFileSystemIO.BlockRead(_stream, f.Id, f.Flags, f.Position, f.SizeOnDisk, _password);
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }
        }

        protected void OpenStream()
        {
            _stream = new FileStream(_filename, FileMode.Open, _readOnly ? FileAccess.Read : FileAccess.ReadWrite, FileShare.Read);
            _header = YanFileSystemIO.HeaderRead(_stream);
            _index = _header.IndexPosition != 0
                ? YanFileSystemIO.IndexRead(_stream, _header.IndexPosition, _password)
                : new List<YanFile>();
        }

        public void Pack()
        {
            if (_stream == null)
                throw new ObjectDisposedException(ErrorAlreadyDisposed);
            if (_readOnly)
                throw new IOException(ErrorArchiveIsReadonly);

            _lock.AcquireWriterLock(Timeout);
            try
            {
                _index.SortByPosition();

                var newIndex = _index.Where(x => !x.Flags.HasFlag(YanFileFlags.Deleted)).Select(x => x.Copy()).ToList();
                var pos = YanHeader.Length;
                foreach (var f in newIndex)
                {
                    f.Position = pos;
                    pos += f.SizeOnDisk;
                }

                if (newIndex.Count < _index.Count)
                    PackInplace(newIndex);
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
        }

        private void PackInplace(List<YanFile> newIndex)
        {
            foreach (var newfile in newIndex)
            {
                var oldfile = _index.Find(newfile.Id);
                if (newfile.Position == oldfile.Position)
                    continue;

                var buffer = new byte[newfile.SizeOnDisk];

                // Read old file
                _stream.Position = oldfile.Position;
                _stream.Read(buffer, 0, buffer.Length);

                // Write new file
                _stream.Position = newfile.Position;
                _stream.Write(buffer, 0, buffer.Length);
            }

            _index = newIndex;
            FlushIndex();
        }
    }
}
