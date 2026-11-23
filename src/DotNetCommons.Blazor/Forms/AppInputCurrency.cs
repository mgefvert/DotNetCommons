using Microsoft.AspNetCore.Components;

namespace DotNetCommons.Blazor.Forms;

public sealed class AppInputCurrency : AppInputNumber
{
    [Parameter] public string CurrencySymbol { get; set; } = "$";

    protected override string? Prefix => CurrencySymbol;
}
