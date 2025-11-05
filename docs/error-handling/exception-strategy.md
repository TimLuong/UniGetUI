# Exception Strategy and Error Handling

## Overview

This document defines the comprehensive exception handling strategy for UniGetUI, including custom exception hierarchy, global exception handling, user-friendly error messages, error recovery strategies, and error tracking.

## Exception Handling Hierarchy

### Custom Exception Base Classes

UniGetUI uses a hierarchical exception structure to provide clear context and enable appropriate handling at different levels.

#### Base Application Exception

```csharp
namespace UniGetUI.Core.Exceptions;

/// <summary>
/// Base exception for all UniGetUI-specific exceptions.
/// Provides common functionality for error tracking and user messaging.
/// </summary>
public class UniGetUIException : Exception
{
    /// <summary>
    /// User-friendly error message that can be displayed to end users.
    /// </summary>
    public string UserMessage { get; protected set; }

    /// <summary>
    /// Indicates whether this error should be reported to telemetry.
    /// </summary>
    public bool ShouldReport { get; protected set; } = true;

    /// <summary>
    /// Error code for tracking and documentation purposes.
    /// </summary>
    public string ErrorCode { get; protected set; }

    public UniGetUIException(string message, string userMessage, string errorCode = "UGI-0000")
        : base(message)
    {
        UserMessage = userMessage;
        ErrorCode = errorCode;
    }

    public UniGetUIException(string message, string userMessage, Exception innerException, string errorCode = "UGI-0000")
        : base(message, innerException)
    {
        UserMessage = userMessage;
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Gets the full error context including error code and messages.
    /// </summary>
    public virtual string GetFullErrorContext()
    {
        return $"[{ErrorCode}] {Message}";
    }
}
```

#### Package Manager Exceptions

```csharp
namespace UniGetUI.PackageEngine.Exceptions;

/// <summary>
/// Base exception for package manager operations.
/// </summary>
public class PackageManagerException : UniGetUIException
{
    public string ManagerName { get; }
    public string? PackageId { get; }

    public PackageManagerException(
        string managerName,
        string message,
        string userMessage,
        string? packageId = null,
        string errorCode = "PM-0000")
        : base(message, userMessage, errorCode)
    {
        ManagerName = managerName;
        PackageId = packageId;
    }

    public PackageManagerException(
        string managerName,
        string message,
        string userMessage,
        Exception innerException,
        string? packageId = null,
        string errorCode = "PM-0000")
        : base(message, userMessage, innerException, errorCode)
    {
        ManagerName = managerName;
        PackageId = packageId;
    }

    public override string GetFullErrorContext()
    {
        var context = $"[{ErrorCode}] Manager: {ManagerName}";
        if (!string.IsNullOrEmpty(PackageId))
        {
            context += $", Package: {PackageId}";
        }
        context += $" - {Message}";
        return context;
    }
}

/// <summary>
/// Exception thrown when a package is not found.
/// </summary>
public class PackageNotFoundException : PackageManagerException
{
    public PackageNotFoundException(string managerName, string packageId)
        : base(
            managerName,
            $"Package '{packageId}' not found in {managerName}",
            $"The package '{packageId}' could not be found. Please check the package name and try again.",
            packageId,
            "PM-1001")
    {
        ShouldReport = false; // User error, don't report to telemetry
    }
}

/// <summary>
/// Exception thrown when a package operation fails.
/// </summary>
public class PackageOperationException : PackageManagerException
{
    public PackageOperation Operation { get; }

    public enum PackageOperation
    {
        Install,
        Uninstall,
        Update,
        Search,
        GetDetails
    }

    public PackageOperationException(
        string managerName,
        string packageId,
        PackageOperation operation,
        string message,
        Exception? innerException = null)
        : base(
            managerName,
            $"Failed to {operation} package '{packageId}': {message}",
            $"Failed to {operation.ToString().ToLower()} package '{packageId}'. {GetUserGuidance(operation)}",
            innerException,
            packageId,
            $"PM-2{(int)operation:D3}")
    {
        Operation = operation;
    }

    private static string GetUserGuidance(PackageOperation operation)
    {
        return operation switch
        {
            PackageOperation.Install => "Please check your internet connection and try again.",
            PackageOperation.Uninstall => "Please ensure the package is installed and try again.",
            PackageOperation.Update => "Please check for available updates and try again.",
            PackageOperation.Search => "Please check your search terms and try again.",
            PackageOperation.GetDetails => "Package details are temporarily unavailable.",
            _ => "Please try again later."
        };
    }
}

/// <summary>
/// Exception thrown when a package manager is not available or not initialized.
/// </summary>
public class PackageManagerUnavailableException : PackageManagerException
{
    public PackageManagerUnavailableException(string managerName, string reason)
        : base(
            managerName,
            $"Package manager '{managerName}' is not available: {reason}",
            $"The {managerName} package manager is not available. Please ensure it is installed and configured correctly.",
            null,
            "PM-3001")
    {
    }
}
```

