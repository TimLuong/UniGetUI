# Serilog Structured Logging Example

This example demonstrates comprehensive structured logging using Serilog.

## Features Demonstrated

- **Structured Properties**: Logging with semantic properties for easy querying
- **Multiple Log Levels**: Proper use of Verbose, Debug, Information, Warning, and Error levels
- **Log Enrichment**: Automatic addition of thread ID, machine name, and timestamp
- **Log Context**: Using `LogContext` to automatically include properties in all logs within a scope
- **File and Console Sinks**: Writing logs to both console and rolling files
- **Exception Logging**: Proper exception logging with context and inner exceptions
- **Configuration**: Reading Serilog configuration from appsettings.json

## Running the Example

```bash
dotnet restore
dotnet run
```

## Output

The example will:
1. Create a `logs/` directory with rolling daily log files
2. Display colored console output
3. Demonstrate various logging patterns

### Console Output
Logs are displayed in the console with:
- Timestamp
- Log level (colored)
- Message with structured properties
- Exception details when applicable

### File Output
Logs are written to `logs/app-YYYY-MM-DD.txt` with:
- Full timestamp with timezone
- Log level
- Message with structured properties as JSON
- Full exception details including stack traces

## Key Code Patterns

### Basic Structured Logging
```csharp
Log.Information("Installing package {PackageId} version {Version}", packageId, version);
```

### Object Destructuring
```csharp
// Use @ to destructure object properties
Log.Information("Package details: {@PackageInfo}", packageInfo);
```

### Log Context
```csharp
using (LogContext.PushProperty("UserId", userId))
{
    Log.Information("Operation started"); // Automatically includes UserId
}
```

### Exception Logging
```csharp
try
{
    await SomeOperation();
}
catch (Exception ex)
{
    Log.Error(ex, "Operation failed for {PackageId}", packageId);
}
```

## Configuration

The `appsettings.json` file configures:
- Minimum log levels
- Console output with ANSI colors
- File output with daily rolling
- 30-day log retention
- Log enrichment

## Best Practices Demonstrated

1. ✅ Use structured properties instead of string interpolation
2. ✅ Choose appropriate log levels
3. ✅ Include contextual information
4. ✅ Use log context for automatic property inclusion
5. ✅ Log exceptions with context
6. ✅ Configure log retention policies
7. ✅ Use multiple sinks for different purposes

## Integration with Applications

To integrate similar logging into your application:

1. Install required NuGet packages
2. Copy the configuration from `appsettings.json`
3. Initialize Serilog in your `Program.cs` or startup class
4. Replace existing logging calls with structured Serilog calls
5. Use `LogContext` for automatic property inclusion in scoped operations

## See Also

- [Logging Standards Documentation](../../../../docs/observability/logging-standards.md)
- [Serilog Official Documentation](https://serilog.net/)
