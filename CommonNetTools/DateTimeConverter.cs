using System;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT

namespace CommonNetTools
{
  public static class DateTimeConverter
  {
    private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTimeOffset UnixEpochOffset = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static DateTime DateTime(long timestamp)
    {
      return UnixEpoch.AddSeconds(timestamp).ToLocalTime();
    }

    public static DateTime DateTimeMillis(long millisTimestamp)
    {
      return UnixEpoch.AddMilliseconds(millisTimestamp).ToLocalTime();
    }

    public static DateTimeOffset DateTimeOffset(long timestamp)
    {
      return UnixEpochOffset.AddSeconds(timestamp);
    }

    public static DateTimeOffset DateTimeOffsetMillis(long timestamp)
    {
      return UnixEpochOffset.AddMilliseconds(timestamp);
    }

    public static long Timestamp(this DateTime datetime)
    {
      return (long)(datetime.ToUniversalTime() - UnixEpoch).TotalSeconds;
    }

    public static long TimestampMillis(this DateTime datetime)
    {
      return (long)(datetime.ToUniversalTime() - UnixEpoch).TotalMilliseconds;
    }

    public static long Timestamp(this DateTimeOffset datetime)
    {
      return (long)(datetime - UnixEpochOffset).TotalSeconds;
    }

    public static long TimestampMillis(this DateTimeOffset datetime)
    {
      return (long)(datetime - UnixEpochOffset).TotalMilliseconds;
    }
  }
}
