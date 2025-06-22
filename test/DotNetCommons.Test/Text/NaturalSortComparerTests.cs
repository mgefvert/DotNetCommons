using DotNetCommons.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.Text;

[TestClass]
public class NaturalSortComparerTests
{
    private readonly NaturalSortComparer _comparerIc = new(false);
    private readonly NaturalSortComparer _comparerCs = new(true);

    private static int Normalize(int x) => x == 0 ? 0 : (x > 0 ? 1 : -1);

    [TestMethod]
    [DataRow("file1", "file2", -1)]
    [DataRow("file10", "file2", 1)]
    [DataRow("file1", "file01", -1)] // Leading zeros, considered equal numerically, but file01 is longer
    [DataRow("File1", "file1", 0)]  // Case-insensitive
    [DataRow("a10", "a2", 1)]
    [DataRow("a", "a1", -1)]
    [DataRow("a1", "a", 1)]
    [DataRow("a1b", "a1B", 0)]      // Case-insensitive text block
    [DataRow("item20", "item3", 1)]
    [DataRow("img12.png", "img2.png", 1)]
    [DataRow("abc", "abc", 0)]
    [DataRow("", "", 0)]
    [DataRow(null, "a", -1)]
    [DataRow("a", null, 1)]
    [DataRow(null, null, 0)]
    public void Compare_CaseInsensitive(string a, string b, int expectedSign)
    {
        var result = _comparerIc.Compare(a, b);
        Assert.AreEqual(expectedSign, Normalize(result));
    }

    [TestMethod]
    [DataRow("file1", "file2", -1)]
    [DataRow("file10", "file2", 1)]
    [DataRow("file1", "file01", -1)] // Leading zeros, considered equal numerically, but file01 is longer
    [DataRow("File1", "file1", -1)]   // Case-sensitive
    [DataRow("a10", "a2", 1)]
    [DataRow("a", "a1", -1)]
    [DataRow("a1", "a", 1)]
    [DataRow("a1b", "a1B", 1)]      // Case-insensitive text block
    [DataRow("item20", "item3", 1)]
    [DataRow("img12.png", "img2.png", 1)]
    [DataRow("abc", "abc", 0)]
    [DataRow("", "", 0)]
    [DataRow(null, "a", -1)]
    [DataRow("a", null, 1)]
    [DataRow(null, null, 0)]
    public void Compare_CaseSensitive(string a, string b, int expectedSign)
    {
        var result = _comparerCs.Compare(a, b);
        Assert.AreEqual(expectedSign, Normalize(result));
    }

    [TestMethod]
    public void Sorts_List_CS_Correctly()
    {
        var list = new List<string>
        {
            "file1", "file10", "file2", "file20", "file11a", "file11", "File01b", "file01A"
        };

        list.Sort(_comparerCs);

        var expected = new List<string>
        {
            "File01b", "file1", "file01A", "file2", "file10", "file11", "file11a", "file20"
        };

        CollectionAssert.AreEqual(expected, list);
    }

    [TestMethod]
    public void Sorts_List_IC_Correctly()
    {
        var list = new List<string>
        {
            "file1", "file10", "file2", "file20", "file11a", "file11", "File01b", "file01A"
        };

        list.Sort(_comparerIc);

        var expected = new List<string>
        {
            "file1", "file01A", "File01b", "file2", "file10", "file11", "file11a", "file20"
        };

        CollectionAssert.AreEqual(expected, list);
    }
}