using DotNetCommons.Temporal;

namespace DotNetCommonTests.Temporal;

[TestClass]
public class DateTimeToolsTest
{
    [TestMethod]
    public void TestGenerate()
    {
        var strings = DateTimeTools.Generate(new DateTime(2020, 1, 1), new DateTime(2020, 6, 1),
                dt => dt.AddMonths(1))
            .Select(x => x.ToString("yyyyMMdd"));

        Assert.AreEqual("20200101,20200201,20200301,20200401,20200501,20200601", string.Join(",", strings));

        strings = DateTimeTools.Generate(new DateTime(2019, 5, 1), 5, dt => dt.AddDays(2))
            .Select(x => x.ToString("yyyyMMdd"));
        Assert.AreEqual("20190501,20190503,20190505,20190507,20190509", string.Join(",", strings));
    }
}