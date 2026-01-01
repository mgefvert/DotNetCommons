using DotNetCommons.Text.FixedWidth;

namespace DotNetCommonTests.Text.FixedWidth;

[TestClass]
public class FixedWidthConverterTests
{
    public class SimpleObject
    {
        [FixedString(1, 5)]
        public string? Name { get; set; }

        [FixedNumber(6, 3, Pad = '0')]
        public int Value { get; set; }
    }

    public class OverlapObject
    {
        [FixedString(1, 5)]
        public string? Name { get; set; }

        [FixedString(5, 5)]
        public string? Overlap { get; set; }
    }

    public class NoAttributesObject
    {
        public string? Name { get; set; }
    }

    [TestMethod]
    public void Convert_WithMultiplePeople_ReturnsExpectedFixedWidthStrings()
    {
        var converter = new FixedWidthConverter();
        var strings = People.All.Select(converter.Convert).ToArray();

        CollectionAssert.AreEqual(new string[] {
            "JOHN                                    DOE                                     0005512341958120406700156000000043700000MN",
            "JANE                                    DOE                                     0005512371962031906300122000000003600000FN",
            "JACK                                    GONE                                    0004433431929050109600000000000000000000MY"
        }, strings);
    }

    [TestMethod]
    public void Parse_WithValidData_ReturnsExpectedObject()
    {
        var converter = new FixedWidthConverter();
        var data = "JOHN                                    DOE                                     0005512341958120406700156000000043700000MN";

        var person = converter.Parse<Person>(data);

        Assert.AreEqual("JOHN", person.FirstName);
        Assert.AreEqual("DOE", person.LastName);
        Assert.AreEqual("000551234", person.SSN);
        Assert.AreEqual(new DateTime(1958, 12, 4), person.BirthDate);
        Assert.AreEqual(67, person.Age);
        Assert.AreEqual(156_000m, person.Income);
        Assert.AreEqual(437_000m, person.Assets);
        Assert.AreEqual('M', person.Gender);
        Assert.IsFalse(person.Deceased);
    }

    [TestMethod]
    public void RoundTrip_WithPeople()
    {
        var converter = new FixedWidthConverter();

        foreach (var person in People.All)
        {
            var data = converter.Convert(person);
            var parsed = converter.Parse<Person>(data);

            Assert.AreEqual(person.FirstName?.ToUpper(), parsed.FirstName);
            Assert.AreEqual(person.LastName?.ToUpper(), parsed.LastName);
            // SSN in Person has AllowedChars = "0123456789", so "000-55-1234" becomes "000551234"
            Assert.AreEqual(person.SSN?.Replace("-", ""), parsed.SSN);
            Assert.AreEqual(person.BirthDate, parsed.BirthDate);
            Assert.AreEqual(person.Age, parsed.Age);
            Assert.AreEqual(person.Income, parsed.Income);
            Assert.AreEqual(person.Assets, parsed.Assets);
            Assert.AreEqual(person.Gender, parsed.Gender);
            Assert.AreEqual(person.Deceased, parsed.Deceased);
        }
    }

    [TestMethod]
    public void Convert_SimpleObject()
    {
        var converter = new FixedWidthConverter();
        var obj = new SimpleObject { Name = "Test", Value = 123 };

        var result = converter.Convert(obj);
        Assert.AreEqual("Test 123", result);
    }

    [TestMethod]
    public void Parse_SimpleObject()
    {
        var converter = new FixedWidthConverter();
        var data = "Hello042";

        var obj = converter.Parse<SimpleObject>(data);
        Assert.AreEqual("Hello", obj.Name);
        Assert.AreEqual(42, obj.Value);
    }

    [TestMethod]
    public void Convert_Overlap_ThrowsInvalidDataException()
    {
        var converter = new FixedWidthConverter();
        var obj = new OverlapObject();

        Assert.ThrowsExactly<InvalidDataException>(() => converter.Convert(obj));
    }

    [TestMethod]
    public void Convert_NoAttributes_ThrowsInvalidDataException()
    {
        var converter = new FixedWidthConverter();
        var obj = new NoAttributesObject();

        Assert.ThrowsExactly<InvalidDataException>(() => converter.Convert(obj));
    }

    [TestMethod]
    public void Parse_ShortData_PadsAndParses()
    {
        var converter = new FixedWidthConverter();
        var data = "ABC  123"; // 8 chars. SimpleObject (5 for Name + 3 for Value)

        var obj = converter.Parse<SimpleObject>(data);
        Assert.AreEqual("ABC", obj.Name);
        Assert.AreEqual(123, obj.Value);
    }
}