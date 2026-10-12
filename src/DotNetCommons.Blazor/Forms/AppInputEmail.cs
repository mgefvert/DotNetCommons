namespace DotNetCommons.Blazor.Forms;

public sealed class AppInputEmail : AppInputText
{
    protected override string InputType => "email";
    protected override string? AutoComplete => "email";
}
