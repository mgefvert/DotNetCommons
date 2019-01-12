using System;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.IO.YanArchive
{
    [Flags]
    public enum YanHeaderFlags
    {
        None = 0x00,
        Encrypted = 0x01
    }

    public class YanHeader
    {
        public const int Length = 16;

        public byte Version { get; set; }
        public int IndexPosition { get; set; }
        public YanHeaderFlags Flags { get; set; }

        public YanHeader()
        {
            Version = 1;
        }

        public YanHeader Copy()
        {
            return (YanHeader)MemberwiseClone();
        }
    }
}