#### Network and Connectivity Exceptions

```csharp
namespace UniGetUI.Core.Exceptions;

/// <summary>
/// Exception thrown for network-related errors.
/// </summary>
public class NetworkException : UniGetUIException
{
    public string? Endpoint { get; }
    public bool IsOffline { get; }

    public NetworkException(string message, string? endpoint = null, bool isOffline = false)
        : base(
            message,
            isOffline
                ? "You appear to be offline. Please check your internet connection and try again."
                : "A network error occurred. Please check your connection and try again.",
            "NET-1001")
    {
        Endpoint = endpoint;
        IsOffline = isOffline;
    }

    public NetworkException(string message, Exception innerException, string? endpoint = null)
        : base(message, "A network error occurred. Please check your connection and try again.", innerException, "NET-1001")
    {
        Endpoint = endpoint;
        IsOffline = false;
    }
}

/// <summary>
/// Exception thrown when a resource is not available due to network issues.
/// </summary>
public class ResourceUnavailableException : NetworkException
{
    public ResourceUnavailableException(string resourceName, string? endpoint = null)
        : base(
            $"Resource '{resourceName}' is currently unavailable",
            endpoint,
            false)
    {
        ErrorCode = "NET-2001";
        UserMessage = $"The resource '{resourceName}' is temporarily unavailable. Please try again later.";
    }
}
```

#### Configuration and Validation Exceptions

```csharp
namespace UniGetUI.Core.Exceptions;

/// <summary>
/// Exception thrown for configuration-related errors.
/// </summary>
public class ConfigurationException : UniGetUIException
{
    public string? ConfigKey { get; }

    public ConfigurationException(string message, string? configKey = null)
        : base(
            message,
            "A configuration error occurred. Please check your settings.",
            "CFG-1001")
    {
        ConfigKey = configKey;
    }

    public ConfigurationException(string message, Exception innerException, string? configKey = null)
        : base(message, "A configuration error occurred. Please check your settings.", innerException, "CFG-1001")
    {
        ConfigKey = configKey;
    }
}

/// <summary>
/// Exception thrown when input validation fails.
/// </summary>
public class ValidationException : UniGetUIException
{
    public string? FieldName { get; }
    public object? InvalidValue { get; }

    public ValidationException(string message, string? fieldName = null, object? invalidValue = null)
        : base(
            message,
            message, // Validation messages are usually user-friendly
            "VAL-1001")
    {
        FieldName = fieldName;
        InvalidValue = invalidValue;
        ShouldReport = false; // User input error, don't report
    }
}
```

## Global Exception Handling

### Application-Level Exception Handler

