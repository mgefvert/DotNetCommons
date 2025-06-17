using DotNetCommons;
using DotNetCommons.Commands;
using Fleet.Args;
using Microsoft.Extensions.Logging;

namespace Fleet.Actions;

[CommandAction(
    ["set", "redcon"],
    "Set the readiness condition for a specific unit.",
    [
        "Set the readiness condition for a specific unit.",
        "Readiness condition is defined as a number from 1 to 5, where 1 is fully ready and 5 is not ready."
    ]
)]
public class SetRedconAction(
        ILogger<SetRedconAction> logger,
        UnitStates unitStates
    ) : CommandAction<UnitValueArgs>
{
    public override int Execute()
    {
        var value = Args.Value.GetValueOrDefault();
        if (value is < 1 or > 5)
        {
            logger.LogError("Invalid REDCON value. Must be between 1 and 5.");
            return 1;
        }

        var units = unitStates.SelectUnits(Args.UnitName, Args.AllUnits);
        if (units.IsEmpty())
        {
            logger.LogError("No unit found.");
            return 1;
        }

        foreach (var unit in units)
        {
            unit.Value.RedCon = value;
            logger.LogInformation("REDCON for {UnitName} set to {Value}", unit.Key, value);
        }

        return 0;
    }
}
