using System;
using System.Security.Cryptography;
using System.Text;

namespace CommonNetTools
{
    public static class Passwords
    {
        private static readonly RNGCryptoServiceProvider Rng = new RNGCryptoServiceProvider();

        private const string AlphaLowercase = "abcdefghijkmnpqrstwxyz";
        private const string AlphaUppercase = "ABCDEFGHJKLMNPQRSTWXYZ";
        private const string Digits = "23456789";
        private const string Punctuation = ",.;:?!@#$*-_+=";

        private const string AlphaMixedCase = AlphaLowercase + AlphaUppercase;
        private const string FullAlphabet = AlphaLowercase + AlphaUppercase + Digits;
        private const string AnyCharacter = AlphaLowercase + AlphaUppercase + Digits + Punctuation;

        private static char Draw(int data, string alphabet)
        {
            return alphabet[data % alphabet.Length];
        }

        public static string GeneratePassword()
        {
            return GeneratePassword("ccXX-cXXc-XXcc");
        }

        /*
            Creates a new password according to a given layout, consisting of the
            following characters:
              A      Alphabetic character (A-Z, a-z)
              C      Alphabetic character, uppercase only (A-Z)
              c      Alphabetic character, lowercase only (a-z)
              N      Digit (0-9)
              X      Any character or digit
              p      Punctuation (,.;:?!@#$*-+=)
              Z      Any character, digit, or punctuation
              -      Dash
              _      Underscore
        */
        public static string GeneratePassword(string template)
        {
            if (string.IsNullOrEmpty(template))
                throw new InvalidOperationException("Password template may not be blank");

            var result = new StringBuilder(template.Length);
            var data = new byte[template.Length * 2];
            Rng.GetBytes(data);

            for (int i=0; i<template.Length; i++)
            {
                var z = (data[i*2] << 16) | data[i*2 + 1];
                char c = template[i];
                switch (c)
                {
                    case 'A': c = Draw(z, AlphaMixedCase); break;
                    case 'C': c = Draw(z, AlphaUppercase); break;
                    case 'c': c = Draw(z, AlphaLowercase); break;
                    case 'N': c = Draw(z, Digits); break;
                    case 'X': c = Draw(z, FullAlphabet); break;
                    case 'p': c = Draw(z, Punctuation); break;
                    case 'Z': c = Draw(z, AnyCharacter); break;

                    case '-': 
                    case '_':
                        break;

                    default:
                        throw new InvalidOperationException($"Invalid character '{c}' in password template");
                }

                result.Append(c);
            }

            return result.ToString();
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
}
