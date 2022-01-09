using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Security;

public static class Passwords
{
    private class PasswordLayout
    {
        public string Alphabet;
        public int Repeat;
    }

    private static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();

    private const string AlphaLowercase = "abcdefghijklmnopqrstuvwxyz";
    private const string AlphaUppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string HexLowercase = "0123456789abcdef";
    private const string HexUppercase = "0123456789ABCDEF";
    private const string Friendly = "abcdefghijkmnopqrstwxyzABCDEFGHJKLMNPQRSTWXYZ23456789";
    private const string Digits = "0123456789";
    private const string Punctuation = ",.;:?!@#$*-_+=";

    private const string AlphaMixedCase = AlphaLowercase + AlphaUppercase;
    private const string FullAlphabet = AlphaLowercase + AlphaUppercase + Digits;
    private const string AnyCharacter = AlphaLowercase + AlphaUppercase + Digits + Punctuation;

    public static int Complexity { get; set; } = 12;

    private static string ComputeHash(string plaintext, byte[] salt, int complexity)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(plaintext, salt, 2 << complexity);
        using var mem = new MemoryStream();
        using var writer = new BinaryWriter(mem);

        writer.Write((byte)complexity);
        writer.Write(salt);

        var data = pbkdf2.GetBytes(64);
        writer.Write(data.Length);
        writer.Write(data);

        return Convert.ToBase64String(mem.ToArray());
    }

    private static char DrawLetter(int data, string alphabet)
    {
        return alphabet[data % alphabet.Length];
    }

    public static byte[] GetSalt(int bytes)
    {
        var result = new byte[bytes];
        Rng.GetBytes(result, 0, bytes);
        return result;
    }

    public static string Encrypt(string plaintext)
    {
        return ComputeHash(plaintext, GetSalt(8), Complexity);
    }

    public static bool Verify(string encryptedPassword, string plaintext)
    {
        if (string.IsNullOrWhiteSpace(encryptedPassword) || string.IsNullOrWhiteSpace(plaintext))
            return false;

        using var mem = new MemoryStream(Convert.FromBase64String(encryptedPassword));
        using var reader = new BinaryReader(mem);

        var complexity = reader.ReadByte();
        var salt = reader.ReadBytes(8);

        var test = ComputeHash(plaintext, salt, complexity);

        return string.CompareOrdinal(encryptedPassword, test) == 0;
    }

    /*
        Creates a new password according to a given layout, consisting of the
        following characters:
          A      Alphabetic character (A-Z, a-z)
          C      Alphabetic character, uppercase only (A-Z)
          c      Alphabetic character, lowercase only (a-z)
          F      Friendly characters or digits, omitting hard-to-read ones (l, 1, I etc)
          H      Uppercase hex digit (0-9, A-F)
          h      Lowercase hex digit (0-9, a-f)
          N      Digit (0-9)
          X      Any character or digit
          p      Punctuation (,.;:?!@#$*-+=)
          Z      Any character, digit, or punctuation
          -      Dash
          _      Underscore
    */
    public static string GeneratePassword(string template)
    {
        return GeneratePasswordFromLayout(GeneratePasswordLayout(template), Rng);
    }

    public static string[] GeneratePassword(string template, int count)
    {
        var layout = GeneratePasswordLayout(template);
        var result = new string[count];
        for (var i = 0; i < count; i++)
            result[i] = GeneratePasswordFromLayout(layout, Rng);

        return result;
    }

    private static string GeneratePasswordFromLayout(List<PasswordLayout> layout, RandomNumberGenerator rng)
    {
        var result = new StringBuilder(layout.Sum(x => x.Repeat));
        var bytes = new byte[2];

        foreach (var item in layout)
            for (var i = 0; i < item.Repeat; i++)
            {
                if (item.Alphabet.Length == 1)
                    result.Append(item.Alphabet);
                else
                {
                    rng.GetBytes(bytes);
                    var value = BitConverter.ToUInt16(bytes, 0);
                    result.Append(DrawLetter(value, item.Alphabet));
                }
            }

        return result.ToString();
    }

    private static List<PasswordLayout> GeneratePasswordLayout(string template)
    {
        if (string.IsNullOrEmpty(template))
            throw new InvalidOperationException("Password template may not be blank.");

        var layout = new List<PasswordLayout>();
        foreach (var c in template)
        {
            string alphabet;
            switch (c)
            {
                case 'A': alphabet = AlphaMixedCase; break;
                case 'C': alphabet = AlphaUppercase; break;
                case 'c': alphabet = AlphaLowercase; break;
                case 'F': alphabet = Friendly; break;
                case 'H': alphabet = HexUppercase; break;
                case 'h': alphabet = HexLowercase; break;
                case 'N': alphabet = Digits; break;
                case 'X': alphabet = FullAlphabet; break;
                case 'p': alphabet = Punctuation; break;
                case 'Z': alphabet = AnyCharacter; break;

                case '-':
                case '_':
                    alphabet = c.ToString();
                    break;

                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    var item = layout.LastOrDefault();
                    if (item == null)
                        throw new InvalidOperationException("Invalid password template.");
                    item.Repeat = item.Repeat == -1 ? c - '0' : item.Repeat * 10 + (c - '0');
                    continue;

                default:
                    throw new InvalidOperationException($"Invalid character '{c}' in password template.");
            }

            layout.Add(new PasswordLayout { Alphabet = alphabet, Repeat = -1 });
        }

        layout.ForEach(x => x.Repeat = Math.Abs(x.Repeat));

        return layout;
    }

    public static string RandomKey(int length)
    {
        if (length <= 0)
            throw new ArgumentOutOfRangeException(nameof(length), "GenerateRandomKey length must be more than zero.");

        var data = new byte[length];
        Rng.GetBytes(data);

        var result = new StringBuilder(length);
        foreach (var b in data)
            result.Append(FullAlphabet[b % FullAlphabet.Length]);

        return result.ToString();
    }
}