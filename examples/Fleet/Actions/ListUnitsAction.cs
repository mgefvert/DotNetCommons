using DotNetCommons.Commands;
using Microsoft.Extensions.Logging;

namespace Fleet.Actions;

[CommandAction(
    ["list", "units"],
    "Lists all units and their DEFCON and REDCON status.",
    [
        "Lists all submarines and wings, whether they're deployed, and their DEFCON and REDCON levels."
    ]
)]
public class ListUnitsAction(
        ILogger<ListUnitsAction> logger,
        UnitStates unitStates
    ) : CommandAction
{
    public override int Execute()
    {
        logger.LogInformation("Listing all units");

        var maxNameLength = unitStates.Keys.Max(name => name.Length);
        foreach (var (name, state) in unitStates.OrderBy(x => x.Value.Submarine).ThenBy(x => x.Key))
            Console.WriteLine($"{name.PadRight(maxNameLength)} => DEFCON:{state.DefCon}, REDCON:{state.RedCon}, Deployed:{state.Deployed}");

        return 0;
    }
}