// Example: Global Exception Handling
// This file demonstrates global exception handling patterns

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace UniGetUI.Examples.ErrorHandling;

// ============================================================================
// GLOBAL EXCEPTION HANDLER
// ============================================================================

/// <summary>
/// Provides application-wide exception handling.
/// Initialize this early in application startup.
/// </summary>
public static class GlobalExceptionHandler
{
    private static bool _isInitialized = false;

    /// <summary>
    /// Initializes global exception handling for the application.
    /// Should be called early in application startup (e.g., App.xaml.cs constructor).
    /// </summary>
    public static void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        // Handle unhandled exceptions in the UI thread (WPF/WinUI specific)
        Application.Current.DispatcherUnhandledException += OnDispatcherUnhandledException;

        // Handle unhandled exceptions in background threads
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        // Handle async void exceptions and unobserved task exceptions
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        _isInitialized = true;
        Console.WriteLine("Global exception handler initialized");
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Console.WriteLine($"[UI Thread] Unhandled exception: {e.Exception.Message}");
        
        var handled = HandleException(e.Exception);
        e.Handled = handled; // Prevent application crash if handled
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            Console.WriteLine($"[App Domain] Unhandled exception: {ex.Message}");
            HandleException(ex);
        }

        if (e.IsTerminating)
        {
            Console.WriteLine("Application is terminating due to unhandled exception");
            PerformEmergencyCleanup();
        }
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Console.WriteLine($"[Task] Unobserved exception: {e.Exception.Message}");
        
        if (HandleException(e.Exception))
        {
            e.SetObserved(); // Mark as observed to prevent application crash
        }
    }

    /// <summary>
    /// Handles an exception and returns true if it was handled gracefully.
    /// </summary>
    private static bool HandleException(Exception ex)
    {
        try
        {
            // Log the exception
            LogException(ex);

            // Report to telemetry if appropriate
            if (ShouldReportException(ex))
            {
                ReportToTelemetry(ex);
            }

            // Show user-friendly error message
            var userMessage = GetUserFriendlyMessage(ex);
            ShowErrorDialog(userMessage, ex);

            return true; // Exception was handled
        }
        catch (Exception handlerEx)
        {
            // If error handling itself fails, log it and don't crash
            Console.WriteLine($"Exception in exception handler: {handlerEx.Message}");
            return false;
        }
    }

    private static void LogException(Exception ex)
    {
        // In production, use proper logging (e.g., Logger.Error)
        Console.WriteLine($"[ERROR] {ex.GetType().Name}: {ex.Message}");
        Console.WriteLine($"[STACK] {ex.StackTrace}");
    }

    private static bool ShouldReportException(Exception ex)
    {
        // Don't report user errors or validation failures
        if (ex is UniGetUIException ugiEx)
        {
            return ugiEx.ShouldReport;
        }
        
        return true; // Report unknown exceptions by default
    }

    private static void ReportToTelemetry(Exception ex)
    {
        // In production, send to telemetry service
        Console.WriteLine($"[TELEMETRY] Reporting exception: {ex.GetType().Name}");
    }

    private static string GetUserFriendlyMessage(Exception ex)
    {
        // Use custom user messages for known exception types
        if (ex is UniGetUIException ugiEx)
        {
            return ugiEx.UserMessage;
        }

        // Provide friendly messages for common .NET exceptions
        return ex switch
        {
            UnauthorizedAccessException => "You don't have permission to perform this operation. Please run the application as administrator.",
            OutOfMemoryException => "The application has run out of memory. Please close some applications and try again.",
            System.IO.IOException => "A file operation failed. Please check that the file is not in use and try again.",
            System.Net.Http.HttpRequestException => "A network request failed. Please check your internet connection.",
            TimeoutException => "The operation took too long to complete. Please try again.",
            _ => "An unexpected error occurred. The error has been logged and will be investigated."
        };
    }

    private static void ShowErrorDialog(string message, Exception ex)
    {
        // In production, show a proper dialog
        Console.WriteLine("=== ERROR DIALOG ===");
        Console.WriteLine($"Message: {message}");
        Console.WriteLine($"Details: {ex.GetType().Name}: {ex.Message}");
        Console.WriteLine("===================");

        // Example for WPF:
        // Application.Current.Dispatcher.Invoke(() =>
        // {
        //     var dialog = new ErrorDialog
        //     {
        //         Title = "Error",
        //         Message = message,
        //         Details = ex.ToString()
        //     };
        //     dialog.ShowDialog();
        // });
    }

    private static void PerformEmergencyCleanup()
    {
        try
        {
            Console.WriteLine("Performing emergency cleanup...");
            
            // Save user settings
            // Settings.SaveAll();
            
            // Cancel pending operations
            // CancellationService.CancelAllOperations();
            
            // Close database connections
            // Database.CloseAllConnections();
            
            Console.WriteLine("Emergency cleanup completed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during emergency cleanup: {ex.Message}");
        }
    }
}

// ============================================================================
// USAGE EXAMPLES
// ============================================================================

public class GlobalExceptionHandlerExamples
{
    /// <summary>
    /// Example: Initialize in App.xaml.cs
    /// </summary>
    public class App : Application
    {
        public App()
        {
            // Initialize global exception handler early
            GlobalExceptionHandler.Initialize();
        }
    }

    /// <summary>
    /// Example: UI thread exception handling
    /// </summary>
    public static void Example_UIThreadException()
    {
        // Initialize handler
        GlobalExceptionHandler.Initialize();

        // Simulate UI thread exception
        try
        {
            throw new InvalidOperationException("Something went wrong in UI");
        }
        catch (Exception ex)
        {
            // This would normally be caught by the global handler
            Console.WriteLine($"Exception: {ex.Message}");
        }
    }

