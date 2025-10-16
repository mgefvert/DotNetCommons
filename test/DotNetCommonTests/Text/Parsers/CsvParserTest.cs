﻿using System.IO.Compression;
using System.Reflection;
using System.Text;
using DotNetCommons.Text.Parsers;

namespace DotNetCommonTests.Text.Parsers;

[TestClass, DoNotParallelize]
public class CsvParserTest
{
    private CsvParser _parser = null!;

    [TestInitialize]
    public void Setup()
    {
        _parser = new CsvParser();
    }

    [TestMethod]
    public void Test()
    {
        const string src = @",1, 2, abc, 'hello world',3,  'hello\' again' , 1  2 3";

        var result = _parser.ParseRows(Cvt(src));
        Assert.HasCount(1, result);
        Assert.AreEqual("|1|2|abc|hello world|3|hello\" again|1  2 3", string.Join("|", result[0]));
    }

    [TestMethod]
    public void TestEscape()
    {
        var result = _parser.ParseRow(Cvt(@"'\''"));
        Assert.HasCount(1, result);
        Assert.AreEqual("\"", string.Join(",", result[0]));
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
            var csv = _parser.ParseRows(text);
            Console.WriteLine((int)(DateTime.Now - t0).TotalMilliseconds + " ms");

            Assert.HasCount(10002, csv);
        }
    }

    private string Cvt(string text)
    {
        return text.Replace('\'', '"');
    }
}