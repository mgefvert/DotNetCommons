using System;

namespace DotNetCommons.Net
{
    public class ProgressArgs : EventArgs
    {
        public Uri Location { get; }
        public long Size { get; }
        public long Progress { get; }

        public ProgressArgs(Uri location, long size, long progress)
        {
            Location = location;
            Size = size;
            Progress = progress;
        }
    }
}