```csharp
namespace UniGetUI.Core.ErrorHandling;

public static class GlobalExceptionHandler
{
    private static bool _isInitialized = false;

    /// <summary>
    /// Initializes global exception handling for the application.
    /// Should be called early in application startup.
    /// </summary>
    public static void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        // Handle unhandled exceptions in the UI thread
        Application.Current.DispatcherUnhandledException += OnDispatcherUnhandledException;

        // Handle unhandled exceptions in background threads
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        // Handle async void exceptions
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        _isInitialized = true;
        Logger.Info("Global exception handler initialized");
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Logger.Error($"Unhandled UI thread exception: {e.Exception}");
        
        var handled = HandleException(e.Exception);
        e.Handled = handled;
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            Logger.Error($"Unhandled application domain exception: {ex}");
            HandleException(ex);
        }

        if (e.IsTerminating)
        {
            Logger.Error("Application is terminating due to unhandled exception");
            // Attempt to save state or perform cleanup
            PerformEmergencyCleanup();
        }
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Logger.Error($"Unobserved task exception: {e.Exception}");
        
        if (HandleException(e.Exception))
        {
            e.SetObserved(); // Prevent application from crashing
        }
    }

    /// <summary>
    /// Handles an exception and returns true if it was handled gracefully.
    /// </summary>
    private static bool HandleException(Exception ex)
    {
        try
        {
            // Report to telemetry if appropriate
            if (ShouldReportException(ex))
            {
                TelemetryService.TrackException(ex);
            }

            // Show user-friendly error message
            var userMessage = GetUserFriendlyMessage(ex);
            ShowErrorDialog(userMessage, ex);

            return true;
        }
        catch (Exception handlerEx)
        {
            // If error handling fails, log it and don't crash
            Logger.Error($"Exception in exception handler: {handlerEx}");
            return false;
        }
    }

    private static bool ShouldReportException(Exception ex)
    {
        if (ex is UniGetUIException ugiEx)
        {
            return ugiEx.ShouldReport;
        }
        return true; // Report unknown exceptions
    }

    private static string GetUserFriendlyMessage(Exception ex)
    {
        if (ex is UniGetUIException ugiEx)
        {
            return ugiEx.UserMessage;
        }

        // Provide friendly messages for common .NET exceptions
        return ex switch
        {
            UnauthorizedAccessException => "You don't have permission to perform this operation. Please run the application as administrator.",
            OutOfMemoryException => "The application has run out of memory. Please close some applications and try again.",
            IOException => "A file operation failed. Please check that the file is not in use and try again.",
            HttpRequestException => "A network request failed. Please check your internet connection.",
            _ => "An unexpected error occurred. The error has been logged and will be investigated."
        };
    }

    private static void ShowErrorDialog(string message, Exception ex)
    {
        // Display error to user (implementation depends on UI framework)
        // This is a simplified example
        Application.Current.Dispatcher.Invoke(() =>
        {
            var dialog = new ErrorDialog
            {
                Title = "Error",
                Message = message,
                Details = ex.ToString()
            };
            dialog.ShowDialog();
        });
    }

    private static void PerformEmergencyCleanup()
    {
        try
        {
            // Save user settings
            Settings.SaveAll();
            
            // Cancel pending operations
            CancellationService.CancelAllOperations();
            
            Logger.Info("Emergency cleanup completed");
        }
        catch (Exception ex)
        {
            Logger.Error($"Error during emergency cleanup: {ex}");
        }
    }
}
```

## Error Recovery Strategies

### Graceful Degradation

Implement graceful degradation to maintain functionality when errors occur:

