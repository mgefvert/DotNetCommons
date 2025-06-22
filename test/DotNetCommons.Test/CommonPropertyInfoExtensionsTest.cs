using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Test;

[TestClass]
public class CommonObjectExtensionsTest
{
    public string StringValue { get; set; } = null!;
    public int IntValue { get; set; }
    public int? IntNullableValue { get; set; }
    public bool BoolValue { get; set; }
    public bool? BoolNullableValue { get; set; }
    public DateTime DateTimeValue { get; set; }
    public TimeSpan TimeSpanValue { get; set; }
    public Guid GuidValue { get; set; }
    public Uri UriValue { get; set; } = null!;

    [TestMethod]
    public void SetPropertyValueTests()
    {
        var p = typeof(CommonObjectExtensionsTest).GetProperty(nameof(StringValue))!;
        p.SetPropertyValue(this, "42");
        Assert.AreEqual("42", StringValue);
        p.SetPropertyValue(this, null);
        Assert.IsNull(StringValue);

        p = typeof(CommonObjectExtensionsTest).GetProperty(nameof(IntValue))!;
        p.SetPropertyValue(this, "42");
        Assert.AreEqual(42, IntValue);

        p = typeof(CommonObjectExtensionsTest).GetProperty(nameof(IntNullableValue))!;
        p.SetPropertyValue(this, "42");
        Assert.AreEqual(42, IntNullableValue);
        p.SetPropertyValue(this, null);
        Assert.IsNull(IntNullableValue);

        p = typeof(CommonObjectExtensionsTest).GetProperty(nameof(BoolValue))!;
        p.SetPropertyValue(this, "true");
        Assert.IsTrue(BoolValue);
        p.SetPropertyValue(this, 0);
        Assert.IsFalse(BoolValue);

        p = typeof(CommonObjectExtensionsTest).GetProperty(nameof(BoolNullableValue))!;
        p.SetPropertyValue(this, "true");
        Assert.IsTrue(BoolNullableValue!.Value);
        p.SetPropertyValue(this, 0);
        Assert.IsFalse(BoolNullableValue!.Value);
        p.SetPropertyValue(this, null);
        Assert.IsNull(BoolNullableValue);

        p = typeof(CommonObjectExtensionsTest).GetProperty(nameof(DateTimeValue))!;
        p.SetPropertyValue(this, "2022-01-05");
        Assert.AreEqual(new DateTime(2022, 1, 5), DateTimeValue);

        p = typeof(CommonObjectExtensionsTest).GetProperty(nameof(TimeSpanValue))!;
        p.SetPropertyValue(this, "01:02:03");
        Assert.AreEqual(new TimeSpan(1, 2, 3), TimeSpanValue);

        p = typeof(CommonObjectExtensionsTest).GetProperty(nameof(GuidValue))!;
        p.SetPropertyValue(this, "c7c2e12d-99cf-40ce-badd-97edc801a210");
        Assert.AreEqual(Guid.Parse("c7c2e12d-99cf-40ce-badd-97edc801a210"), GuidValue);

        p = typeof(CommonObjectExtensionsTest).GetProperty(nameof(UriValue))!;
        p.SetPropertyValue(this, "https://example.com/");
        Assert.AreEqual(new Uri("https://example.com/"), UriValue);
    }
}