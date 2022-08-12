using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Linq;

namespace DotNetCommons.Test;

[TestClass]
public class CommonStringExtensionsTest
{
    [TestMethod]
    public void TestBreakUp()
    {
        CollectionAssert.AreEqual(Array.Empty<string>(), ((string?)null).BreakUp(5).ToArray());
        CollectionAssert.AreEqual(Array.Empty<string>(), "".BreakUp(5).ToArray());

        CollectionAssert.AreEqual(new[] { "A" }, "A".BreakUp(5).ToArray());
        CollectionAssert.AreEqual(new[] { "ABC" }, "ABC".BreakUp(5).ToArray());
        CollectionAssert.AreEqual(new[] { "ABCDE" }, "ABCDE".BreakUp(5).ToArray());

        CollectionAssert.AreEqual(new[] { "ABCDE", "A" }, "ABCDEA".BreakUp(5).ToArray());
        CollectionAssert.AreEqual(new[] { "ABCDE", "ABCDE" }, "ABCDEABCDE".BreakUp(5).ToArray());
        CollectionAssert.AreEqual(new[] { "ABCDE", "ABCDE", "A" }, "ABCDEABCDEA".BreakUp(5).ToArray());
    }

    [TestMethod]
    public void TestChomp()
    {
        string? remaining;

        Assert.IsNull(null, ((string?)null).Chomp(out remaining));
        Assert.AreEqual("", remaining);

        Assert.AreEqual(null, "".Chomp(out remaining));
        Assert.AreEqual("", remaining);

        Assert.AreEqual("A", "A".Chomp(out remaining));
        Assert.AreEqual("", remaining);

        Assert.AreEqual("Two", "Two happy birds".Chomp(out remaining));
        Assert.AreEqual("happy birds", remaining);

        Assert.AreEqual("Two happy", "'Two happy' birds".Chomp(out remaining, ' ', '\''));
        Assert.AreEqual("birds", remaining);

        Assert.AreEqual("Two happy birds", "'Two happy birds'".Chomp(out remaining, ' ', '\''));
        Assert.AreEqual("", remaining);

        Assert.AreEqual("x='two'", "x='two' y=5".Chomp(out remaining, ' ', '\''));
        Assert.AreEqual("y=5", remaining);
    }

    [TestMethod]
    public void TestChompAll()
    {
        CollectionAssert.AreEqual(
            Array.Empty<string>(),
            ((string?)null).ChompAll().ToArray());

        CollectionAssert.AreEqual(
            new[] { "A", "B", "C" },
            "A B C".ChompAll().ToArray());

        CollectionAssert.AreEqual(
            new[] { "node=\"A B C\"", "five", "two-zero", "x=3", "one to", "x=\"223 456\"" },
            "  node=\"A B C\" five two-zero x=3 \"one to\" x=\"223 456\"       ".ChompAll().ToArray());
    }

    [TestMethod]
    public void TestLeft()
    {
        Assert.AreEqual("", "".Left(5));
        Assert.AreEqual("", ((string?)null).Left(5));

        Assert.AreEqual("", "ABCD".Left(0));
        Assert.AreEqual("AB", "ABCD".Left(2));
        Assert.AreEqual("ABCD", "ABCD".Left(4));
        Assert.AreEqual("ABCD", "ABCD".Left(10));
    }

    [TestMethod]
    public void TestEqualsInsensitive()
    {
        Assert.IsTrue(((string?)null).EqualsInsensitive(null));
        Assert.IsFalse(((string?)null).EqualsInsensitive(""));
        Assert.IsFalse(((string?)null).EqualsInsensitive("AB"));

        Assert.IsFalse("".EqualsInsensitive(null));
        Assert.IsTrue("".EqualsInsensitive(""));
        Assert.IsFalse("".EqualsInsensitive("AB"));

        Assert.IsFalse("Abcd".EqualsInsensitive(null));
        Assert.IsFalse("Abcd".EqualsInsensitive(""));
        Assert.IsFalse("Abcd".EqualsInsensitive("AB"));

        Assert.IsTrue("Abcd".EqualsInsensitive("abcd"));
        Assert.IsTrue("Abcd".EqualsInsensitive("Abcd"));
        Assert.IsTrue("Abcd".EqualsInsensitive("ABCD"));
    }

