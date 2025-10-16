﻿using System.Net;
using DotNetCommons.Net;

namespace DotNetCommonTests.Net;

[TestClass]
public class IpAccessListTest
{
    [TestMethod]
    public void TestParse()
    {
        var al = IPAccessList.Parse(" 192.168.1.20  ,, ,   192.168.2.20,8.0.0.0/8 ,");

        Assert.AreEqual("192.168.1.20, 192.168.2.20, 8.0.0.0/8", al.ToString());
        Assert.HasCount(2, al.Addresses);
        Assert.HasCount(1, al.Ranges);
    }

    [TestMethod]
    public void TestAdd()
    {
        var al = new IPAccessList();

        al.Add(IPAddress.Parse("192.168.1.20"));
        al.Add(new[] { IPAddress.Parse("192.168.1.100"), IPAddress.Parse("192.168.1.101") });

        al.Add(IPNetwork.Parse("10.47.1.0/24")!);
        al.Add(new[] { IPNetwork.Parse("10.47.2.0/24")! });

        Assert.HasCount(3, al.Addresses);
        Assert.HasCount(2, al.Ranges);
    }

    [TestMethod]
    public void TestAddName()
    {
        var al = new IPAccessList();

        al.Add("google-public-dns-a.google.com");
        al.Add("google-public-dns-b.google.com");

        Assert.IsGreaterThanOrEqualTo(2, al.Addresses.Count);
        Assert.IsTrue(al.Contains(IPAddress.Parse("8.8.8.8")));
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

    [TestMethod]
    public void TestGlobal()
    {
        var al = IPAccessList.Parse("0.0.0.0/0");
        Assert.IsTrue(al.Contains(IPAddress.Parse("74.65.2.190")));
    }
}