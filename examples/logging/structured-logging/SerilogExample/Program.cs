using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Context;

namespace SerilogExample;

/// <summary>
/// Demonstrates structured logging using Serilog with various features:
/// - Structured properties
/// - Multiple log levels
/// - Log enrichment
/// - Log context
/// - File and console sinks
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        try
        {
            Log.Information("Application starting up");
            
            // Demonstrate different logging patterns
            await DemonstrateBasicLogging();
            await DemonstrateStructuredLogging();
            await DemonstrateLogContext();
            await DemonstrateExceptionLogging();
            
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
    /// Demonstrates basic logging at different levels
    /// </summary>
    static async Task DemonstrateBasicLogging()
    {
        Log.Information("=== Basic Logging Demonstration ===");
        
        Log.Verbose("This is a verbose message - typically used for very detailed diagnostic information");
        Log.Debug("This is a debug message - used for detailed diagnostic information");
        Log.Information("This is an information message - tracks general application flow");
        Log.Warning("This is a warning message - indicates unexpected but recoverable situations");
        Log.Error("This is an error message - indicates a failure in the current operation");
        
        await Task.Delay(100); // Simulate some work
    }

    /// <summary>
    /// Demonstrates structured logging with properties
    /// </summary>
    static async Task DemonstrateStructuredLogging()
    {
        Log.Information("=== Structured Logging Demonstration ===");
        
        // Simple structured properties
        var packageId = "Microsoft.PowerToys";
        var version = "0.75.1";
        
        Log.Information("Installing package {PackageId} version {Version}", packageId, version);
        
        // Complex object logging
        var packageInfo = new PackageInfo
        {
            Id = packageId,
            Name = "PowerToys",
            Version = version,
            Size = 145_234_567,
            Source = "winget"
        };
        
        Log.Information("Package details: {@PackageInfo}", packageInfo);
        
        // Destructuring with $
        Log.Information("Package summary: {$PackageInfo}", packageInfo);
        
        // Timing operations
        var startTime = DateTime.UtcNow;
        await Task.Delay(250); // Simulate package installation
        var duration = DateTime.UtcNow - startTime;
        
        Log.Information(
            "Package {PackageId} installed successfully in {Duration}ms",
            packageId,
            duration.TotalMilliseconds);
        
        // Multiple properties
        Log.Information(
            "Operation completed: {PackageId}, {Version}, {Duration}ms, {Success}",
            packageId, version, duration.TotalMilliseconds, true);
    }

    /// <summary>
    /// Demonstrates using log context for automatic property inclusion
    /// </summary>
    static async Task DemonstrateLogContext()
    {
        Log.Information("=== Log Context Demonstration ===");
        
        var userId = "user123";
        var operationId = Guid.NewGuid().ToString();
        
        // All logs within this scope will include UserId and OperationId
        using (LogContext.PushProperty("UserId", userId))
        using (LogContext.PushProperty("OperationId", operationId))
        {
            Log.Information("Starting user operation");
            
            await Task.Delay(100);
            Log.Debug("Processing step 1");
            
            await Task.Delay(100);
            Log.Debug("Processing step 2");
            
            await Task.Delay(100);
            Log.Information("User operation completed");
        }
        
        // Nested contexts
        using (LogContext.PushProperty("RequestId", Guid.NewGuid()))
        {
            Log.Information("Outer context");
            
            using (LogContext.PushProperty("SubRequestId", Guid.NewGuid()))
            {
                Log.Information("Inner context - has both RequestId and SubRequestId");
            }
            
            Log.Information("Back to outer context - only has RequestId");
        }
    }

    /// <summary>
    /// Demonstrates exception logging
    /// </summary>
    static async Task DemonstrateExceptionLogging()
    {
        Log.Information("=== Exception Logging Demonstration ===");
        
        try
        {
            await SimulateFailingOperation();
        }
        catch (InvalidOperationException ex)
        {
            // Log the exception with context
            Log.Error(ex, "Failed to complete operation for package {PackageId}", "TestPackage");
        }
        
        try
        {
            await SimulateNestedExceptions();
        }
        catch (Exception ex)
        {
            // Serilog automatically includes inner exceptions
            Log.Error(ex, "Operation failed with nested exceptions");
        }
    }

    static async Task SimulateFailingOperation()
    {
        await Task.Delay(50);
        throw new InvalidOperationException("Package installation failed: Access denied");
    }

    static async Task SimulateNestedExceptions()
    {
        try
        {
            await Task.Delay(50);
            throw new IOException("Disk write failed");
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException("Package extraction failed", ex);
        }
    }
}

/// <summary>
/// Example data class for structured logging
/// </summary>
public class PackageInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Source { get; set; } = string.Empty;
    
    public override string ToString()
    {
        return $"{Name} {Version} ({Id})";
    }
}
