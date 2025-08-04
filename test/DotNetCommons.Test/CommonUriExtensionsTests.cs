using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace DotNetCommons.Test;

[TestClass]
public class CommonUriExtensionsTests
{
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void WithQuery_NullUri_ThrowsArgumentNullException()
    {
        Uri? uri = null;
        uri!.WithQuery("query=value");
    }

    [TestMethod]
    public void WithQuery_EmptyQueryString_RemovesQueryString()
    {
        var uri = new Uri("https://example.com?existing=value");
        var result = uri.WithQuery(string.Empty);
        result.ToString().Should().Be("https://example.com/");
    }

    [TestMethod]
    public void WithQuery_SimpleQueryString_AddsQueryString()
    {
        var uri = new Uri("https://example.com");
        var result = uri.WithQuery("param=value");
        result.ToString().Should().Be("https://example.com/?param=value");
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void WithQuery_NullQueryParameters_ThrowsArgumentNullException()
    {
        var uri = new Uri("https://example.com");
        uri.WithQuery((IEnumerable<KeyValuePair<string, string?>>)null!);
    }

    [TestMethod]
    public void WithQuery_EmptyQueryParameters_RemovesQueryString()
    {
        var uri = new Uri("https://example.com?existing=value");
        var result = uri.WithQuery(new Dictionary<string, string?>());
        result.ToString().Should().Be("https://example.com/");
    }

    [TestMethod]
    public void WithQuery_QueryParametersWithNullValues_IgnoresNullValues()
    {
        var uri = new Uri("https://example.com");
        var parameters = new Dictionary<string, string?>
        {
            { "valid", "value" },
            { "invalid", null }
        };
        var result = uri.WithQuery(parameters);
        result.ToString().Should().Be("https://example.com/?valid=value");
    }

    [TestMethod]
    public void WithQuery_MultipleQueryParameters_AddsAllParameters()
    {
        var uri = new Uri("https://example.com");
        var parameters = new Dictionary<string, string?>
        {
            { "param1", "value 1" },
            { "param2", "value 2" }
        };
        var result = uri.WithQuery(parameters);
        result.AbsoluteUri.Should().Be("https://example.com/?param1=value%201&param2=value%202");
    }
}