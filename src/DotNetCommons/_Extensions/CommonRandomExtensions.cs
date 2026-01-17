// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons;

public static class CommonRandomInfoExtensions
{
    /// <summary>
    /// Generates a random value from a normal (Gaussian) distribution with the specified mean and standard deviation.
    /// </summary>
    /// <param name="rng">The random number generator to use.</param>
    /// <param name="stdDev">The standard deviation (σ) of the normal distribution.</param>
    /// <param name="mean">The mean (μ) of the normal distribution.</param>
    /// <returns>A random double value from the normal distribution with the specified mean and standard deviation.</returns>
    public static double GenerateNormal(this Random rng, double stdDev = 1, double mean = 0)
    {
        // Avoid 0 because log(0) is -inf.
        var u1 = 1.0 - rng.NextDouble(); // (0,1]
        var u2 = 1.0 - rng.NextDouble(); // (0,1]
        var rnd = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);

        return mean + stdDev * rnd;
    }
}