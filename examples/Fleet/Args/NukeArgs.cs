using DotNetCommons.Sys;

namespace Fleet.Args;

public class NukeArgs
{
    [CommandLineOption('u', "unit", "Unit to launch from.")]
    public string? UnitName { get; set; }

    [CommandLineOption('t', "target", "Target to nuke.")]
    public string? Target { get; set; }
}