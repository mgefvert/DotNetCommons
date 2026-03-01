namespace DotNetCommons.Text.FixedWidth;

public class FixedMoneyAttribute : FixedNumberAttribute
{
    public FixedMoneyAttribute(int start, int length) : base(start, length)
    {
        Scale = 2;
    }
}
