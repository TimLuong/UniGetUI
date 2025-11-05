// Example: Error Recovery Strategies and Graceful Degradation
// This file demonstrates various error recovery patterns and fallback strategies

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UniGetUI.Examples.ErrorHandling;

// ============================================================================
// GRACEFUL DEGRADATION
// ============================================================================

/// <summary>
/// Demonstrates graceful degradation patterns - maintaining functionality
/// even when errors occur by falling back to alternative approaches.
/// </summary>
public class GracefulDegradationExamples
{
    /// <summary>
    /// Example 1: Multi-level fallback strategy
    /// Try primary -> cache -> default value
    /// </summary>
    public static async Task<List<Package>> Example_MultiLevelFallback(string query)
    {
        Console.WriteLine($"Searching for: {query}");

        try
        {
            // Primary: Try online search
            Console.WriteLine("Attempting online search...");
            return await SearchOnlineAsync(query);
        }
        catch (NetworkException ex)
        {
            Console.WriteLine($"Online search failed: {ex.Message}");

            try
            {
                // Fallback 1: Try cache
                Console.WriteLine("Falling back to cache...");
                var cached = await SearchCacheAsync(query);
                if (cached.Any())
                {
                    Console.WriteLine($"✓ Returned {cached.Count} results from cache");
                    return cached;
                }
            }
            catch (Exception cacheEx)
            {
                Console.WriteLine($"Cache search failed: {cacheEx.Message}");
            }

            // Fallback 2: Return empty list to maintain functionality
            Console.WriteLine("No results available, returning empty list");
            return new List<Package>();
        }
    }

    /// <summary>
    /// Example 2: Partial success pattern
    /// Continue even if some operations fail
    /// </summary>
    public static async Task<PackageListResult> Example_PartialSuccess()
    {
        var managers = new[] { "WinGet", "Chocolatey", "Scoop", "Cargo" };
        var allPackages = new List<Package>();
        var failedManagers = new List<string>();
        var warningMessage = string.Empty;

        foreach (var manager in managers)
        {
            try
            {
                Console.WriteLine($"Getting packages from {manager}...");
                var packages = await GetPackagesFromManagerAsync(manager);
                allPackages.AddRange(packages);
                Console.WriteLine($"✓ Got {packages.Count} packages from {manager}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed to get packages from {manager}: {ex.Message}");
                failedManagers.Add(manager);
            }
        }

        // Create warning message if some managers failed
        if (failedManagers.Any())
        {
            warningMessage = $"Some package managers were unavailable: {string.Join(", ", failedManagers)}";
            Console.WriteLine($"⚠ {warningMessage}");
        }

        return new PackageListResult
        {
            Packages = allPackages,
            SuccessCount = managers.Length - failedManagers.Count,
            FailureCount = failedManagers.Count,
            WarningMessage = warningMessage,
            IsPartialSuccess = failedManagers.Any()
        };
    }

    /// <summary>
    /// Example 3: Degraded mode operation
    /// Continue with limited functionality when services are unavailable
    /// </summary>
    public static async Task<OperationResult> Example_DegradedMode(string packageId)
    {
        var result = new OperationResult { PackageId = packageId };

        try
        {
            // Try full operation with all features
            Console.WriteLine($"Attempting full installation of {packageId}...");
            result.Details = await GetFullPackageDetailsAsync(packageId);
            result.InstallationSuccess = await InstallWithValidationAsync(packageId, result.Details);
            result.OperationMode = OperationMode.Full;
            Console.WriteLine("✓ Full installation completed");
        }
        catch (NetworkException ex)
        {
            Console.WriteLine($"Network unavailable: {ex.Message}");
            Console.WriteLine("Switching to degraded mode...");

            try
            {
                // Degraded mode: Install without full details
                result.Details = GetBasicPackageDetails(packageId);
                result.InstallationSuccess = await InstallBasicAsync(packageId);
                result.OperationMode = OperationMode.Degraded;
                result.WarningMessage = "Installation completed in degraded mode. Some features were unavailable.";
                Console.WriteLine("✓ Installation completed with limited functionality");
            }
            catch (Exception degradedEx)
            {
                Console.WriteLine($"✗ Installation failed: {degradedEx.Message}");
                result.OperationMode = OperationMode.Failed;
                result.ErrorMessage = "Installation failed. Please try again later.";
            }
        }

        return result;
    }

