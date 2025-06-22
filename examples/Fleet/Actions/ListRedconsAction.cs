using DotNetCommons.Commands;
using Microsoft.Extensions.Logging;

namespace Fleet.Actions;

[CommandAction(
    ["list", "redcons"],
    "Lists all REDCON values and their meanings.",
    [
        "Lists all Readiness Condition (REDCON) values, from 1 to 5, and explains their meanings."
    ]
)]
public class ListRedconsAction(
        ILogger<ListRedconsAction> logger
    ) : CommandAction
{
    public override int Execute()
    {
        logger.LogInformation("Listing all REDCON values");

        Console.WriteLine("REDCON Values:");
        Console.WriteLine("REDCON 1: Fully combat ready");
        Console.WriteLine("REDCON 2: Combat ready with minor deficiencies");
        Console.WriteLine("REDCON 3: Combat ready with major deficiencies");
        Console.WriteLine("REDCON 4: Not combat ready");
        Console.WriteLine("REDCON 5: Combat ineffective");

        return 0;
    }
}
