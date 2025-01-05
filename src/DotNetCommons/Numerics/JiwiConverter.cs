using DotNetCommons.CheckDigits;

namespace DotNetCommons.Numerics;

/// <summary>
/// A Jiwi is a representation of a number in letter-like format. For instance, 4711 is represented
/// as 'mafy8'. This exists because Jiwis are a lot easier to read than long numerical ID numbers,
/// so where accuracy matters - on the command line, for instance, referencing a specific transaction
/// or file - you can use Jiwis instead of IDs. Jiwis also have Mod10 check digit embedded, which
/// adds to the safety; a misspelling has a 90% chance of being flagged as invalid. 
/// </summary>
public static class JiwiConverter
{
    /// <summary>
    /// First letter options in each syllable
    /// </summary>
    private const string L1 = "bcdfghjklmnprstvwz";
    
    /// <summary>
    /// Second letter options in each syllable
    /// </summary>
    private const string L2 = "aeiouy";
    
    private static readonly List<string> Syllables;
    private static readonly LuhnCheckDigits CheckDigits = new();

    /// <summary>
    /// Total count of syllables available. This is fixed and should never change.
    /// </summary>
    public static int SyllableCount => Syllables.Count;
    
    static JiwiConverter()
    {
        Syllables = Initialize();
    }
    
    /// <summary>
    /// Initialize the JiwiConverter by creating all syllables. Note that it uses a
    /// highly predictive randomizer (LcgRandomizer) to generate a randomly shuffled
    /// list of syllables, in order to impose a certain variety on the generated
    /// numbers. This is regarded as part of the specification and must not change. 
    /// </summary>
    private static List<string> Initialize()
    {
        var random = new LcgRandomizer(4711);
        var shuffle = (from l1 in L1 from l2 in L2 select $"{l1}{l2}").ToList();
        var words = new List<string>();
        while (shuffle.Any())
        {
            var n = random.Next(shuffle.Count);
            words.Add(shuffle[n]);
            shuffle.RemoveAt(n);
        }

        return words;
    }

    /// <summary>
    /// Convert a Jiwi to a number. Formatting may be mixed case, and may include hyphens or not.
    /// Whitespaces and other foreign characters are not allowed.
    /// </summary>
    /// <param name="id">The string forming the Jiwi.</param>
    /// <returns>The decoded ID.</returns>
    /// <exception cref="ArgumentException">Argument exceptions are thrown for Jiwis that are not
    ///     decoded properly or that has an invalid check digit.</exception>
    public static long FromJiwi(string id)
    {
        // Basic sanity checks
        var workingId = id.Replace("-", "").ToLowerInvariant();
        if (workingId.IsEmpty() || !char.IsDigit(workingId[^1]))
            throw NotValid();

        // Extract check digit, it's not part of the conversion
        var checkDigit = workingId[^1];
        var checkNum = (checkDigit - '0') + 1;
        var result = 0L;
        workingId = workingId.Left(-1);
        var max = workingId.Length - 1;

        // Iterate through the string, decoding either numbers or syllables
        for (var i = 0; i <= max; )
        {
            // Convert digit
            if (char.IsDigit(workingId[i]))
            {
                result *= 10;
                result += workingId[i] - '0';
                i++;
                continue;
            }

            // "jw" is used as an empty padding for when strings start with a number, discard
            if (i <= max - 1 && workingId[i] == 'j' && workingId[i + 1] == 'w')
            {
                i += 2;
                continue;
            }

            // Extract and find syllable
            string? syllable;
            if (i <= max - 1 && L1.Contains(workingId[i]) && L2.Contains(workingId[i+1]))
                syllable = workingId.Substring(i, 2);
            else
                throw NotValid();

            var syllableIndex = Syllables.IndexOf(syllable);
            if (syllableIndex == -1)
                throw NotValid();

            result *= Syllables.Count;
            result += syllableIndex;
            i += syllable.Length;
        }

        // The result is "padded" back to the original value
        result -= 1 + checkNum * 7;

        // Verify the check digit
        var resultString = result.ToString();
        var realCheckDigit = CheckDigits.Calculate(resultString);
        if (checkDigit != realCheckDigit)
            throw ChecksumIncorrect(realCheckDigit);
        
        return result;

        ArgumentException ChecksumIncorrect(char n) => new($"'{id}' is not a valid jiwi number; incorrect check digit, expected '{n}'.");
        ArgumentException NotValid() => new($"'{id}' is not a valid jiwi number.");
    }

    /// <summary>
    /// Convert a long integer to a well-formatted Jiwi.
    /// </summary>
    /// <param name="id">A positive 32-bit or 64-bit integer.</param>
    /// <returns>A well-formatted Jiwi.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the ID is less than zero or touching the upper
    ///     bound of long integers.</exception>
    public static string ToJiwi(long id)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(id, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(id, long.MaxValue - 1_000);

        // Calculate check digit
        var ids = id.ToString();
        var checkDigit = CheckDigits.Calculate(ids);
        var checkNum = (checkDigit - '0') + 1;

        // Pad the number slightly, to introduce some variability and to avoid pure zeroes
        id += 1 + checkNum * 7;
        
        // Start by adding the final check digit
        var buf = new List<string> { checkDigit.ToString() };
        
        var c = 0;
        while (id != 0)
        {
            if (c++ % 3 == 2)
            {
                // Every 3rd position should be a digit and hyphen 
                buf.Add($"{id % 10}-");
                id /= 10;
            }
            else
            {
                // Look up the syllable and add
                var n = (int)(id % Syllables.Count);
                id /= Syllables.Count;
                buf.Add(Syllables[n]);
            }
        }

        // If the last added field is a digit, add a meaningless "jw" to make it start with letters instead 
        if (char.IsDigit(buf.Last()[0]))
            buf.Add("jw");
        buf.Reverse();
		
        return string.Join("", buf);
    }
}