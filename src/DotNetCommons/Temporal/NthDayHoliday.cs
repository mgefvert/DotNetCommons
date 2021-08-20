using System;

namespace DotNetCommons.Temporal
{
    public class NthDayHoliday : Holiday
    {
        public int Month { get; }
        public int Occurrence { get; }
        public int Offset { get; }
        public DayOfWeek DayOfWeek { get; }

        public NthDayHoliday(string name, HolidayType type, int month, int occurrence, int dayOfWeek)
            : this(name, type, month, occurrence, (DayOfWeek)dayOfWeek, 0)
        {
        }

        public NthDayHoliday(string name, HolidayType type, int month, int occurrence, int dayOfWeek, int offset)
            : this(name, type, month, occurrence, (DayOfWeek)dayOfWeek, offset)
        {
        }

        public NthDayHoliday(string name, HolidayType type, int month, int occurrence, DayOfWeek dayOfWeek)
            : this(name, type, month, occurrence, dayOfWeek, 0)
        {
        }

        public NthDayHoliday(string name, HolidayType type, int month, int occurrence, DayOfWeek dayOfWeek, int offset)
            : base(name, type)
        {
            Month = month;
            Occurrence = occurrence;
            DayOfWeek = dayOfWeek;
            Offset = 0;

            if (Occurrence < 1 || Occurrence > 5)
                throw new ArgumentOutOfRangeException(nameof(occurrence), "Occurrence must in the range of [1..5].");
        }

        public override string TextDefinition() => $"[nth,{Name},{Type},{Month},{Occurrence},{DayOfWeek}{(Offset == 0 ? "" : "," + Offset)}]";

        protected internal override DateTime InternalCalculateDate(int year)
        {
            var result = new DateTime(year, Month, 1);
            while (result.DayOfWeek != DayOfWeek)
                result = result.AddDays(1);

            return result.AddDays((Occurrence - 1) * 7 + Offset);
        }
    }
}
