namespace DotNetCommons.CheckDigits;

/// <summary>
/// Check digit calculations.
/// </summary>
public abstract class CheckDigit
{
    /// <summary>
    /// Calculate a new check digit from a given input string.
    /// </summary>
    public abstract char Calculate(string input);

    /// <summary>
    /// Append a new check digit to a given input string.
    /// </summary>
    public string Append(string input)
    {
        return input + Calculate(input);
    }

    /// <summary>
    /// Validate whether the input string has a correct check digit or not.
    /// </summary>
    public bool Validate(string input)
    {
        return input == Append(input[..^1]);
    }
}