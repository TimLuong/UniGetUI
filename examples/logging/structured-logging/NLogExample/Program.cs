using NLog;

namespace NLogExample;

class Program
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    static async Task Main(string[] args)
    {
        try
        {
            Logger.Info("NLog Example starting");

            DemonstrateBasicLogging();
            await DemonstrateStructuredLogging();
            DemonstrateExceptionLogging();

            Logger.Info("NLog Example completed");
        }
        catch (Exception ex)
        {
            Logger.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            LogManager.Shutdown();
        }
    }

    static void DemonstrateBasicLogging()
    {
        Logger.Info("=== Basic Logging ===");
        
        Logger.Trace("This is a trace message");
        Logger.Debug("This is a debug message");
        Logger.Info("This is an info message");
        Logger.Warn("This is a warning message");
        Logger.Error("This is an error message");
    }

    static async Task DemonstrateStructuredLogging()
    {
        Logger.Info("=== Structured Logging ===");
        
        var packageId = "Microsoft.PowerToys";
        var version = "0.75.1";
        
        Logger.Info("Installing package {PackageId} version {Version}", packageId, version);
        
        await Task.Delay(100);
        
        Logger.Info("Package {PackageId} installed successfully", packageId);
    }

    static void DemonstrateExceptionLogging()
    {
        Logger.Info("=== Exception Logging ===");
        
        try
        {
            throw new InvalidOperationException("Test exception");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Operation failed for package {PackageId}", "TestPackage");
        }
    }
}
