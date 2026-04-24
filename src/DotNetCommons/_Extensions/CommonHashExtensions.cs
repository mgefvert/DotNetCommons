using System.Security.Cryptography;
using System.Text;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons;

public static class CommonHashExtensions
{
    /// <summary>
    /// Computes a hash for the given byte array and returns it as a formatted string.
    /// </summary>
    /// <param name="hashAlgorithm">The hash algorithm to use for computing the hash.</param>
    /// <param name="buffer">The input byte array to compute the hash for.</param>
    /// <param name="format">The format string used for each byte in the computed hash. Default value is "x2".</param>
    /// <returns>A string representation of the computed hash, formatted according to the specified format.</returns>
    public static string ComputeString(this HashAlgorithm hashAlgorithm, byte[] buffer, string format = "x2")
    {
        var hash = hashAlgorithm.ComputeHash(buffer);

        var result = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
            result.Append(b.ToString(format));

        return result.ToString();
    }
}