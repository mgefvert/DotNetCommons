using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace DotNetCommons.Blazor.Forms;

public partial class AppInputDate : AppInputBase<DateTime?>
{
    [Parameter] public DateTime? Minimum { get; set; }
    [Parameter] public DateTime? Maximum { get; set; }
    protected virtual string InputType => "date";
    protected virtual string Format => "yyyy-MM-dd";
    protected string FormattedValue => Value?.ToString(Format, CultureInfo.InvariantCulture) ?? "";
    protected string? Min => Minimum?.ToString(Format, CultureInfo.InvariantCulture);
    protected string? Max => Maximum?.ToString(Format, CultureInfo.InvariantCulture);

    protected Task OnChangeAsync(ChangeEventArgs e)
    {
        var raw = e.Value?.ToString();
        if (string.IsNullOrWhiteSpace(raw)) return SetValueAsync(null);
        return DateTime.TryParseExact(raw, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var value)
            ? SetValueAsync(value) : Task.CompletedTask;
    }
}
