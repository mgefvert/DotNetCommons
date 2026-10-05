using BlazorDemo.Components;
using DotNetCommons.Logging;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddCommonConsole();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAntiforgery();

var app = builder.Build();

app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();