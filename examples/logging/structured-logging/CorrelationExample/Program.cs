using System.Diagnostics;
using Serilog;
using Serilog.Context;

namespace CorrelationExample;

/// <summary>
/// Demonstrates correlation ID implementation for request tracking
/// </summary>
class Program
{
    private static readonly ActivitySource ActivitySource = 
        new ActivitySource("CorrelationExample", "1.0.0");

    static async Task Main(string[] args)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                "logs/correlation-.txt",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        try
        {
            Log.Information("Application starting - Correlation Example");

            // Demonstrate different correlation approaches
            await DemonstrateAsyncLocalCorrelation();
            await DemonstrateActivityBasedCorrelation();
            await DemonstrateLogContextCorrelation();
            await DemonstrateMultiOperationTracking();

            Log.Information("Application completed successfully");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    /// <summary>
    /// Demonstrates correlation using AsyncLocal for thread-safe context propagation
    /// </summary>
    static async Task DemonstrateAsyncLocalCorrelation()
    {
        Log.Information("=== AsyncLocal Correlation Demonstration ===");

        // Start a new operation with correlation
        using (CorrelationContext.BeginCorrelationScope())
        {
            var correlationId = CorrelationContext.CorrelationId;
            
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                Log.Information("Starting package installation operation");

                await DownloadPackageAsync("chrome");
                await InstallPackageAsync("chrome");
                await VerifyInstallationAsync("chrome");

                Log.Information("Package installation completed");
            }
        }

        // Start another independent operation
        using (CorrelationContext.BeginCorrelationScope())
        {
            var correlationId = CorrelationContext.CorrelationId;
            
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                Log.Information("Starting package update operation");

                await CheckForUpdatesAsync("firefox");
                await DownloadPackageAsync("firefox");
                await InstallPackageAsync("firefox");

                Log.Information("Package update completed");
            }
        }
    }

    /// <summary>
    /// Demonstrates correlation using System.Diagnostics.Activity
    /// </summary>
    static async Task DemonstrateActivityBasedCorrelation()
    {
        Log.Information("=== Activity-Based Correlation Demonstration ===");

        using var activity = ActivitySource.StartActivity("PackageOperation");
        
        if (activity != null)
        {
            activity.SetTag("package.id", "nodejs");
            activity.SetTag("operation.type", "install");

            var correlationId = activity.Id;
            
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                Log.Information("Starting activity-tracked operation");

                // Child activity
                using (var childActivity = ActivitySource.StartActivity("DownloadPackage"))
                {
                    childActivity?.SetTag("package.id", "nodejs");
                    await DownloadPackageAsync("nodejs");
                }

                // Another child activity
                using (var childActivity = ActivitySource.StartActivity("InstallPackage"))
                {
                    childActivity?.SetTag("package.id", "nodejs");
                    await InstallPackageAsync("nodejs");
                }

                activity.SetStatus(ActivityStatusCode.Ok);
                Log.Information("Activity-tracked operation completed");
            }
        }
    }

    /// <summary>
    /// Demonstrates using Serilog's LogContext for correlation
    /// </summary>
    static async Task DemonstrateLogContextCorrelation()
    {
        Log.Information("=== LogContext Correlation Demonstration ===");

        var correlationId = Guid.NewGuid().ToString();
        var operationId = Guid.NewGuid().ToString();

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("OperationId", operationId))
        using (LogContext.PushProperty("UserId", "user123"))
        {
            Log.Information("Multi-package operation started");

            var packages = new[] { "vscode", "git", "python" };

            foreach (var package in packages)
            {
                using (LogContext.PushProperty("PackageId", package))
                {
                    Log.Information("Processing package");
                    await DownloadPackageAsync(package);
                    await InstallPackageAsync(package);
                }
            }

            Log.Information("Multi-package operation completed");
        }
    }

    /// <summary>
    /// Demonstrates tracking multiple concurrent operations
    /// </summary>
    static async Task DemonstrateMultiOperationTracking()
    {
        Log.Information("=== Multi-Operation Tracking Demonstration ===");

        var packages = new[] { "package1", "package2", "package3" };
        var tasks = new List<Task>();

        foreach (var package in packages)
        {
            tasks.Add(Task.Run(async () =>
            {
                var correlationId = Guid.NewGuid().ToString();
                
                using (LogContext.PushProperty("CorrelationId", correlationId))
                using (LogContext.PushProperty("PackageId", package))
                {
                    Log.Information("Starting concurrent operation");
                    
                    await DownloadPackageAsync(package);
                    await InstallPackageAsync(package);
                    
                    Log.Information("Concurrent operation completed");
                }
            }));
        }

        await Task.WhenAll(tasks);
        Log.Information("All concurrent operations completed");
    }

    // Simulated operations
    static async Task DownloadPackageAsync(string packageId)
    {
        Log.Debug("Downloading package from repository");
        await Task.Delay(Random.Shared.Next(100, 300));
        Log.Information("Package downloaded successfully");
    }

    static async Task InstallPackageAsync(string packageId)
    {
        Log.Debug("Installing package to system");
        await Task.Delay(Random.Shared.Next(200, 400));
        Log.Information("Package installed successfully");
    }

    static async Task VerifyInstallationAsync(string packageId)
    {
        Log.Debug("Verifying package installation");
        await Task.Delay(Random.Shared.Next(50, 150));
        Log.Information("Installation verified");
    }

    static async Task CheckForUpdatesAsync(string packageId)
    {
        Log.Debug("Checking for package updates");
        await Task.Delay(Random.Shared.Next(100, 200));
        Log.Information("Update check completed");
    }
}

/// <summary>
/// Provides correlation context using AsyncLocal for thread-safe propagation
/// </summary>
public static class CorrelationContext
{
    private static readonly AsyncLocal<string?> _correlationId = new();

    public static string CorrelationId
    {
        get => _correlationId.Value ?? throw new InvalidOperationException("No correlation scope active");
        private set => _correlationId.Value = value;
    }

    public static IDisposable BeginCorrelationScope(string? correlationId = null)
    {
        var previousId = _correlationId.Value;
        CorrelationId = correlationId ?? Guid.NewGuid().ToString();

        return new CorrelationScope(previousId);
    }

    private class CorrelationScope : IDisposable
    {
        private readonly string? _previousId;

        public CorrelationScope(string? previousId)
        {
            _previousId = previousId;
        }

        public void Dispose()
        {
            _correlationId.Value = _previousId;
        }
    }
}
