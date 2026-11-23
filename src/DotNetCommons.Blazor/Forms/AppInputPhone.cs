namespace DotNetCommons.Blazor.Forms;

public sealed class AppInputPhone : AppInputText
{
    protected override string InputType => "tel";
    protected override string AutoComplete => "tel";
}
