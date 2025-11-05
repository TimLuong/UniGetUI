using System.Diagnostics;

namespace WpfReferenceApp.Utilities;

/// <summary>
/// Simple logging utility
/// Demonstrates error handling and logging best practices
/// </summary>
public static class Logger
{
    private static readonly object _lock = new();
    private static readonly string _logPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WpfReferenceApp",
        "Logs");

    static Logger()
    {
        try
        {
            // Ensure log directory exists
            if (!Directory.Exists(_logPath))
            {
                Directory.CreateDirectory(_logPath);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to create log directory: {ex.Message}");
        }
    }

    /// <summary>
    /// Logs an informational message
    /// </summary>
    public static void Info(string message)
    {
        Log("INFO", message);
    }

    /// <summary>
    /// Logs a warning message
    /// </summary>
    public static void Warning(string message)
    {
        Log("WARN", message);
    }

    /// <summary>
    /// Logs an error message
    /// </summary>
    public static void Error(string message)
    {
        Log("ERROR", message);
    }

    /// <summary>
    /// Logs an exception
    /// </summary>
    public static void Error(Exception exception)
    {
        Log("ERROR", $"{exception.GetType().Name}: {exception.Message}");
        Log("ERROR", $"StackTrace: {exception.StackTrace}");
    }

    private static void Log(string level, string message)
    {
        lock (_lock)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string logMessage = $"[{timestamp}] [{level}] {message}";

                // Write to console/debug
                Debug.WriteLine(logMessage);
                Console.WriteLine(logMessage);

                // Write to file
                string logFile = Path.Combine(_logPath, $"log-{DateTime.Now:yyyy-MM-dd}.txt");
                File.AppendAllText(logFile, logMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // If logging fails, at least write to debug output
                Debug.WriteLine($"Failed to log message: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Gets the path to the log directory
    /// </summary>
    public static string LogPath => _logPath;
}
