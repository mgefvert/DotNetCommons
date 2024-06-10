using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Web.Tests;

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
    public void GetRemoteIpAddress_XForwardedFor_Works()
    {
        var httpContext = new DefaultHttpContext
        {
            Request    = { Headers         = { ["X-Forwarded-For"] = "192.168.1.14, 172.16.10.2" } },
            Connection = { RemoteIpAddress = IPAddress.Parse("192.168.1.12") }
        };

        httpContext.GetRemoteIpAddress().Should().Be(IPAddress.Parse("192.168.1.14"));
    }

    [TestMethod]
    public void GetRemoteIpAddress_XRealIp_Works()
    {
        var httpContext = new DefaultHttpContext
        {
            Request = { Headers = { ["X-Real-IP"] = "192.168.1.14" } },
            Connection = { RemoteIpAddress = IPAddress.Parse("192.168.1.12") }
        };

        httpContext.GetRemoteIpAddress().Should().Be(IPAddress.Parse("192.168.1.14"));
    }
}