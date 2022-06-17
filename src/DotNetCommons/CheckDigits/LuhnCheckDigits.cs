using System;
using System.Linq;

namespace DotNetCommons.CheckDigits;

/// <summary>
/// Class that calculates a Luhn (mod-10) check digit for credit card numbers, IMEI numbers,
/// etc (U.S. Patent 2,950,048, ISO/IEC 7812-1). Handles extra characters and punctuation gracefully
/// but does not include them in the calculation.
/// </summary>
public class LuhnCheckDigits : ICheckDigits
{
    public char Calculate(string input)
    {
        var digits = (input ?? "").Where(char.IsDigit).Select(c => (byte)(c - '0')).ToArray();
        if (digits.Length == 0)
            throw new InvalidOperationException($"{nameof(LuhnCheckDigits)}: No digits in input string.");

        var sum = 0;
        for (int i = digits.Length - 1, weight = 2; i >= 0; i--, weight = 3 - weight)
        {
            var r = digits[i] * weight;
            sum += r >= 10 ? r - 9 : r;
        }

        return (char)((byte)'0' + (10 - sum % 10) % 10);
    }
}