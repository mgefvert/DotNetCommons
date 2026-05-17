using System.Text;
using DotNetCommons.Text.Csv;

namespace DotNetCommonTests.Text.Csv;

[TestClass]
public class CsvConverterTests
{
    private static readonly Encoding Utf8 = new UTF8Encoding(false);
    
    // Test model classes

    public class SimplePerson
    {
        [CsvField("Name")]  public string? Name { get; set; }
        [CsvField("Age")]   public int Age { get; set; }
        [CsvField("Score")] public decimal Score { get; set; }
    }

    public class AllTypesRecord
    {
        [CsvField("StringVal")] public string? StringVal { get; set; }
        [CsvField("IntVal")] public int IntVal { get; set; }
        [CsvField("NullableInt")] public int? NullableInt { get; set; }
        [CsvField("BoolVal")] public bool BoolVal { get; set; }
        [CsvField("DateVal")] public DateTime DateVal { get; set; }
        [CsvField("DecimalVal")] public decimal DecimalVal { get; set; }
    }

    public class FormattedRecord
    {
        [CsvField("Name")] public string? Name { get; set; }
        [CsvField("Date", Format = "yyyyMMdd")] public DateTime Date { get; set; }
        [CsvField("NullableDate", Format = "yyyy-MM-dd")] public DateTime? NullableDate { get; set; }
        [CsvField("Amount", Format = "0.00")] public decimal Amount { get; set; }
    }

    public class NoAttributesRecord
    {
        public string? Name { get; set; }
        public int Value { get; set; }
    }

    // Convert – serialization

    [TestMethod]
    public void Convert_SimpleObject_ReturnsExpectedCsv()
    {
        var converter = new CsvConverter<SimplePerson>();
        var person = new SimplePerson { Name = "Alice", Age = 30, Score = 9.5m };

        var result = converter.Convert(person);

        Assert.AreEqual("Alice,30,9.5", result);
    }

    [TestMethod]
    public void Convert_FieldContainsDelimiter_QuotesField()
    {
        var converter = new CsvConverter<SimplePerson>();
        var person = new SimplePerson { Name = "Smith, John", Age = 25, Score = 8m };

        var result = converter.Convert(person);

        Assert.AreEqual("\"Smith, John\",25,8", result);
    }

    [TestMethod]
    public void Convert_FieldContainsQuoteCharacter_EscapesQuote()
    {
        var converter = new CsvConverter<SimplePerson>();
        var person = new SimplePerson { Name = "O\"Brien", Age = 40, Score = 7m };

        var result = converter.Convert(person);

        Assert.AreEqual("\"O\"\"Brien\",40,7", result);
    }

    [TestMethod]
    public void Convert_FieldContainsNewline_QuotesField()
    {
        var converter = new CsvConverter<SimplePerson>();
        var person = new SimplePerson { Name = "Line1\nLine2", Age = 1, Score = 1m };

        var result = converter.Convert(person);

        Assert.AreEqual("\"Line1\nLine2\",1,1", result);
    }

    [TestMethod]
    public void Convert_FieldContainsCarriageReturn_QuotesField()
    {
        var converter = new CsvConverter<SimplePerson>();
        var person = new SimplePerson { Name = "Line1\rLine2", Age = 1, Score = 1m };

        var result = converter.Convert(person);

        Assert.AreEqual("\"Line1\rLine2\",1,1", result);
    }

    [TestMethod]
    public void Convert_NullStringField_ReturnsEmptyField()
    {
        var converter = new CsvConverter<SimplePerson>();
        var person = new SimplePerson { Name = null, Age = 0, Score = 0m };

        var result = converter.Convert(person);

        Assert.AreEqual(",0,0", result);
    }

    [TestMethod]
    public void Convert_EmptyStringField_ReturnsEmptyField()
    {
        var converter = new CsvConverter<SimplePerson>();
        var person = new SimplePerson { Name = "", Age = 0, Score = 0m };

        var result = converter.Convert(person);

        Assert.AreEqual(",0,0", result);
    }

    [TestMethod]
    public void Convert_CustomDelimiter_UsesNewDelimiter()
    {
        var converter = new CsvConverter<SimplePerson> { Delimiter = ';' };
        var person = new SimplePerson { Name = "Alice", Age = 30, Score = 9.5m };

        var result = converter.Convert(person);

        Assert.AreEqual("Alice;30;9.5", result);
    }

