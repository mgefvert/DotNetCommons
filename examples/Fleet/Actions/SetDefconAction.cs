using DotNetCommons;
using DotNetCommons.Commands;
using Fleet.Args;
using Microsoft.Extensions.Logging;

namespace Fleet.Actions;

[CommandAction(
    ["set", "defcon"],
    "Set the defense condition for a specific unit.",
    [
        "Set the defense condition for a specific unit.",
        "Defense condition is defined as a number from 1 to 5, where 1 is highest alert level and 5 is lowest alert level."
    ]
)]
public class SetDefconAction(
        ILogger<SetDefconAction> logger,
        UnitStates unitStates
    ) : CommandAction<UnitValueArgs>
{
    public override int Execute()
    {
        var value = Args.Value.GetValueOrDefault();
        if (value is < 1 or > 5)
        {
            logger.LogError("Invalid DEFCON value. Must be between 1 and 5.");
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
            unit.Value.DefCon = value;
            logger.LogInformation("DEFCON for {UnitName} set to {Value}", unit.Key, value);
        }

        return 0;
    }
}