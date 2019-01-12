using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Temporal
{
    /// <summary>
    /// Helper class for working with datetime values and timezones.
    /// </summary>
    public static class DateTimeTools
    {
        private static void IterateOver<T1, T2>(object obj, Func<T1, T1> action1, Func<T2, T2> action2)
        {
            foreach (var p in obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanRead && p.CanWrite))
                if (p.PropertyType == typeof(T1))
                {
                    var value = (T1)p.GetValue(obj);
                    p.SetValue(obj, action1(value));
                }
                else if (p.PropertyType == typeof(T2))
                {
                    var value = (T2)p.GetValue(obj);
                    p.SetValue(obj, action2(value));
                }
        }

        /// <summary>
        /// Convert all DateTimeOffset in an object to local time, altering
        /// the time as necessary to the reflect the new timezone.
        /// </summary>
        /// <param name="obj"></param>
        public static void ConvertDateTimeOffsetsToLocal(object obj)
        {
            IterateOver<DateTimeOffset, DateTimeOffset?>(obj, dt => dt.ToLocalTime(), dt => dt?.ToLocalTime());
        }

        /// <summary>
        /// Convert all DateTimeOffset in an object to UTC, altering
        /// the time as necessary to the reflect the new timezone.
        /// </summary>
        /// <param name="obj"></param>
        public static void ConvertDateTimeOffsetsToUtc(object obj)
        {
            IterateOver<DateTimeOffset, DateTimeOffset?>(obj, dt => dt.ToUniversalTime(), dt => dt?.ToUniversalTime());
        }

        /// <summary>
        /// Forces all DateTime properties in an object to use the Unspecified timezone format. This
        /// means that it becomes effectively timezone agnostic, and won't use timezone handling in 
        /// JSON properties.
        /// </summary>
        /// <param name="obj"></param>
        public static void ForceUnspecifiedDateTimes(object obj)
        {
            IterateOver<DateTime, DateTime?>(obj,
              dt => dt.ForceKind(DateTimeKind.Unspecified),
              dt => dt != null ? (DateTime?)ForceKind(dt.Value, DateTimeKind.Unspecified) : null);
        }

        /// <summary>
        /// Force DateTimeOffset properties in an object to the UTC timezone. This is necessary when
        /// loading time data from the database, because the database doesn't understand timezones and
        /// Dapper will automatically assume the Local timezone. This converts it to UTC without
        /// changing the time.
        /// </summary>
        /// <param name="obj"></param>
        public static void ForceUtcDateTimeOffsets(object obj)
        {
            IterateOver<DateTimeOffset, DateTimeOffset?>(obj, ForceUtcAndConvert, dt => dt != null ? (DateTimeOffset?)ForceUtcAndConvert(dt.Value) : null);
        }

        /// <summary>
        /// Convert an individual DateTimeOffset to UTC without changing the actual time, and
        /// then converts to local time.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTimeOffset ForceUtcAndConvert(this DateTimeOffset date)
        {
            return new DateTimeOffset(date.Ticks, TimeSpan.Zero).ToLocalTime();
        }

        /// <summary>
        /// Convert an individual DateTime to Unspecified without changing the actual time.
        /// </summary>
        /// <param name="date"></param>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static DateTime ForceKind(this DateTime date, DateTimeKind kind)
        {
            return new DateTime(date.Ticks, kind);
        }

        /// <summary>
        /// Convert an individual DateTime to Unspecified without changing the actual time.
        /// </summary>
        /// <param name="date"></param>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static DateTime? ForceKind(this DateTime? date, DateTimeKind kind)
        {
            return date != null ? (DateTime?)new DateTime(date.Value.Ticks, kind) : null;
        }

        public static DateTime? MinDate(IEnumerable<DateTime?> datetimes)
        {
            var all = datetimes.Where(x => x.HasValue).ToList();
            return all.Any() ? all.Min() : null;
        }

        public static DateTimeOffset? MinDate(IEnumerable<DateTimeOffset?> datetimes)
        {
            var all = datetimes.Where(x => x.HasValue).ToList();
            return all.Any() ? all.Min() : null;
        }

        public static DateTime MaxDate(DateTime datetime1, DateTime datetime2)
        {
            return datetime1.Ticks > datetime2.Ticks ? datetime1 : datetime2;
        }

        public static DateTimeOffset MaxDate(DateTimeOffset datetime1, DateTimeOffset datetime2)
        {
            return datetime1.Ticks > datetime2.Ticks ? datetime1 : datetime2;
        }

        public static DateTime? MaxDate(IEnumerable<DateTime?> datetimes)
        {
            var all = datetimes.Where(x => x.HasValue).ToList();
            return all.Any() ? all.Max() : null;
        }

        public static DateTimeOffset? MaxDate(IEnumerable<DateTimeOffset?> datetimes)
        {
            var all = datetimes.Where(x => x.HasValue).ToList();
            return all.Any() ? all.Max() : null;
        }

        public static TimeZoneInfo FindTimeZone(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (InvalidTimeZoneException)
            {
                return null;
            }
        }
    }
}
