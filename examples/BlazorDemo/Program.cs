using BlazorDemo.Components;
using DotNetCommons.Blazor.Components;
using DotNetCommons.Logging;

namespace BlazorDemo;

internal static class Program
{
    internal static bool IsDevelopment { get; private set; }

    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.AddCommonConsole();
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        IsDevelopment = builder.Environment.IsDevelopment();

        builder.Services.AddAntiforgery();
        builder.Services.AddSingleton(new AppIconProvider(Path.Combine(builder.Environment.WebRootPath, "icons")));

        var app = builder.Build();

        app.UseAntiforgery();
        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}