    // Helper methods
    private static async Task<List<Package>> SearchOnlineAsync(string query)
    {
        await Task.Delay(100);
        if (new Random().Next(0, 2) == 0)
            throw new NetworkException("Network unavailable");
        return new List<Package> { new Package { Id = query, Name = $"{query} package" } };
    }

    private static async Task<List<Package>> SearchCacheAsync(string query)
    {
        await Task.Delay(50);
        return new List<Package> { new Package { Id = query, Name = $"{query} (cached)" } };
    }

    private static async Task<List<Package>> GetPackagesFromManagerAsync(string manager)
    {
        await Task.Delay(100);
        if (manager == "Cargo" && new Random().Next(0, 2) == 0)
            throw new PackageManagerUnavailableException(manager, "Service unavailable");
        return new List<Package> { new Package { Id = $"{manager}-package", Name = $"Package from {manager}" } };
    }

    private static async Task<PackageDetails> GetFullPackageDetailsAsync(string packageId)
    {
        await Task.Delay(100);
        if (new Random().Next(0, 2) == 0)
            throw new NetworkException("Cannot fetch details");
        return new PackageDetails { Id = packageId, Description = "Full details" };
    }

    private static PackageDetails GetBasicPackageDetails(string packageId)
    {
        return new PackageDetails { Id = packageId, Description = "Basic info only" };
    }

    private static async Task<bool> InstallWithValidationAsync(string packageId, PackageDetails details)
    {
        await Task.Delay(100);
        return true;
    }

    private static async Task<bool> InstallBasicAsync(string packageId)
    {
        await Task.Delay(100);
        return true;
    }
}

// ============================================================================
// SAFE EXECUTION WRAPPERS
// ============================================================================

/// <summary>
/// Utility class for safely executing operations with automatic error handling.
/// </summary>
public static class SafeExecutor
{
    /// <summary>
    /// Execute synchronous operation safely with default value on error
    /// </summary>
    public static T Execute<T>(
        Func<T> operation,
        T defaultValue,
        string operationName,
        bool logErrors = true)
    {
        try
        {
            return operation();
        }
        catch (Exception ex)
        {
            if (logErrors)
            {
                Console.WriteLine($"[ERROR] {operationName}: {ex.Message}");
            }
            return defaultValue;
        }
    }

    /// <summary>
    /// Execute async operation safely with default value on error
    /// </summary>
    public static async Task<T> ExecuteAsync<T>(
        Func<Task<T>> operation,
        T defaultValue,
        string operationName,
        bool logErrors = true)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex)
        {
            if (logErrors)
            {
                Console.WriteLine($"[ERROR] {operationName}: {ex.Message}");
            }
            return defaultValue;
        }
    }

    /// <summary>
    /// Try to execute a void operation, return success status
    /// </summary>
    public static bool TryExecute(
        Action operation,
        string operationName,
        bool logErrors = true)
    {
        try
        {
            operation();
            return true;
        }
        catch (Exception ex)
        {
            if (logErrors)
            {
                Console.WriteLine($"[ERROR] {operationName}: {ex.Message}");
            }
            return false;
        }
    }

    /// <summary>
    /// Execute with multiple fallback operations
    /// </summary>
    public static async Task<T> ExecuteWithFallbacksAsync<T>(
        params (Func<Task<T>> operation, string name)[] operations)
    {
        Exception? lastException = null;

        foreach (var (operation, name) in operations)
        {
            try
            {
                Console.WriteLine($"Trying: {name}");
                var result = await operation();
                Console.WriteLine($"✓ Success: {name}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed: {name} - {ex.Message}");
                lastException = ex;
            }
        }

        throw new InvalidOperationException(
            "All operations failed",
            lastException);
    }
}