    [TestMethod]
    public void Convert_FieldContainsCustomDelimiter_QuotesField()
    {
        var converter = new CsvConverter<SimplePerson> { Delimiter = ';' };
        var person = new SimplePerson { Name = "Alice;Bob", Age = 30, Score = 9.5m };

        var result = converter.Convert(person);

        Assert.AreEqual("\"Alice;Bob\";30;9.5", result);
    }

    [TestMethod]
    public void Convert_CustomQuoteCharacter_UsesNewQuoteChar()
    {
        var converter = new CsvConverter<SimplePerson> { QuoteCharacter = '\'' };
        var person = new SimplePerson { Name = "Alice,Bob", Age = 30, Score = 9.5m };

        var result = converter.Convert(person);

        Assert.AreEqual("'Alice,Bob',30,9.5", result);
    }

    [TestMethod]
    public void Convert_FieldContainsCustomQuoteCharacter_EscapesIt()
    {
        var converter = new CsvConverter<SimplePerson> { QuoteCharacter = '\'' };
        var person = new SimplePerson { Name = "It's fine", Age = 30, Score = 9.5m };

        var result = converter.Convert(person);

        Assert.AreEqual("'It''s fine',30,9.5", result);
    }

    [TestMethod]
    public void Convert_FieldWithBothDelimiterAndQuote_EscapesCorrectly()
    {
        var converter = new CsvConverter<SimplePerson>();
        var person = new SimplePerson { Name = "Say \"hello\", friend", Age = 1, Score = 1m };

        var result = converter.Convert(person);

        Assert.AreEqual("\"Say \"\"hello\"\", friend\",1,1", result);
    }

    [TestMethod]
    public void Convert_FormattedFields_UsesAttributeFormat()
    {
        var converter = new CsvConverter<FormattedRecord>();
        var record = new FormattedRecord
        {
            Name = "Alice",
            Date = new DateTime(2024, 1, 15),
            NullableDate = new DateTime(2024, 2, 5),
            Amount = 12.3m,
        };

        var result = converter.Convert(record);

        Assert.AreEqual("Alice,20240115,2024-02-05,12.30", result);
    }

    // ConvertHeader

    [TestMethod]
    public void ConvertHeader_ReturnsFieldNamesInOrder()
    {
        var converter = new CsvConverter<SimplePerson>();
        var header = converter.ConvertHeader();
        Assert.AreEqual("Name,Age,Score", header);
    }

    [TestMethod]
    public void ConvertHeader_NoAttributes_ReturnsEmptyString()
    {
        var converter = new CsvConverter<NoAttributesRecord>();
        var header = converter.ConvertHeader();
        Assert.AreEqual("", header);
    }

    [TestMethod]
    public void ConvertHeader_CustomDelimiter_UsesThatDelimiter()
    {
        var converter = new CsvConverter<SimplePerson> { Delimiter = '|' };
        var header = converter.ConvertHeader();
        Assert.AreEqual("Name|Age|Score", header);
    }

    // ParseHeader + Parse

    [TestMethod]
    public void Parse_WithoutParseHeader_ThrowsInvalidOperationException()
    {
        var converter = new CsvConverter<SimplePerson>();
        Assert.ThrowsExactly<InvalidOperationException>(() => converter.Parse("Alice,30,9.5"));
    }

    [TestMethod]
    public void ParseHeader_UnknownField_ThrowsInvalidOperationException()
    {
        var converter = new CsvConverter<SimplePerson>();
        Assert.ThrowsExactly<InvalidOperationException>(() => converter.ParseHeader("Name,Age,Unknown"));
    }

    [TestMethod]
    public void ParseHeader_ThenParse_ReturnsCorrectObject()
    {
        var converter = new CsvConverter<SimplePerson>();
        converter.ParseHeader("Name,Age,Score");

        var person = converter.Parse("Alice,30,9.5");

        Assert.AreEqual("Alice", person.Name);
        Assert.AreEqual(30, person.Age);
        Assert.AreEqual(9.5m, person.Score);
    }

    [TestMethod]
    public void ParseHeader_CaseInsensitive_Works()
    {
        var converter = new CsvConverter<SimplePerson>();
        converter.ParseHeader("NAME,AGE,SCORE");

        var person = converter.Parse("Bob,25,8.0");

        Assert.AreEqual("Bob", person.Name);
        Assert.AreEqual(25, person.Age);
    }

    [TestMethod]
    public void ParseHeader_ReorderedColumns_ParsesCorrectly()
    {
        var converter = new CsvConverter<SimplePerson>();
        converter.ParseHeader("Score,Name,Age");

        var person = converter.Parse("9.5,Alice,30");

        Assert.AreEqual("Alice", person.Name);
        Assert.AreEqual(30, person.Age);
        Assert.AreEqual(9.5m, person.Score);
    }

