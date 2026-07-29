using DotNetCommons.Commands;
using DotNetCommons.Security;
using DotNetCommons.Sys;
using BCrypter = BCrypt.Net.BCrypt;

namespace commons.Commands;

public class GenPwArgs
{
    [CommandLineOption('f', "format", "Password format, default F4-F4-F4")]
    public string Format { get; set; } = "F4-F4-F4";

    [CommandLineOption('c', "count", "Generate count password, default 1")]
    public int Count { get; set; } = 1;
}

[CommandAction(["gen", "pw"], "Generate a new password", [])]
public class GenPwCommand : CommandAction<GenPwArgs>
{
    public override int Execute()
    {
        var result = new List<(string Pw, string Hash)>();

        for (var i = 0; i < Args.Count; i++)
        {
            var pw   = Passwords.GeneratePassword(Args.Format);
            var hash = BCrypter.HashPassword(pw);
            result.Add((pw, hash));
        }

        var pwlen = result.Max(x => x.Pw.Length);
        foreach (var (pw, hash) in result)
            Console.WriteLine($"{pw.PadRight(pwlen)} : {hash}");

        Console.Error.WriteLine($"{result.Count} password generated");
        return 0;
    }
}