namespace Fleet;

public class UnitState
{
    public bool Deployed { get; set; }
    public bool Submarine { get; set; }
    public int DefCon { get; set; } = 5;
    public int RedCon { get; set; } = 4;

    public UnitState()
    {
    }

    public UnitState(bool submarine)
    {
        Submarine = submarine;
    }

    /// <summary>
    /// Determines if the unit is ready to fire based on deployment status, DEFCON, and REDCON levels.
    /// </summary>
    /// <returns>True if the unit is ready to fire; otherwise, false.</returns>
    public bool ReadyToFire()
    {
        return Deployed && DefCon == 1 && RedCon <= 3;
    }

    /// <summary>
    /// Gets a description of why the unit is not ready to fire, or null if it is ready.
    /// </summary>
    /// <returns>A string describing why the unit cannot fire, or null if it can.</returns>
    public string? GetFireReadinessIssue()
    {
        if (!Deployed)
            return "Unit is not deployed";

        if (DefCon != 1)
            return $"Unit is at DEFCON {DefCon}, but must be at DEFCON 1";

        if (RedCon > 3)
            return $"Unit is at REDCON {RedCon}, but must be at REDCON 3 or better";

        return null; // No issues, ready to fire
    }
}