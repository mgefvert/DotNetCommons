using DotNetCommons.Commands;
using Fleet.Args;
using Microsoft.Extensions.Logging;

namespace Fleet.Actions;

[CommandAction(
    ["broken", "arrow"],
    "Deploys all units and sets force DEFCON and REDCON to maximum readiness.",
    [
        "Executes multiple commands in sequence to deploy all units and set both DEFCON and REDCON to their maximum readiness levels."
    ]
)]
public class BrokenArrowAction(
    ILogger<BrokenArrowAction> logger
) : CommandAction
{
    public override Task<int> ExecuteAsync(CancellationToken ct)
    {
        logger.LogInformation("Executing rapid deployment according to BROKEN ARROW protocol");

        Registry.Schedule<SetRedconAction, UnitValueArgs>(CommandActionRegistry.HighPriority, false, new UnitValueArgs { AllUnits = true, Value = 1 });
        Registry.Schedule<SetDefconAction, UnitValueArgs>(CommandActionRegistry.HighPriority, false, new UnitValueArgs { AllUnits = true, Value = 1 });
        Registry.Schedule<DeployAction, UnitArgs>(CommandActionRegistry.LastPriority, false, new UnitArgs { AllUnits = true });

        logger.LogInformation("'Broken Arrow' command executed successfully.");
        return Task.FromResult(0);
    }
}