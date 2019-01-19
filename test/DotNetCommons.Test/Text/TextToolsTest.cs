using System;
using System.Text;
using DotNetCommons.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.Text
{
    [TestClass]
    public class TextToolsTest
    {
        [TestMethod]
        public void TestAsciify()
        {
            Assert.AreEqual("Abcdefgh!", TextTools.Asciify("Abcdefgh!"));
            Assert.AreEqual("Aaeurg!", TextTools.Asciify("Åäurg!"));
            Assert.AreEqual("(c) 1993", TextTools.Asciify("© 1993"));
        }

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
            var s =
                //            1    1    2    2    3    3    4
                //   1---5----0----5----0----5----0----5----0
                    "This is a random string. This is a " + 
                    "random string. This is a random string. " +
                    "This is a random string. This is the " +
                    "end.";

            var lines = TextTools.WordWrap(s, 40);
            Assert.AreEqual(4, lines.Count);
            Assert.AreEqual("This is a random string. This is a", lines[0]);
            Assert.AreEqual("random string. This is a random string.", lines[1]);
            Assert.AreEqual("This is a random string. This is the", lines[2]);
            Assert.AreEqual("end.", lines[3]);

            lines = TextTools.WordWrap(s, 40, 10);
            Assert.AreEqual(5, lines.Count);
            Assert.AreEqual("This is a random string. This is a", lines[0]);
            Assert.AreEqual("          random string. This is a", lines[1]);
            Assert.AreEqual("          random string. This is a", lines[2]);
            Assert.AreEqual("          random string. This is the", lines[3]);
            Assert.AreEqual("          end.", lines[4]);

            lines = TextTools.WordWrap(s, 40, -10);
            Assert.AreEqual(4, lines.Count);
            Assert.AreEqual("          This is a random string. This", lines[0]);
            Assert.AreEqual("is a random string. This is a random", lines[1]);
            Assert.AreEqual("string. This is a random string. This", lines[2]);
            Assert.AreEqual("is the end.", lines[3]);
        }
    }
}
