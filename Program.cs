using LedImageUpdaterService;
using LedImageUpdaterService.Controllers;
using LedImageUpdaterService.Models;
using LedImageUpdaterService.Services;
using LedImageUpdaterService.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.OpenApi.Models;

// ─── Helper-process mode (Onbon SDK isolation) ────────────────────────────────
if (OnbonSendIsolationHelper.IsHelperInvocation(args))
{
    Environment.ExitCode = await OnbonSendIsolationHelper.RunAsync(args);
    return;
}

// ─── Decide: Windows Service vs Tray app ─────────────────────────────────────
bool runAsService = WindowsServiceHelpers.IsWindowsService() || args.Contains("--service");

if (runAsService)
{
    // Headless service mode (installed as Windows Service)
    var app = Program.BuildWebApp(args);
    await app.RunAsync();
}
else
{
    // Interactive tray-app mode — single instance only.
    using var singleInstance = new Mutex(initiallyOwned: true, "eCashTablo_SingleInstance_Mutex", out bool isNew);
    if (!isNew)
    {
        MessageBox.Show(
            "eCash Tablo уже запущен.\nЗначок находится в области уведомлений (рядом с часами).",
            "eCash Tablo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
    }

    // Global safety net: log unhandled exceptions and keep the UI alive instead of
    // letting Windows kill the app with the default crash dialog.
    CrashLogger.Install();

    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);
    Application.Run(new TrayApplicationContext(args));
    GC.KeepAlive(singleInstance);
}

// ─── Shared host factory ──────────────────────────────────────────────────────

internal static partial class Program
{
    internal static WebApplication BuildWebApp(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Information);

        // ─── Point config overlay ─────────────────────────────────────────
        var activePointId = builder.Configuration["ActivePointId"];
        if (string.IsNullOrWhiteSpace(activePointId))
            throw new InvalidOperationException(
                "ActivePointId is not set in appsettings.json.");

        var pointConfigPath = Path.Combine(
            builder.Environment.ContentRootPath, "config", "points", $"{activePointId}.json");

        if (!File.Exists(pointConfigPath))
            throw new InvalidOperationException(
                $"Point config file not found: {pointConfigPath}");

        builder.Configuration.AddJsonFile(pointConfigPath, optional: false, reloadOnChange: true);

        // Windows Service support
        builder.Host.UseWindowsService(options =>
        {
            options.ServiceName = "eCashTabloService";
        });

        // ─── Options ──────────────────────────────────────────────────────
        builder.Services.AddOptions<ServiceOptions>()
            .Bind(builder.Configuration.GetSection(ServiceOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddOptions<OnbonOptions>()
            .Bind(builder.Configuration.GetSection(OnbonOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Telegram notifications are optional; bind without ValidateOnStart so a
        // missing/empty section never blocks startup.
        builder.Services.AddOptions<TelegramOptions>()
            .Bind(builder.Configuration.GetSection(TelegramOptions.SectionName));

        // ─── Infrastructure ───────────────────────────────────────────────
        builder.Services.AddHttpClient();
        builder.Services.AddSingleton<InMemoryLogStore>();
        builder.Services.AddSingleton<TelegramNotifier>();

        // ─── Domain services ──────────────────────────────────────────────
        builder.Services.AddSingleton<ScreenModelReader>();
        builder.Services.AddSingleton<LedPayloadBuilder>();
        builder.Services.AddSingleton<WifiNetworkGuard>();
        builder.Services.AddSingleton<ControllerDiscovery>();
        builder.Services.AddSingleton<DotnetComposer>();
        builder.Services.AddSingleton<RenderOnlyRunner>();
        builder.Services.AddSingleton<IPublishStrategy, FtpPublisher>();
        builder.Services.AddSingleton<IPublishStrategy, RelayPublisher>();

        // Onbon BX-Y (YQNetCom.dll) is the only LED transport in this application.
        builder.Services.AddSingleton<OnbonLedController>();
        builder.Services.AddSingleton<ILedController>(sp =>
            sp.GetRequiredService<OnbonLedController>());

        // ─── Background workers ───────────────────────────────────────────
        builder.Services.AddHostedService<RatesFetcherService>();
        builder.Services.AddHostedService<Worker>();
        builder.Services.AddSingleton<LedBoardService>();
        builder.Services.AddHostedService(sp => sp.GetRequiredService<LedBoardService>());

        // ─── Web API + Swagger ────────────────────────────────────────────
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        const string CorsPolicy = "AllowAll";
        builder.Services.AddCors(options =>
            options.AddPolicy(CorsPolicy, p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "eCash Tablo — LED Management API",
                Version = "v1",
                Description = "REST API for manual control of the Onbon BX-Y LED controller.",
            });
        });

        // ─── Build & configure pipeline ───────────────────────────────────
        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "eCash Tablo API v1");
            c.RoutePrefix = string.Empty;
            c.DocumentTitle = "eCash Tablo — LED API";
        });

        app.UseCors(CorsPolicy);
        app.MapControllers();

        return app;
    }
}
