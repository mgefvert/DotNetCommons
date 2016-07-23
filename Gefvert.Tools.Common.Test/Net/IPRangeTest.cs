using System;
using System.Net;
using Gefvert.Tools.Common.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gefvert.Tools.Common.Test.Net
{
  [TestClass]
  public class IPRangeTest
  {
    [TestMethod]
    public void TestConstructor()
    {
      var r = new IPRange(IPAddress.Parse("192.168.1.120"));
      Test("Constructor 1", r, "192.168.1.120", "255.255.255.255", 32);

      r = new IPRange(IPAddress.Parse("192.168.1.120"), IPAddress.Parse("255.255.255.0"));
      Test("Constructor 2", r, "192.168.1.0", "255.255.255.0", 24);

      r = new IPRange(IPAddress.Parse("192.168.1.120"), 24);
      Test("Constructor 2", r, "192.168.1.0", "255.255.255.0", 24);
    }

    private void Test(string name, IPRange range, string address, string netmask, int mask)
    {
      Assert.AreEqual(address, range.Address.ToString(), name + ": IP address fail");
      Assert.AreEqual(netmask, range.Netmask.ToString(), name + ": IP netmask fail");
      Assert.AreEqual(mask, range.MaskLen, name + ": Netmask length fail");
    }

    [TestMethod]
    public void TestContains()
    {
      var r = IPRange.Parse("192.168.1.0/24");

      Assert.IsTrue(r.Contains(IPAddress.Parse("192.168.1.0")));
      Assert.IsTrue(r.Contains(IPAddress.Parse("192.168.1.1")));
      Assert.IsTrue(r.Contains(IPAddress.Parse("192.168.1.128")));
      Assert.IsTrue(r.Contains(IPAddress.Parse("192.168.1.255")));

      Assert.IsFalse(r.Contains(IPAddress.Parse("193.168.1.0")));
      Assert.IsFalse(r.Contains(IPAddress.Parse("192.169.1.0")));
      Assert.IsFalse(r.Contains(IPAddress.Parse("192.168.3.0")));
      Assert.IsFalse(r.Contains(IPAddress.Parse("255.255.255.255")));
      Assert.IsFalse(r.Contains(IPAddress.Parse("0.0.0.0")));
    }

    [TestMethod]
    public void TestParse(string network)
    {
      var r = IPRange.Parse("192.168.1.120");
      Test("Parse 1", r, "192.168.1.120", "255.255.255.255", 32);

      r = IPRange.Parse("192.168.1.120/24");
      Test("Parse 2", r, "192.168.1.0", "255.255.255.0", 0);

      r = IPRange.Parse("192.168.1.120/0");
      Test("Parse 3", r, "0.0.0.0", "0.0.0.0", 0);
    }

    [TestMethod]
    public void TestTryParse()
    {
      IPRange r;
      Assert.IsTrue(IPRange.TryParse("192.168.1.0", out r));
      Assert.IsTrue(IPRange.TryParse("192.168.1.0/24", out r));
      Assert.IsFalse(IPRange.TryParse("192.168.1.9/m", out r));
      Assert.IsFalse(IPRange.TryParse("192.168.1/", out r));
      Assert.IsFalse(IPRange.TryParse("/24", out r));
      Assert.IsFalse(IPRange.TryParse("mike/24", out r));
    }

    [TestMethod]
    public void TestToString()
    {
      var r = IPRange.Parse("192.168.1.0/24");
      Assert.AreEqual("192.168.1.0/24", r.ToString());
    }
  }
}
