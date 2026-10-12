using System.Globalization;
using DotNetCommons.Base;
using DotNetCommons.Blazor.Components;
using Microsoft.AspNetCore.Components;

namespace DotNetCommons.Blazor.Forms;

public partial class AppInputNumber : AppInputBase<decimal?>
{
    [Parameter] public decimal? Min { get; set; }
    [Parameter] public decimal? Max { get; set; }
    [Parameter] public decimal Step { get; set; } = 1m;
    [Parameter] public string? PrefixCss { get; set; }
    protected virtual string? Prefix => null;
    protected string FormattedValue => Value?.ToString(CultureInfo.InvariantCulture) ?? "";
    protected readonly CssClassOverrides _prefixCss = new();

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        _inputCss.State = $"{(Mode == AppControlMode.Compact ? AppTheme.CompactControl : AppTheme.StandardControl)} {(Prefix is null ? "" : AppTheme.NumberPrefixInput)}";
        _prefixCss.Defaults = AppTheme.NumberPrefix;
        _prefixCss.Overrides = PrefixCss;
    }

    protected Task OnChangeAsync(ChangeEventArgs e)
    {
        var raw = e.Value?.ToString();
        if (string.IsNullOrWhiteSpace(raw)) return SetValueAsync(null);
        return decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
            ? SetValueAsync(value) : Task.CompletedTask;
    }
}
