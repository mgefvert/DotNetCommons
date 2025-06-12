namespace DotNetCommons.EF;

[AttributeUsage(AttributeTargets.Property)]
public class UpdateableAttribute : Attribute
{
    public bool NullRemovesValue { get; init; }
}