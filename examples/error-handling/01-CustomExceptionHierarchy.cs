// Example: Custom Exception Hierarchy for UniGetUI
// This file demonstrates the exception hierarchy defined in the exception-strategy.md

using System;

namespace UniGetUI.Examples.ErrorHandling;

// ============================================================================
// BASE EXCEPTIONS
// ============================================================================

/// <summary>
/// Base exception for all UniGetUI-specific exceptions.
/// </summary>
public class UniGetUIException : Exception
{
    public string UserMessage { get; protected set; }
    public bool ShouldReport { get; protected set; } = true;
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

    public virtual string GetFullErrorContext()
    {
        return $"[{ErrorCode}] {Message}";
    }
}

// ============================================================================
// PACKAGE MANAGER EXCEPTIONS
// ============================================================================

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
        ShouldReport = false;
    }
}

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

// ============================================================================
// NETWORK EXCEPTIONS
// ============================================================================

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

// ============================================================================
// CONFIGURATION AND VALIDATION EXCEPTIONS
// ============================================================================

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

public class ValidationException : UniGetUIException
{
    public string? FieldName { get; }
    public object? InvalidValue { get; }

    public ValidationException(string message, string? fieldName = null, object? invalidValue = null)
        : base(
            message,
            message,
            "VAL-1001")
    {
        FieldName = fieldName;
        InvalidValue = invalidValue;
        ShouldReport = false;
    }
}

// ============================================================================
// USAGE EXAMPLES
// ============================================================================

public class ExceptionUsageExamples
{
    public void Example_PackageNotFoundException()
    {
        try
        {
            // Simulating package not found
            throw new PackageNotFoundException("WinGet", "nonexistent-package");
        }
        catch (PackageNotFoundException ex)
        {
            Console.WriteLine($"Error Code: {ex.ErrorCode}");
            Console.WriteLine($"Technical Message: {ex.Message}");
            Console.WriteLine($"User Message: {ex.UserMessage}");
            Console.WriteLine($"Should Report: {ex.ShouldReport}");
            // Output:
            // Error Code: PM-1001
            // Technical Message: Package 'nonexistent-package' not found in WinGet
            // User Message: The package 'nonexistent-package' could not be found. Please check the package name and try again.
            // Should Report: False
        }
    }

    public void Example_PackageOperationException()
    {
        try
        {
            // Simulating installation failure
            throw new PackageOperationException(
                "Chocolatey",
                "nodejs",
                PackageOperationException.PackageOperation.Install,
                "Network timeout",
                new TimeoutException("Connection timed out"));
        }
        catch (PackageOperationException ex)
        {
            Console.WriteLine($"Error Code: {ex.ErrorCode}");
            Console.WriteLine($"Manager: {ex.ManagerName}");
            Console.WriteLine($"Package: {ex.PackageId}");
            Console.WriteLine($"Operation: {ex.Operation}");
            Console.WriteLine($"User Message: {ex.UserMessage}");
            Console.WriteLine($"Full Context: {ex.GetFullErrorContext()}");
            // Output:
            // Error Code: PM-2000
            // Manager: Chocolatey
            // Package: nodejs
            // Operation: Install
            // User Message: Failed to install package 'nodejs'. Please check your internet connection and try again.
            // Full Context: [PM-2000] Manager: Chocolatey, Package: nodejs - Failed to Install package 'nodejs': Network timeout
        }
    }

    public void Example_NetworkException()
    {
        try
        {
            // Simulating offline condition
            throw new NetworkException(
                "Network unavailable",
                endpoint: "https://api.example.com",
                isOffline: true);
        }
        catch (NetworkException ex)
        {
            Console.WriteLine($"Error Code: {ex.ErrorCode}");
            Console.WriteLine($"Is Offline: {ex.IsOffline}");
            Console.WriteLine($"Endpoint: {ex.Endpoint}");
            Console.WriteLine($"User Message: {ex.UserMessage}");
            // Output:
            // Error Code: NET-1001
            // Is Offline: True
            // Endpoint: https://api.example.com
            // User Message: You appear to be offline. Please check your internet connection and try again.
        }
    }

    public void Example_ValidationException()
    {
        try
        {
            string userInput = "";
            if (string.IsNullOrWhiteSpace(userInput))
            {
                throw new ValidationException(
                    "Package name cannot be empty",
                    fieldName: "PackageName",
                    invalidValue: userInput);
            }
        }
        catch (ValidationException ex)
        {
            Console.WriteLine($"Error Code: {ex.ErrorCode}");
            Console.WriteLine($"Field Name: {ex.FieldName}");
            Console.WriteLine($"Invalid Value: '{ex.InvalidValue}'");
            Console.WriteLine($"User Message: {ex.UserMessage}");
            Console.WriteLine($"Should Report: {ex.ShouldReport}");
            // Output:
            // Error Code: VAL-1001
            // Field Name: PackageName
            // Invalid Value: ''
            // User Message: Package name cannot be empty
            // Should Report: False
        }
    }

    public void Example_ConfigurationException()
    {
        try
        {
            // Simulating configuration error
            throw new ConfigurationException(
                "Invalid package source URL",
                configKey: "PackageSourceUrl",
                new UriFormatException("Invalid URI format"));
        }
        catch (ConfigurationException ex)
        {
            Console.WriteLine($"Error Code: {ex.ErrorCode}");
            Console.WriteLine($"Config Key: {ex.ConfigKey}");
            Console.WriteLine($"User Message: {ex.UserMessage}");
            Console.WriteLine($"Inner Exception: {ex.InnerException?.GetType().Name}");
            // Output:
            // Error Code: CFG-1001
            // Config Key: PackageSourceUrl
            // User Message: A configuration error occurred. Please check your settings.
            // Inner Exception: UriFormatException
        }
    }

    public void Example_HandlingInService()
    {
        try
        {
            // Simulating a package installation
            InstallPackage("example-package", "WinGet");
        }
        catch (UniGetUIException ex) when (ex is PackageNotFoundException)
        {
            // Handle user error - don't report to telemetry
            Console.WriteLine($"User Error: {ex.UserMessage}");
        }
        catch (UniGetUIException ex) when (ex.ShouldReport)
        {
            // Log and report to telemetry
            Console.WriteLine($"Reporting error: {ex.GetFullErrorContext()}");
            // TelemetryService.TrackException(ex);
        }
        catch (UniGetUIException ex)
        {
            // Log but don't report
            Console.WriteLine($"Non-reportable error: {ex.UserMessage}");
        }
    }

    private void InstallPackage(string packageId, string managerName)
    {
        // Simulated installation logic
        bool packageExists = false;
        
        if (!packageExists)
        {
            throw new PackageNotFoundException(managerName, packageId);
        }
    }
}
