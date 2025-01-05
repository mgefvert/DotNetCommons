using System.Text;

// From https://stackoverflow.com/questions/7040289/converting-integers-to-roman-numerals

namespace DotNetCommons.Numerics;

public static class RomanNumerals
{
    private static readonly Dictionary<char, int> RomanNumberDictionary = new()
    {
        { 'I', 1 },
        { 'V', 5 },
        { 'X', 10 },
        { 'L', 50 },
        { 'C', 100 },
        { 'D', 500 },
        { 'M', 1000 },
    };

    private static readonly Dictionary<int, string> NumberRomanDictionary = new()
    {
        { 1000, "M" },
        { 900, "CM" },
        { 500, "D" },
        { 400, "CD" },
        { 100, "C" },
        { 90, "XC" },
        { 50, "L" },
        { 40, "XL" },
        { 10, "X" },
        { 9, "IX" },
        { 5, "V" },
        { 4, "IV" },
        { 1, "I" },
    };

    public static string Render(int number)
    {
        var roman = new StringBuilder();

        foreach (var item in NumberRomanDictionary)
        {
            while (number >= item.Key)
            {
                roman.Append(item.Value);
                number -= item.Key;
            }
        }

        return roman.ToString();
    }

    public static int Parse(string roman)
    {
        var result = 0;
        var previousLetter = '\0';

        foreach (var currentRoman in roman)
        {
            var previous = previousLetter != '\0' ? RomanNumberDictionary[previousLetter] : '\0';
            var current = RomanNumberDictionary[currentRoman];

            if (previous != 0 && current > previous)
                result = result - (2 * previous) + current;
            else
                result += current;

            previousLetter = currentRoman;
        }

        return result;
    }
}
