using LedImageUpdaterService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LedImageUpdaterService.Services;

internal static class OnbonSendIsolationHelper
{
    public const string HelperFlag = "--onbon-send-helper";

    public static bool IsHelperInvocation(string[] args) =>
        args.Any(a => string.Equals(a, HelperFlag, StringComparison.OrdinalIgnoreCase));

    public static async Task<int> RunAsync(string[] args)
    {
        try
        {
            var imageArg = GetArgValue(args, "--image");
            if (string.IsNullOrWhiteSpace(imageArg))
            {
                Console.Error.WriteLine("Missing required argument: --image <path>");
                return 1;
            }

            var imagePath = Path.IsPathRooted(imageArg)
                ? imageArg
                : Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), imageArg));

            if (!File.Exists(imagePath))
            {
                Console.Error.WriteLine($"Image file not found: {imagePath}");
                return 2;
            }

            Environment.SetEnvironmentVariable("ONBON_HELPER_MODE", "1");

            var baseDir = AppContext.BaseDirectory;

            var baseConfig = new ConfigurationBuilder()
                .SetBasePath(baseDir)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .Build();

            var activePointId = baseConfig["ActivePointId"];

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(baseDir)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);

            if (!string.IsNullOrWhiteSpace(activePointId))
            {
                var pointConfigRelative = Path.Combine("config", "points", $"{activePointId}.json");
                var pointConfigFull = Path.Combine(baseDir, pointConfigRelative);
                if (File.Exists(pointConfigFull))
                    configBuilder.AddJsonFile(pointConfigRelative, optional: false, reloadOnChange: false);
            }

            var config = configBuilder.Build();

            var options = config.GetSection(OnbonOptions.SectionName).Get<OnbonOptions>();
            if (options is null)
            {
                Console.Error.WriteLine("Onbon options are missing in appsettings.json");
                return 3;
            }

            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(LogLevel.None);
            });

            var logStore = new InMemoryLogStore();
            using var controller = new OnbonLedController(
                loggerFactory.CreateLogger<OnbonLedController>(),
                Options.Create(options),
                logStore);

            // The parent process already made the duplicate-skip decision before spawning
            // this helper; here we always send what we were handed.
            var status = await controller.SendImageWithStatusAsync(
                imagePath, bypassDuplicateCheck: true, CancellationToken.None);
            if (status.Success)
            {
                Console.WriteLine(status.Message);
                return 0;
            }

            Console.Error.WriteLine(status.Message);
            return status.ErrorType switch
            {
                LedSendErrorType.FileNotFound => 2,
                LedSendErrorType.InvalidImageSize => 7,
                LedSendErrorType.SdkNotInitialized => 3,
                LedSendErrorType.SdkSendFailed => 4,
                LedSendErrorType.NativeInteropError => 5,
                LedSendErrorType.Cancelled => 6,
                _ => 5,
            };
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine("Cancelled");
            return 6;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unhandled helper error: {ex.Message}");
            return 5;
        }
    }

    private static string? GetArgValue(string[] args, string key)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (!string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase)) continue;
            return i + 1 < args.Length ? args[i + 1] : null;
        }

        return null;
    }
}
