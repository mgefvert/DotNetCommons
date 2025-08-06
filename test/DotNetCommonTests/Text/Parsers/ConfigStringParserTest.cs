using DotNetCommons.Text.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommonTests.Text.Parsers;

[TestClass]
public class ConfigStringParserTest
{
    [TestMethod]
    public void Parse_Works()
    {
        var cfg = new ConfigStringParser(allowSpacesInKeywords: true).Parse(
            "Data Source=MYSERVER;Initial Catalog=A_DATABASE;Provider=SQLNCLI11.1;Integrated Security=SSPI;Auto Translate=false;");

        Assert.AreEqual(5, cfg.Count);
        Assert.AreEqual("MYSERVER", cfg["Data Source"]);
        Assert.AreEqual("A_DATABASE", cfg["Initial Catalog"]);
        Assert.AreEqual("SQLNCLI11.1", cfg["Provider"]);
        Assert.AreEqual("SSPI", cfg["Integrated Security"]);
        Assert.AreEqual("false", cfg["Auto Translate"]);
    }

    [TestMethod]
    public void Parse_WithQuotes_Works()
    {
        var cfg = new ConfigStringParser(allowSpacesInKeywords: true).Parse(
            "Data Source=MYSERVER\\Instance;Initial Catalog=MyDatabase;Integrated Security=True;Connect Timeout=30;" +
            "Application Name=\"SSIS-execute spRunBatchJob-{C7786B48-1383-1111-2222-22747007DC66}02\\Integration Daily\";");

        Assert.AreEqual("MYSERVER\\Instance", cfg["Data Source"]);
        Assert.AreEqual("MyDatabase", cfg["Initial Catalog"]);
        Assert.AreEqual("True", cfg["Integrated Security"]);
        Assert.AreEqual("30", cfg["Connect Timeout"]);
        Assert.AreEqual("SSIS-execute spRunBatchJob-{C7786B48-1383-1111-2222-22747007DC66}02\\Integration Daily", cfg["Application Name"]);
    }

    [TestMethod]
    public void Parse_WithQuotesAndSemicolons_Works()
    {
        var cfg = new ConfigStringParser(allowSpacesInKeywords: true).Parse(
            "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=\\\\myserver\\temp\\exports.xls;Extended Properties=\"Excel 8.0;HDR=YES\";");

        Assert.AreEqual("Microsoft.Jet.OLEDB.4.0", cfg["Provider"]);
        Assert.AreEqual("\\\\myserver\\temp\\exports.xls", cfg["Data Source"]);
        Assert.AreEqual("Excel 8.0;HDR=YES", cfg["Extended Properties"]);
    }
}
