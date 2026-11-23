namespace DotNetCommons.Blazor.Components;

/// <summary>Default Tailwind classes shared by the Blazor controls.</summary>
public static class AppTheme
{
    // Fields and native inputs
    public static string Container { get; set; } = "flex min-w-0 flex-col";
    public static string StandardContainer { get; set; } = "gap-1.5";
    public static string CompactContainer { get; set; } = "gap-1";
    public static string Label { get; set; } = "font-semibold text-page-foreground";
    public static string StandardLabel { get; set; } = "text-sm";
    public static string CompactLabel { get; set; } = "text-xs";
    public static string Description { get; set; } = "text-xs leading-5 opacity-70";
    public static string Control { get; set; } = "block w-full min-w-0 border border-control-border bg-control-background text-page-foreground shadow-sm outline-none transition focus:border-blue-500 focus:ring-2 focus:ring-blue-500/25 disabled:cursor-not-allowed disabled:opacity-50";
    public static string StandardControl { get; set; } = "rounded-lg px-3 py-2.5 text-sm";
    public static string CompactControl { get; set; } = "rounded-md px-2 py-1.5 text-sm";

    // Checkboxes, radio buttons, and range inputs
    public static string ChoiceGroup { get; set; } = "flex flex-wrap";
    public static string StandardChoiceGroup { get; set; } = "gap-3";
    public static string CompactChoiceGroup { get; set; } = "gap-2";
    public static string Choice { get; set; } = "flex cursor-pointer items-center text-sm";
    public static string StandardChoice { get; set; } = "gap-2";
    public static string CompactChoice { get; set; } = "gap-1.5";
    public static string Checkable { get; set; } = "size-4 accent-blue-600";
    public static string CheckBoxesPlaceholder { get; set; } = "text-xs opacity-60";
    public static string RangeContainer { get; set; } = "flex items-center";
    public static string StandardRangeContainer { get; set; } = "gap-3";
    public static string CompactRangeContainer { get; set; } = "gap-2";
    public static string Range { get; set; } = "w-full cursor-pointer accent-blue-600";
    public static string StandardRange { get; set; } = "h-6";
    public static string CompactRange { get; set; } = "h-4";
    public static string RangeOutput { get; set; } = "min-w-10 text-right text-sm tabular-nums";
    public static string NumberPrefix { get; set; } = "pointer-events-none absolute inset-y-0 left-3 flex items-center text-sm opacity-60";
    public static string NumberPrefixInput { get; set; } = "pl-7";

    // Buttons and icons
    public static string Button { get; set; } = "inline-flex items-center justify-center gap-2 rounded-lg font-semibold shadow-sm transition focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-500 disabled:cursor-not-allowed disabled:opacity-50";
    public static string StandardButton { get; set; } = "px-4 py-2.5 text-sm";
    public static string CompactButton { get; set; } = "px-2.5 py-1.5 text-sm";
    public static string PrimaryButton { get; set; } = "bg-blue-600 text-white hover:bg-blue-700";
    public static string SecondaryButton { get; set; } = "border border-control-border bg-control-background text-page-foreground hover:bg-control-hover-background";
    public static string WarningButton { get; set; } = "bg-amber-600 text-white hover:bg-amber-700";
    public static string Icon { get; set; } = "inline-flex shrink-0 align-middle";
    public static string StandardIcon { get; set; } = "size-5";
    public static string CompactIcon { get; set; } = "size-4";
}
