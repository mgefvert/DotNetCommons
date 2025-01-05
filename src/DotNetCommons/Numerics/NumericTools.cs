namespace DotNetCommons.Numerics;

public static class NumericTools
{
    public static IEnumerable<int> FindFactors(this int number)
    {
        var root = (int)Math.Sqrt(number);
        
        for (var i = 2; i <= root; i++)
        {
            if (number % i != 0) 
                continue;

            yield return i;
            if (i != root)
                yield return number / i;
        }
    }

	/// <summary>
    /// Render a sequence of unordered numbers to a sorted list, like "1-3, 5, 9, 13-17 and 20-21"
    /// </summary>
    public static string ToDelimitedSequence(IEnumerable<int> number, string andWord = "and")
    {
        var ordered = number.OrderBy(x => x).ToList();
        if (ordered.Count == 0)
            return "";
        if (ordered.Count == 1)
            return ordered.Single().ToString();

        var result = new List<string>();
        var start = ordered[0];
        var end = start;

        for (int i = 1; i < ordered.Count; i++)
        {
            if (ordered[i] == end + 1)
                end = ordered[i];
            else
            {
                result.Add(GetRange(start, end));
                start = end = ordered[i];
            }
        }
        
        result.Add(GetRange(start, end));

        return result.Count <= 1
            ? result.SingleOrDefault() ?? ""
            : string.Join(", ", result.Take(result.Count - 1)) + $" {andWord} {result.Last()}";

        string GetRange(int n1, int n2) => n1 == n2 ? n1.ToString() : $"{n1}-{n2}";
    }

    /// <summary>
    /// Transform a number into its ordinal text, e.g. 1 => 1st, 5 => 5th, 23 => 23rd.
    /// </summary>
    /// <remarks>
    /// From https://stackoverflow.com/questions/20156/is-there-an-easy-way-to-create-ordinals-in-c
    /// </remarks>
    public static string ToOrdinal(int? number)
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