using DotNetCommons.Commands;
using Microsoft.Extensions.Logging;

namespace Fleet.Actions;

[CommandAction(
    ["list", "defcons"],
    "Lists all DEFCON values and their meanings.",
    [
        "Lists all Defense Condition (DEFCON) values, from 1 to 5, and explains their meanings."
    ]
)]
public class ListDefconsAction(
        ILogger<ListDefconsAction> logger
    ) : CommandAction
{
    public override int Execute()
    {
        logger.LogInformation("Listing all DEFCON values");

        Console.WriteLine("DEFCON Values:");
        Console.WriteLine("DEFCON 1: Maximum readiness - Nuclear war imminent");
        Console.WriteLine("DEFCON 2: Next to maximum readiness - Armed Forces ready to deploy");
        Console.WriteLine("DEFCON 3: Increased readiness - Air Force ready to mobilize");
        Console.WriteLine("DEFCON 4: Increased intelligence watch and heightened security measures");
        Console.WriteLine("DEFCON 5: Normal peacetime readiness");

        return 0;
    }
}
