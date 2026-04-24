using System.IO.Compression;
using System.Reflection;
using System.Text;
using DotNetCommons.Text.Parsers;

namespace DotNetCommonTests.Text.Parsers;

[TestClass, DoNotParallelize]
public class CsvParserOfTTest
{
    private CsvParser<Name> _parser = null!;

    [TestInitialize]
    public void Setup()
    {
        _parser = new CsvParser<Name>();
    }

    [TestMethod]
    public void TestReadCsvFile()
    {
        var data = Assembly.GetExecutingAssembly().GetManifestResourceStream(GetType(), "name-test.csv.gz");
        if (data == null)
            throw new FileNotFoundException("Unable to find resource");

        using (data)
        using (var gz = new GZipStream(data, CompressionMode.Decompress))
        using (var reader = new StreamReader(gz, Encoding.UTF8))
        {
            var text = reader.ReadToEnd();

            var t0 = DateTime.Now;
            var csv = _parser.ProcessLines(text);
            Console.WriteLine((int)(DateTime.Now - t0).TotalMilliseconds + " ms");

            Assert.HasCount(10000, csv);

            var name = csv.First();
            Assert.AreEqual("Sean", name.FirstName);
            Assert.AreEqual("Reinert", name.LastName);
            Assert.AreEqual("Philadelphia", name.City);
            Assert.AreEqual("215-787-6199", name.PhoneNumber);
            Assert.AreEqual(new DateTime(1971, 11, 23), name.Birthday);
            Assert.AreEqual(Guid.Parse("bbb7ac57-8cce-4908-9580-c56bcb2ddb5c"), name.Guid);

            name = csv.Last();
            Assert.AreEqual("Archie", name.FirstName);
            Assert.AreEqual("Eichorn", name.LastName);
            Assert.AreEqual("Herbertsville", name.City);
            Assert.AreEqual("732-840-6655", name.PhoneNumber);
            Assert.AreEqual(new DateTime(1972, 5, 9), name.Birthday);
            Assert.AreEqual(Guid.Parse("8ca5a0e5-ee04-4acb-b0ed-bb6e0acde520"), name.Guid);
        }
    }
}