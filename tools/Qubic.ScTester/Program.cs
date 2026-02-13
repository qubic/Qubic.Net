using System.Diagnostics;
using System.Runtime.InteropServices;
using Qubic.ScTester;
using Qubic.ScTester.Components;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSingleton<ContractDiscovery>();
builder.Services.AddScoped<ScQueryService>();

builder.WebHost.UseUrls("http://localhost:5050");

var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Lifetime.ApplicationStarted.Register(() =>
{
    var url = "http://localhost:5050";
    Console.WriteLine($"Qubic SC Tester running at {url}");
    try
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            Process.Start("open", url);
        else
            Process.Start("xdg-open", url);
    }
    catch { /* Browser auto-open is best-effort */ }
});

app.Run();
