namespace DotNetCommons.CheckDigits
{
    /// <summary>
    /// Check digit calculations.
    /// </summary>
    public interface ICheckDigits
    {
        /// <summary>
        /// Calculate a new check digit from a given input string.
        /// </summary>
        char Calculate(string input);

        /// <summary>
        /// Append a new check digit to a given input string.
        /// </summary>
        string Append(string input) => input + Calculate(input);

        /// <summary>
        /// Validate whether the input string has a correct check digit or not.
        /// </summary>
        bool Validate(string input) => input == Append(input[0..^1]);
    }
}
