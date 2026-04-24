using DotNetCommons;
using FluentAssertions;

namespace DotNetCommonTests;

[TestClass]
public class CommonStringExtensionsTest_SubItems
{
    [TestMethod]
    public void AddSubItem_Works()
    {
        ((string?)null).AddSubItem('|', "foo").Should().Be("foo");
        "".AddSubItem('|', "foo").Should().Be("|foo");
        "|".AddSubItem('|', "foo").Should().Be("||foo");
        "||".AddSubItem('|', "foo").Should().Be("|||foo");
        "terry".AddSubItem('|', "foo").Should().Be("terry|foo");
        "put|it|in|reverse||terry".AddSubItem('|', "foo").Should().Be("put|it|in|reverse||terry|foo");
    }

    [TestMethod]
    public void FindSubItem_Works()
    {
        CommonStringExtensions.FindSubItemIndex("put|it|in|reverse||terry", '|', 0).Should().Be(0);
        CommonStringExtensions.FindSubItemIndex("put|it|in|reverse||terry", '|', 1).Should().Be(4);
        CommonStringExtensions.FindSubItemIndex("put|it|in|reverse||terry", '|', 2).Should().Be(7);
        CommonStringExtensions.FindSubItemIndex("put|it|in|reverse||terry", '|', 3).Should().Be(10);
        CommonStringExtensions.FindSubItemIndex("put|it|in|reverse||terry", '|', 4).Should().Be(18);
        CommonStringExtensions.FindSubItemIndex("put|it|in|reverse||terry", '|', 5).Should().Be(19);
        CommonStringExtensions.FindSubItemIndex("put|it|in|reverse||terry", '|', 6).Should().Be(-1);

        CommonStringExtensions.FindSubItemIndex("put|it|in|reverse||terry", '|', 0, 4).Should().Be(4);
        CommonStringExtensions.FindSubItemIndex("put|it|in|reverse||terry", '|', 1, 4).Should().Be(7);
        CommonStringExtensions.FindSubItemIndex("put|it|in|reverse||terry", '|', 2, 4).Should().Be(10);
        CommonStringExtensions.FindSubItemIndex("put|it|in|reverse||terry", '|', 3, 4).Should().Be(18);
        CommonStringExtensions.FindSubItemIndex("put|it|in|reverse||terry", '|', 4, 4).Should().Be(19);
        CommonStringExtensions.FindSubItemIndex("put|it|in|reverse||terry", '|', 5, 4).Should().Be(-1);

        CommonStringExtensions.FindSubItemIndex("|||", '|', 0).Should().Be(0);
        CommonStringExtensions.FindSubItemIndex("|||", '|', 1).Should().Be(1);
        CommonStringExtensions.FindSubItemIndex("|||", '|', 2).Should().Be(2);
        CommonStringExtensions.FindSubItemIndex("|||", '|', 3).Should().Be(3);
        CommonStringExtensions.FindSubItemIndex("|||", '|', 4).Should().Be(-1);
    }

    [TestMethod]
    public void GetSubItem_Works()
    {
        ((string?)null).GetSubItem('|', 0).Should().BeNull();
        ((string?)null).GetSubItem('|', 1).Should().BeNull();

        "".GetSubItem('|', 0).Should().Be("");
        "".GetSubItem('|', 1).Should().BeNull();

        "|".GetSubItem('|', 0).Should().Be("");
        "|".GetSubItem('|', 1).Should().Be("");
        "|".GetSubItem('|', 2).Should().BeNull();

        "||".GetSubItem('|', 0).Should().Be("");
        "||".GetSubItem('|', 1).Should().Be("");
        "||".GetSubItem('|', 2).Should().Be("");
        "||".GetSubItem('|', 3).Should().BeNull();

        "terry".GetSubItem('|', 0).Should().Be("terry");
        "terry".GetSubItem('|', 1).Should().Be(null);

        "put|it|in|reverse||terry".GetSubItem('|', 0).Should().Be("put");
        "put|it|in|reverse||terry".GetSubItem('|', 1).Should().Be("it");
        "put|it|in|reverse||terry".GetSubItem('|', 2).Should().Be("in");
        "put|it|in|reverse||terry".GetSubItem('|', 3).Should().Be("reverse");
        "put|it|in|reverse||terry".GetSubItem('|', 4).Should().Be("");
        "put|it|in|reverse||terry".GetSubItem('|', 5).Should().Be("terry");
        "put|it|in|reverse||terry".GetSubItem('|', 6).Should().BeNull();
    }

