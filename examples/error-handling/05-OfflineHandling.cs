// Example: Offline Scenario Handling and User-Friendly Error Messages
// This file demonstrates handling offline scenarios and creating user-friendly error messages

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace UniGetUI.Examples.ErrorHandling;

// ============================================================================
// NETWORK MONITORING
// ============================================================================

/// <summary>
/// Monitors network connectivity and notifies subscribers of changes
/// </summary>
public class NetworkMonitor
{
    private bool _isOnline = true;
    public event EventHandler<bool>? ConnectivityChanged;

    public bool IsOnline => _isOnline;

    public NetworkMonitor()
    {
        // Monitor network availability changes
        NetworkChange.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged;
        UpdateNetworkStatus();
    }

    private void OnNetworkAvailabilityChanged(object? sender, NetworkAvailabilityEventArgs e)
    {
        UpdateNetworkStatus();
    }

    private void UpdateNetworkStatus()
    {
        var wasOnline = _isOnline;
        _isOnline = NetworkInterface.GetIsNetworkAvailable();

        if (wasOnline != _isOnline)
        {
            Console.WriteLine($"[NetworkMonitor] Status changed: {(_isOnline ? "Online" : "Offline")}");
            ConnectivityChanged?.Invoke(this, _isOnline);
        }
    }

    /// <summary>
    /// Simulate network status change for testing
    /// </summary>
    public void SimulateStatusChange(bool online)
    {
        if (_isOnline != online)
        {
            _isOnline = online;
            Console.WriteLine($"[NetworkMonitor] Simulated status: {(_isOnline ? "Online" : "Offline")}");
            ConnectivityChanged?.Invoke(this, _isOnline);
        }
    }
}

// ============================================================================
// OFFLINE-AWARE SERVICE
// ============================================================================

/// <summary>
/// Package service that gracefully handles offline scenarios
/// </summary>
public class OfflineAwarePackageService
{
    private readonly NetworkMonitor _networkMonitor;
    private readonly Dictionary<string, PackageDetails> _cache = new();
    private readonly Dictionary<string, DateTime> _cacheTimestamps = new();
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(24);

    public OfflineAwarePackageService(NetworkMonitor networkMonitor)
    {
        _networkMonitor = networkMonitor;
        _networkMonitor.ConnectivityChanged += OnConnectivityChanged;
    }

    private void OnConnectivityChanged(object? sender, bool isOnline)
    {
        if (isOnline)
        {
            Console.WriteLine("✓ Network restored - online features available");
        }
        else
        {
            Console.WriteLine("⚠ Network lost - switching to offline mode");
        }
    }

    /// <summary>
    /// Get package details with offline support
    /// </summary>
    public async Task<OfflineResult<PackageDetails>> GetPackageDetailsAsync(string packageId)
    {
        // Check if we're offline
        if (!_networkMonitor.IsOnline)
        {
            Console.WriteLine($"[Offline Mode] Checking cache for {packageId}");
            
            var cached = TryGetFromCache(packageId);
            if (cached != null)
            {
                return OfflineResult<PackageDetails>.FromCache(
                    cached,
                    "You're offline. Showing cached information.");
            }

            return OfflineResult<PackageDetails>.Unavailable(
                "You're offline and this package information is not available in cache. Please connect to the internet and try again.");
        }

        // We're online - try to fetch
        try
        {
            Console.WriteLine($"[Online Mode] Fetching details for {packageId}");
            var details = await FetchPackageDetailsAsync(packageId);
            
            // Update cache
            CachePackageDetails(packageId, details);
            
            return OfflineResult<PackageDetails>.Online(details);
        }
        catch (NetworkException ex)
        {
            Console.WriteLine($"[Network Error] {ex.Message}");
            
            // Network error while supposedly online - check cache
            var cached = TryGetFromCache(packageId);
            if (cached != null)
            {
                return OfflineResult<PackageDetails>.FromCache(
                    cached,
                    "Unable to fetch latest information. Showing cached data.");
            }

            return OfflineResult<PackageDetails>.Unavailable(
                "Unable to fetch package information. Please check your connection and try again.");
        }
    }

