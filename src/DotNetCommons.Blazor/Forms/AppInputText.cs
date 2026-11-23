namespace DotNetCommons.Blazor.Forms;

public partial class AppInputText : AppInputBase<string?>
{
    protected virtual string InputType => "text";
    protected virtual string? AutoComplete => null;

    protected Task OnInputAsync(Microsoft.AspNetCore.Components.ChangeEventArgs e) =>
        SetValueAsync(e.Value?.ToString());
}
