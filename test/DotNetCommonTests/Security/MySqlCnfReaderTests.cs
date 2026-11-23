using System.Security.Cryptography;
using System.Text;
using DotNetCommons.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommonTests.Security;

[TestClass]
public sealed class MySqlCnfReaderTests
{
    [TestMethod]
    public void LoginFileCanBeLoadedWithoutWriteAccess()
    {
        var path = Path.Combine(Path.GetTempPath(), "mysql-login-test-" + Guid.NewGuid().ToString("N"));
        var loginKey = RandomNumberGenerator.GetBytes(20);
        var key = new byte[16];
        for (var i = 0; i < loginKey.Length; i++) key[i % 16] ^= loginKey[i];
        try
        {
            using (var writer = new BinaryWriter(File.Create(path)))
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                writer.Write(0);
                writer.Write(loginKey);
                foreach (var line in new[] { "[test]", "host = localhost", "user = tester", "password = \"test-only\"" })
                {
                    var bytes = aes.EncryptEcb(Encoding.UTF8.GetBytes(line), PaddingMode.PKCS7);
                    writer.Write(bytes.Length);
                    writer.Write(bytes);
                }
            }
            // On Windows this sharing mode rejects a reader requesting read/write access.
            // Read-only file mode additionally covers ordinary non-root Unix users.
            if (OperatingSystem.IsWindows()) File.SetAttributes(path, FileAttributes.ReadOnly);
            else File.SetUnixFileMode(path, UnixFileMode.UserRead);
            using var shared = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            var reader = new MySqlCnfReader();
            Assert.IsTrue(reader.Load(path));
            Assert.AreEqual("localhost", reader.Entries["test"].Host);
            Assert.AreEqual("tester", reader.Entries["test"].User);
            Assert.AreEqual("test-only", reader.Entries["test"].Password);
        }
        finally
        {
            if (File.Exists(path))
            {
                if (OperatingSystem.IsWindows()) File.SetAttributes(path, FileAttributes.Normal);
                else File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
                File.Delete(path);
            }
        }
    }
}
