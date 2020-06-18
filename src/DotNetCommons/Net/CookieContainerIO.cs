using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using DotNetCommons.Text;
using DotNetCommons.Temporal;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Net
{
    public static class CookieContainerIO
    {
        public static IEnumerable<Cookie> GetAllCookies(this CookieContainer container)
        {
            var domains = (Hashtable) container?.GetType().GetField("m_domainTable", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(container);
            if (domains == null)
                yield break;

            foreach (DictionaryEntry element in domains)
            {
                var items = (SortedList)element.Value.GetType().GetField("m_list", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(element.Value);
                if (items == null)
                    continue;

                foreach (var item in items)
                {
                    var collection = (CookieCollection)((DictionaryEntry)item).Value;
                    foreach (Cookie cookie in collection)
                        yield return cookie;
                }
            }
        }

        public static void ReadFrom(this CookieContainer container, Stream stream)
        {
            using var reader = new StreamReader(stream, Encoding.UTF8);

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                    continue;

                var items = line.Split('\t');
                if (items.Length < 7)
                    continue;

                var cookie = new Cookie
                {
                    Domain = items[0],
                    Path = items[2],
                    Secure = items[3].Like("TRUE"),
                    Expires = DateTimeExtensions.FromUnixSeconds(long.Parse(items[4])),
                    Name = items[5],
                    Value = ReencodeHtmlString(items[6])
                };

                container.Add(cookie);
            }
        }

        private static string ReencodeHtmlString(string s)
        {
            return s.Replace("&amp;", "%26");
        }

        public static void WriteTo(this CookieContainer container, Stream stream)
        {
            using var writer = new StreamWriter(stream, Encoding.UTF8);

            foreach (var cookie in container.GetAllCookies())
            {
                writer.Write(cookie.Domain);
                writer.Write('\t');
                writer.Write(cookie.Domain.StartsWith(".") ? "TRUE" : "FALSE");
                writer.Write('\t');
                writer.Write(cookie.Path);
                writer.Write('\t');
                writer.Write(cookie.Secure ? "TRUE" : "FALSE");
                writer.Write('\t');
                writer.Write(cookie.Expires.ToUnixSeconds());
                writer.Write('\t');
                writer.Write(cookie.Name);
                writer.Write('\t');
                writer.Write(cookie.Value);
                writer.Write('\n');
            }
        }
    }
}
