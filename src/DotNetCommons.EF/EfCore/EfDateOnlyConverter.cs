using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DotNetCommons.EF.EfCore;

public sealed class EfDateOnlyConverter : ValueConverter<DateOnly, DateTime>
{
    public EfDateOnlyConverter() : base(
        dateOnly => dateOnly.ToDateTime(TimeOnly.MinValue),
        dateTime => DateOnly.FromDateTime(dateTime))
    {
    }
}