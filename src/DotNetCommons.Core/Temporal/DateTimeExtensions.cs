using System;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Core.Temporal
{
    public enum ISO8601Format
    {
        Date,
        DateTime,
        DateTimeOffset
    }

    public static class DateTimeExtensions
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly DateTimeOffset UnixEpochOffset = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        /// <summary>
        /// Calculate the end of month (e.g. 2019-06-30).
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static DateTime EndOfMonth(this DateTime datetime)
        {
            return StartOfMonth(datetime).AddMonths(1).AddDays(-1);
        }

        /// <summary>
        /// Calculate the end of week (e.g. Sunday on the given week).
        /// </summary>
        /// <param name="datetime"></param>
        /// <param name="firstDayOfWeek"></param>
        /// <returns></returns>
        public static DateTime EndOfWeek(this DateTime datetime, DayOfWeek firstDayOfWeek = DayOfWeek.Monday)
        {
            return StartOfWeek(datetime, firstDayOfWeek).AddDays(6);
        }

        /// <summary>
        /// Calculate the end of year (e.g. 2019-12-31).
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static DateTime EndOfYear(this DateTime datetime)
        {
            return StartOfYear(datetime).AddYears(1).AddDays(-1);
        }

        /// <summary>
        /// Return the local datetime from a UTC unix timestamp.
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static DateTime FromUnixSeconds(long timestamp)
        {
            return UnixEpoch.AddSeconds(timestamp).ToLocalTime();
        }

        /// <summary>
        /// Return the local date from a UTC millisecond unix timestamp.
        /// </summary>
        /// <param name="millisTimestamp"></param>
        /// <returns></returns>
        public static DateTime FromUnixMilliseconds(long millisTimestamp)
        {
            return UnixEpoch.AddMilliseconds(millisTimestamp).ToLocalTime();
        }

        /// <summary>
        /// Return a UTC DateTimeOffset from a unix timestamp.
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static DateTimeOffset FromUnixSecondsOffset(long timestamp)
        {
            return UnixEpochOffset.AddSeconds(timestamp);
        }

        /// <summary>
        /// Return a UTC DateTimeOffset from a unix millisecond timestam.
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static DateTimeOffset FromUnixMillisecondsOffset(long timestamp)
        {
            return UnixEpochOffset.AddMilliseconds(timestamp);
        }

        /// <summary>
        /// Verify if a DateTime is between two optional datetimes (endpoint exclusive).
        /// </summary>
        /// <param name="datetime"></param>
        /// <param name="dateStart"></param>
        /// <param name="dateEnd"></param>
        /// <returns></returns>
        public static bool IsBetween(this DateTime datetime, DateTime? dateStart, DateTime? dateEnd)
        {
            return datetime >= (dateStart ?? DateTime.MinValue) && datetime < (dateEnd ?? DateTime.MaxValue);
        }

        /// <summary>
        /// Return an Excel DateTime value for the current day, time ignored.
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static int OADay(this DateTime datetime)
        {
            return (int)datetime.ToOADate();
        }

        /// <summary>
        /// Calculate the start of the month.
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static DateTime StartOfMonth(this DateTime datetime)
        {
            return new DateTime(datetime.Year, datetime.Month, 1);
        }

        /// <summary>
        /// Calculate the start of the week.
        /// </summary>
        /// <param name="datetime"></param>
        /// <param name="firstDayOfWeek"></param>
        /// <returns></returns>
        public static DateTime StartOfWeek(this DateTime datetime, DayOfWeek firstDayOfWeek = DayOfWeek.Monday)
        {
            datetime = datetime.Date;
            while (datetime.DayOfWeek != firstDayOfWeek)
                datetime = datetime.AddDays(-1);

            return datetime;
        }

        /// <summary>
        /// Calculate the start of the year.
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static DateTime StartOfYear(this DateTime datetime)
        {
            return new DateTime(datetime.Year, 1, 1);
        }

        /// <summary>
        /// Return a UTC unix timestamp.
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static long ToUnixSeconds(this DateTime datetime)
        {
            return (long)(datetime.ToUniversalTime() - UnixEpoch).TotalSeconds;
        }

        /// <summary>
        /// Return a UTC unix millisecond timestamp.
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static long ToUnixMilliseconds(this DateTime datetime)
        {
            return (long)(datetime.ToUniversalTime() - UnixEpoch).TotalMilliseconds;
        }

        /// <summary>
        /// Return a UTC unix timestamp.
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static long ToUnixSeconds(this DateTimeOffset datetime)
        {
            return (long)(datetime - UnixEpochOffset).TotalSeconds;
        }

        /// <summary>
        /// Return a UTC unix millisecond timestamp.
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static long ToUnixMilliseconds(this DateTimeOffset datetime)
        {
            return (long)(datetime - UnixEpochOffset).TotalMilliseconds;
        }

        /// <summary>
        /// Return an ISO-8601 time string (e.g. 2019-06-01T12:00:00-04:00).
        /// </summary>
        /// <param name="datetime"></param>
        /// <param name="format">Format to use, default is datetime including timezone offset.</param>
        /// <returns></returns>
        public static string ToISO8601String(this DateTime datetime, ISO8601Format format = ISO8601Format.DateTimeOffset)
        {
            switch (format)
            {
                case ISO8601Format.Date:
                    return datetime.ToString("yyyy-MM-dd");
                case ISO8601Format.DateTime:
                    return datetime.ToString("yyyy-MM-dd'T'HH:mm:ss");
                case ISO8601Format.DateTimeOffset:
                    return datetime.ToString("yyyy-MM-dd'T'HH:mm:ssK");
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }
    }
}
