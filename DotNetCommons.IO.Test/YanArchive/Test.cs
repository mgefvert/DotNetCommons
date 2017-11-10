using System;
using System.IO;
using System.Text;
using DotNetCommons.IO.YanArchive;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.IO.Test.YanArchive
{
    [TestClass]
    public class Test
    {
        private readonly byte[] _testBytes;

        public Test()
        {
            var sb = new StringBuilder();
            for (var i = 0; i < 100; i++)
                sb.Append("ABCD1234");

            _testBytes = Encoding.ASCII.GetBytes(sb.ToString());
        }

        [TestInitialize]
        public void Setup()
        {
            File.Delete("test.dat");
        }

        [TestMethod]
        public void TestUnencrypted()
        {
            RunTest(null);
        }

        [TestMethod]
        public void TestEncrypted()
        {
            RunTest(Encoding.ASCII.GetBytes("My tiny little gerbil"));
        }

        public void RunTest(byte[] password)
        {
            using (var fs = new YanFileSystem("test.dat", password))
            {
                fs.Add("normal.html", _testBytes);

                Assert.AreEqual(1, fs.Files.Count);
                Assert.AreEqual("normal.html", fs.Files[0].Name);
                Assert.AreEqual(800, fs.Files[0].Size);
                Assert.IsTrue(fs.Files[0].SizeOnDisk == 800 || fs.Files[0].SizeOnDisk == 816);
                Assert.AreEqual(YanFileFlags.None, fs.Files[0].Flags);
                Assert.AreEqual(16, fs.Files[0].Position);
                CollectionAssert.AreEqual(_testBytes, ((MemoryStream)fs.Load("normal.html")).ToArray());

                fs.Add("packed.html", _testBytes, YanFileFlags.GZip);

                Assert.AreEqual(2, fs.Files.Count);
                Assert.AreEqual("packed.html", fs.Files[1].Name);
                Assert.AreEqual(800, fs.Files[1].Size);
                Assert.IsTrue(fs.Files[1].SizeOnDisk < 800);
                Assert.AreEqual(YanFileFlags.GZip, fs.Files[1].Flags);
                Assert.IsTrue(fs.Files[1].Position == 816 || fs.Files[1].Position == 832);
                CollectionAssert.AreEqual(_testBytes, ((MemoryStream)fs.Load("packed.html")).ToArray());

                fs.Delete("normal.html");

                Assert.AreEqual(2, fs.Files.Count);
                Assert.AreEqual(YanFileFlags.Deleted, fs.Files[0].Flags);
                Assert.AreEqual(YanFileFlags.GZip, fs.Files[1].Flags);

                fs.Pack();

                Assert.AreEqual(1, fs.Files.Count);
                Assert.AreEqual("packed.html", fs.Files[0].Name);
                Assert.AreEqual(800, fs.Files[0].Size);
                Assert.IsTrue(fs.Files[0].SizeOnDisk < 800);
                Assert.AreEqual(YanFileFlags.GZip, fs.Files[0].Flags);
                Assert.AreEqual(16, fs.Files[0].Position);
                CollectionAssert.AreEqual(_testBytes, ((MemoryStream)fs.Load("packed.html")).ToArray());
            }

            using (var fs = new YanFileSystem("test.dat", password))
            {
                Assert.AreEqual(1, fs.Files.Count);
                Assert.AreEqual("packed.html", fs.Files[0].Name);
                Assert.AreEqual(800, fs.Files[0].Size);
                Assert.IsTrue(fs.Files[0].SizeOnDisk < 800);
                Assert.AreEqual(YanFileFlags.GZip, fs.Files[0].Flags);
                Assert.AreEqual(16, fs.Files[0].Position);
                CollectionAssert.AreEqual(_testBytes, ((MemoryStream)fs.Load("packed.html")).ToArray());

                fs.DeleteArchive();
            }
        }
    }
}
