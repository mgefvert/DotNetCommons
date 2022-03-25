using System;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Test;

[TestClass]
public class ObjectExtensionsTest
{
    public string StringValue { get; set; }
    public int IntValue { get; set; }
    public int? IntNullableValue { get; set; }
    public bool BoolValue { get; set; }
    public bool? BoolNullableValue { get; set; }
    public DateTime DateTimeValue { get; set; }
    public TimeSpan TimeSpanValue { get; set; }
    public Guid GuidValue { get; set; }
    public Uri UriValue { get; set; }


    [TestMethod]
    public void SetPropertyValueTests()
    {
        var p = typeof(ObjectExtensionsTest).GetProperty(nameof(StringValue));
        this.SetPropertyValue(p, "42"); 
        Assert.AreEqual("42", StringValue);
        this.SetPropertyValue(p, null); 
        Assert.IsNull(StringValue);

        p = typeof(ObjectExtensionsTest).GetProperty(nameof(IntValue));
        this.SetPropertyValue(p, "42");
        Assert.AreEqual(42, IntValue);

        p = typeof(ObjectExtensionsTest).GetProperty(nameof(IntNullableValue));
        this.SetPropertyValue(p, "42");
        Assert.AreEqual(42, IntNullableValue);
        this.SetPropertyValue(p, null);
        Assert.IsNull(IntNullableValue);

        p = typeof(ObjectExtensionsTest).GetProperty(nameof(BoolValue));
        this.SetPropertyValue(p, "true");
        Assert.IsTrue(BoolValue);
        this.SetPropertyValue(p, 0);
        Assert.IsFalse(BoolValue);

        p = typeof(ObjectExtensionsTest).GetProperty(nameof(BoolNullableValue));
        this.SetPropertyValue(p, "true");
        Assert.IsTrue(BoolNullableValue);
        this.SetPropertyValue(p, 0);
        Assert.IsFalse(BoolNullableValue);
        this.SetPropertyValue(p, null);
        Assert.IsNull(BoolNullableValue);

        p = typeof(ObjectExtensionsTest).GetProperty(nameof(DateTimeValue));
        this.SetPropertyValue(p, "2022-01-05");
        Assert.AreEqual(new DateTime(2022, 1, 5), DateTimeValue);

        p = typeof(ObjectExtensionsTest).GetProperty(nameof(TimeSpanValue));
        this.SetPropertyValue(p, "01:02:03");
        Assert.AreEqual(new TimeSpan(1, 2, 3), TimeSpanValue);

        p = typeof(ObjectExtensionsTest).GetProperty(nameof(GuidValue));
        this.SetPropertyValue(p, "c7c2e12d-99cf-40ce-badd-97edc801a210");
        Assert.AreEqual(Guid.Parse("c7c2e12d-99cf-40ce-badd-97edc801a210"), GuidValue);

        p = typeof(ObjectExtensionsTest).GetProperty(nameof(UriValue));
        this.SetPropertyValue(p, "https://example.com/");
        Assert.AreEqual(new Uri("https://example.com/"), UriValue);
    }
}