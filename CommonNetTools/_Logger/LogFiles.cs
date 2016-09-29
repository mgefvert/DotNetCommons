using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;

namespace CommonNetTools
{
    internal class LogFiles
    {
        private readonly LogConfig _config;

        public LogFiles(LogConfig config)
        {
            _config = config;
        }

        internal void CompressFile(string file)
        {
            var oldFileName = Path.Combine(_config.Directory, file);
            var newFileName = Path.Combine(_config.Directory, file + ".gz");

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

        private static ILogFileNaming GetFileNamingProvider(LogFileNaming naming)
        {
            switch (naming)
            {
                case LogFileNaming.Daily:
                    return new LogFileDaily();

                case LogFileNaming.Monthly:
                    return new LogFileMonthly();
            }

            throw new InvalidOperationException("Undefined file naming scheme: " + naming);
        }

        public FileInfo GetCurrentLogfile(string extension)
        {
            var namer = GetFileNamingProvider(_config.FileNaming);
            return new FileInfo(Path.Combine(_config.Directory, namer.GetCurrentFileName(_config.Name, extension, null)));
        }

        public Stream OpenCurrent(string extension)
        {
            var namer = GetFileNamingProvider(_config.FileNaming);
            var fileName = Path.Combine(_config.Directory, namer.GetCurrentFileName(_config.Name, extension, null));

            var stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            stream.Seek(0, SeekOrigin.End);
            return stream;
        }

        public void Rotate(string extension)
        {
            var namer = GetFileNamingProvider(_config.FileNaming);

            var fileSpec = namer.GetFileSpec(_config.Name, extension);
            var allowedFiles = namer.GetAllowedFiles(_config.Name, extension, _config.MaxRotations).ToList();

            var oldFiles = 
                Directory.EnumerateFiles(_config.Directory, fileSpec)
                .Concat(Directory.EnumerateFiles(_config.Directory, fileSpec + ".gz"))
                .ToList();

            // Delete old log files
            var toDelete = oldFiles.ExtractAll(f => !allowedFiles.Contains(f));
            foreach(var file in toDelete)
                File.Delete(file);

            // Compress files if needed
            if (_config.CompressOnRotate)
            {
                foreach (var file in oldFiles.Where(f => !f.EndsWith(".gz", StringComparison.CurrentCultureIgnoreCase)))
                    CompressFile(file);
            }
        }
    }
}
