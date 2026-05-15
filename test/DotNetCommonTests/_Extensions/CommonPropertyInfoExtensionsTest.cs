using DotNetCommons;

// ReSharper disable UnusedMember.Global

namespace DotNetCommonTests;

[TestClass]
public class CommonObjectExtensionsTest
{
    public string StringValue { get; set; } = null!;
    public int IntValue { get; set; }
    public int? IntNullableValue { get; set; }
    public bool BoolValue { get; set; }
    public bool? BoolNullableValue { get; set; }
    public DateTime DateTimeValue { get; set; }
    public DateOnly DateOnlyValue { get; set; }
    public TimeOnly TimeOnlyValue { get; set; }
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

    [TestMethod]
    public void SetPropertyValue_WithFormat_ParsesExactDateAndTimeValues()
    {
        var p = typeof(CommonObjectExtensionsTest).GetProperty(nameof(DateTimeValue))!;
        p.SetPropertyValue(this, "20240115", System.Globalization.CultureInfo.InvariantCulture, "yyyyMMdd");
        Assert.AreEqual(new DateTime(2024, 1, 15), DateTimeValue);

        p = typeof(CommonObjectExtensionsTest).GetProperty(nameof(DateOnlyValue))!;
        p.SetPropertyValue(this, "2024-02-05", System.Globalization.CultureInfo.InvariantCulture, "yyyy-MM-dd");
        Assert.AreEqual(new DateOnly(2024, 2, 5), DateOnlyValue);

        p = typeof(CommonObjectExtensionsTest).GetProperty(nameof(TimeOnlyValue))!;
        p.SetPropertyValue(this, "142530", System.Globalization.CultureInfo.InvariantCulture, "HHmmss");
        Assert.AreEqual(new TimeOnly(14, 25, 30), TimeOnlyValue);
    }

    [TestMethod]
    public void ConvertPropertyValue_ReturnsConvertedValueWithoutSettingProperty()
    {
        var p = typeof(CommonObjectExtensionsTest).GetProperty(nameof(IntNullableValue))!;

        var value = p.ConvertPropertyValue("42", System.Globalization.CultureInfo.InvariantCulture);

        Assert.AreEqual(42, value);
        Assert.IsNull(IntNullableValue);
    }
}
