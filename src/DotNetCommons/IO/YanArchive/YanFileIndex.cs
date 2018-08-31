using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCommons.IO.YanArchive
{
    public static class YanFileIndex
    {
        public static YanFile Find(this IEnumerable<YanFile> index, Guid id)
        {
            return index.FirstOrDefault(x => x.Id == id);
        }

        public static YanFile Find(this IEnumerable<YanFile> index, string filename)
        {
            return index.FirstOrDefault(x => filename.Equals(x.Name, StringComparison.CurrentCultureIgnoreCase));
        }

        public static IEnumerable<YanFile> FindAll(this IEnumerable<YanFile> index, Guid id)
        {
            return index.Where(x => x.Id == id);
        }

        public static IEnumerable<YanFile> FindAll(this IEnumerable<YanFile> index, string filename)
        {
            return index.Where(x => filename.Equals(x.Name, StringComparison.CurrentCultureIgnoreCase));
        }

        internal static int InsertPosition(this IEnumerable<YanFile> index)
        {
            return YanHeader.Length + index.Sum(x => x.SizeOnDisk);
        }

        internal static void SortByPosition(this List<YanFile> index)
        {
            index.Sort((a, b) => a.Position - b.Position);
        }
    }
}
