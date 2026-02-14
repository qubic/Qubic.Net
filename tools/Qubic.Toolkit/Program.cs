using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Microsoft.Extensions.FileProviders;
using Photino.Blazor;
using Qubic.Toolkit;
using Qubic.Toolkit.Components;

if (args.Contains("--server"))
{
    RunServer(args);
}
else
{
    RunDesktop(args);
}

static void RegisterServices(IServiceCollection services)
{
    services.AddSingleton<ToolkitSettingsService>();
    services.AddSingleton<ContractDiscovery>();
    services.AddSingleton<ToolkitBackendService>();
    services.AddSingleton<SeedSessionService>();
    services.AddSingleton<TickMonitorService>();
    services.AddSingleton<TransactionTrackerService>();
    services.AddSingleton<AssetRegistryService>();
}

[System.STAThread]
static void RunDesktop(string[] args)
{
    var wwwrootPath = GetWwwrootPath();
    var fileProvider = new PhysicalFileProvider(wwwrootPath);

    var appBuilder = PhotinoBlazorAppBuilder.CreateDefault(fileProvider, args);
    RegisterServices(appBuilder.Services);
    appBuilder.RootComponents.Add<Routes>("app");

    var app = appBuilder.Build();
    app.MainWindow
        .SetTitle("Qubic.Net Toolkit")
        .SetSize(1400, 900);

    AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
    {
        app.MainWindow.ShowMessage("Fatal exception", error.ExceptionObject.ToString());
    };

    app.Run();
}

static void RunServer(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddRazorComponents().AddInteractiveServerComponents();
    RegisterServices(builder.Services);
    builder.WebHost.UseUrls("http://127.0.0.1:0");

    var app = builder.Build();
    app.UseStaticFiles();
    app.UseAntiforgery();
    app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

    app.Lifetime.ApplicationStarted.Register(() =>
    {
        var address = app.Urls.FirstOrDefault() ?? "http://127.0.0.1:5060";
        Console.WriteLine($"Qubic.Net Toolkit running at {address}");
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Process.Start(new ProcessStartInfo(address) { UseShellExecute = true });
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                Process.Start("open", address);
            else
                Process.Start("xdg-open", address);
        }
        catch { /* Browser auto-open is best-effort */ }
    });

    app.Run();
}

/// <summary>
/// Returns the path to the wwwroot directory, extracting from the embedded zip if needed.
/// For dotnet run / normal publish: wwwroot is already at BaseDirectory via Content copy.
/// For single-file publish: extracts embedded wwwroot.zip to LocalApplicationData.
/// </summary>
static string GetWwwrootPath()
{
    // Check for embedded wwwroot.zip (present in single-file publish)
    var stream = typeof(Program).Assembly.GetManifestResourceStream("wwwroot.zip");
    if (stream == null)
    {
        // No embedded zip — use wwwroot from output directory (dotnet run / normal publish)
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot");
    }

    // Single-file publish: extract to local app data
    var appData = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Qubic.Toolkit");
    var wwwrootDir = Path.Combine(appData, "wwwroot");
    var versionFile = Path.Combine(wwwrootDir, ".version");
    var currentVersion = typeof(Program).Assembly.ManifestModule.ModuleVersionId.ToString();

    // Already up to date?
    if (Directory.Exists(wwwrootDir) && File.Exists(versionFile)
        && File.ReadAllText(versionFile).Trim() == currentVersion)
    {
        stream.Dispose();
        return wwwrootDir;
    }

    // Extract
    using (stream)
    using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
    {
        if (Directory.Exists(wwwrootDir))
            Directory.Delete(wwwrootDir, true);
        Directory.CreateDirectory(wwwrootDir);
        archive.ExtractToDirectory(wwwrootDir);
    }

    File.WriteAllText(versionFile, currentVersion);
    return wwwrootDir;
}