```csharp
public class PackageManagerService
{
    private readonly List<IPackageManager> _managers;
    private readonly ICache _cache;

    /// <summary>
    /// Get installed packages with fallback strategies.
    /// </summary>
    public async Task<IReadOnlyList<IPackage>> GetInstalledPackagesAsync()
    {
        try
        {
            // Primary strategy: Get fresh data from managers
            return await GetInstalledPackagesFromManagersAsync();
        }
        catch (Exception ex)
        {
            Logger.Warn($"Failed to get installed packages from managers: {ex.Message}");
            
            // Fallback 1: Try cache
            var cachedPackages = _cache.GetInstalledPackages();
            if (cachedPackages.Any())
            {
                Logger.Info("Returning cached package list");
                return cachedPackages;
            }
            
            // Fallback 2: Return empty list to maintain functionality
            Logger.Warn("No cache available, returning empty list");
            return Array.Empty<IPackage>();
        }
    }

    private async Task<IReadOnlyList<IPackage>> GetInstalledPackagesFromManagersAsync()
    {
        var allPackages = new List<IPackage>();
        var failedManagers = new List<string>();

        foreach (var manager in _managers)
        {
            try
            {
                var packages = await manager.GetInstalledPackagesAsync();
                allPackages.AddRange(packages);
            }
            catch (Exception ex)
            {
                Logger.Error($"Manager {manager.Name} failed to get packages: {ex.Message}");
                failedManagers.Add(manager.Name);
                // Continue with other managers
            }
        }

        if (failedManagers.Any())
        {
            // Show warning but don't fail completely
            ShowWarning($"Some package managers failed: {string.Join(", ", failedManagers)}");
        }

        return allPackages;
    }
}
```

### Safe Execution Wrapper

```csharp
public static class SafeExecutor
{
    /// <summary>
    /// Executes an action safely with error handling and default return value.
    /// </summary>
    public static T Execute<T>(
        Func<T> action,
        T defaultValue,
        string operationName,
        bool logErrors = true)
    {
        try
        {
            return action();
        }
        catch (Exception ex)
        {
            if (logErrors)
            {
                Logger.Error($"Error in {operationName}: {ex.Message}");
                Logger.Debug(ex);
            }
            return defaultValue;
        }
    }

    /// <summary>
    /// Executes an async action safely with error handling and default return value.
    /// </summary>
    public static async Task<T> ExecuteAsync<T>(
        Func<Task<T>> action,
        T defaultValue,
        string operationName,
        bool logErrors = true)
    {
        try
        {
            return await action().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            if (logErrors)
            {
                Logger.Error($"Error in {operationName}: {ex.Message}");
                Logger.Debug(ex);
            }
            return defaultValue;
        }
    }

    /// <summary>
    /// Executes a void action safely with error handling.
    /// </summary>
    public static bool TryExecute(
        Action action,
        string operationName,
        bool logErrors = true)
    {
        try
        {
            action();
            return true;
        }
        catch (Exception ex)
        {
            if (logErrors)
            {
                Logger.Error($"Error in {operationName}: {ex.Message}");
                Logger.Debug(ex);
            }
            return false;
        }
    }
}
```

## User-Friendly Error Messages

### Guidelines for Error Messages

1. **Be Clear and Specific**: Explain what went wrong in simple terms
2. **Provide Context**: Include relevant information (package name, operation, etc.)
3. **Offer Actionable Guidance**: Tell users what they can do to resolve the issue
4. **Avoid Technical Jargon**: Use language that non-technical users can understand
5. **Be Empathetic**: Acknowledge the inconvenience

### Examples

#### Bad Error Messages
```
❌ "Error: NullReferenceException in PackageManager.cs line 42"
❌ "Operation failed with exit code -1"
❌ "An error occurred"
```

#### Good Error Messages
```
✅ "Could not install package 'example-app'. Please check your internet connection and try again."
✅ "The package 'example-app' is already installed. Would you like to update it instead?"
✅ "Unable to connect to the package server. You can continue working with already installed packages."
```

### Error Message Builder

