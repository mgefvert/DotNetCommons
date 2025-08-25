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
    public override Task<int> ExecuteAsync(CancellationToken ct)
    {
        var units = unitStates.SelectUnits(Args.UnitName, Args.AllUnits);
        if (units.IsEmpty())
        {
            logger.LogError("No unit found.");
            return Task.FromResult(1);
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

        return Task.FromResult(0);
    }
}
