namespace DotNetCommons.Numerics;

/// <summary>
/// Class that generates predictable number series from a specific seed.
/// Not to be used for any actual random work. Based on a Linear Congruential Generator (LCG).
/// </summary>
public class LcgRandomizer
{
    private long _seed;
    private const long Multiplier = 6364136223846793005;
    private const long Modulus = (1L << 31);

    public LcgRandomizer(long seed)
    {
        _seed = seed;
    }

    public int Next(int maxValue) => Next(0, maxValue);

    public int Next(int minValue, int maxValue)
    {
        _seed = (_seed * Multiplier + 1) % Modulus;
        return (int)Math.Abs(_seed % (maxValue - minValue) + minValue);
    }
}