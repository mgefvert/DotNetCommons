using DotNetCommons.Sys;

namespace Fleet.Args;

public class UnitArgs
{
    [CommandLineOption('u', "unit", "Name of the unit; wildcards accepted")]
    public string? UnitName { get; set; }

    [CommandLineOption('a', "all", "Select all units")]
    public bool AllUnits { get; set; }
}
