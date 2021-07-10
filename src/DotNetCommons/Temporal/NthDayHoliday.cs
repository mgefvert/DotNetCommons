using System;

namespace DotNetCommons.Temporal
{
    public class NthDayHoliday : Holiday
    {
        public int Month { get; }
        public int Occurrence { get; }
        public DayOfWeek DayOfWeek { get; }

        public NthDayHoliday(string name, HolidayType type, int month, int occurrence, DayOfWeek dayOfWeek)
            : base(name, type)
        {
            Month = month;
            Occurrence = occurrence;
            DayOfWeek = dayOfWeek;

            if (Occurrence < 1 || Occurrence > 5)
                throw new ArgumentOutOfRangeException(nameof(occurrence), "Occurrence must in the range of [1..5].");
        }

        public override string TextDefinition() => $"[nth,{Name},{Type},{Month},{Occurrence},{DayOfWeek}]";

        protected internal override DateTime InternalCalculateDate(int year)
        {
            var result = new DateTime(year, Month, 1);
            while (result.DayOfWeek != DayOfWeek)
                result = result.AddDays(1);

            return result.AddDays((Occurrence - 1) * 7);
        }
    }
}
