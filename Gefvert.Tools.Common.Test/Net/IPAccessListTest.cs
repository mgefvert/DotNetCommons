using System;
using System.Net;
using Gefvert.Tools.Common.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gefvert.Tools.Common.Test.Net
{
  [TestClass]
  public class IPAccessListTest
  {
    [TestMethod]
    public void TestParse()
    {
      var al = IPAccessList.Parse(" 192.168.1.20  ,, ,   192.168.2.20,8.0.0.0/8, google-dns.gefvert.org ,");

      Assert.AreEqual(4, al.Addresses.Count);
      Assert.AreEqual(1, al.Ranges.Count);

      Assert.AreEqual("192.168.1.20, 192.168.2.20, 8.0.0.0/8, 8.8.4.4, 8.8.8.8", al.ToString());
    }

    [TestMethod]
    public void TestAdd()
    {
      var al = new IPAccessList();

      al.Add(IPAddress.Parse("192.168.1.20"));
      al.Add(new[] { IPAddress.Parse("192.168.1.100"), IPAddress.Parse("192.168.1.101") });

      al.Add(IPRange.Parse("10.47.1.0/24"));
      al.Add(new[] { IPRange.Parse("10.47.2.0/24") });

      Assert.AreEqual(3, al.Addresses.Count);
      Assert.AreEqual(2, al.Ranges.Count);
    }

    [TestMethod]
    public void TestAddName()
    {
      var al = new IPAccessList();

      al.Add("google-dns.gefvert.org");

      Assert.AreEqual(2, al.Addresses.Count);
      Assert.AreEqual("8.8.4.4, 8.8.8.8", al.ToString());
    }

    [TestMethod]
    public void TestContains()
    {
      var al = IPAccessList.Parse("8.0.0.0/8, 192.168.1.0/24, 172.16.10.94");

      Assert.IsTrue(al.Contains(IPAddress.Parse("8.8.8.8")));
      Assert.IsTrue(al.Contains(IPAddress.Parse("192.168.1.10")));
      Assert.IsTrue(al.Contains(IPAddress.Parse("172.16.10.94")));

      Assert.IsFalse(al.Contains(IPAddress.Parse("0.0.0.0")));
      Assert.IsFalse(al.Contains(IPAddress.Parse("172.16.20.0")));
      Assert.IsFalse(al.Contains(IPAddress.Parse("192.168.2.0")));
    }
  }
}
