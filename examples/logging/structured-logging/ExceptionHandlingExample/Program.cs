using Serilog;

namespace ExceptionHandlingExample;

class Program
{
    static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        GlobalExceptionHandler.Initialize();

        try
        {
            Log.Information("Exception Handling Example starting");

            await DemonstrateLogAndRethrow();
            await DemonstrateLogAndWrap();
            await DemonstrateLogAndHandle();
            await DemonstrateStructuredExceptionLogging();

            Log.Information("Exception Handling Example completed");
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

    static async Task DemonstrateLogAndRethrow()
    {
        Log.Information("=== Log and Rethrow Pattern ===");
        
        try
        {
            await OperationThatMayFail("package1");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to process package {PackageId}", "package1");
            // In real scenario, would rethrow to let caller handle
            // throw;
        }
    }

    static async Task DemonstrateLogAndWrap()
    {
        Log.Information("=== Log and Wrap Pattern ===");
        
        try
        {
            await DatabaseOperation();
        }
        catch (IOException ex)
        {
            Log.Error(ex, "Database operation failed");
            var wrappedException = new InvalidOperationException(
                "Failed to complete database operation", ex);
            // In real scenario, would throw the wrapped exception
            Log.Error(wrappedException, "Wrapped exception created");
        }
    }

    static async Task DemonstrateLogAndHandle()
    {
        Log.Information("=== Log and Handle Pattern ===");
        
        var result = await TryGetPackageAsync("unknown-package");
        if (result == null)
        {
            Log.Warning("Package not found, using default");
        }
    }

    static async Task DemonstrateStructuredExceptionLogging()
    {
        Log.Information("=== Structured Exception Logging ===");
        
        try
        {
            await ComplexOperation();
        }
        catch (Exception ex)
        {
            LogException(ex, "ComplexOperation", new { PackageId = "test", UserId = "user123" });
        }
    }

    static async Task OperationThatMayFail(string packageId)
    {
        await Task.Delay(50);
        throw new InvalidOperationException($"Package {packageId} not found");
    }

    static async Task DatabaseOperation()
    {
        await Task.Delay(50);
        throw new IOException("Connection timeout");
    }

    static async Task<object?> TryGetPackageAsync(string packageId)
    {
        try
        {
            await Task.Delay(50);
            throw new KeyNotFoundException($"Package {packageId} not found");
        }
        catch (KeyNotFoundException ex)
        {
            Log.Warning(ex, "Package {PackageId} not found", packageId);
            return null;
        }
    }

    static async Task ComplexOperation()
    {
        try
        {
            await Task.Delay(50);
            throw new IOException("Disk error");
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException("Operation failed at critical stage", ex);
        }
    }

    static void LogException(Exception ex, string context, object? additionalData = null)
    {
        var properties = new Dictionary<string, object>
        {
            ["ExceptionType"] = ex.GetType().Name,
            ["ExceptionMessage"] = ex.Message,
            ["StackTrace"] = ex.StackTrace ?? "",
            ["InnerException"] = ex.InnerException?.GetType().Name ?? "None",
            ["Context"] = context
        };

        if (additionalData != null)
        {
            properties["AdditionalData"] = additionalData;
        }

        Log.Error(ex, "Exception in {Context}: {@Properties}", context, properties);
    }
}

public static class GlobalExceptionHandler
{
    public static void Initialize()
    {
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        Log.Fatal(exception, "Unhandled exception, IsTerminating: {IsTerminating}", 
            e.IsTerminating);
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unobserved task exception");
        e.SetObserved();
    }
}
