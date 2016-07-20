using System;
using System.Globalization;

namespace Gefvert.Tools.Common
{
  public static class StringHelpers
  {
    public static string Left(this string value, int count)
    {
      return string.IsNullOrEmpty(value) ? value : Mid(value, 0, count);
    }

    public static bool Like(this string value, string compare)
    {
      return string.Equals(value, compare, StringComparison.CurrentCultureIgnoreCase);
    }

    public static string Mid(this string value, int offset)
    {
      if (string.IsNullOrEmpty(value))
        return value;

      return offset < value.Length ? value.Substring(offset) : string.Empty;
    }

    public static string Mid(this string value, int offset, int count)
    {
      if (string.IsNullOrEmpty(value))
        return value;

      var len = value.Length;
      if (offset > len)
        return string.Empty;

      if (count < len - offset)
        count -= len - offset;

      return count > 0 ? value.Substring(offset, count) : string.Empty;
    }

    public static decimal ParseDecimal(this string value, CultureInfo culture, decimal defaultValue = 0)
    {
      decimal result;
      return decimal.TryParse((value ?? "").Trim(), NumberStyles.Number, culture, out result) ? result : defaultValue;
    }

    public static decimal ParseDecimal(this string value, decimal defaultValue = 0)
    {
      return ParseDecimal(value, CultureInfo.CurrentCulture, defaultValue);
    }

    public static decimal ParseDecimalInvariant(this string value, decimal defaultValue = 0)
    {
      return ParseDecimal(value, CultureInfo.InvariantCulture, defaultValue);
    }

    public static double ParseDouble(this string value, CultureInfo culture, double defaultValue = 0)
    {
      double result;
      return double.TryParse((value ?? "").Trim(), NumberStyles.Number, culture, out result) ? result : defaultValue;
    }

    public static double ParseDouble(this string value, double defaultValue = 0)
    {
      return ParseDouble(value, CultureInfo.CurrentCulture, defaultValue);
    }

    public static double ParseDoubleInvariant(this string value, double defaultValue = 0)
    {
      return ParseDouble(value, CultureInfo.InvariantCulture, defaultValue);
    }

    public static int ParseInt(this string value, int defaultValue = 0)
    {
      int result;
      return int.TryParse((value ?? "").Trim(), out result) ? result : defaultValue;
    }

    public static long ParseLong(this string value, long defaultValue = 0)
    {
      long result;
      return long.TryParse((value ?? "").Trim(), out result) ? result : defaultValue;
    }

    public static string Right(this string value, int count)
    {
      return string.IsNullOrEmpty(value) ? value : Mid(value, value.Length - count, count);
    }
  }
}
