using System.Globalization;
using DotNetCommons;
using FluentAssertions;

namespace DotNetCommonTests;

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
    public void TestConvertToCrLf()
    {
        ((string?)null).ConvertToCrLf().Should().BeNull();
        "".ConvertToCrLf().Should().Be("");
        "\n".ConvertToCrLf().Should().Be("\r\n");
        "\n\n".ConvertToCrLf().Should().Be("\r\n\r\n");
        "\r\n\r\n".ConvertToCrLf().Should().Be("\r\n\r\n");
        "This is\na mix\r\nof different\nformats.\r\n".ConvertToCrLf().Should().Be("This is\r\na mix\r\nof different\r\nformats.\r\n");
    }

    [TestMethod]
    public void TestConvertToLf()
    {
        ((string?)null).ConvertToLf().Should().BeNull();
        "".ConvertToLf().Should().Be("");
        "\n".ConvertToLf().Should().Be("\n");
        "\n\n".ConvertToLf().Should().Be("\n\n");
        "\r\n\r\n".ConvertToLf().Should().Be("\n\n");
        "This is\na mix\r\nof different\nformats.\r\n".ConvertToLf().Should().Be("This is\na mix\nof different\nformats.\n");
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
    public void TestIsEmpty()
    {
        Assert.IsTrue(((string?)null).IsEmpty());
        Assert.IsTrue("".IsEmpty());
        Assert.IsFalse("X".IsEmpty());
    }

    [TestMethod]
    public void TestIsSet()
    {
        Assert.IsFalse(((string?)null).IsSet());
        Assert.IsFalse("".IsSet());
        Assert.IsTrue("X".IsSet());
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
    public void TestLeftEllipsis()
    {
        Assert.AreEqual("", "".LeftEllipsis(5));
        Assert.AreEqual("", ((string?)null).LeftEllipsis(5));

        Assert.AreEqual("", "ABCD".LeftEllipsis(0));
        Assert.AreEqual("…", "ABCD".LeftEllipsis(1));
        Assert.AreEqual("A…", "ABCD".LeftEllipsis(2));
        Assert.AreEqual("AB…", "ABCD".LeftEllipsis(3));
        Assert.AreEqual("ABCD", "ABCD".LeftEllipsis(4));
        Assert.AreEqual("ABCD", "ABCD".LeftEllipsis(5));
        Assert.AreEqual("ABCD", "ABCD".LeftEllipsis(10));
    }

    [TestMethod]
    public void TestMaskLeft()
    {
        Assert.AreEqual("", "".MaskLeft(4, '*'));
        Assert.AreEqual("*", "1".MaskLeft(4, '*'));
        Assert.AreEqual("****", "1234".MaskLeft(4, '*'));
        Assert.AreEqual("**** 5678", "1234 5678".MaskLeft(4, '*'));

        Assert.AreEqual("*234 5678 9ABC", "1234 5678 9ABC".MaskLeft(1, '*'));
        Assert.AreEqual("**34 5678 9ABC", "1234 5678 9ABC".MaskLeft(2, '*'));
        Assert.AreEqual("***4 5678 9ABC", "1234 5678 9ABC".MaskLeft(3, '*'));
        Assert.AreEqual("**** 5678 9ABC", "1234 5678 9ABC".MaskLeft(4, '*'));
        Assert.AreEqual("*****5678 9ABC", "1234 5678 9ABC".MaskLeft(5, '*'));
    }

    [TestMethod]
    public void TestMaskRight()
    {
        Assert.AreEqual("", "".MaskRight(4, '*'));
        Assert.AreEqual("*", "1".MaskRight(4, '*'));
        Assert.AreEqual("****", "1234".MaskRight(4, '*'));
        Assert.AreEqual("1234 ****", "1234 5678".MaskRight(4, '*'));

        Assert.AreEqual("1234 5678 9AB*", "1234 5678 9ABC".MaskRight(1, '*'));
        Assert.AreEqual("1234 5678 9A**", "1234 5678 9ABC".MaskRight(2, '*'));
        Assert.AreEqual("1234 5678 9***", "1234 5678 9ABC".MaskRight(3, '*'));
        Assert.AreEqual("1234 5678 ****", "1234 5678 9ABC".MaskRight(4, '*'));
        Assert.AreEqual("1234 5678*****", "1234 5678 9ABC".MaskRight(5, '*'));
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

        Assert.AreEqual("ABCDef", "ABCDefgh".Mid(0, -2));
        Assert.AreEqual("", "ABCDefgh".Mid(0, -10));
        Assert.AreEqual("", "ABCDefgh".Mid(4, -10));
        Assert.AreEqual("efgh", "ABCDefgh".Mid(4, -4));
    }

    [TestMethod]
    public void TestNormalizeSpacing()
    {
        "\r\nThis\t is a test      of spacing ".NormalizeSpacing().Should().Be("This is a test of spacing");
        "\r\nThis\t is a test      of spacing ".NormalizeSpacing(Spacing.Trim).Should().Be("This\t is a test      of spacing");
        "\r\nThis\t is a test      of spacing ".NormalizeSpacing(Spacing.ReplaceDoubleSpaces).Should().Be("\r\nThis\t is a test of spacing ");
        "\r\nThis\t is a test      of spacing ".NormalizeSpacing(Spacing.ReplaceTabs).Should().Be("\r\nThis  is a test      of spacing ");
        "\r\nThis\t is a test      of spacing ".NormalizeSpacing(Spacing.ReplaceCrLfs).Should().Be(" This\t is a test      of spacing ");
    }

    [TestMethod]
    public void TestParseBoolean()
    {
        Assert.IsTrue("1".ParseBoolean());
        Assert.IsTrue("-1".ParseBoolean());
        Assert.IsTrue("999".ParseBoolean());
        Assert.IsTrue(int.MaxValue.ToString().ParseBoolean());
        Assert.IsTrue(long.MaxValue.ToString().ParseBoolean());
        Assert.IsTrue(int.MinValue.ToString().ParseBoolean());
        Assert.IsTrue(long.MinValue.ToString().ParseBoolean());
        Assert.IsTrue("TRUE".ParseBoolean());
        Assert.IsTrue("True".ParseBoolean());
        Assert.IsTrue("true".ParseBoolean());
        Assert.IsTrue("T".ParseBoolean());
        Assert.IsTrue("t".ParseBoolean());
        Assert.IsTrue("YES".ParseBoolean());
        Assert.IsTrue("Yes".ParseBoolean());
        Assert.IsTrue("yes".ParseBoolean());
        Assert.IsTrue("Y".ParseBoolean());
        Assert.IsTrue("y".ParseBoolean());

        Assert.IsFalse("0".ParseBoolean());
        Assert.IsFalse("FALSE".ParseBoolean());
        Assert.IsFalse("False".ParseBoolean());
        Assert.IsFalse("false".ParseBoolean());
        Assert.IsFalse("F".ParseBoolean());
        Assert.IsFalse("f".ParseBoolean());
        Assert.IsFalse("NO".ParseBoolean());
        Assert.IsFalse("No".ParseBoolean());
        Assert.IsFalse("no".ParseBoolean());
        Assert.IsFalse("N".ParseBoolean());
        Assert.IsFalse("n".ParseBoolean());
        Assert.IsFalse("0".ParseBoolean());

        Assert.IsNull("".ParseBoolean());
        Assert.IsNull(((string?)null).ParseBoolean());
        Assert.IsNull("X".ParseBoolean());
        Assert.IsNull("FALSY".ParseBoolean());
        Assert.IsNull("falsy".ParseBoolean());
        Assert.IsNull("3.14".ParseBoolean());
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

    [TestMethod]
    public void TestStartLowerCase()
    {
        Assert.IsNull(((string?)null).StartLowerCase());
        Assert.AreEqual("", "".StartLowerCase());
        Assert.AreEqual("x", "x".StartLowerCase());
        Assert.AreEqual("x", "X".StartLowerCase());
        Assert.AreEqual("1234", "1234".StartLowerCase());
        Assert.AreEqual("word", "Word".StartLowerCase());
        Assert.AreEqual("word", "word".StartLowerCase());
        Assert.AreEqual("wORD", "WORD".StartLowerCase());
    }

    [TestMethod]
    public void TestStartUpperCase()
    {
        Assert.IsNull(((string?)null).StartUpperCase());
        Assert.AreEqual("", "".StartUpperCase());
        Assert.AreEqual("X", "x".StartUpperCase());
        Assert.AreEqual("X", "X".StartUpperCase());
        Assert.AreEqual("1234", "1234".StartUpperCase());
        Assert.AreEqual("Word", "Word".StartUpperCase());
        Assert.AreEqual("Word", "word".StartUpperCase());
        Assert.AreEqual("WORD", "WORD".StartUpperCase());
    }
}