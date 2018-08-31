using System;

namespace DotNetCommons.IO.YanArchive
{
    [Flags]
    public enum YanFileFlags
    {
        None = 0x00,
        Deleted = 0x01,
        GZip = 0x0100
    }

    public class YanFile
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Size { get; set; }
        public int SizeOnDisk { get; set; }
        public int Position { get; set; }
        public YanFileFlags Flags { get; set; }

        public bool IsDeleted => Flags.HasFlag(YanFileFlags.Deleted);

        public YanFile Copy()
        {
            return (YanFile)MemberwiseClone();
        }
    }
}
