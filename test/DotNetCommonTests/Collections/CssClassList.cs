using DotNetCommons.Collections;

namespace DotNetCommonTests.Collections;

[TestClass]
public class CssClassListTests
{
    [TestMethod]
    public void Add_Works()
    {
        var classList = new CssClassList("btn");

        classList.Add((string?)null);
        Assert.AreEqual("btn", classList.Text);

        classList.Add("btn");
        Assert.AreEqual("btn", classList.Text);

        classList.Add("btn  primary large  ");
        Assert.AreEqual("btn large primary", classList.Text);
    }

    [TestMethod]
    public void Remove_Works()
    {
        var classList = new CssClassList("btn primary xl");

        classList.Add((string?)null);
        Assert.AreEqual("btn primary xl", classList.Text);

        classList.Remove("btn");
        Assert.AreEqual("primary xl", classList.ToString());

        classList.Remove(" a b c xl ");
        Assert.AreEqual("primary", classList.ToString());
    }

    [TestMethod]
    public void Clear_Works()
    {
        var classList = new CssClassList("btn primary");

        classList.Clear();
        Assert.AreEqual("", classList.Text);
    }

    [TestMethod]
    public void Contains_Works()
    {
        var classList = new CssClassList("btn primary");

        Assert.IsFalse(classList.Contains((string?)null));
        Assert.IsFalse(classList.Contains(""));
        Assert.IsTrue(classList.Contains("btn"));
        Assert.IsTrue(classList.Contains(" primary "));
        Assert.IsTrue(classList.Contains("primary btn"));
        Assert.IsFalse(classList.Contains("xl"));
    }

    [TestMethod]
    public void Toggle_Works()
    {
        var classList = new CssClassList("btn primary");
        Assert.AreEqual("btn primary", classList.Text);

        classList.Toggle("xl btn");
        Assert.AreEqual("primary xl", classList.Text);

        classList.Toggle("xl");
        Assert.AreEqual("primary", classList.Text);
    }
}