    [TestMethod]
    public void Parse_QuotedField_ReturnsUnquotedValue()
    {
        var converter = new CsvConverter<SimplePerson>();
        converter.ParseHeader("Name,Age,Score");

        var person = converter.Parse("\"Alice Smith\",30,9.5");

        Assert.AreEqual("Alice Smith", person.Name);
    }

    [TestMethod]
    public void Parse_QuotedFieldWithCommaInside_ParsesAsOneField()
    {
        var converter = new CsvConverter<SimplePerson>();
        converter.ParseHeader("Name,Age,Score");

        var person = converter.Parse("\"Smith, John\",30,9.5");

        Assert.AreEqual("Smith, John", person.Name);
    }

    [TestMethod]
    public void Parse_EscapedQuoteInsideQuotedField_UnescapesCorrectly()
    {
        var converter = new CsvConverter<SimplePerson>();
        converter.ParseHeader("Name,Age,Score");

        var person = converter.Parse("\"O\"\"Brien\",40,7.0");

        Assert.AreEqual("O\"Brien", person.Name);
    }

    [TestMethod]
    public void Parse_EmptyField_LeavesDefaultValue()
    {
        var converter = new CsvConverter<SimplePerson>();
        converter.ParseHeader("Name,Age,Score");

        var person = converter.Parse(",0,0");

        Assert.IsNull(person.Name);
        Assert.AreEqual(0, person.Age);
    }

    [TestMethod]
    public void Parse_TypeConversions_AllTypes()
    {
        var converter = new CsvConverter<AllTypesRecord>();
        converter.ParseHeader("StringVal,IntVal,NullableInt,BoolVal,DateVal,DecimalVal");

        var record = converter.Parse("Hello,42,7,True,01/15/2024 00:00:00,3.14");

        Assert.AreEqual("Hello", record.StringVal);
        Assert.AreEqual(42, record.IntVal);
        Assert.AreEqual(7, record.NullableInt);
        Assert.IsTrue(record.BoolVal);
        Assert.AreEqual(new DateTime(2024, 1, 15), record.DateVal);
        Assert.AreEqual(3.14m, record.DecimalVal);
    }

    [TestMethod]
    public void Parse_EmptyNullableInt_ReturnsNull()
    {
        var converter = new CsvConverter<AllTypesRecord>();
        converter.ParseHeader("StringVal,IntVal,NullableInt,BoolVal,DateVal,DecimalVal");

        var record = converter.Parse("Hello,1,,True,01/01/2020 00:00:00,1.0");

        Assert.IsNull(record.NullableInt);
    }

    [TestMethod]
    public void Parse_FewerFieldsThanHeader_SetsRemainingToDefaults()
    {
        var converter = new CsvConverter<SimplePerson>();
        converter.ParseHeader("Name,Age,Score");

        var person = converter.Parse("Alice,30");

        Assert.AreEqual("Alice", person.Name);
        Assert.AreEqual(30, person.Age);
        Assert.AreEqual(0m, person.Score);
    }

    [TestMethod]
    public void Parse_TrailingDelimiter_ProducesEmptyLastField()
    {
        var converter = new CsvConverter<SimplePerson>();
        converter.ParseHeader("Name,Age,Score");

        var person = converter.Parse("Alice,30,");

        Assert.AreEqual("Alice", person.Name);
        Assert.AreEqual(30, person.Age);
        Assert.AreEqual(0m, person.Score);
    }

    [TestMethod]
    public void Parse_CustomDelimiter_ParsesCorrectly()
    {
        var converter = new CsvConverter<SimplePerson> { Delimiter = ';' };
        converter.ParseHeader("Name;Age;Score");

        var person = converter.Parse("Alice;30;9.5");

        Assert.AreEqual("Alice", person.Name);
        Assert.AreEqual(30, person.Age);
        Assert.AreEqual(9.5m, person.Score);
    }

    [TestMethod]
    public void Parse_CustomQuoteCharacter_ParsesCorrectly()
    {
        var converter = new CsvConverter<SimplePerson> { QuoteCharacter = '\'' };
        converter.ParseHeader("Name,Age,Score");

        var person = converter.Parse("'Smith, John',30,9.5");

        Assert.AreEqual("Smith, John", person.Name);
    }

