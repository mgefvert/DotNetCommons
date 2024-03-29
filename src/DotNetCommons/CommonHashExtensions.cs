﻿using System.Security.Cryptography;
using System.Text;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons;

public static class CommonHashExtensions
{
    public static string ComputeString(this HashAlgorithm hashAlgorithm, byte[] buffer, string format = "x2")
    {
        var hash = hashAlgorithm.ComputeHash(buffer);

        var result = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
            result.Append(b.ToString(format));

        return result.ToString();
    }
}