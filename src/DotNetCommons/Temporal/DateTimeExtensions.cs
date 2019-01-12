using System;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Temporal
{
    public static class DateTimeExtensions
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly DateTimeOffset UnixEpochOffset = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public static DateTime EndOfMonth(this DateTime datetime)
        {
            return StartOfMonth(datetime).AddMonths(1).AddDays(-1);
        }

        public static DateTime EndOfWeek(this DateTime datetime, DayOfWeek firstDayOfWeek = DayOfWeek.Monday)
        {
            return StartOfWeek(datetime, firstDayOfWeek).AddDays(6);
        }

        public static DateTime EndOfYear(this DateTime datetime)
        {
            return StartOfYear(datetime).AddYears(1).AddDays(-1);
        }

        public static DateTime FromUnixSeconds(long timestamp)
        {
            return UnixEpoch.AddSeconds(timestamp).ToLocalTime();
        }

        public static DateTime FromUnixMilliseconds(long millisTimestamp)
        {
            return UnixEpoch.AddMilliseconds(millisTimestamp).ToLocalTime();
        }

        public static DateTimeOffset FromUnixSecondsOffset(long timestamp)
        {
            return UnixEpochOffset.AddSeconds(timestamp);
        }

        public static DateTimeOffset FromUnixMillisecondsOffset(long timestamp)
        {
            return UnixEpochOffset.AddMilliseconds(timestamp);
        }

        public static bool IsBetween(this DateTime datetime, DateTime? dateStart, DateTime? dateEnd)
        {
            return datetime >= (dateStart ?? DateTime.MinValue) && datetime < (dateEnd ?? DateTime.MaxValue);
        }

        public static int OADay(this DateTime datetime)
        {
            return (int)datetime.ToOADate();
        }

        public static DateTime StartOfMonth(this DateTime datetime)
        {
            return new DateTime(datetime.Year, datetime.Month, 1);
        }

        public static DateTime StartOfWeek(this DateTime datetime, DayOfWeek firstDayOfWeek = DayOfWeek.Monday)
        {
            datetime = datetime.Date;
            while (datetime.DayOfWeek != firstDayOfWeek)
                datetime = datetime.AddDays(-1);

            return datetime;
        }

        public static DateTime StartOfYear(this DateTime datetime)
        {
            return new DateTime(datetime.Year, 1, 1);
        }

        public static long ToUnixSeconds(this DateTime datetime)
        {
            return (long)(datetime.ToUniversalTime() - UnixEpoch).TotalSeconds;
        }

        public static long ToUnixMilliseconds(this DateTime datetime)
        {
            return (long)(datetime.ToUniversalTime() - UnixEpoch).TotalMilliseconds;
        }

        public static long ToUnixSeconds(this DateTimeOffset datetime)
        {
            return (long)(datetime - UnixEpochOffset).TotalSeconds;
        }

        public static long ToUnixMilliseconds(this DateTimeOffset datetime)
        {
            return (long)(datetime - UnixEpochOffset).TotalMilliseconds;
        }

        public static string ToISO8601String(this DateTime datetime)
        {
            return datetime.ToString("yyyy-MM-dd'T'HH:mm:ssK");
        }
    }
}
