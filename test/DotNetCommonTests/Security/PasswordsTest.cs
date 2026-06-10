using DotNetCommons.Security;

namespace DotNetCommonTests.Security;

[TestClass]
public class PasswordsTest
{
    [TestMethod]
    public void ComputeHash_Works()
    {
        var salt   = Enumerable.Range(0, 16).Select(x => (byte)x).ToArray();
        var result = Passwords.ComputeHash("abcABC123", salt, 10);

        Assert.AreEqual(
            "CgABAgMEBQYHCAkKCwwNDg9AAAAAUcjUY18dzxHEYZHI6+pLzL9VQ9GIiZDF7XkSO2MQ0dlplMRCJYAJF2N843/iSgHSYUoL3yB/yAwc2L3h4l2q6A==",
            result);
    }

    [TestMethod]
    public void GeneratePasswordTest()
    {
        var pw = Passwords.GeneratePassword("A3-N2-A1");

        Assert.IsTrue(char.IsLetter(pw[0]));
        Assert.IsTrue(char.IsLetter(pw[1]));
        Assert.IsTrue(char.IsLetter(pw[2]));
        Assert.AreEqual('-', pw[3]);
        Assert.IsTrue(char.IsDigit(pw[4]));
        Assert.IsTrue(char.IsDigit(pw[5]));
        Assert.AreEqual('-', pw[6]);
        Assert.IsTrue(char.IsLetter(pw[7]));

        var pw2 = Passwords.GeneratePassword("A13", 3);

        Assert.HasCount(3, pw2);
        Assert.IsTrue(pw2.All(x => x.Length == 13));

        Assert.AreNotEqual(pw2[0], pw2[1]);
        Assert.AreNotEqual(pw2[0], pw2[1]);
        Assert.AreNotEqual(pw2[1], pw2[2]);
    }
}