using DotNetCommons.Sys;

namespace Fleet.Args;

public class UnitValueArgs : UnitArgs
{
    [CommandLineOption('v', "value", "Value to set")]
    public int? Value { get; set; }
}
