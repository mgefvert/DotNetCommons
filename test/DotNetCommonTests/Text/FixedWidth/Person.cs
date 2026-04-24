using DotNetCommons.Text.FixedWidth;

namespace DotNetCommonTests.Text.FixedWidth;

public class Person
{
    [FixedString(1, 40, UpperCase = true)]
    public string? FirstName { get; set; }

    [FixedString(41, 40, UpperCase = true)]
    public string? LastName { get; set; }

    [FixedString(81, 9, AllowedChars = "0123456789")]
    public string? SSN { get; set; }

    [FixedDate(90, 8, Format = "yyyyMMdd")]
    public DateTime? BirthDate { get; set; }

    [FixedNumber(98, 3, Pad = '0')]
    public int? Age { get; set; }

    [FixedNumber(101, 10, Pad = '0', Scale = 2)]
    public decimal Income { get; set; }

    [FixedNumber(111, 10, Pad = '0', Scale = 2)]
    public decimal Assets { get; set; }

    [FixedChar(121)]
    public char Gender { get; set; }

    [FixedBool(122, 1)]
    public bool Deceased { get; set; }
}