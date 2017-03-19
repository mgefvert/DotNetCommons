using System;
using CommonNetTools.IO.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonNetTools.IO.Test.Parsers
{
    [TestClass]
    public class CsvParserTest
    {
        [TestMethod]
        public void Test()
        {
            const string src = @",1, 2, abc, 'hello world',3,  'hello\' again' , 1  2 3";

            var result = CsvParser.Parse(Cvt(src));
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(",1,2,abc,hello world,3,hello\" again,1  2 3", string.Join(",", result[0]));
        }

        [TestMethod]
        public void TestEscape()
        {
            var result = CsvParser.Parse(Cvt(@"'\''"));
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("\"", string.Join(",", result[0]));
        }

        private string Cvt(string text)
        {
            return text.Replace('\'', '"');
        }
    }
}