    /// <summary>
    /// Example: Background thread exception
    /// </summary>
    public static void Example_BackgroundThreadException()
    {
        GlobalExceptionHandler.Initialize();

        // Start background work that throws
        Task.Run(() =>
        {
            throw new InvalidOperationException("Background operation failed");
        });

        // Give time for exception to occur
        Task.Delay(100).Wait();
    }

    /// <summary>
    /// Example: Async void exception (dangerous!)
    /// </summary>
    public static async void Example_AsyncVoidException()
    {
        GlobalExceptionHandler.Initialize();

        // This is dangerous - exceptions in async void are hard to catch
        await Task.Delay(10);
        throw new InvalidOperationException("Async void exception");
    }

    /// <summary>
    /// Example: Unobserved task exception
    /// </summary>
    public static void Example_UnobservedTaskException()
    {
        GlobalExceptionHandler.Initialize();

        // Create a task that fails but is never awaited
        var task = Task.Run(() =>
        {
            throw new InvalidOperationException("Unobserved task exception");
        });

        // Don't await the task - let it fail
        // The exception will be caught when the task is garbage collected

        // Force garbage collection to trigger UnobservedTaskException
        task = null;
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    /// <summary>
    /// Example: Handling custom exceptions
    /// </summary>
    public static void Example_CustomExceptionHandling()
    {
        GlobalExceptionHandler.Initialize();

        try
        {
            // Throw a custom exception
            throw new PackageNotFoundException("WinGet", "nonexistent-package");
        }
        catch (Exception ex)
        {
            // Global handler will extract user-friendly message
            Console.WriteLine("This would show a user-friendly dialog");
        }
    }

    /// <summary>
    /// Example: Exception that should not be reported
    /// </summary>
    public static void Example_NonReportableException()
    {
        GlobalExceptionHandler.Initialize();

        try
        {
            // Validation errors are user mistakes, not bugs
            throw new ValidationException(
                "Package name cannot be empty",
                fieldName: "PackageName",
                invalidValue: "");
        }
        catch (Exception ex)
        {
            // Global handler checks ShouldReport flag
            Console.WriteLine("This error won't be reported to telemetry");
        }
    }

    /// <summary>
    /// Example: Exception during exception handling
    /// </summary>
    public static void Example_NestedExceptionHandling()
    {
        GlobalExceptionHandler.Initialize();

        try
        {
            // Simulate an exception
            throw new InvalidOperationException("Primary exception");
        }
        catch (Exception ex)
        {
            // Even if showing the error dialog fails,
            // the global handler prevents complete crash
            Console.WriteLine("Error handled gracefully");
        }
    }
}

// ============================================================================
// CUSTOM ERROR DIALOG (Example)
// ============================================================================

/// <summary>
/// Example error dialog for displaying exceptions to users.
/// In production, this would be a proper WPF/WinUI dialog.
/// </summary>
public class ErrorDialog
{
    public string Title { get; set; } = "Error";
    public string Message { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public bool ShowDetails { get; set; } = false;

    public void ShowDialog()
    {
        Console.WriteLine($"=== {Title} ===");
        Console.WriteLine(Message);
        
        if (ShowDetails && !string.IsNullOrEmpty(Details))
        {
            Console.WriteLine("\nDetails:");
            Console.WriteLine(Details);
        }
        
        Console.WriteLine("=================");
    }
}

// ============================================================================
// PRODUCTION EXAMPLE
// ============================================================================

/// <summary>
/// Example of complete global exception handling setup for production.
/// </summary>
public static class ProductionExceptionHandling
{
    public static void InitializeForProduction()
    {
        // 1. Initialize global handlers
        GlobalExceptionHandler.Initialize();

        // 2. Configure logging
        Console.WriteLine("Logging system initialized");

        // 3. Configure telemetry
        Console.WriteLine("Telemetry system initialized");

        // 4. Set up crash reporting
        Console.WriteLine("Crash reporting initialized");

        // 5. Register custom exception filters
        Console.WriteLine("Custom exception filters registered");
    }

    /// <summary>
    /// Example: Production-ready exception handling in a service method
    /// </summary>
    public static async Task<bool> InstallPackageAsync(string packageId)
    {
        try
        {
            // Attempt installation
            Console.WriteLine($"Installing {packageId}...");
            
            // Simulate potential failure
            if (new Random().Next(0, 2) == 0)
            {
                throw new PackageOperationException(
                    "WinGet",
                    packageId,
                    PackageOperationException.PackageOperation.Install,
                    "Simulated installation failure");
            }

            Console.WriteLine($"Successfully installed {packageId}");
            return true;
        }
        catch (PackageManagerException ex)
        {
            // Log with context
            Console.WriteLine($"Package manager error: {ex.GetFullErrorContext()}");
            
            // Show user-friendly message
            Console.WriteLine($"User notification: {ex.UserMessage}");
            
            // Report if needed
            if (ex.ShouldReport)
            {
                Console.WriteLine("Reporting to telemetry");
            }
            
            return false;
        }
        catch (Exception ex)
        {
            // Unexpected error
            Console.WriteLine($"Unexpected error: {ex.Message}");
            Console.WriteLine("Reporting to telemetry");
            
            // Show generic error message
            Console.WriteLine("User notification: An unexpected error occurred");
            
            return false;
        }
    }
}
