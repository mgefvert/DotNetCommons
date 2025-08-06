using System.Net;
using DotNetCommons.Web;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace DotNetCommonTests.Web;

[TestClass]
public class HttpContextExtensionsTests
{
    [TestMethod]
    public void GetRemoteIpAddress_Connection_Works()
    {
        var httpContext = new DefaultHttpContext
        {
            Connection = { RemoteIpAddress = IPAddress.Parse("192.168.1.12") }
        };

        httpContext.GetRemoteIpAddress().Should().Be(IPAddress.Parse("192.168.1.12"));
    }

    [TestMethod]
    public void GetRemoteIpAddress_Forwarded_Works()
    {
        var httpContext = new DefaultHttpContext
        {
            Request    = { Headers         = { ["Forwarded"] = "Forwarded: for=192.168.1.14, for=198.51.100.17;by=203.0.113.60;proto=https;host=example.com" } },
            Connection = { RemoteIpAddress = IPAddress.Parse("192.168.1.12") }
        };

        httpContext.GetRemoteIpAddress(true).Should().Be(IPAddress.Parse("192.168.1.14"));
        httpContext.GetRemoteIpAddress(false).Should().Be(IPAddress.Parse("192.168.1.12"));
    }

    [TestMethod]
    public void GetRemoteIpAddress_XForwardedFor_Works()
    {
        var httpContext = new DefaultHttpContext
        {
            Request    = { Headers         = { ["X-Forwarded-For"] = "192.168.1.14, 172.16.10.2" } },
            Connection = { RemoteIpAddress = IPAddress.Parse("192.168.1.12") }
        };

        httpContext.GetRemoteIpAddress(true).Should().Be(IPAddress.Parse("192.168.1.14"));
        httpContext.GetRemoteIpAddress(false).Should().Be(IPAddress.Parse("192.168.1.12"));
    }

    [TestMethod]
    public void GetRemoteIpAddress_XRealIp_Works()
    {
        var httpContext = new DefaultHttpContext
        {
            Request = { Headers = { ["X-Real-IP"] = "192.168.1.14" } },
            Connection = { RemoteIpAddress = IPAddress.Parse("192.168.1.12") }
        };

        httpContext.GetRemoteIpAddress(true).Should().Be(IPAddress.Parse("192.168.1.14"));
        httpContext.GetRemoteIpAddress(false).Should().Be(IPAddress.Parse("192.168.1.12"));
    }
}