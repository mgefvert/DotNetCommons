using System;

namespace DotNetCommons.Temporal
{
    public class LastDayHoliday : Holiday
    {
        public int Month { get; }
        public DayOfWeek DayOfWeek { get; }

        public LastDayHoliday(string name, HolidayType type, int month, DayOfWeek dayOfWeek)
            : base(name, type)
        {
            Month = month;
            DayOfWeek = dayOfWeek;
        }

        public override string TextDefinition() => $"[last,{Name},{Type},{Month},{DayOfWeek}]";

        protected internal override DateTime InternalCalculateDate(int year)
        {
            var result = new DateTime(year, Month, 1).AddMonths(1).AddDays(-1);
            while (result.DayOfWeek != DayOfWeek)
                result = result.AddDays(-1);

            return result;
        }
    }
}
