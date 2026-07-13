using System.Globalization;
using System.Net;
using DotNetCommons;
using FluentAssertions;

namespace DotNetCommonTests;

// Condition is always known to be true
#pragma warning disable MSTEST0032

[TestClass]
public class CommonIPAddressExtensionsTest
{
    [TestMethod]
    public void TestUInt32_IpV4()
    {
        var ip = IPAddress.Parse("1.2.128.0");
        ip.ToUInt32(out var a).Should().BeTrue();
        a.Should().Be(0x01028000);

        var ip2 = IPAddress.FromUInt32(a);
        ip2.Should().Be(ip);
    }

    [TestMethod]
    public void TestUInt32_IpV6()
    {
        var ip = IPAddress.Parse("2001:218:4000:6::");
        ip.ToUInt32(out var a).Should().BeFalse();
    }

    [TestMethod]
    public void TestUInt64_IpV4()
    {
        var ip = IPAddress.Parse("1.2.128.0");
        ip.ToUInt64(out var a1, out var a2).Should().BeTrue();
        a1.Should().Be(0ul);
        a2.Should().Be(0x0000FFFF_01028000ul);

        var ip2 = IPAddress.FromUInt64(a1, a2);
        ip2.Should().Be(ip.MapToIPv6());
    }

    [TestMethod]
    public void TestUInt64_IpV6()
    {
        var ip = IPAddress.Parse("2001:218:4000:6::");
        ip.ToUInt64(out var a1, out var a2).Should().BeTrue();
        a1.Should().Be(0x20010218_40000006ul);
        a2.Should().Be(0ul);

        var ip2 = IPAddress.FromUInt64(a1, a2);
        ip2.Should().Be(ip);
    }

    [TestMethod]
    public void TestUInt128_IpV4()
    {
        var ip = IPAddress.Parse("1.2.128.0");
        ip.ToUInt128(out var a).Should().BeTrue();
        a.ToString("X32").Should().Be("00000000000000000000FFFF01028000");

        var ip2 = IPAddress.FromUInt128(a);
        ip2.Should().Be(ip.MapToIPv6());
    }

    [TestMethod]
    public void TestUInt128_IpV6()
    {
        var ip = IPAddress.Parse("2001:218:4000:6::");
        ip.ToUInt128(out var a).Should().BeTrue();
        a.ToString("X32").Should().Be("20010218400000060000000000000000");

        var ip2 = IPAddress.FromUInt128(a);
        ip2.Should().Be(ip);
    }
}