    [TestMethod]
    public void GetSubItemCount_Works()
    {
        ((string?)null).GetSubItemCount('|').Should().Be(0);
        "".GetSubItemCount('|').Should().Be(1);
        "|".GetSubItemCount('|').Should().Be(2);
        "||".GetSubItemCount('|').Should().Be(3);
        "terry".GetSubItemCount('|').Should().Be(1);
        "|terry".GetSubItemCount('|').Should().Be(2);
        "terry|".GetSubItemCount('|').Should().Be(2);
        "put|it|in|reverse||terry".GetSubItemCount('|').Should().Be(6);
    }

    [TestMethod]
    public void InsertSubItem_Works()
    {
        ((string?)null).InsertSubItem('|', 0, "foo").Should().Be("foo");
        ((string?)null).InsertSubItem('|', 1, "foo").Should().Be("|foo");
        ((string?)null).InsertSubItem('|', 2, "foo").Should().Be("||foo");

        "".InsertSubItem('|', 0, "foo").Should().Be("foo|");
        "".InsertSubItem('|', 1, "foo").Should().Be("|foo");
        "".InsertSubItem('|', 2, "foo").Should().Be("||foo");

        "|".InsertSubItem('|', 0, "foo").Should().Be("foo||");
        "|".InsertSubItem('|', 1, "foo").Should().Be("|foo|");
        "|".InsertSubItem('|', 2, "foo").Should().Be("||foo");

        "||".InsertSubItem('|', 0, "foo").Should().Be("foo|||");
        "||".InsertSubItem('|', 1, "foo").Should().Be("|foo||");
        "||".InsertSubItem('|', 2, "foo").Should().Be("||foo|");
        "||".InsertSubItem('|', 3, "foo").Should().Be("|||foo");

        "terry".InsertSubItem('|', 0, "foo").Should().Be("foo|terry");
        "terry".InsertSubItem('|', 1, "foo").Should().Be("terry|foo");
        "terry".InsertSubItem('|', 2, "foo").Should().Be("terry||foo");

        "put|it|in|reverse||terry".InsertSubItem('|', 0, "foo").Should().Be("foo|put|it|in|reverse||terry");
        "put|it|in|reverse||terry".InsertSubItem('|', 1, "foo").Should().Be("put|foo|it|in|reverse||terry");
        "put|it|in|reverse||terry".InsertSubItem('|', 2, "foo").Should().Be("put|it|foo|in|reverse||terry");
        "put|it|in|reverse||terry".InsertSubItem('|', 3, "foo").Should().Be("put|it|in|foo|reverse||terry");
        "put|it|in|reverse||terry".InsertSubItem('|', 4, "foo").Should().Be("put|it|in|reverse|foo||terry");
        "put|it|in|reverse||terry".InsertSubItem('|', 5, "foo").Should().Be("put|it|in|reverse||foo|terry");
        "put|it|in|reverse||terry".InsertSubItem('|', 6, "foo").Should().Be("put|it|in|reverse||terry|foo");
        "put|it|in|reverse||terry".InsertSubItem('|', 7, "foo").Should().Be("put|it|in|reverse||terry||foo");
    }

    [TestMethod]
    public void RemoveSubItem_Works()
    {
        ((string?)null).RemoveSubItem('|', 0).Should().BeNull();
        ((string?)null).RemoveSubItem('|', 1).Should().BeNull();
        ((string?)null).RemoveSubItem('|', 2).Should().BeNull();

        "".RemoveSubItem('|', 0).Should().BeNull();
        "".RemoveSubItem('|', 1).Should().Be("");
        "".RemoveSubItem('|', 2).Should().Be("");

        "|".RemoveSubItem('|', 0).Should().Be("");
        "|".RemoveSubItem('|', 1).Should().Be("");
        "|".RemoveSubItem('|', 2).Should().Be("|");

        "||".RemoveSubItem('|', 0).Should().Be("|");
        "||".RemoveSubItem('|', 1).Should().Be("|");
        "||".RemoveSubItem('|', 2).Should().Be("|");
        "||".RemoveSubItem('|', 3).Should().Be("||");

        "terry".RemoveSubItem('|', 0).Should().BeNull();
        "terry".RemoveSubItem('|', 1).Should().Be("terry");
        "terry".RemoveSubItem('|', 2).Should().Be("terry");

        "put|it|in|reverse||terry".RemoveSubItem('|', 0).Should().Be("it|in|reverse||terry");
        "put|it|in|reverse||terry".RemoveSubItem('|', 1).Should().Be("put|in|reverse||terry");
        "put|it|in|reverse||terry".RemoveSubItem('|', 2).Should().Be("put|it|reverse||terry");
        "put|it|in|reverse||terry".RemoveSubItem('|', 3).Should().Be("put|it|in||terry");
        "put|it|in|reverse||terry".RemoveSubItem('|', 4).Should().Be("put|it|in|reverse|terry");
        "put|it|in|reverse||terry".RemoveSubItem('|', 5).Should().Be("put|it|in|reverse|");
        "put|it|in|reverse||terry".RemoveSubItem('|', 6).Should().Be("put|it|in|reverse||terry");
        "put|it|in|reverse||terry".RemoveSubItem('|', 7).Should().Be("put|it|in|reverse||terry");
    }

