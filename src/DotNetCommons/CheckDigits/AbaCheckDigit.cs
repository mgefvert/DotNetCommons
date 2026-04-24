namespace DotNetCommons.CheckDigits;

/// <summary>
/// Class that calculates an ABA account check digit for check routing. Handles extra characters and punctuation
/// gracefully but does not include them in the calculation.
/// </summary>
public class AbaCheckDigit : CheckDigit
{
    public override char Calculate(string input)
    {
        var digits = (input ?? "").Where(char.IsDigit).Select(c => (byte)(c - '0')).ToArray();
        if (digits.Length == 0)
            throw new InvalidOperationException($"{nameof(AbaCheckDigit)}: No digits in input string.");

        var sum = 0;
        for (int i = digits.Length - 1, weight = 7; i >= 0; i--)
        {
            sum += digits[i] * weight;
            weight = weight switch
            {
                7 => 3,
                3 => 1,
                _ => 7
            };
        }

        return (char)((byte)'0' + (10 - sum % 10) % 10);
    }
}