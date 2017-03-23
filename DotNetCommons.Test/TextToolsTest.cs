using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test
{
    [TestClass]
    public class TextToolsTest
    {
        [TestMethod]
        public void TestDetermineEncoding()
        {
            Assert.AreEqual(Encoding.ASCII, TextTools.DetermineEncoding(Encoding.Default.GetBytes("Abcdefgh")));
            Assert.AreEqual(Encoding.Default, TextTools.DetermineEncoding(Encoding.Default.GetBytes("Åäurg!")));
            Assert.AreEqual(Encoding.UTF8, TextTools.DetermineEncoding(Encoding.UTF8.GetBytes("Åäurg!")));
        }

        [TestMethod]
        public void TestLevenshtein()
        {
            Assert.AreEqual(0, TextTools.Levenshtein("abcd", "abcd", false));
            Assert.AreEqual(1, TextTools.Levenshtein("abcd", "Abcd", false));
            Assert.AreEqual(0, TextTools.Levenshtein("abcd", "Abcd", true));
            Assert.AreEqual(4, TextTools.Levenshtein("abcd", "ABCD", false));

            Assert.AreEqual(4, TextTools.Levenshtein("abcd", "", false));
            Assert.AreEqual(4, TextTools.Levenshtein("abcd", null, false));
            Assert.AreEqual(0, TextTools.Levenshtein("", "", false));
        }

        [TestMethod]
        public void WordWrap()
        {
        }
    }
}
