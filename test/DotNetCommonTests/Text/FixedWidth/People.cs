using DotNetCommons;

namespace DotNetCommonTests.Text.FixedWidth;

public static class People
{
    public static DateTime Now { get; set; } = new(2026, 1, 1);

    public static readonly Person John = new Person
    {
        FirstName = "John",
        LastName  = "Doe",
        SSN       = "000-55-1234",
        BirthDate = new DateTime(1958, 12, 4),
        Income    = 156_000m,
        Assets    = 437_000m,
        Gender    = 'M',
        Deceased  = false
    };

    public static readonly Person Jane = new Person
    {
        FirstName = "Jane",
        LastName  = "Doe",
        SSN       = "000-55-1237",
        BirthDate = new DateTime(1962, 3, 19),
        Income    = 122_000m,
        Assets    = 36_000m,
        Gender    = 'F',
        Deceased  = false
    };

    public static readonly Person Jack = new Person
    {
        FirstName = "Jack",
        LastName  = "Gone",
        SSN       = "000-44-3343",
        BirthDate = new DateTime(1929, 5, 1),
        Income    = 0m,
        Assets    = 0m,
        Gender    = 'M',
        Deceased  = true
    };

    public static IEnumerable<Person> All()
    {
        foreach (var person in new[] { John, Jane, Jack })
            person.Age = person.BirthDate?.AgeYears(Now);

        yield return John;
        yield return Jane;
        yield return Jack;
    }
}