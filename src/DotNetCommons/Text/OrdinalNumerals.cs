// From https://stackoverflow.com/questions/20156/is-there-an-easy-way-to-create-ordinals-in-c

namespace DotNetCommons.Text;

public static class OrdinalNumerals
{
    public static string Render(int? number)
    {
        switch (number)
        {
            case null:
                return "";
            case <= 0:
                return number.ToString()!;
            default:
                switch (number % 100)
                {
                    case 11:
                    case 12:
                    case 13:
                        return number + "th";
                }

                return (number % 10) switch
                {
                    1 => number + "st",
                    2 => number + "nd",
                    3 => number + "rd",
                    _ => number + "th"
                };
        }
    }
}