using DotNetCommons;
using DotNetCommons.Commands;
using Fleet.Args;
using Microsoft.Extensions.Logging;

namespace Fleet.Actions;

[CommandAction(
    ["deploy"],
    "Deploys a vessel (submarine or wing).",
    [
        "Deploy a submarine or wing (sets deployment status to true)."
    ]
)]
public class DeployAction(
        ILogger<DeployAction> logger,
        UnitStates unitStates
    ) : CommandAction<UnitArgs>
{
    public override int Execute()
    {
        var units = unitStates.SelectUnits(Args.UnitName, Args.AllUnits);
        if (units.IsEmpty())
        {
            logger.LogError("No unit found.");
            return 1;
        }

        foreach (var unit in units)
        {
            if (unit.Value.Deployed)
                logger.LogWarning("Unit {UnitName} is already deployed", unit.Key);
            else
            {
                unit.Value.Deployed = true;
                logger.LogInformation("Unit {UnitName} has been deployed", unit.Key);
            }
        }

        return 0;
    }
}
