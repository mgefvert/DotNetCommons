namespace DotNetCommons.Blazor.Forms;

public sealed class AppInputDateTime : AppInputDate
{
    protected override string InputType => "datetime-local";
    protected override string Format => "yyyy-MM-ddTHH:mm";
}
