using DotNetCommons.Commands;
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
    public override int Execute()
    {
        logger.LogInformation("Executing rapid deployment according to BROKEN ARROW protocol");

        Registry.ExecuteCommand<SetRedconAction>(false, ["--all", "--value", "1"]);
        Registry.ExecuteCommand<SetDefconAction>(false, ["--all", "--value", "1"]);
        Registry.ExecuteCommand<DeployAction>(false, ["--all"]);

        logger.LogInformation("'Broken Arrow' command executed successfully.");
        return 0;
    }
}