    [TestMethod]
    public void TestMid()
    {
        Assert.AreEqual("", "".Mid(0));
        Assert.AreEqual("", "".Mid(5));
        Assert.AreEqual("", "".Mid(0, 1));
        Assert.AreEqual("", "".Mid(5, 1));

        Assert.AreEqual("", ((string?)null).Mid(0));
        Assert.AreEqual("", ((string?)null).Mid(5));
        Assert.AreEqual("", ((string?)null).Mid(0, 1));
        Assert.AreEqual("", ((string?)null).Mid(5, 1));

        Assert.AreEqual("ABCD", "ABCD".Mid(0));
        Assert.AreEqual("CD", "ABCD".Mid(2));
        Assert.AreEqual("", "ABCD".Mid(4));
        Assert.AreEqual("", "ABCD".Mid(6));

        Assert.AreEqual("AB", "ABCD".Mid(0, 2));
        Assert.AreEqual("CD", "ABCD".Mid(2, 2));
        Assert.AreEqual("", "ABCD".Mid(4, 2));
        Assert.AreEqual("", "ABCD".Mid(6, 2));

        Assert.AreEqual("ABCD", "ABCD".Mid(0, 10));
        Assert.AreEqual("CD", "ABCD".Mid(2, 10));
        Assert.AreEqual("", "ABCD".Mid(4, 10));
        Assert.AreEqual("", "ABCD".Mid(6, 10));
    }

    [TestMethod]
    public void TestParseDecimal()
    {
        Assert.AreEqual(4711.57M, "4711,57".ParseDecimal(CultureInfo.GetCultureInfo("sv-SE")));
        Assert.AreEqual(0.0M, "mike".ParseDecimal());
        Assert.AreEqual(2.3M, "mike".ParseDecimal(2.3M));
    }

    [TestMethod]
    public void TestParseDecimalInvariant()
    {
        Assert.AreEqual(4711.57M, "4711.57".ParseDecimalInvariant());
        Assert.AreEqual(0.0M, "mike".ParseDecimalInvariant());
        Assert.AreEqual(2.3M, "mike".ParseDecimalInvariant(2.3M));
    }

    [TestMethod]
    public void TestParseDouble()
    {
        Assert.AreEqual(4711.57, "4711,57".ParseDouble(CultureInfo.GetCultureInfo("sv-SE")));
        Assert.AreEqual(0.0, "mike".ParseDouble());
        Assert.AreEqual(2.3, "mike".ParseDouble(2.3));
    }

    [TestMethod]
    public void TestParseDoubleInvariant()
    {
        Assert.AreEqual(4711.57, "4711.57".ParseDoubleInvariant());
        Assert.AreEqual(0.0, "mike".ParseDoubleInvariant());
        Assert.AreEqual(2.3, "mike".ParseDoubleInvariant(2.3));
    }

    [TestMethod]
    public void TestParseInt()
    {
        Assert.AreEqual(4711, "4711".ParseInt());
        Assert.AreEqual(0, "mike".ParseInt());
        Assert.AreEqual(2, "mike".ParseInt(2));
    }

    [TestMethod]
    public void TestParseLong()
    {
        Assert.AreEqual(4711L, "4711".ParseLong());
        Assert.AreEqual(0L, "mike".ParseLong());
        Assert.AreEqual(2L, "mike".ParseLong(2));
    }

    [TestMethod]
    public void TestRight()
    {
        Assert.AreEqual("", "".Right(5));
        Assert.AreEqual("", ((string?)null).Right(5));

        Assert.AreEqual("", "ABCD".Right(0));
        Assert.AreEqual("CD", "ABCD".Right(2));
        Assert.AreEqual("ABCD", "ABCD".Right(4));
        Assert.AreEqual("ABCD", "ABCD".Right(10));
    }
}