```csharp
public class ErrorMessageBuilder
{
    private string _operation;
    private string _target;
    private string _reason;
    private string _suggestion;

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

    public string Build()
    {
        var message = new StringBuilder();

        if (!string.IsNullOrEmpty(_operation) && !string.IsNullOrEmpty(_target))
        {
            message.Append($"Could not {_operation} {_target}");
        }
        else
        {
            message.Append("An error occurred");
        }

        if (!string.IsNullOrEmpty(_reason))
        {
            message.Append($": {_reason}");
        }

        message.Append(".");

        if (!string.IsNullOrEmpty(_suggestion))
        {
            message.Append($" {_suggestion}");
        }

        return message.ToString();
    }
}

// Usage example
var message = new ErrorMessageBuilder()
    .ForOperation("install")
    .OnTarget("package 'nodejs'")
    .Because("the server is temporarily unavailable")
    .WithSuggestion("Please try again in a few minutes")
    .Build();
// Result: "Could not install package 'nodejs': the server is temporarily unavailable. Please try again in a few minutes."
```

## Error Tracking and Reporting

### Structured Error Logging

```csharp
public class ErrorTracker
{
    private readonly ILogger _logger;
    private readonly ITelemetryService _telemetry;

    public void TrackError(Exception ex, Dictionary<string, string>? properties = null)
    {
        var errorContext = new ErrorContext
        {
            Exception = ex,
            Timestamp = DateTime.UtcNow,
            Properties = properties ?? new Dictionary<string, string>()
        };

        // Add system context
        errorContext.Properties["OSVersion"] = Environment.OSVersion.ToString();
        errorContext.Properties["AppVersion"] = GetApplicationVersion();

        // Add exception-specific context
        if (ex is UniGetUIException ugiEx)
        {
            errorContext.Properties["ErrorCode"] = ugiEx.ErrorCode;
            errorContext.Properties["UserMessage"] = ugiEx.UserMessage;
            errorContext.Properties["ShouldReport"] = ugiEx.ShouldReport.ToString();
        }

        if (ex is PackageManagerException pmEx)
        {
            errorContext.Properties["ManagerName"] = pmEx.ManagerName;
            if (pmEx.PackageId != null)
            {
                errorContext.Properties["PackageId"] = pmEx.PackageId;
            }
        }

        // Log to file
        _logger.Error($"Error tracked: {errorContext.Exception.GetFullErrorContext()}", errorContext.Properties);

        // Send to telemetry if appropriate
        if (ex is not UniGetUIException ugiException || ugiException.ShouldReport)
        {
            _telemetry.TrackException(ex, errorContext.Properties);
        }
    }

    public void TrackWarning(string message, Dictionary<string, string>? properties = null)
    {
        _logger.Warn(message);
        _telemetry.TrackTrace(message, SeverityLevel.Warning, properties);
    }

    private string GetApplicationVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return version?.ToString() ?? "Unknown";
    }
}

public class ErrorContext
{
    public Exception Exception { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string> Properties { get; set; } = new();
}
```

## Offline Scenario Handling

### Network Availability Detection

```csharp
public class NetworkMonitor
{
    private bool _isOnline = true;
    public event EventHandler<bool>? ConnectivityChanged;

    public bool IsOnline => _isOnline;

    public NetworkMonitor()
    {
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
            Logger.Info($"Network status changed: {(_isOnline ? "Online" : "Offline")}");
            ConnectivityChanged?.Invoke(this, _isOnline);
        }
    }
}
```

### Offline-Aware Operations

```csharp
public class OfflineAwarePackageService
{
    private readonly NetworkMonitor _networkMonitor;
    private readonly ICache _cache;

    public async Task<PackageDetails?> GetPackageDetailsAsync(string packageId)
    {
        if (!_networkMonitor.IsOnline)
        {
            // Try cache first when offline
            var cached = _cache.GetPackageDetails(packageId);
            if (cached != null)
            {
                Logger.Info($"Returning cached details for {packageId} (offline mode)");
                return cached;
            }

            throw new NetworkException(
                $"Cannot get details for {packageId} while offline",
                isOffline: true);
        }

        try
        {
            // Online: fetch from network
            var details = await FetchPackageDetailsAsync(packageId);
            
            // Update cache for offline use
            _cache.SetPackageDetails(packageId, details);
            
            return details;
        }
        catch (HttpRequestException ex)
        {
            Logger.Warn($"Network request failed, trying cache: {ex.Message}");
            
            // Network error: fallback to cache
            return _cache.GetPackageDetails(packageId);
        }
    }

    public async Task<bool> InstallPackageAsync(string packageId)
    {
        if (!_networkMonitor.IsOnline)
        {
            throw new NetworkException(
                $"Cannot install {packageId} while offline. Package installation requires an internet connection.",
                isOffline: true);
        }

        // Proceed with installation
        return await PerformInstallAsync(packageId);
    }
}
```

