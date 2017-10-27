using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace DotNetCommons.Logger.LogMethods
{
    public class FileLogger : ILogMethod
    {
        private readonly object _lock = new object();
        private readonly ILogFileNaming _provider;
        private readonly string _logName;
        private readonly string _extension;
        private readonly DirectoryInfo _directory;
        private readonly int _maxRotations;
        private readonly bool _compress;
        private DateTime _lastDate = DateTime.MinValue;
        private Stream _stream;

        public FileLogger(LogRotation rotation, string directory, string logname, string extension, int maxRotations, bool compress)
        {
            _directory = new DirectoryInfo(directory);
            _logName = logname;
            _extension = extension.StartsWith(".") ? extension : "." + extension;
            _compress = compress;
            _maxRotations = maxRotations;

            switch (rotation)
            {
                case LogRotation.Daily:
                    _provider = new LogFileDaily();
                    break;

                case LogRotation.Monthly:
                    _provider = new LogFileMonthly();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(rotation), rotation, null);
            }
        }

        internal interface ILogFileNaming
        {
            IEnumerable<string> GetAllowedFiles(string name, string extension, int rotations, DateTime? date = null);
            string GetCurrentFileName(string name, string extension, DateTime? date = null);
            string GetFileSpec(string name, string extension);
        }

        internal class LogFileDaily : ILogFileNaming
        {
            public IEnumerable<string> GetAllowedFiles(string name, string extension, int rotations, DateTime? date = null)
            {
                var dt = date ?? DateTime.Today;

                for (var i = 0; i <= rotations; i++)
                {
                    var filename = GetCurrentFileName(name, extension, dt);
                    yield return filename;
                    yield return filename + ".gz";

                    dt = dt.AddDays(-1);
                }
            }

            public string GetCurrentFileName(string name, string extension, DateTime? date)
            {
                return name + "-" + (date ?? DateTime.Today).ToString("yyyy-MM-dd") + extension;
            }

            public string GetFileSpec(string name, string extension)
            {
                return name + "-????-??-??" + extension;
            }
        }

        internal class LogFileMonthly : ILogFileNaming
        {
            public IEnumerable<string> GetAllowedFiles(string name, string extension, int rotations, DateTime? date = null)
            {
                var dt = date ?? DateTime.Today;

                for (var i = 0; i <= rotations; i++)
                {
                    var filename = GetCurrentFileName(name, extension, dt);
                    yield return filename;
                    yield return filename + ".gz";

                    dt = dt.AddMonths(-1);
                }
            }

            public string GetCurrentFileName(string name, string extension, DateTime? date = null)
            {
                return name + "-" + (date ?? DateTime.Today).ToString("yyyy-MM") + extension;
            }

            public string GetFileSpec(string name, string extension)
            {
                return name + "-????-??" + extension;
            }
        }

        internal void CompressFile(FileInfo file)
        {
            var oldFileName = file.FullName;
            var newFileName = file.FullName + ".gz";

            // If new file already exists and has data, skip the compression step
            if (!File.Exists(newFileName) || new FileInfo(newFileName).Length == 0)
            {
                using (var oldFile = new FileStream(oldFileName, FileMode.Open, FileAccess.Read))
                using (var newFile = new FileStream(newFileName, FileMode.Create, FileAccess.Write))
                using (var gz = new GZipStream(newFile, CompressionMode.Compress))
                {
                    oldFile.CopyTo(gz);
                }
            }

            try
            {
                File.Delete(oldFileName);
            }
            catch (Exception)
            {
                // Couldn't delete old file... strange; just leave for now
            }
        }

        public FileInfo GetCurrentLogfile()
        {
            return new FileInfo(Path.Combine(_directory.FullName, _provider.GetCurrentFileName(_logName, _extension)));
        }

        public Stream OpenCurrent()
        {
            var filename = GetCurrentLogfile();

            var dir = filename.Directory;
            if (dir != null && !dir.Exists)
                dir.Create();

            var stream = filename.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            stream.Seek(0, SeekOrigin.End);
            return stream;
        }

        public void Rotate()
        {
            var fileSpec = _provider.GetFileSpec(_logName, _extension);
            var allowedFiles = _provider.GetAllowedFiles(_logName, _extension, _maxRotations).ToList();
            var currentFile = _provider.GetCurrentFileName(_logName, _extension);

            var oldFiles =
                _directory.EnumerateFiles(fileSpec)
                .Concat(_directory.EnumerateFiles(fileSpec + ".gz"))
                .ToList();

            oldFiles.RemoveAll(x => x.Name.Like(currentFile));

            // Delete old log files
            var toDelete = oldFiles.ExtractAll(f => !allowedFiles.Contains(f.Name));
            foreach (var file in toDelete)
                file.Delete();

            // Compress files if needed
            if (_compress)
            {
                foreach (var file in oldFiles.Where(f => !f.Name.EndsWith(".gz", StringComparison.CurrentCultureIgnoreCase)))
                    CompressFile(file);
            }
        }

        public List<LogEntry> Handle(List<LogEntry> entries, bool flush)
        {
            var rotate = false;

            lock (_lock)
            {
                if (_lastDate != DateTime.Today || _stream == null)
                {
                    _lastDate = DateTime.Today;
                    _stream?.Dispose();
                    rotate = true;
                    _stream = OpenCurrent();
                }

                if (_stream == null)
                    return entries;

                var encoding = Encoding.UTF8;
                using (var mem = new MemoryStream())
                {
                    foreach (var entry in entries)
                    {
                        var buffer = encoding.GetBytes(entry.ToString(LogFormat.Long));
                        mem.Write(buffer, 0, buffer.Length);
                        mem.WriteByte(13);
                        mem.WriteByte(10);
                    }

                    mem.Position = 0;
                    mem.CopyTo(_stream);
                }

                _stream.Flush();
            }

            if (rotate)
                Rotate();

            return entries;
        }
    }
}