    /// <summary>
    /// Install package - requires internet connection
    /// </summary>
    public async Task<OfflineResult<bool>> InstallPackageAsync(string packageId)
    {
        if (!_networkMonitor.IsOnline)
        {
            return OfflineResult<bool>.RequiresOnline(
                false,
                $"Cannot install '{packageId}' while offline. Package installation requires an internet connection.");
        }

        try
        {
            Console.WriteLine($"[Online Mode] Installing {packageId}");
            await Task.Delay(100); // Simulate installation
            
            Console.WriteLine($"✓ Successfully installed {packageId}");
            return OfflineResult<bool>.Online(true);
        }
        catch (Exception ex)
        {
            return OfflineResult<bool>.Error(
                false,
                $"Failed to install '{packageId}': {ex.Message}");
        }
    }

    /// <summary>
    /// Search packages with offline support
    /// </summary>
    public async Task<OfflineResult<List<string>>> SearchPackagesAsync(string query)
    {
        if (!_networkMonitor.IsOnline)
        {
            // Offline: Search only cached packages
            var cachedResults = _cache.Keys
                .Where(k => k.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (cachedResults.Any())
            {
                return OfflineResult<List<string>>.FromCache(
                    cachedResults,
                    $"You're offline. Showing {cachedResults.Count} cached results. Connect to the internet for more results.");
            }

            return OfflineResult<List<string>>.Unavailable(
                "You're offline and no cached results are available. Please connect to the internet to search.");
        }

        try
        {
            Console.WriteLine($"[Online Mode] Searching for: {query}");
            var results = await SearchOnlineAsync(query);
            return OfflineResult<List<string>>.Online(results);
        }
        catch (NetworkException)
        {
            // Fallback to cache
            var cachedResults = _cache.Keys
                .Where(k => k.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (cachedResults.Any())
            {
                return OfflineResult<List<string>>.FromCache(
                    cachedResults,
                    "Network error. Showing cached results.");
            }

            return OfflineResult<List<string>>.Error(
                new List<string>(),
                "Search failed. Please check your connection and try again.");
        }
    }

    // Helper methods
    private PackageDetails? TryGetFromCache(string packageId)
    {
        if (_cache.TryGetValue(packageId, out var details) &&
            _cacheTimestamps.TryGetValue(packageId, out var timestamp))
        {
            if (DateTime.Now - timestamp < _cacheExpiration)
            {
                Console.WriteLine($"✓ Cache hit for {packageId} (age: {(DateTime.Now - timestamp).TotalMinutes:F1} minutes)");
                return details;
            }
            else
            {
                Console.WriteLine($"⚠ Cache expired for {packageId}");
            }
        }
        return null;
    }

    private void CachePackageDetails(string packageId, PackageDetails details)
    {
        _cache[packageId] = details;
        _cacheTimestamps[packageId] = DateTime.Now;
        Console.WriteLine($"✓ Cached details for {packageId}");
    }

    private async Task<PackageDetails> FetchPackageDetailsAsync(string packageId)
    {
        await Task.Delay(100);
        return new PackageDetails
        {
            Id = packageId,
            Name = $"{packageId} Package",
            Description = "This is a detailed description",
            Version = "1.0.0"
        };
    }

    private async Task<List<string>> SearchOnlineAsync(string query)
    {
        await Task.Delay(100);
        return new List<string> { $"{query}1", $"{query}2", $"{query}3" };
    }
}

/// <summary>
/// Result wrapper that tracks offline/online status
/// </summary>
public class OfflineResult<T>
{
    public T Data { get; set; }
    public bool IsSuccess { get; set; }
    public ResultSource Source { get; set; }
    public string? Message { get; set; }

    public enum ResultSource
    {
        Online,
        Cache,
        Unavailable,
        RequiresOnline,
        Error
    }

    public static OfflineResult<T> Online(T data)
    {
        return new OfflineResult<T>
        {
            Data = data,
            IsSuccess = true,
            Source = ResultSource.Online
        };
    }

    public static OfflineResult<T> FromCache(T data, string message)
    {
        return new OfflineResult<T>
        {
            Data = data,
            IsSuccess = true,
            Source = ResultSource.Cache,
            Message = message
        };
    }

    public static OfflineResult<T> Unavailable(string message)
    {
        return new OfflineResult<T>
        {
            Data = default!,
            IsSuccess = false,
            Source = ResultSource.Unavailable,
            Message = message
        };
    }

    public static OfflineResult<T> RequiresOnline(T defaultData, string message)
    {
        return new OfflineResult<T>
        {
            Data = defaultData,
            IsSuccess = false,
            Source = ResultSource.RequiresOnline,
            Message = message
        };
    }

    public static OfflineResult<T> Error(T defaultData, string message)
    {
        return new OfflineResult<T>
        {
            Data = defaultData,
            IsSuccess = false,
            Source = ResultSource.Error,
            Message = message
        };
    }
}

// ============================================================================
// USER-FRIENDLY ERROR MESSAGES
// ============================================================================

/// <summary>
/// Builder for creating user-friendly error messages
/// </summary>
public class ErrorMessageBuilder
{
    private string? _operation;
    private string? _target;
    private string? _reason;
    private string? _suggestion;
    private string? _context;

    public ErrorMessageBuilder ForOperation(string operation)
    {
        _operation = operation;
        return this;
    }

    public ErrorMessageBuilder OnTarget(string target)
    {
        _target = target;
        return this;
    }

    public ErrorMessageBuilder Because(string reason)
    {
        _reason = reason;
        return this;
    }

    public ErrorMessageBuilder WithSuggestion(string suggestion)
    {
        _suggestion = suggestion;
        return this;
    }

    public ErrorMessageBuilder WithContext(string context)
    {
        _context = context;
        return this;
    }

    public string Build()
    {
        var message = new StringBuilder();

        // Main message
        if (!string.IsNullOrEmpty(_operation) && !string.IsNullOrEmpty(_target))
        {
            message.Append($"Could not {_operation} {_target}");
        }
        else if (!string.IsNullOrEmpty(_operation))
        {
            message.Append($"Could not {_operation}");
        }
        else
        {
            message.Append("An error occurred");
        }

        // Reason
        if (!string.IsNullOrEmpty(_reason))
        {
            message.Append($": {_reason}");
        }

        message.Append(".");

        // Context
        if (!string.IsNullOrEmpty(_context))
        {
            message.Append($" {_context}.");
        }

        // Suggestion
        if (!string.IsNullOrEmpty(_suggestion))
        {
            message.Append($" {_suggestion}.");
        }

        return message.ToString();
    }
}

/// <summary>
/// Examples of user-friendly error messages
/// </summary>
public static class ErrorMessageExamples
{
    /// <summary>
    /// Example 1: Basic error message building
    /// </summary>
    public static void Example_BasicErrorMessages()
    {
        Console.WriteLine("=== Basic Error Messages ===\n");

        // Simple error
        var msg1 = new ErrorMessageBuilder()
            .ForOperation("install")
            .OnTarget("package 'nodejs'")
            .Build();
        Console.WriteLine($"1. {msg1}");
        // Output: Could not install package 'nodejs'.

        // Error with reason
        var msg2 = new ErrorMessageBuilder()
            .ForOperation("install")
            .OnTarget("package 'nodejs'")
            .Because("the server is temporarily unavailable")
            .Build();
        Console.WriteLine($"2. {msg2}");
        // Output: Could not install package 'nodejs': the server is temporarily unavailable.

        // Error with reason and suggestion
        var msg3 = new ErrorMessageBuilder()
            .ForOperation("install")
            .OnTarget("package 'nodejs'")
            .Because("the server is temporarily unavailable")
            .WithSuggestion("Please try again in a few minutes")
            .Build();
        Console.WriteLine($"3. {msg3}");
        // Output: Could not install package 'nodejs': the server is temporarily unavailable. Please try again in a few minutes.

        // Full error message with context
        var msg4 = new ErrorMessageBuilder()
            .ForOperation("update")
            .OnTarget("package 'python'")
            .Because("you don't have permission")
            .WithContext("Administrator privileges are required")
            .WithSuggestion("Please run UniGetUI as administrator and try again")
            .Build();
        Console.WriteLine($"4. {msg4}");
        // Output: Could not update package 'python': you don't have permission. Administrator privileges are required. Please run UniGetUI as administrator and try again.
    }

    /// <summary>
    /// Example 2: Context-specific error messages
    /// </summary>
    public static void Example_ContextSpecificMessages()
    {
        Console.WriteLine("\n=== Context-Specific Messages ===\n");

        // Network error
        var networkError = new ErrorMessageBuilder()
            .ForOperation("download")
            .OnTarget("package updates")
            .Because("you appear to be offline")
            .WithSuggestion("Please check your internet connection and try again")
            .Build();
        Console.WriteLine($"Network: {networkError}");

        // Permission error
        var permissionError = new ErrorMessageBuilder()
            .ForOperation("install")
            .OnTarget("system package")
            .Because("administrator privileges are required")
            .WithSuggestion("Right-click UniGetUI and select 'Run as administrator'")
            .Build();
        Console.WriteLine($"Permission: {permissionError}");

        // Validation error
        var validationError = new ErrorMessageBuilder()
            .ForOperation("search for")
            .OnTarget("packages")
            .Because("the search term is too short")
            .WithSuggestion("Please enter at least 3 characters")
            .Build();
        Console.WriteLine($"Validation: {validationError}");

        // Not found error
        var notFoundError = new ErrorMessageBuilder()
            .ForOperation("find")
            .OnTarget("package 'nonexistent-app'")
            .Because("it doesn't exist in any package manager")
            .WithSuggestion("Please check the package name and try again")
            .Build();
        Console.WriteLine($"Not Found: {notFoundError}");
    }

    /// <summary>
    /// Example 3: Good vs Bad error messages
    /// </summary>
    public static void Example_GoodVsBadMessages()
    {
        Console.WriteLine("\n=== Good vs Bad Error Messages ===\n");

        Console.WriteLine("❌ BAD EXAMPLES:");
        Console.WriteLine("1. Error: NullReferenceException in PackageManager.cs line 42");
        Console.WriteLine("2. Operation failed with exit code -1");
        Console.WriteLine("3. An error occurred");
        Console.WriteLine("4. HTTP 503 Service Unavailable");
        Console.WriteLine("5. Failed to deserialize JSON response");

        Console.WriteLine("\n✅ GOOD EXAMPLES:");
        Console.WriteLine("1. Could not install package 'nodejs'. Please check your internet connection and try again.");
        Console.WriteLine("2. The package 'example-app' is already installed. Would you like to update it instead?");
        Console.WriteLine("3. Unable to connect to the package server. You can continue working with already installed packages.");
        Console.WriteLine("4. Package installation requires an internet connection. Please connect to the internet and try again.");
        Console.WriteLine("5. Could not load package information. The data may be corrupted. Please try refreshing the package list.");
    }
}

// ============================================================================
// OFFLINE-AWARE UI FEEDBACK
// ============================================================================

/// <summary>
/// Manages UI feedback for offline scenarios
/// </summary>
public class OfflineModeBanner
{
    private readonly NetworkMonitor _networkMonitor;
    private bool _isVisible;

    public OfflineModeBanner(NetworkMonitor networkMonitor)
    {
        _networkMonitor = networkMonitor;
        _networkMonitor.ConnectivityChanged += OnConnectivityChanged;
        UpdateBannerVisibility();
    }

    private void OnConnectivityChanged(object? sender, bool isOnline)
    {
        UpdateBannerVisibility();
    }

    private void UpdateBannerVisibility()
    {
        if (!_networkMonitor.IsOnline)
        {
            if (!_isVisible)
            {
                ShowOfflineBanner();
                _isVisible = true;
            }
        }
        else
        {
            if (_isVisible)
            {
                HideOfflineBanner();
                _isVisible = false;
            }
        }
    }

    private void ShowOfflineBanner()
    {
        Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  ⚠ You're Offline                                       ║");
        Console.WriteLine("║  Some features are unavailable. You can still view and  ║");
        Console.WriteLine("║  manage installed packages.                             ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝\n");
    }

    private void HideOfflineBanner()
    {
        Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  ✓ You're Back Online                                   ║");
        Console.WriteLine("║  All features are now available.                        ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝\n");
    }
}

// ============================================================================
// USAGE EXAMPLES
// ============================================================================

public static class OfflineHandlingExamples
{
    public static async Task Example_OfflineAwareOperations()
    {
        var monitor = new NetworkMonitor();
        var service = new OfflineAwarePackageService(monitor);
        var banner = new OfflineModeBanner(monitor);

        Console.WriteLine("=== Online Operations ===\n");
        
        // Get package details while online
        var result1 = await service.GetPackageDetailsAsync("nodejs");
        DisplayResult(result1, "Package Details");

        // Search while online
        var searchResult1 = await service.SearchPackagesAsync("node");
        DisplaySearchResult(searchResult1);

        Console.WriteLine("\n=== Switching to Offline ===\n");
        monitor.SimulateStatusChange(false);
        await Task.Delay(500);

        // Get cached package details while offline
        var result2 = await service.GetPackageDetailsAsync("nodejs");
        DisplayResult(result2, "Package Details (Offline)");

        // Try to install while offline
        var installResult = await service.InstallPackageAsync("python");
        DisplayInstallResult(installResult);

        // Search while offline
        var searchResult2 = await service.SearchPackagesAsync("node");
        DisplaySearchResult(searchResult2);

        Console.WriteLine("\n=== Back Online ===\n");
        monitor.SimulateStatusChange(true);
        await Task.Delay(500);

        // Install while online
        var installResult2 = await service.InstallPackageAsync("python");
        DisplayInstallResult(installResult2);
    }

    private static void DisplayResult(OfflineResult<PackageDetails> result, string operation)
    {
        Console.WriteLine($"--- {operation} ---");
        if (result.IsSuccess)
        {
            Console.WriteLine($"✓ Success ({result.Source})");
            Console.WriteLine($"  Package: {result.Data.Name}");
            Console.WriteLine($"  Version: {result.Data.Version}");
            if (!string.IsNullOrEmpty(result.Message))
            {
                Console.WriteLine($"  ⚠ {result.Message}");
            }
        }
        else
        {
            Console.WriteLine($"✗ Failed ({result.Source})");
            Console.WriteLine($"  {result.Message}");
        }
        Console.WriteLine();
    }

    private static void DisplaySearchResult(OfflineResult<List<string>> result)
    {
        Console.WriteLine("--- Search Results ---");
        if (result.IsSuccess)
        {
            Console.WriteLine($"✓ Found {result.Data.Count} results ({result.Source})");
            foreach (var item in result.Data)
            {
                Console.WriteLine($"  - {item}");
            }
            if (!string.IsNullOrEmpty(result.Message))
            {
                Console.WriteLine($"  ⚠ {result.Message}");
            }
        }
        else
        {
            Console.WriteLine($"✗ Search failed ({result.Source})");
            Console.WriteLine($"  {result.Message}");
        }
        Console.WriteLine();
    }

    private static void DisplayInstallResult(OfflineResult<bool> result)
    {
        Console.WriteLine("--- Installation ---");
        if (result.IsSuccess && result.Data)
        {
            Console.WriteLine($"✓ Installation succeeded ({result.Source})");
        }
        else
        {
            Console.WriteLine($"✗ Installation failed ({result.Source})");
            Console.WriteLine($"  {result.Message}");
        }
        Console.WriteLine();
    }
}

// ============================================================================
// SUPPORTING CLASSES
// ============================================================================

public class PackageDetails
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}

// ============================================================================
// DEMO
// ============================================================================

public class OfflineHandlingDemo
{
    public static async Task Main()
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  Offline Scenario Handling & User-Friendly Error Messages ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

        ErrorMessageExamples.Example_BasicErrorMessages();
        ErrorMessageExamples.Example_ContextSpecificMessages();
        ErrorMessageExamples.Example_GoodVsBadMessages();

        Console.WriteLine("\n\n" + new string('═', 60) + "\n");

        await OfflineHandlingExamples.Example_OfflineAwareOperations();
    }
}
