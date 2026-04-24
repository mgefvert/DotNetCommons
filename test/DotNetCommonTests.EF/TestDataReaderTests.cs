using DotNetCommons.EF.DataReading;
using DotNetCommonTests.EF.TestData;
using FluentAssertions;

namespace DotNetCommonTests.EF;

[TestClass]
public class TestDataReaderTests
{
    [TestMethod]
    public void Fetch_ShouldParseSampleData_FromStream()
    {
        var reader = new TestDataReader();
        using var stream = File.OpenRead(GetSampleDataPath());

        reader.Load(stream);

        var accounts = reader.Fetch<Account>();
        var customers = reader.Fetch<Customer>();

        accounts.Should().HaveCount(7);
        accounts[0].AccountId.Should().Be(-1);
        accounts[0].AccountType.Should().Be(AccountType.Checking);
        accounts[0].Balance.Should().Be(3509.20m);
        accounts[0].BalanceDate.Should().Be(DateOnly.FromDateTime(DateTime.Today));

        customers.Should().HaveCount(2);
        customers[0].CompanyName.Should().BeNull();
        customers[0].Country.Should().Be("US");
    }

    [TestMethod]
    public void Load_ShouldSupportFileOverload_AndDefaultOverrideRules()
    {
        const string markdown = """
                                # Customer:default
                                State = NY
                                Country = US
                                Email = @null

                                # Customer
                                | Id | FirstName | State | Email |
                                |----|-----------|-------|-------|
                                | 1  | Alice     |       | alice@example.com |
                                | 2  | Bob       | @null |       |
                                """;

        var fileName = Path.GetTempFileName();
        try
        {
            File.WriteAllText(fileName, markdown);

            var reader = new TestDataReader();
            reader.Load(fileName);

            var customers = reader.Fetch<Customer>();
            customers.Should().HaveCount(2);

            customers[0].State.Should().Be("NY"); // Blank means uninitialized, so default applies.
            customers[0].Email.Should().Be("alice@example.com");
            customers[0].Country.Should().Be("US");

            customers[1].State.Should().BeNull(); // @null is explicit and does not get replaced by defaults.
            customers[1].Email.Should().BeNull(); // Blank picks up default @null.
            customers[1].Country.Should().Be("US");
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [TestMethod]
    public void Fetch_ShouldExpandBuiltInMacros()
    {
        const string markdown = """
                                # MacroRecord
                                | Id    | LocalNow | UtcNow  | Today   | EmptyText |
                                |-------|----------|---------|---------|-----------|
                                | @uuid | @now     | @utcnow | @today  | @empty    |
                                """;

        var reader = new TestDataReader();
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(markdown));

        reader.Load(stream);

        var first = reader.Fetch<MacroRecord>().Single();
        var second = reader.Fetch<MacroRecord>().Single();

        first.Id.Should().NotBe(Guid.Empty);
        second.Id.Should().NotBe(first.Id);
        first.LocalNow.Should().BeOnOrAfter(DateTime.Now.AddMinutes(-1));
        first.UtcNow.Kind.Should().Be(DateTimeKind.Utc);
        first.Today.Should().Be(DateOnly.FromDateTime(DateTime.Today));
        first.EmptyText.Should().Be(string.Empty);
    }

    private static string GetSampleDataPath()
    {
        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../TestData/data.md"));
    }

    public class MacroRecord
    {
        public Guid Id { get; set; }
        public DateTime LocalNow { get; set; }
        public DateTime UtcNow { get; set; }
        public DateOnly Today { get; set; }
        public string EmptyText { get; set; } = string.Empty;
    }
}