/// <summary>
/// Examples of using SafeExecutor
/// </summary>
public static class SafeExecutorExamples
{
    public static void Example_BasicSafeExecution()
    {
        // Execute with automatic error handling
        var result = SafeExecutor.Execute(
            operation: () => ParseIntSafely("123"),
            defaultValue: 0,
            operationName: "ParseInteger");

        Console.WriteLine($"Result: {result}");

        // This will fail gracefully
        var failedResult = SafeExecutor.Execute(
            operation: () => ParseIntSafely("invalid"),
            defaultValue: 0,
            operationName: "ParseInteger");

        Console.WriteLine($"Failed result (default): {failedResult}");
    }

    public static async Task Example_AsyncSafeExecution()
    {
        var packages = await SafeExecutor.ExecuteAsync(
            operation: async () => await FetchPackagesAsync(),
            defaultValue: new List<Package>(),
            operationName: "FetchPackages");

        Console.WriteLine($"Retrieved {packages.Count} packages");
    }

    public static async Task Example_MultipleFallbacks()
    {
        var result = await SafeExecutor.ExecuteWithFallbacksAsync(
            (async () => await FetchFromPrimarySourceAsync(), "Primary Source"),
            (async () => await FetchFromCacheAsync(), "Cache"),
            (async () => await FetchFromBackupSourceAsync(), "Backup Source"),
            (() => Task.FromResult(GetDefaultData()), "Default Data")
        );

        Console.WriteLine($"Got data: {result}");
    }

    private static int ParseIntSafely(string value)
    {
        return int.Parse(value);
    }

    private static async Task<List<Package>> FetchPackagesAsync()
    {
        await Task.Delay(100);
        if (new Random().Next(0, 2) == 0)
            throw new NetworkException("Network error");
        return new List<Package>();
    }

    private static async Task<string> FetchFromPrimarySourceAsync()
    {
        await Task.Delay(100);
        throw new NetworkException("Primary source unavailable");
    }

    private static async Task<string> FetchFromCacheAsync()
    {
        await Task.Delay(50);
        return "Cached data";
    }

    private static async Task<string> FetchFromBackupSourceAsync()
    {
        await Task.Delay(100);
        throw new NetworkException("Backup source unavailable");
    }

    private static string GetDefaultData()
    {
        return "Default data";
    }
}

// ============================================================================
// ERROR RECOVERY WITH STATE MANAGEMENT
// ============================================================================

/// <summary>
/// Service that maintains state and can recover from errors
/// </summary>
public class StatefulServiceWithRecovery
{
    private List<Package> _packageCache = new();
    private DateTime _lastSuccessfulUpdate = DateTime.MinValue;
    private int _consecutiveFailures = 0;
    private const int MaxConsecutiveFailures = 5;

    /// <summary>
    /// Get packages with automatic recovery and state management
    /// </summary>
    public async Task<ServiceResult<List<Package>>> GetPackagesAsync()
    {
        try
        {
            // Try to fetch fresh data
            var packages = await FetchPackagesFromSourceAsync();
            
            // Update state on success
            _packageCache = packages;
            _lastSuccessfulUpdate = DateTime.Now;
            _consecutiveFailures = 0;

            Console.WriteLine($"✓ Successfully fetched {packages.Count} packages");
            return ServiceResult<List<Package>>.Success(packages);
        }
        catch (Exception ex)
        {
            _consecutiveFailures++;
            Console.WriteLine($"✗ Fetch failed (attempt {_consecutiveFailures}): {ex.Message}");

            // Check if we should use cache
            if (_packageCache.Any() && _lastSuccessfulUpdate > DateTime.Now.AddHours(-24))
            {
                Console.WriteLine($"⚠ Returning cached data from {_lastSuccessfulUpdate:HH:mm:ss}");
                return ServiceResult<List<Package>>.SuccessWithWarning(
                    _packageCache,
                    $"Using cached data. Last updated: {_lastSuccessfulUpdate:g}");
            }

            // Check if service should enter recovery mode
            if (_consecutiveFailures >= MaxConsecutiveFailures)
            {
                Console.WriteLine("⚠ Service entering recovery mode");
                return ServiceResult<List<Package>>.Failure(
                    new List<Package>(),
                    "Service is temporarily unavailable. Please try again later.");
            }

            // Return empty list with error
            return ServiceResult<List<Package>>.Failure(
                new List<Package>(),
                "Unable to fetch packages. Please check your connection.");
        }
    }