    [TestMethod]
    public void SetSubItem_Works()
    {
        ((string?)null).SetSubItem('|', 0, "foo").Should().Be("foo");
        ((string?)null).SetSubItem('|', 1, "foo").Should().Be("|foo");
        ((string?)null).SetSubItem('|', 2, "foo").Should().Be("||foo");

        "".SetSubItem('|', 0, "foo").Should().Be("foo");
        "".SetSubItem('|', 1, "foo").Should().Be("|foo");
        "".SetSubItem('|', 2, "foo").Should().Be("||foo");

        "|".SetSubItem('|', 0, "foo").Should().Be("foo|");
        "|".SetSubItem('|', 1, "foo").Should().Be("|foo");
        "|".SetSubItem('|', 2, "foo").Should().Be("||foo");

        "||".SetSubItem('|', 0, "foo").Should().Be("foo||");
        "||".SetSubItem('|', 1, "foo").Should().Be("|foo|");
        "||".SetSubItem('|', 2, "foo").Should().Be("||foo");
        "||".SetSubItem('|', 3, "foo").Should().Be("|||foo");

        "terry".SetSubItem('|', 0, "foo").Should().Be("foo");
        "terry".SetSubItem('|', 1, "foo").Should().Be("terry|foo");
        "terry".SetSubItem('|', 2, "foo").Should().Be("terry||foo");

        "put|it|in|reverse||terry".SetSubItem('|', 0, "foo").Should().Be("foo|it|in|reverse||terry");
        "put|it|in|reverse||terry".SetSubItem('|', 1, "foo").Should().Be("put|foo|in|reverse||terry");
        "put|it|in|reverse||terry".SetSubItem('|', 2, "foo").Should().Be("put|it|foo|reverse||terry");
        "put|it|in|reverse||terry".SetSubItem('|', 3, "foo").Should().Be("put|it|in|foo||terry");
        "put|it|in|reverse||terry".SetSubItem('|', 4, "foo").Should().Be("put|it|in|reverse|foo|terry");
        "put|it|in|reverse||terry".SetSubItem('|', 5, "foo").Should().Be("put|it|in|reverse||foo");
        "put|it|in|reverse||terry".SetSubItem('|', 6, "foo").Should().Be("put|it|in|reverse||terry|foo");
        "put|it|in|reverse||terry".SetSubItem('|', 7, "foo").Should().Be("put|it|in|reverse||terry||foo");

        "put|it|in|reverse||terry".SetSubItem('|', 0, "").Should().Be("|it|in|reverse||terry");
        "put|it|in|reverse||terry".SetSubItem('|', 1, "").Should().Be("put||in|reverse||terry");
        "put|it|in|reverse||terry".SetSubItem('|', 2, "").Should().Be("put|it||reverse||terry");
        "put|it|in|reverse||terry".SetSubItem('|', 3, "").Should().Be("put|it|in|||terry");
        "put|it|in|reverse||terry".SetSubItem('|', 4, "").Should().Be("put|it|in|reverse||terry");
        "put|it|in|reverse||terry".SetSubItem('|', 5, "").Should().Be("put|it|in|reverse||");
        "put|it|in|reverse||terry".SetSubItem('|', 6, "").Should().Be("put|it|in|reverse||terry|");
        "put|it|in|reverse||terry".SetSubItem('|', 7, "").Should().Be("put|it|in|reverse||terry||");
    }
}