### Offline Mode UI Feedback

```csharp
public class OfflineModeBanner
{
    private readonly NetworkMonitor _networkMonitor;
    private readonly InfoBar _banner;

    public OfflineModeBanner(NetworkMonitor networkMonitor, InfoBar banner)
    {
        _networkMonitor = networkMonitor;
        _banner = banner;

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
            _banner.Title = "You're offline";
            _banner.Message = "Some features are unavailable. You can still view and manage installed packages.";
            _banner.Severity = InfoBarSeverity.Warning;
            _banner.IsOpen = true;
        }
        else
        {
            _banner.IsOpen = false;
        }
    }
}
```

## Best Practices

### Exception Handling Guidelines

1. **Catch Specific Exceptions First**: Order catch blocks from most specific to most general
2. **Don't Swallow Exceptions**: Always log or handle exceptions appropriately
3. **Use Finally for Cleanup**: Ensure resources are released even when exceptions occur
4. **Avoid Throwing Exceptions for Control Flow**: Use exceptions for exceptional conditions only
5. **Include Context in Exceptions**: Add relevant information to exception messages
6. **Don't Catch What You Can't Handle**: Let exceptions bubble up if you can't handle them meaningfully

### Async Exception Handling

```csharp
public async Task<bool> SafeAsyncOperation()
{
    try
    {
        await PerformOperationAsync().ConfigureAwait(false);
        return true;
    }
    catch (OperationCanceledException)
    {
        // User cancelled, not an error
        Logger.Info("Operation cancelled by user");
        return false;
    }
    catch (TimeoutException ex)
    {
        Logger.Warn($"Operation timed out: {ex.Message}");
        return false;
    }
    catch (Exception ex)
    {
        Logger.Error($"Operation failed: {ex}");
        throw; // Re-throw if we can't handle it
    }
}
```

### Resource Cleanup

```csharp
public async Task ProcessFileAsync(string filePath)
{
    FileStream? stream = null;
    try
    {
        stream = File.OpenRead(filePath);
        await ProcessStreamAsync(stream);
    }
    catch (IOException ex)
    {
        Logger.Error($"File processing failed: {ex.Message}");
        throw new PackageOperationException(
            "FileProcessor",
            filePath,
            PackageOperation.GetDetails,
            "Failed to read package file",
            ex);
    }
    finally
    {
        stream?.Dispose();
    }
}

// Better: Use using statement
public async Task ProcessFileAsync(string filePath)
{
    try
    {
        using var stream = File.OpenRead(filePath);
        await ProcessStreamAsync(stream);
    }
    catch (IOException ex)
    {
        Logger.Error($"File processing failed: {ex.Message}");
        throw new PackageOperationException(
            "FileProcessor",
            filePath,
            PackageOperation.GetDetails,
            "Failed to read package file",
            ex);
    }
}
```

## Related Documentation

- [Resilience Patterns](./resilience-patterns.md) - Retry logic, circuit breakers, and timeout strategies
- [Coding Standards](../codebase-analysis/07-best-practices/patterns-standards.md) - General coding standards
- [Logging, Monitoring & Diagnostics](#) - Related to Issue #46

## References

- [.NET Exception Handling Best Practices](https://learn.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions)
- [Microsoft Exception Design Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/exceptions)
- [Task Exception Handling](https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/exception-handling-task-parallel-library)
