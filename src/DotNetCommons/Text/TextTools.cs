using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Text
{
    public static class TextTools
    {
        public static readonly Encoding Win1252 = Encoding.GetEncoding(1252);

        private static readonly string[] HighBitsXlate =
        {
            /* 0x80 */ null, null, "\'", "f",  "\"", "...", null, null, "^",  null,   "S",  "<", "OE",  null,  "Z",   null,
            /* 0x90 */ null, "\'", "\'", "\"", "\"", "*",   "-",  "-",  "~",  "(tm)", "s",  ">", "oe",  null,  "z",   "Y",
            /* 0xA0 */ " ",  "!",  "c",  null, "$",  null,  "|",  null, null, "(c)",  null, "<", null,  null,  "(r)", null,
            /* 0xB0 */ null, "+-", "^2", "^3", "\'", "u",   "pi", "*",  null, "^1",   null, ">", "1/4", "1/2", "3/4", "?",
            /* 0xC0 */ "A",  "A",  "A",  "A",  "AE", "A",   "AE", "C",  "E",  "E",    "E",  "E", "I",   "I",   "I",   "I",
            /* 0xD0 */ "DH", "N",  "O",  "O",  "O",  "O",   "OE", "x",  "O",  "U",    "U",  "U", "U",   "Y",   "TH",  "ss",
            /* 0xE0 */ "a",  "a",  "a",  "a",  "ae", "a",   "ae", "c",  "e",  "e",    "e",  "e", "i",   "i",   "i",   "i",
            /* 0xF0 */ "dh", "n",  "o",  "o",  "o",  "o",   "oe", "/",  "o",  "u",    "u",  "u", "u",   "y",   "th",  "y"
        };

        /// <summary>
        /// Turn text into ASCII (7-bit), using some letter transformations as necessary. Handles ANSI and UTF8 gracefully.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Asciify(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var textbuffer = Encoding.UTF8.GetBytes(text);
            if (textbuffer.All(x => x <= 127))
                return text;

            // First, convert the string down to ANSI.
            var resultbuffer = Encoding.Convert(Encoding.UTF8, Win1252, textbuffer);

            // Now translate all characters down to ASCII.
            var result = new StringBuilder(resultbuffer.Length);
            foreach (var c in resultbuffer)
            {
                if (c <= 127)
                    result.Append((char)c);
                else
                    result.Append(HighBitsXlate[c - 128]);
            }

            return result.ToString();
        }

        /// <summary>
        /// Try to determine the encoding for text stored in a byte buffer. Differentiates
        /// between ASCII, ANSI and UTF8.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static Encoding DetermineEncoding(byte[] buffer)
        {
            return DetermineEncoding(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Try to determine the encoding for text stored in a byte buffer. Differentiates
        /// between ASCII, Win1252 and UTF8.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static Encoding DetermineEncoding(byte[] buffer, int offset, int length)
        {
            var highbits = false;
            for (int i = offset; i < offset + length; i++)
                if (buffer[i] >= 128)
                {
                    highbits = true;
                    break;
                }

            if (highbits == false)
                return Encoding.ASCII;

            if (InternalValidateUtf8(buffer, offset, length))
                return Encoding.UTF8;

            return Win1252;
        }

        /// <summary>
        /// Find a suitable word break in a string, given the maximum length of a single row of text.
        /// For instance, can break down text to fit on an 80-column wide screen.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="maxlen"></param>
        /// <returns></returns>
        public static int FindWordBreak(string text, int maxlen)
        {
            var i = Math.Min(text.Length - 1, maxlen);

            while (i > 1)
            {
                if (char.IsWhiteSpace(text[i]))
                    return i;
                i--;
            }

            return maxlen;
        }

        /// <summary>
        /// Determine if a byte buffer is valid UTF8.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool IsUtf8Valid(byte[] data)
        {
            return InternalValidateUtf8(data, 0, data.Length);
        }

        /// <summary>
        /// Determine if a byte buffer is valid UTF8.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static bool IsUtf8Valid(byte[] data, int offset, int length)
        {
            return InternalValidateUtf8(data, offset, length);
        }

        /// <summary>
        /// Determine the levenshtein distance between two strings.
        /// From http://michalis.site/2013/12/levenshtein/
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        public static int Levenshtein(string a, string b, bool ignoreCase)
        {
            a ??= "";
            b ??= "";

            if (ignoreCase)
            {
                a = a.ToUpper();
                b = b.ToUpper();
            }

            if (a == b)
                return 0;

            if (a.Length == 0 || b.Length == 0)
                return a.Length + b.Length;

            var len = b.Length;
            var v0 = new int[len + 1];
            var v1 = new int[len + 1];
            for (var i = 0; i < len + 1; i++)
                v0[i] = i;

            for (var i = 0; i < a.Length; i++)
            {
                v1[0] = i + 1;
                for (var j = 0; j < len; j++)
                {
                    var cost = a[i] == b[j] ? 0 : 1;
                    v1[j + 1] = Math.Min(Math.Min(v1[+j] + 1, v0[j + 1] + 1), v0[j] + cost);
                    v0[j] = v1[j];
                }
                v0[len] = v1[len];
            }

            return v1[len];
        }

        /// <summary>
        /// Word wrap a certain text to fit the maximum width, optionally indenting the first row.
        /// </summary>
        /// <param name="text">Text to fit.</param>
        /// <param name="width">Maximum width of a single line.</param>
        /// <param name="indent">If more than zero, will indent subsequent lines but not the first line.
        ///     If less than 0, will only indent the first line.</param>
        /// <returns></returns>
        public static List<string> WordWrap(string text, int width, int indent = 0)
        {
            var indentFirst = indent < 0 ? new string(' ', -indent) : "";
            var indentNext = indent > 0 ? new string(' ', indent) : "";

            var maxlen = width - 1;
            if (string.IsNullOrEmpty(text) || text.Length < maxlen)
                return new List<string> { indentFirst + text };

            var result = new List<string>();
            while (text != "")
            {
                var indentString = result.Count == 0 ? indentFirst : indentNext;

                if (text.Length + indentString.Length < maxlen)
                {
                    result.Add(indentString + text);
                    break;
                }

                var n = FindWordBreak(text, maxlen - indentString.Length);
                result.Add(indentString + text.Mid(0, n).TrimEnd());
                text = text.Mid(n).TrimStart();
            }

            return result;
        }

        private static bool InternalValidateUtf8(byte[] data, int offset, int length)
        {
            int i = offset, len = offset + length;

            while (i < len)
            {
                if (data[i] <= 0x7F) /* 00..7F */
                {
                    i++;
                    continue;
                }

                if (data[i] >= 0xC2 && data[i] <= 0xDF) /* C2..DF 80..BF */
                {
                    if (i + 1 >= len)
                        return false;
                    if (data[i + 1] < 0x80 || data[i + 1] > 0xBF)
                        return false;

                    i += 2;
                    continue;
                }

                if (data[i] == 0xE0) /* E0 A0..BF 80..BF */
                {
                    if (i + 2 >= len)
                        return false;
                    if (data[i + 1] < 0xA0 || data[i + 1] > 0xBF)
                        return false;
                    if (data[i + 2] < 0x80 || data[i + 2] > 0xBF)
                        return false;

                    i += 3;
                    continue;
                }

                if (data[i] >= 0xE1 && data[i] <= 0xEC) /* E1..EC 80..BF 80..BF */
                {
                    if (i + 2 >= len)
                        return false;
                    if (data[i + 1] < 0x80 || data[i + 1] > 0xBF)
                        return false;
                    if (data[i + 2] < 0x80 || data[i + 2] > 0xBF)
                        return false;

                    i += 3;
                    continue;
                }

                if (data[i] == 0xED) /* ED 80..9F 80..BF */
                {
                    if (i + 2 >= len)
                        return false;
                    if (data[i + 1] < 0x80 || data[i + 1] > 0x9F)
                        return false;
                    if (data[i + 2] < 0x80 || data[i + 2] > 0xBF)
                        return false;

                    i += 3;
                    continue;
                }

                if (data[i] >= 0xEE && data[i] <= 0xEF) /* EE..EF 80..BF 80..BF */
                {
                    if (i + 2 >= len)
                        return false;
                    if (data[i + 1] < 0x80 || data[i + 1] > 0xBF)
                        return false;
                    if (data[i + 2] < 0x80 || data[i + 2] > 0xBF)
                        return false;
                    i += 3;
                    continue;
                }

                if (data[i] == 0xF0) /* F0 90..BF 80..BF 80..BF */
                {
                    if (i + 3 >= len)
                        return false;
                    if (data[i + 1] < 0x90 || data[i + 1] > 0xBF)
                        return false;
                    if (data[i + 2] < 0x80 || data[i + 2] > 0xBF)
                        return false;
                    if (data[i + 3] < 0x80 || data[i + 3] > 0xBF)
                        return false;
                    i += 4;
                    continue;
                }

                if (data[i] >= 0xF1 && data[i] <= 0xF3) /* F1..F3 80..BF 80..BF 80..BF */
                {
                    if (i + 3 >= len)
                        return false;
                    if (data[i + 1] < 0x80 || data[i + 1] > 0xBF)
                        return false;
                    if (data[i + 2] < 0x80 || data[i + 2] > 0xBF)
                        return false;
                    if (data[i + 3] < 0x80 || data[i + 3] > 0xBF)
                        return false;
                    i += 4;
                    continue;
                }

                if (data[i] == 0xF4) /* F4 80..8F 80..BF 80..BF */
                {
                    if (i + 3 >= len)
                        return false;
                    if (data[i + 1] < 0x80 || data[i + 1] > 0x8F)
                        return false;
                    if (data[i + 2] < 0x80 || data[i + 2] > 0xBF)
                        return false;
                    if (data[i + 3] < 0x80 || data[i + 3] > 0xBF)
                        return false;
                    i += 4;
                    continue;
                }

                return false;
            }

            return true;
        }
    }
}
