using DotNetCommons;
using DotNetCommons.Commands;
using Fleet.Args;
using Microsoft.Extensions.Logging;

namespace Fleet.Actions;

[CommandAction(
    ["recall"],
    "Recalls a vessel from deployment.",
    [
        "Recall a submarine or wing from deployment (sets deployment status to false)."
    ]
)]
public class RecallAction(
        ILogger<RecallAction> logger,
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
            if (!unit.Value.Deployed)
                logger.LogWarning("Unit {UnitName} is not deployed", unit.Key);
            else
            {
                unit.Value.Deployed = false;
                logger.LogInformation("Unit {UnitName} has been recalled from deployment", unit.Key);
            }
        }

        return Task.FromResult(0);
    }
}