    [TestMethod]
    public void Parse_FormattedDateTime_UsesAttributeFormat()
    {
        var converter = new CsvConverter<FormattedRecord>();
        converter.ParseHeader("Name,Date,NullableDate,Amount");

        var record = converter.Parse("Alice,20240115,2024-02-05,12.30");

        Assert.AreEqual("Alice", record.Name);
        Assert.AreEqual(new DateTime(2024, 1, 15), record.Date);
        Assert.AreEqual(new DateTime(2024, 2, 5), record.NullableDate);
        Assert.AreEqual(12.30m, record.Amount);
    }

    [TestMethod]
    public void Parse_FormattedDateTimeWithReorderedColumns_UsesMatchingAttributeFormat()
    {
        var converter = new CsvConverter<FormattedRecord>();
        converter.ParseHeader("NullableDate,Name,Amount,Date");

        var record = converter.Parse("2024-02-05,Alice,12.30,20240115");

        Assert.AreEqual("Alice", record.Name);
        Assert.AreEqual(new DateTime(2024, 1, 15), record.Date);
        Assert.AreEqual(new DateTime(2024, 2, 5), record.NullableDate);
        Assert.AreEqual(12.30m, record.Amount);
    }

    // Round-trip tests

    [TestMethod]
    public void RoundTrip_SimplePersons_AllFieldsPreserved()
    {
        var converter = new CsvConverter<SimplePerson>();
        var people = new[]
        {
            new SimplePerson { Name = "Alice", Age = 30, Score = 9.5m },
            new SimplePerson { Name = "Bob", Age = 25, Score = 8.0m },
            new SimplePerson { Name = null, Age = 0, Score = 0m },
        };

        converter.ParseHeader(converter.ConvertHeader());

        foreach (var person in people)
        {
            var csv = converter.Convert(person);
            var parsed = converter.Parse(csv);

            Assert.AreEqual(person.Name, parsed.Name);
            Assert.AreEqual(person.Age, parsed.Age);
            Assert.AreEqual(person.Score, parsed.Score);
        }
    }

    [TestMethod]
    public void RoundTrip_FieldsWithSpecialCharacters_Preserved()
    {
        var converter = new CsvConverter<SimplePerson>();
        converter.ParseHeader(converter.ConvertHeader());

        var people = new[]
        {
            new SimplePerson { Name = "Smith, John", Age = 1, Score = 1m },
            new SimplePerson { Name = "O\"Brien", Age = 2, Score = 2m },
            new SimplePerson { Name = "Multi\nLine", Age = 3, Score = 3m },
        };

        foreach (var person in people)
        {
            var csv = converter.Convert(person);
            var parsed = converter.Parse(csv);
            Assert.AreEqual(person.Name, parsed.Name);
        }
    }

    [TestMethod]
    public void RoundTrip_BothDelimiterAndQuoteInField_Preserved()
    {
        var converter = new CsvConverter<SimplePerson>();
        converter.ParseHeader(converter.ConvertHeader());
        var person = new SimplePerson { Name = "Say \"hello\", friend", Age = 1, Score = 1m };

        var csv = converter.Convert(person);
        var parsed = converter.Parse(csv);

        Assert.AreEqual(person.Name, parsed.Name);
    }

    // Async Read / Write

    private static StreamReader ToReader(string content)
        => new(new MemoryStream(Utf8.GetBytes(content)), Utf8);

