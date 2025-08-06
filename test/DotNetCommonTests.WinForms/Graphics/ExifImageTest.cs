using System.Reflection;
using DotNetCommons.WinForms.Graphics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommonTests.WinForms.Graphics;

[TestClass]
public class ExifImageTest
{
    private ExifImage _img = null!;

    [TestInitialize]
    public void Setup()
    {
        var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(GetType(), "test.jpg");
        _img = new ExifImage(resource);
    }

    [TestMethod]
    public void Test()
    {
        Assert.AreEqual("This is a title.", _img.Title);
        Assert.AreEqual("Subject.", _img.Subject);
        Assert.AreEqual((short)4, _img.Rating);
        Assert.AreEqual("Random comments.", _img.Comments);
        Assert.AreEqual("@tag1; @tag2; @tag3", _img.TagAsText);

        _img.Title = "Bork!";
        _img.Tags = new[] { "hello", "bonk" };
        _img.Rating = 5;
        _img.Subject = null;

        using var mem = new MemoryStream();

        _img.Save(mem);

        mem.Position = 0;
        var x = new ExifImage(mem);

        Assert.AreEqual("Bork!", x.Title);
        Assert.IsFalse(x.Exists(ExifTags.XpSubject));
        Assert.IsNull(x.Subject);
        Assert.AreEqual((short)5, x.Rating);
        Assert.AreEqual("hello; bonk", x.TagAsText);
    }
}