using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace DistributedTracingExample;

class Program
{
    private static readonly ActivitySource ActivitySource = 
        new ActivitySource("UniGetUI.PackageOperations", "1.0.0");

    static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService("UniGetUI", serviceVersion: "1.0.0"))
            .AddSource("UniGetUI.PackageOperations")
            .AddConsoleExporter()
            .Build();

        try
        {
            Log.Information("Distributed Tracing Example starting");

            await DemonstrateBasicTracing();
            await DemonstrateNestedSpans();
            await DemonstrateErrorTracing();

            Log.Information("Distributed Tracing Example completed");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    static async Task DemonstrateBasicTracing()
    {
        Log.Information("=== Basic Tracing ===");

        using var activity = ActivitySource.StartActivity("InstallPackage");
        activity?.SetTag("package.id", "vscode");
        activity?.SetTag("package.version", "1.85.0");
        
        await Task.Delay(100);
        
        activity?.SetStatus(ActivityStatusCode.Ok);
        Log.Information("Package installed successfully");
    }

    static async Task DemonstrateNestedSpans()
    {
        Log.Information("=== Nested Spans ===");

        using var activity = ActivitySource.StartActivity("CompletePackageOperation");
        activity?.SetTag("package.id", "python");
        
        using (var downloadActivity = ActivitySource.StartActivity("DownloadPackage"))
        {
            downloadActivity?.SetTag("package.id", "python");
            downloadActivity?.SetTag("download.size", 25_000_000);
            await Task.Delay(200);
            downloadActivity?.SetStatus(ActivityStatusCode.Ok);
        }
        
        using (var installActivity = ActivitySource.StartActivity("InstallPackage"))
        {
            installActivity?.SetTag("package.id", "python");
            await Task.Delay(300);
            installActivity?.SetStatus(ActivityStatusCode.Ok);
        }
        
        using (var verifyActivity = ActivitySource.StartActivity("VerifyInstallation"))
        {
            verifyActivity?.SetTag("package.id", "python");
            await Task.Delay(100);
            verifyActivity?.SetStatus(ActivityStatusCode.Ok);
        }
        
        activity?.SetStatus(ActivityStatusCode.Ok);
    }

    static async Task DemonstrateErrorTracing()
    {
        Log.Information("=== Error Tracing ===");

        using var activity = ActivitySource.StartActivity("FailingOperation");
        activity?.SetTag("package.id", "problematic-package");
        
        try
        {
            await Task.Delay(100);
            throw new InvalidOperationException("Package not found");
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("error.type", ex.GetType().Name);
            activity?.SetTag("error.message", ex.Message);
            Log.Error(ex, "Operation failed");
        }
    }
}