    private static (StreamWriter writer, MemoryStream stream) CreateWriter()
    {
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms, Utf8, leaveOpen: true);
        return (writer, ms);
    }

    [TestMethod]
    public async Task ReadHeader_ThenReadOne_ParsesFirstDataRow()
    {
        var content = "Name,Age,Score\r\nAlice,30,9.5\r\n";
        var converter = new CsvConverter<SimplePerson>();
        using var reader = ToReader(content);

        await converter.ReadHeader(reader);
        var person = await converter.ReadOne(reader);

        Assert.IsNotNull(person);
        Assert.AreEqual("Alice", person.Name);
        Assert.AreEqual(30, person.Age);
        Assert.AreEqual(9.5m, person.Score);
    }

    [TestMethod]
    public async Task ReadOne_AtEndOfStream_ReturnsNull()
    {
        var content = "Name,Age,Score\r\n";
        var converter = new CsvConverter<SimplePerson>();
        using var reader = ToReader(content);

        await converter.ReadHeader(reader);
        var person = await converter.ReadOne(reader);

        Assert.IsNull(person);
    }

    [TestMethod]
    public async Task Read_MultipleItems_ReturnsAll()
    {
        var content = "Name,Age,Score\r\nAlice,30,9.5\r\nBob,25,8.0\r\nCharlie,40,7.0\r\n";
        var converter = new CsvConverter<SimplePerson>();
        using var reader = ToReader(content);

        var results = new List<SimplePerson>();
        await foreach (var item in converter.Read(reader))
            results.Add(item);

        Assert.AreEqual(3, results.Count);
        Assert.AreEqual("Alice", results[0].Name);
        Assert.AreEqual("Bob", results[1].Name);
        Assert.AreEqual("Charlie", results[2].Name);
    }

    [TestMethod]
    public async Task Read_EmptyFile_ReturnsNoItems()
    {
        var content = "";
        var converter = new CsvConverter<SimplePerson>();
        using var reader = ToReader(content);

        var results = new List<SimplePerson>();
        await foreach (var item in converter.Read(reader))
            results.Add(item);

        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public async Task ReadHeader_SkipsBlankLines_FindsHeader()
    {
        var content = "\r\n\r\nName,Age,Score\r\nAlice,30,9.5\r\n";
        var converter = new CsvConverter<SimplePerson>();
        using var reader = ToReader(content);

        await converter.ReadHeader(reader);
        var person = await converter.ReadOne(reader);

        Assert.IsNotNull(person);
        Assert.AreEqual("Alice", person.Name);
    }

    [TestMethod]
    public async Task ReadHeader_SkipsCommentLines_FindsHeader()
    {
        var content = "# This is a comment\r\nName,Age,Score\r\nAlice,30,9.5\r\n";
        var converter = new CsvConverter<SimplePerson> { CommentCharacter = '#' };
        using var reader = ToReader(content);

        await converter.ReadHeader(reader);
        var person = await converter.ReadOne(reader);

        Assert.IsNotNull(person);
        Assert.AreEqual("Alice", person.Name);
    }

    [TestMethod]
    public async Task WriteOne_WritesLineWithNewline()
    {
        var converter = new CsvConverter<SimplePerson>();
        var person = new SimplePerson { Name = "Alice", Age = 30, Score = 9.5m };
        var (writer, ms) = CreateWriter();

        await converter.WriteOne(writer, person);
        await writer.FlushAsync();

        var result = Encoding.UTF8.GetString(ms.ToArray());
        Assert.IsTrue(result.Contains("Alice,30,9.5"));
    }

    [TestMethod]
    public async Task Write_WritesHeaderThenAllItems()
    {
        var converter = new CsvConverter<SimplePerson>();
        var people = new[]
        {
            new SimplePerson { Name = "Alice", Age = 30, Score = 9.5m },
            new SimplePerson { Name = "Bob", Age = 25, Score = 8.0m },
        };
        var (writer, ms) = CreateWriter();

        await converter.Write(writer, people);
        await writer.FlushAsync();

        var text = Encoding.UTF8.GetString(ms.ToArray());
        var lines = text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.AreEqual("Name,Age,Score", lines[0]);
        Assert.IsTrue(lines[1].Contains("Alice"));
        Assert.IsTrue(lines[2].Contains("Bob"));
    }

    [TestMethod]
    public async Task Write_ThenRead_RoundTrip()
    {
        var converter = new CsvConverter<SimplePerson>();
        var people = new[]
        {
            new SimplePerson { Name = "Alice", Age = 30, Score = 9.5m },
            new SimplePerson { Name = "Bob", Age = 25, Score = 8.0m },
            new SimplePerson { Name = "Smith, Jane", Age = 35, Score = 7.5m },
        };

        var (writer, ms) = CreateWriter();
        await converter.Write(writer, people);
        await writer.FlushAsync();

        ms.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(ms, Encoding.UTF8);
        var results = new List<SimplePerson>();
        await foreach (var item in converter.Read(reader))
            results.Add(item);

        Assert.AreEqual(people.Length, results.Count);
        for (var i = 0; i < people.Length; i++)
        {
            Assert.AreEqual(people[i].Name, results[i].Name);
            Assert.AreEqual(people[i].Age, results[i].Age);
            Assert.AreEqual(people[i].Score, results[i].Score);
        }
    }

    [TestMethod]
    public async Task Write_CancelledToken_ThrowsOperationCanceledException()
    {
        var converter = new CsvConverter<SimplePerson>();
        var people = new[] { new SimplePerson { Name = "Alice", Age = 30, Score = 9.5m } };
        var (writer, _) = CreateWriter();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(
            async () => await converter.Write(writer, people, cts.Token));
    }
}
