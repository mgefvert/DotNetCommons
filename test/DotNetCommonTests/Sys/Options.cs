using System.ComponentModel.DataAnnotations;
using DotNetCommons.Sys;

namespace DotNetCommonTests.Sys;

public class Options
{
    [CommandLineOption('u', "user")]
    public string? User { get; set; }

    [CommandLineOption('p', "password")]
    public string? Password { get; set; }

    [CommandLineOption('P', "port")]
    public int Port { get; set; }

    [CommandLineOption('z', "zip")]
    public bool Zip { get; set; }

    [CommandLineOption('e', "encrypt")]
    public bool Encrypt { get; set; }

    [CommandLinePosition(1)]
    public string? Param1 { get; set; }

    [CommandLinePosition(2)]
    public string? Param2 { get; set; }

    [CommandLineRemaining]
    public List<string> Params { get; } = new();
}

public class OptionsRequired
{
    [CommandLineOption('u', "user"), Required]
    public string User { get; set; } = null!;

    [CommandLineOption('p', "password")]
    public string? Password { get; set; }
}

