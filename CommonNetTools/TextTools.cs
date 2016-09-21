using System;
using System.Collections.Generic;
using System.Text;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT

namespace CommonNetTools
{
    public static class TextTools
    {
        public static Encoding DetermineEncoding(byte[] buffer)
        {
            return DetermineEncoding(buffer, 0, buffer.Length);
        }

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

            return Encoding.Default;
        }

        public static bool IsUtf8Valid(byte[] data)
        {
            return InternalValidateUtf8(data, 0, data.Length);
        }

        public static bool IsUtf8Valid(byte[] data, int offset, int length)
        {
            return InternalValidateUtf8(data, offset, length);
        }

        // From http://michalis.site/2013/12/levenshtein/
        public static int Levenshtein(string a, string b, bool ignoreCase)
        {
            a = a ?? "";
            b = b ?? "";

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
                var n = FindWordBreak(text, maxlen);

                result.Add((result.Count == 0 ? indentFirst : indentNext) + text.Mid(0, n).TrimEnd());
                text = text.Mid(n).TrimStart();
            }

            return result;
        }

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
