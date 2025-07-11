﻿using DotNetCommons.Commands;
using Fleet.Args;
using Microsoft.Extensions.Logging;

namespace Fleet.Actions;

[CommandAction(
    ["nuke"],
    "Launches a nuclear missile at a target.",
    [
        "Launch a nuclear missile at a target from a specific vessel.",
        "Unit must be deployed, at DEFCON 1, and at REDCON 3 or better to fire."
    ]
)]
public class NukeAction(
        ILogger<NukeAction> logger,
        UnitStates unitStates
    ) : CommandAction<NukeArgs>
{
    public override int Execute()
    {
        ArgumentException.ThrowIfNullOrEmpty(Args.UnitName, nameof(Args.UnitName));
        ArgumentException.ThrowIfNullOrEmpty(Args.Target, nameof(Args.Target));

        var unit = unitStates.GetValueOrDefault(Args.UnitName);
        if (unit == null)
        {
            logger.LogError("Unit {UnitName} not found", Args.UnitName);
            return 1;
        }

        if (!unit.ReadyToFire())
        {
            logger.LogError("Unit {UnitName} cannot launch: {Reason}", Args.UnitName, unit.GetFireReadinessIssue());
            return 1;
        }

        logger.LogInformation("{UnitName} is nuking target: {Target}", Args.UnitName, Args.Target);

        return 0;
    }
}