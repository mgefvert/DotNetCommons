using DotNetCommons.IO;

namespace DotNetCommonTests.IO;

[TestClass]
public class FortuneCookieReaderTests
{
    [TestMethod]
    public void Load_ReadsFortunesSeparatedByPercentLines()
    {
        var source = new MemoryStream([ .. "First fortune\r\nwith two lines\r\n%\r\nSecond fortune\r\n%\r\n"u8]);

        var reader = new FortuneCookieReader();
        var crlf   = Environment.NewLine;
        reader.Load(source);

        CollectionAssert.AreEqual(
            new[] { $"First fortune{crlf}with two lines", "Second fortune" },
            reader.Fortunes);
    }

    [TestMethod]
    public void Load_CanLoadMultipleFiles()
    {
        var first  = new MemoryStream([.. "first"u8]);
        var second = new MemoryStream([.. "second"u8]);

        var reader = new FortuneCookieReader();
        reader.Load(first);
        reader.Load(second);

        CollectionAssert.AreEqual(new[] { "first", "second" }, reader.Fortunes);
    }

    [TestMethod]
    public void Random_ReturnsNullWhenNoFortunesAreLoaded()
    {
        var reader = new FortuneCookieReader();
        Assert.IsNull(reader.Random());
    }
}
