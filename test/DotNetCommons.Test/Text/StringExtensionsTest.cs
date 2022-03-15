using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DotNetCommons.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.Text
{
    [TestClass]
    public class StringExtensionsTest
    {
        [TestMethod]
        public void TestBreakUp()
        {
            CollectionAssert.AreEqual(Array.Empty<string>(), ((string)null).BreakUp(5).ToArray());
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
            string result;

            Assert.IsNull(null, ((string)null).Chomp(out result));
            Assert.AreEqual(null, result);

            Assert.AreEqual("", "".Chomp(out result));
            Assert.AreEqual(null, result);

            Assert.AreEqual("A", "A".Chomp(out result));
            Assert.AreEqual("", result);

            Assert.AreEqual("Two", "Two happy birds".Chomp(out result));
            Assert.AreEqual("happy birds", result);

            Assert.AreEqual("Two happy", "'Two happy' birds".Chomp(out result, ' ', '\''));
            Assert.AreEqual("birds", result);

            Assert.AreEqual("Two happy birds", "'Two happy birds'".Chomp(out result, ' ', '\''));
            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void TestLeft()
        {
            Assert.AreEqual("", "".Left(5));
            Assert.AreEqual("", ((string)null).Left(5));

            Assert.AreEqual("", "ABCD".Left(0));
            Assert.AreEqual("AB", "ABCD".Left(2));
            Assert.AreEqual("ABCD", "ABCD".Left(4));
            Assert.AreEqual("ABCD", "ABCD".Left(10));
        }

        [TestMethod]
        public void TestLike()
        {
            Assert.IsTrue(((string)null).Like(null));
            Assert.IsFalse(((string)null).Like(""));
            Assert.IsFalse(((string)null).Like("AB"));

            Assert.IsFalse("".Like(null));
            Assert.IsTrue("".Like(""));
            Assert.IsFalse("".Like("AB"));

            Assert.IsFalse("Abcd".Like(null));
            Assert.IsFalse("Abcd".Like(""));
            Assert.IsFalse("Abcd".Like("AB"));

            Assert.IsTrue("Abcd".Like("abcd"));
            Assert.IsTrue("Abcd".Like("Abcd"));
            Assert.IsTrue("Abcd".Like("ABCD"));
        }

        [TestMethod]
        public void TestMid()
        {
            Assert.AreEqual("", "".Mid(0));
            Assert.AreEqual("", "".Mid(5));
            Assert.AreEqual("", "".Mid(0, 1));
            Assert.AreEqual("", "".Mid(5, 1));

            Assert.AreEqual("", ((string)null).Mid(0));
            Assert.AreEqual("", ((string)null).Mid(5));
            Assert.AreEqual("", ((string)null).Mid(0, 1));
            Assert.AreEqual("", ((string)null).Mid(5, 1));

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
            Assert.AreEqual("", ((string)null).Right(5));

            Assert.AreEqual("", "ABCD".Right(0));
            Assert.AreEqual("CD", "ABCD".Right(2));
            Assert.AreEqual("ABCD", "ABCD".Right(4));
            Assert.AreEqual("ABCD", "ABCD".Right(10));
        }
    }
}
