using LedImageUpdaterService;
using LedImageUpdaterService.Controllers;
using LedImageUpdaterService.Models;
using LedImageUpdaterService.Services;
using Microsoft.OpenApi.Models;

if (OnbonSendIsolationHelper.IsHelperInvocation(args))
{
    Environment.ExitCode = await OnbonSendIsolationHelper.RunAsync(args);
    return;
}

// ─── Builder ──────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// ─── Point config overlay ─────────────────────────────────────────────────────
// Read ActivePointId from the base configuration, then layer config/points/{id}.json
// on top so point-specific settings (IP, screen size, paths) override the defaults.
{
    var activePointId = builder.Configuration["ActivePointId"];
    if (string.IsNullOrWhiteSpace(activePointId))
        throw new InvalidOperationException(
            "ActivePointId is not set in appsettings.json. " +
            "Add \"ActivePointId\": \"<pointId>\" and create config/points/<pointId>.json.");

    var pointConfigPath = Path.Combine(
        builder.Environment.ContentRootPath, "config", "points", $"{activePointId}.json");

    if (!File.Exists(pointConfigPath))
        throw new InvalidOperationException(
            $"Point config file not found: {pointConfigPath}. " +
            $"Create config/points/{activePointId}.json with point-specific settings.");

    builder.Configuration.AddJsonFile(pointConfigPath, optional: false, reloadOnChange: true);
}

// Windows Service support (no-op on macOS / Linux — safe to always call)
builder.Host.UseWindowsService(options =>
{
    options.ServiceName = "TabloPosterService";
});

// ─── Configuration ────────────────────────────────────────────────────────────

builder.Services.AddOptions<ServiceOptions>()
    .Bind(builder.Configuration.GetSection(ServiceOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<OnbonOptions>()
    .Bind(builder.Configuration.GetSection(OnbonOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// ─── Infrastructure services ──────────────────────────────────────────────────

builder.Services.AddHttpClient();

// Shared in-memory log buffer used by /api/led/logs
builder.Services.AddSingleton<InMemoryLogStore>();

// ─── Domain services ──────────────────────────────────────────────────────────

builder.Services.AddSingleton<ScreenModelReader>();
builder.Services.AddSingleton<LedPayloadBuilder>();
builder.Services.AddSingleton<WifiNetworkGuard>();
builder.Services.AddSingleton<ControllerDiscovery>();
builder.Services.AddSingleton<DotnetComposer>();
builder.Services.AddSingleton<RenderOnlyRunner>();
builder.Services.AddSingleton<IPublishStrategy, FtpPublisher>();
builder.Services.AddSingleton<IPublishStrategy, RelayPublisher>();

// Onbon SDK wrapper — must be Singleton so init_sdk/release_sdk is called once
builder.Services.AddSingleton<OnbonLedController>();

// ─── Background services ──────────────────────────────────────────────────────

builder.Services.AddHostedService<RatesFetcherService>();
builder.Services.AddHostedService<Worker>();

// LedBoardService is also registered as a singleton so LedController can call
// ForceUpdateAsync() on the same instance the background loop uses.
builder.Services.AddSingleton<LedBoardService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<LedBoardService>());

// ─── Web API + Swagger ────────────────────────────────────────────────────────

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// CORS — allow any origin so Windows apps (WPF, WinForms, Blazor, WebView2) can call the API
const string CorsPolicy = "AllowAll";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Tablo Poster — LED Management API",
        Version = "v1",
        Description =
            "REST API for manual control of the Onbon BX-Y LED controller.\n\n" +
            "**Note:** endpoints that interact with the SDK (update, clear, upload) " +
            "require the service to be running on Windows with YQNetCom.dll present. " +
            "Connection check and log retrieval work on any platform.",
    });
});

// ─── App pipeline ─────────────────────────────────────────────────────────────

var app = builder.Build();

// Swagger is always enabled — restrict in production if needed
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tablo Poster API v1");
    c.RoutePrefix = string.Empty; // Serve Swagger UI at https://localhost:<port>/
    c.DocumentTitle = "Tablo Poster — LED API";
});

app.UseCors(CorsPolicy);
app.MapControllers();

app.Run();
