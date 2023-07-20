namespace DotNetCommons.PlaywrightTesting;

public enum Parallelism
{
    Single,
    Parallel,
    First,
    Last
}

[AttributeUsage(AttributeTargets.Class)]
public class PlaywrightTestClassAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Method)]
public class PlaywrightClassSetupAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Method)]
public class PlaywrightClassTeardownAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Method)]
public class PlaywrightSetupAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Method)]
public class PlaywrightTeardownAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Method)]
public class PlaywrightTestAttribute : Attribute
{
    public Parallelism Parallelism { get; }

    public PlaywrightTestAttribute(Parallelism parallelism)
    {
        Parallelism = parallelism;
    }
}