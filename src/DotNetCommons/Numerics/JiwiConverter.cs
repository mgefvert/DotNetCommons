using DotNetCommons.CheckDigits;

namespace DotNetCommons.Numerics;

public static class JiwiConverter
{
    private const string L1 = "bcdfghjklmnprstvwyz";
    private const string L2 = "aeiou";
    
    private static readonly List<string> Syllables;
    private static readonly LuhnCheckDigits CheckDigits = new();

    static JiwiConverter()
    {
        Syllables = Initialize();
    }
    
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
    
    public static long FromJiwi(string id)
    {
        var workingId = id.Replace("-", "");
        if (workingId.IsEmpty() || !char.IsDigit(workingId[^1]))
            throw NotValid();

        var checkDigit = workingId[^1];
        var checkNum = (checkDigit - '0') + 1;
        var result = 0L;
        workingId = workingId.Left(-1).ToLowerInvariant();
        var max = workingId.Length - 1;

        for (var i = 0; i <= max; )
        {
            if (char.IsDigit(workingId[i]))
            {
                result *= 10;
                result += workingId[i] - '0';
                i++;
                continue;
            }

            if (i <= max - 1 && workingId[i] == 'j' && workingId[i + 1] == 'w')
            {
                i += 2;
                continue;
            }

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

        result -= 1 + checkNum * 7;

        var resultString = result.ToString();
        var realCheckDigit = CheckDigits.Calculate(resultString);
        if (checkDigit != realCheckDigit)
            throw ChecksumIncorrect(realCheckDigit);
        
        return result;

        ArgumentException ChecksumIncorrect(char n) => new($"'{id}' is not a valid jiwi number; incorrect check digit, expected '{n}'.");
        ArgumentException NotValid() => new($"'{id}' is not a valid jiwi number.");
    }

    public static string ToJiwi(long id)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(id, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(id, long.MaxValue - 1_000_000);

        var ids = id.ToString();
        var checkDigit = CheckDigits.Calculate(ids);
        var checkNum = (checkDigit - '0') + 1;

        id += 1 + checkNum * 7;
        
        var buf = new List<string> { checkDigit.ToString() };
        var c = 0;
        while (id != 0)
        {
            if (c++ % 3 == 0)
            {
                buf.Add($"{id % 10}-");
                id /= 10;
            }
            else
            {
                var n = (int)(id % Syllables.Count);
                id /= Syllables.Count;
                buf.Add(Syllables[n]);
            }
        }

        if (char.IsDigit(buf.Last()[0]))
            buf.Add("jw");
        buf.Reverse();
		
        return string.Join("", buf);
    }
}