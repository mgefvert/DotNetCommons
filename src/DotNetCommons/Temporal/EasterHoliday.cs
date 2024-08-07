﻿namespace DotNetCommons.Temporal;

/// <summary>
/// Class that encapsulates the Easter calculation.
/// </summary>
public class EasterHoliday : Holiday
{
    public int Offset { get; }

    public EasterHoliday(string name, HolidayType type, int offset = 0) : base(name, type)
    {
        Offset = offset;
    }

    public override string TextDefinition() => $"[easter,{Name},{Type}{(Offset == 0 ? "" : "," + Offset)}]";

    protected internal override DateTime InternalCalculateDate(int year)
    {
        var a = year % 19;
        var b = year / 100;
        var c = year % 100;
        var d = b / 4;
        var e = b % 4;
        var f = (b + 8) / 25;
        var g = (b - f + 1) / 3;
        var h = (19 * a + b - d - g + 15) % 30;
        var i = c / 4;
        var k = c % 4;
        var l = (32 + 2 * e + 2 * i - h - k) % 7;
        var m = (a + 11 * h + 22 * l) / 451;
        var n = (h + l - 7 * m + 114) / 31;
        var p = (h + l - 7 * m + 114) % 31;

        return new DateTime(year, n, p + 1).AddDays(Offset);
    }
}