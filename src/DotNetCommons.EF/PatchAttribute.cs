namespace DotNetCommons.EF;

[AttributeUsage(AttributeTargets.Property)]
public class PatchAttribute : Attribute
{
    public bool NullRemovesValue { get; }

    public PatchAttribute(bool nullRemovesValue = false)
    {
        NullRemovesValue = nullRemovesValue;
    }
}