    /// <summary>
    /// Reset the service state after recovery
    /// </summary>
    public void Reset()
    {
        _consecutiveFailures = 0;
        Console.WriteLine("Service state reset");
    }

    private async Task<List<Package>> FetchPackagesFromSourceAsync()
    {
        await Task.Delay(100);
        
        // Simulate failures
        if (new Random().Next(0, 3) < 2) // 66% failure rate for demo
        {
            throw new NetworkException("Simulated fetch failure");
        }

        return new List<Package>
        {
            new Package { Id = "package1", Name = "Package 1" },
            new Package { Id = "package2", Name = "Package 2" }
        };
    }
}

/// <summary>
/// Generic result wrapper for service operations
/// </summary>
public class ServiceResult<T>
{
    public T Data { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? WarningMessage { get; set; }

    public static ServiceResult<T> Success(T data)
    {
        return new ServiceResult<T>
        {
            Data = data,
            IsSuccess = true
        };
    }

    public static ServiceResult<T> SuccessWithWarning(T data, string warning)
    {
        return new ServiceResult<T>
        {
            Data = data,
            IsSuccess = true,
            WarningMessage = warning
        };
    }

    public static ServiceResult<T> Failure(T defaultData, string error)
    {
        return new ServiceResult<T>
        {
            Data = defaultData,
            IsSuccess = false,
            ErrorMessage = error
        };
    }
}

/// <summary>
/// Example usage of stateful service
/// </summary>
public static class StatefulServiceExamples
{
    public static async Task Example_StatefulServiceWithRecovery()
    {
        var service = new StatefulServiceWithRecovery();

        // Simulate multiple attempts
        for (int i = 1; i <= 8; i++)
        {
            Console.WriteLine($"\n--- Attempt {i} ---");
            var result = await service.GetPackagesAsync();

            if (result.IsSuccess)
            {
                Console.WriteLine($"Success! Got {result.Data.Count} packages");
                if (!string.IsNullOrEmpty(result.WarningMessage))
                {
                    Console.WriteLine($"Warning: {result.WarningMessage}");
                }
            }
            else
            {
                Console.WriteLine($"Failed: {result.ErrorMessage}");
            }

            await Task.Delay(500);
        }

        // Reset and try again
        Console.WriteLine("\n--- Resetting service ---");
        service.Reset();
        
        var finalResult = await service.GetPackagesAsync();
        Console.WriteLine($"Final result: {(finalResult.IsSuccess ? "Success" : "Failure")}");
    }
}

// ============================================================================
// SUPPORTING CLASSES
// ============================================================================

public class Package
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class PackageDetails
{
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class PackageListResult
{
    public List<Package> Packages { get; set; } = new();
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public string WarningMessage { get; set; } = string.Empty;
    public bool IsPartialSuccess { get; set; }
}

public enum OperationMode
{
    Full,
    Degraded,
    Failed
}

public class OperationResult
{
    public string PackageId { get; set; } = string.Empty;
    public PackageDetails? Details { get; set; }
    public bool InstallationSuccess { get; set; }
    public OperationMode OperationMode { get; set; }
    public string? WarningMessage { get; set; }
    public string? ErrorMessage { get; set; }
}

// ============================================================================
// DEMO
// ============================================================================

public class ErrorRecoveryDemo
{
    public static async Task Main()
    {
        Console.WriteLine("=== Multi-Level Fallback ===\n");
        await GracefulDegradationExamples.Example_MultiLevelFallback("nodejs");
        
        Console.WriteLine("\n\n=== Partial Success ===\n");
        await GracefulDegradationExamples.Example_PartialSuccess();
        
        Console.WriteLine("\n\n=== Stateful Service Recovery ===");
        await StatefulServiceExamples.Example_StatefulServiceWithRecovery();
    }
}
