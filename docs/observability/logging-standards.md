# Logging Standards for Windows Applications

## Overview

This document defines comprehensive logging standards for Windows applications, focusing on structured logging, best practices, and implementation guidelines using popular logging frameworks like Serilog and NLog.

## Table of Contents

1. [Logging Frameworks](#logging-frameworks)
2. [Log Levels](#log-levels)
3. [Structured Logging](#structured-logging)
4. [Correlation IDs](#correlation-ids)
5. [Exception Handling and Logging](#exception-handling-and-logging)
6. [Best Practices](#best-practices)
7. [Performance Considerations](#performance-considerations)
8. [Security and Compliance](#security-and-compliance)

## Logging Frameworks

### Recommended Frameworks

#### Serilog
**Recommended for:** Modern .NET applications, structured logging, rich ecosystem

**Advantages:**
- First-class structured logging support
- Extensive sink ecosystem (console, file, database, cloud services)
- Semantic logging with property capture
- Excellent performance with async logging
- Clean, fluent API

**Basic Setup:**
```csharp
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

#### NLog
**Recommended for:** Enterprise applications, complex routing, legacy system integration

**Advantages:**
- Highly configurable routing rules
- Strong XML/JSON configuration support
- Mature and battle-tested
- Good performance with buffering
- Wide platform support

**Basic Setup:**
```csharp
using NLog;

var logger = LogManager.GetCurrentClassLogger();
logger.Info("Application started");
```

#### Microsoft.Extensions.Logging
**Recommended for:** ASP.NET Core applications, unified logging abstraction

**Advantages:**
- Built into .NET Core/5+
- Provider-agnostic abstraction
- Dependency injection integration
- Easy to switch providers

**Basic Setup:**
```csharp
using Microsoft.Extensions.Logging;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole()
           .AddDebug();
});

var logger = loggerFactory.CreateLogger<Program>();
```

## Log Levels

### Standard Log Levels

Proper use of log levels is critical for effective troubleshooting and performance.

#### 1. **Trace/Verbose**
- **When to use:** Extremely detailed diagnostic information
- **Examples:**
  - Method entry/exit points
  - Loop iterations with data
  - Variable state changes
  - Performance timing details
- **Production:** Typically disabled in production
```csharp
logger.Verbose("Entering ProcessPackage with id: {PackageId}", packageId);
logger.Verbose("Loop iteration {Index} processing item {ItemName}", i, item.Name);
```

#### 2. **Debug**
- **When to use:** Detailed diagnostic information useful for debugging
- **Examples:**
  - Configuration values
  - Service responses (non-sensitive)
  - Intermediate calculation results
  - Cache hits/misses
- **Production:** Enabled for troubleshooting, then disabled
```csharp
logger.Debug("Package cache hit for {PackageId}, cached version: {Version}", 
    packageId, cachedVersion);
logger.Debug("API response status: {StatusCode}, duration: {Duration}ms", 
    statusCode, duration);
```

#### 3. **Information**
- **When to use:** General informational messages about application flow
- **Examples:**
  - Application startup/shutdown
  - Service connection established
  - Request processing started/completed
  - Configuration changes
- **Production:** Typically enabled
```csharp
logger.Information("Package {PackageId} installed successfully in {Duration}ms", 
    packageId, duration);
logger.Information("Application started, version {Version}", version);
```

#### 4. **Warning**
- **When to use:** Unexpected but recoverable situations
- **Examples:**
  - Fallback to default values
  - Deprecated API usage
  - Slow operations (but not errors)
  - Resource usage approaching limits
- **Production:** Always enabled
```csharp
logger.Warning("Package installation took longer than expected: {Duration}ms (threshold: {Threshold}ms)", 
    duration, threshold);
logger.Warning("Failed to load custom configuration, using defaults: {Reason}", reason);
```

#### 5. **Error**
- **When to use:** Errors and exceptions that are caught and handled
- **Examples:**
  - Failed operations
  - Handled exceptions
  - Integration failures with retry
  - Invalid user input
- **Production:** Always enabled
```csharp
logger.Error(ex, "Failed to install package {PackageId} after {Attempts} attempts", 
    packageId, attempts);
logger.Error("Database connection failed: {ErrorMessage}", ex.Message);
```

#### 6. **Fatal/Critical**
- **When to use:** Unrecoverable errors that cause application shutdown
- **Examples:**
  - Unhandled exceptions
  - Configuration corruption
  - Critical resource unavailability
  - Security breaches
- **Production:** Always enabled, should trigger alerts
```csharp
logger.Fatal(ex, "Unrecoverable error during startup, application will exit");
logger.Fatal("Critical security violation detected: {Details}", details);
```

### Log Level Selection Matrix

| Scenario | Recommended Level | Notes |
|----------|------------------|-------|
| Method entry/exit | Trace | Only in development |
| Configuration loaded | Information | Include non-sensitive values |
| User action started | Information | Track user workflows |
| Cache miss | Debug | Track cache efficiency |
| Slow query (< threshold) | Warning | Monitor performance degradation |
| Validation failure | Warning | Expected user errors |
| API call failed (retry succeeded) | Warning | Track reliability |
| Database operation failed | Error | Include exception details |
| File not found (expected) | Warning | Context-dependent |
| File not found (critical) | Error | Context-dependent |
| Unhandled exception | Fatal | Trigger immediate alert |

## Structured Logging

### Benefits of Structured Logging

1. **Queryability:** Search and filter logs efficiently
2. **Consistency:** Standardized log format
3. **Analytics:** Aggregate and analyze log data
4. **Monitoring:** Create alerts based on properties
5. **Context:** Rich context without string parsing

### Property Naming Conventions

**Guidelines:**
- Use PascalCase for property names
- Use descriptive, consistent names
- Avoid abbreviations unless widely understood
- Group related properties with prefixes

**Good Examples:**
```csharp
logger.Information("User {UserId} installed package {PackageId} from {SourceName}", 
    userId, packageId, sourceName);

logger.Information("Package operation completed: {PackageId}, {OperationType}, {Duration}ms, {Success}", 
    package.Id, operationType, duration, success);
```

**Bad Examples:**
```csharp
// Avoid: String interpolation loses structure
logger.Information($"User {userId} installed {packageId}");

// Avoid: Inconsistent naming
logger.Information("Package {pkg_id} from {source_name}", pkgId, sourceName);

// Avoid: Generic property names
logger.Information("Operation: {Value1}, {Value2}, {Value3}", val1, val2, val3);
```

### Standard Properties

Define standard properties for consistency across the application:

```csharp
// Common context properties
public static class LogProperties
{
    // User context
    public const string UserId = "UserId";
    public const string UserName = "UserName";
    public const string UserRole = "UserRole";
    
    // Operation context
    public const string OperationId = "OperationId";
    public const string OperationType = "OperationType";
    public const string OperationDuration = "OperationDuration";
    public const string OperationSuccess = "OperationSuccess";
    
    // Package context
    public const string PackageId = "PackageId";
    public const string PackageName = "PackageName";
    public const string PackageVersion = "PackageVersion";
    public const string PackageManager = "PackageManager";
    public const string PackageSource = "PackageSource";
    
    // System context
    public const string MachineName = "MachineName";
    public const string ApplicationVersion = "ApplicationVersion";
    public const string EnvironmentName = "EnvironmentName";
}
```

### Enrichment

Add contextual information automatically to all log entries:

**Serilog Enrichment:**
```csharp
Log.Logger = new LoggerConfiguration()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("Application", "UniGetUI")
    .Enrich.WithProperty("Version", Assembly.GetExecutingAssembly().GetName().Version)
    .WriteTo.Console()
    .CreateLogger();
```

**Custom Enrichment:**
```csharp
public class CorrelationIdEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
        var property = propertyFactory.CreateProperty("CorrelationId", correlationId);
        logEvent.AddPropertyIfAbsent(property);
    }
}

// Usage
.Enrich.With(new CorrelationIdEnricher())
```

## Correlation IDs

### Purpose

Correlation IDs enable tracking of requests/operations across multiple systems and components.

### Implementation Strategies

#### 1. Activity-Based (Recommended for .NET 5+)

```csharp
using System.Diagnostics;

public class OperationTracker
{
    private static readonly ActivitySource ActivitySource = 
        new ActivitySource("UniGetUI.PackageOperations");
    
    public async Task<PackageResult> InstallPackageAsync(string packageId)
    {
        using var activity = ActivitySource.StartActivity("InstallPackage");
        activity?.SetTag("package.id", packageId);
        
        logger.Information("Installing package {PackageId} [CorrelationId: {CorrelationId}]", 
            packageId, activity?.Id);
        
        try
        {
            // Installation logic
            var result = await InstallAsync(packageId);
            activity?.SetTag("operation.success", true);
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetTag("operation.success", false);
            activity?.SetTag("error.type", ex.GetType().Name);
            logger.Error(ex, "Failed to install package {PackageId}", packageId);
            throw;
        }
    }
}
```

#### 2. AsyncLocal-Based

```csharp
public static class CorrelationContext
{
    private static readonly AsyncLocal<string> _correlationId = new AsyncLocal<string>();
    
    public static string CorrelationId
    {
        get => _correlationId.Value ??= Guid.NewGuid().ToString();
        set => _correlationId.Value = value;
    }
    
    public static IDisposable BeginCorrelationScope(string? correlationId = null)
    {
        var previousId = _correlationId.Value;
        _correlationId.Value = correlationId ?? Guid.NewGuid().ToString();
        
        return new DisposableAction(() => _correlationId.Value = previousId);
    }
}

// Usage
using (CorrelationContext.BeginCorrelationScope())
{
    logger.Information("Processing request {RequestId}", CorrelationContext.CorrelationId);
    // All logs in this scope will have the same correlation ID
}
```

#### 3. Log Context (Serilog)

```csharp
using Serilog.Context;

public async Task ProcessPackageOperation(PackageOperation operation)
{
    var correlationId = Guid.NewGuid().ToString();
    
    using (LogContext.PushProperty("CorrelationId", correlationId))
    using (LogContext.PushProperty("OperationId", operation.Id))
    using (LogContext.PushProperty("PackageId", operation.PackageId))
    {
        logger.Information("Starting package operation");
        // All logs within this scope automatically include the properties
        
        await ExecuteOperationAsync(operation);
        
        logger.Information("Package operation completed");
    }
}
```

### Correlation ID Best Practices

1. **Generate at entry point:** Create correlation IDs at application boundaries
2. **Propagate throughout:** Pass correlation IDs through all layers
3. **Include in responses:** Return correlation IDs to callers
4. **Log consistently:** Include correlation ID in all related log entries
5. **Format consistently:** Use consistent format (GUID, UUID, custom)

## Exception Handling and Logging

### Exception Logging Patterns

#### Pattern 1: Log and Rethrow

Use when you need to log at the current level but let the exception propagate:

```csharp
public async Task<Package> GetPackageAsync(string packageId)
{
    try
    {
        return await _packageRepository.GetAsync(packageId);
    }
    catch (Exception ex)
    {
        logger.Error(ex, "Failed to retrieve package {PackageId}", packageId);
        throw; // Preserve stack trace
    }
}
```

#### Pattern 2: Log and Wrap

Use when adding context or converting exception types:

```csharp
public async Task<Package> GetPackageAsync(string packageId)
{
    try
    {
        return await _packageRepository.GetAsync(packageId);
    }
    catch (DatabaseException ex)
    {
        logger.Error(ex, "Database error while retrieving package {PackageId}", packageId);
        throw new PackageNotFoundException($"Could not find package {packageId}", ex);
    }
}
```

#### Pattern 3: Log and Handle

Use when the error is expected and can be handled:

```csharp
public async Task<Package?> TryGetPackageAsync(string packageId)
{
    try
    {
        return await _packageRepository.GetAsync(packageId);
    }
    catch (PackageNotFoundException ex)
    {
        logger.Warning(ex, "Package {PackageId} not found", packageId);
        return null; // Expected scenario
    }
    catch (Exception ex)
    {
        logger.Error(ex, "Unexpected error retrieving package {PackageId}", packageId);
        throw; // Unexpected scenario
    }
}
```

#### Pattern 4: Global Exception Handler

Catch unhandled exceptions at the application level:

```csharp
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
        logger.Fatal(exception, "Unhandled exception occurred, application will terminate");
        
        // Attempt to flush logs before shutdown
        Log.CloseAndFlush();
    }
    
    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        logger.Error(e.Exception, "Unobserved task exception");
        e.SetObserved(); // Prevent application crash
    }
}
```

### Exception Information to Log

Always include:
- Exception type and message
- Stack trace
- Inner exceptions
- Contextual information (operation being performed)
- User ID (if applicable)
- Correlation ID

```csharp
try
{
    await InstallPackageAsync(packageId);
}
catch (Exception ex)
{
    logger.Error(ex, 
        "Package installation failed: {PackageId}, User: {UserId}, Attempt: {Attempt}",
        packageId, userId, attemptNumber);
}
```

### Sensitive Data in Exceptions

**Never log:**
- Passwords or credentials
- Personal identification numbers
- Credit card information
- API keys or secrets
- Full connection strings

**Sanitize before logging:**
```csharp
public static string SanitizeConnectionString(string connectionString)
{
    var builder = new SqlConnectionStringBuilder(connectionString);
    builder.Password = "***";
    return builder.ToString();
}

logger.Debug("Database connection: {ConnectionString}", 
    SanitizeConnectionString(connectionString));
```

## Best Practices

### 1. Message Templates

**Use message templates, not string interpolation:**

✅ **Good:**
```csharp
logger.Information("Package {PackageId} installed in {Duration}ms", packageId, duration);
```

❌ **Bad:**
```csharp
logger.Information($"Package {packageId} installed in {duration}ms");
```

**Reason:** Message templates enable structured logging and efficient log analysis.

### 2. Consistent Naming

Use consistent property names across the application:

```csharp
// Define constants
public static class LogPropertyNames
{
    public const string PackageId = "PackageId";
    public const string UserId = "UserId";
    public const string Duration = "Duration";
}

// Use consistently
logger.Information("Operation completed for package {PackageId} by user {UserId} in {Duration}ms",
    packageId, userId, duration);
```

### 3. Avoid Over-Logging

❌ **Bad:**
```csharp
logger.Debug("Entering method ProcessPackage");
logger.Debug("Validating package ID");
logger.Debug("Package ID is valid");
logger.Debug("Fetching package details");
logger.Debug("Package details retrieved");
logger.Debug("Installing package");
logger.Debug("Package installed");
logger.Debug("Exiting method ProcessPackage");
```

✅ **Good:**
```csharp
using (logger.BeginScope("ProcessPackage for {PackageId}", packageId))
{
    logger.Information("Installing package {PackageId}", packageId);
    // ... installation logic ...
    logger.Information("Package {PackageId} installed successfully in {Duration}ms", 
        packageId, duration);
}
```

### 4. Use Scopes for Context

**Serilog:**
```csharp
using (LogContext.PushProperty("UserId", userId))
using (LogContext.PushProperty("OperationId", operationId))
{
    // All logs in this scope include UserId and OperationId
    logger.Information("Starting operation");
    await ProcessAsync();
    logger.Information("Operation completed");
}
```

**Microsoft.Extensions.Logging:**
```csharp
using (logger.BeginScope(new Dictionary<string, object>
{
    ["UserId"] = userId,
    ["OperationId"] = operationId
}))
{
    logger.LogInformation("Starting operation");
    await ProcessAsync();
    logger.LogInformation("Operation completed");
}
```

### 5. Log Lifecycle Events

Always log key application lifecycle events:

```csharp
public class Application
{
    public async Task StartAsync()
    {
        logger.Information("Application starting, version {Version}, environment {Environment}",
            AppVersion, Environment);
        
        // Startup logic
        
        logger.Information("Application started successfully in {Duration}ms", duration);
    }
    
    public async Task StopAsync()
    {
        logger.Information("Application stopping");
        
        // Shutdown logic
        
        logger.Information("Application stopped gracefully");
    }
}
```

### 6. Structured Exception Logging

```csharp
public static void LogException(this ILogger logger, Exception ex, string context)
{
    var properties = new Dictionary<string, object>
    {
        ["ExceptionType"] = ex.GetType().Name,
        ["ExceptionMessage"] = ex.Message,
        ["StackTrace"] = ex.StackTrace ?? "",
        ["InnerExceptionType"] = ex.InnerException?.GetType().Name ?? "None",
        ["Context"] = context
    };
    
    logger.Error(ex, "Exception occurred: {Context}", context);
}

// Usage
try
{
    await InstallPackageAsync(packageId);
}
catch (Exception ex)
{
    logger.LogException(ex, $"Installing package {packageId}");
    throw;
}
```

## Performance Considerations

### 1. Async Logging

Enable async logging for better performance:

**Serilog:**
```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Async(a => a.File("logs/app-.txt", rollingInterval: RollingInterval.Day))
    .CreateLogger();
```

**NLog:**
```xml
<targets>
    <target name="asyncFile" 
            xsi:type="AsyncWrapper" 
            queueLimit="10000"
            overflowAction="Discard">
        <target name="file" xsi:type="File" fileName="logs/app.log" />
    </target>
</targets>
```

### 2. Conditional Logging

Avoid expensive operations when log level is disabled:

❌ **Bad:**
```csharp
logger.Debug("Package details: " + GetExpensivePackageDetails());
```

✅ **Good:**
```csharp
if (logger.IsEnabled(LogLevel.Debug))
{
    logger.Debug("Package details: {Details}", GetExpensivePackageDetails());
}
```

### 3. Buffering and Batching

For high-throughput scenarios:

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("logs/app.txt", 
        buffered: true,
        flushToDiskInterval: TimeSpan.FromSeconds(10))
    .CreateLogger();
```

### 4. Sampling

For very high-volume scenarios, consider log sampling:

```csharp
private static int _logCounter = 0;

public void ProcessItem(Item item)
{
    if (Interlocked.Increment(ref _logCounter) % 100 == 0)
    {
        logger.Debug("Processed {Count} items, current: {ItemId}", _logCounter, item.Id);
    }
}
```

## Security and Compliance

### 1. PII Protection

Never log personally identifiable information:

❌ **Bad:**
```csharp
logger.Information("User {Email} logged in from {IpAddress}", email, ipAddress);
```

✅ **Good:**
```csharp
logger.Information("User {UserId} logged in from {IpAddressHash}", 
    userId, HashIpAddress(ipAddress));
```

### 2. Log Redaction

Implement automatic redaction for sensitive data:

```csharp
public class RedactingEnricher : ILogEventEnricher
{
    private static readonly Regex PasswordPattern = new Regex(
        @"password[\""]?\s*[:=]\s*[\""]?([^\s\""]+)", 
        RegexOptions.IgnoreCase);
    
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var message = logEvent.MessageTemplate.Text;
        var redacted = PasswordPattern.Replace(message, "password=***");
        
        // Update message if redaction occurred
        if (redacted != message)
        {
            var property = propertyFactory.CreateProperty("OriginalMessage", message);
            logEvent.AddPropertyIfAbsent(property);
        }
    }
}
```

### 3. Log Retention

Define and enforce log retention policies:

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("logs/app-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30) // Keep 30 days
    .CreateLogger();
```

### 4. Access Control

Ensure logs are stored securely with appropriate access controls:

```csharp
// Set file permissions on log directory
var logDirectory = new DirectoryInfo("logs");
var security = logDirectory.GetAccessControl();
security.SetAccessRuleProtection(true, false); // Disable inheritance

// Add specific permissions
var adminRule = new FileSystemAccessRule(
    "Administrators",
    FileSystemRights.FullControl,
    AccessControlType.Allow);
security.AddAccessRule(adminRule);

logDirectory.SetAccessControl(security);
```

## Configuration Examples

### Serilog Configuration

**Code-based:**
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("Application", "UniGetUI")
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/app-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.Async(a => a.File(
        path: "logs/errors-.txt",
        restrictedToMinimumLevel: LogEventLevel.Error,
        rollingInterval: RollingInterval.Day))
    .CreateLogger();
```

**appsettings.json:**
```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "logs/app-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
  }
}
```

### NLog Configuration

**NLog.config:**
```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">

  <variable name="logDirectory" value="${basedir}/logs"/>
  
  <targets>
    <!-- Console target -->
    <target name="console" xsi:type="Console"
            layout="${longdate} ${level:uppercase=true} ${logger} ${message} ${exception:format=tostring}" />
    
    <!-- File target -->
    <target name="file" xsi:type="File"
            fileName="${logDirectory}/app-${shortdate}.log"
            layout="${longdate} ${level:uppercase=true} ${logger} ${message} ${exception:format=tostring}"
            archiveEvery="Day"
            archiveNumbering="Rolling"
            maxArchiveFiles="30" />
    
    <!-- Error file target -->
    <target name="errorFile" xsi:type="File"
            fileName="${logDirectory}/errors-${shortdate}.log"
            layout="${longdate} ${level:uppercase=true} ${logger} ${message} ${exception:format=tostring,Data:maxInnerExceptionLevel=10}"
            archiveEvery="Day"
            archiveNumbering="Rolling"
            maxArchiveFiles="30" />
  </targets>

  <rules>
    <logger name="*" minlevel="Debug" writeTo="console" />
    <logger name="*" minlevel="Info" writeTo="file" />
    <logger name="*" minlevel="Error" writeTo="errorFile" />
  </rules>
</nlog>
```

## Summary

Effective logging is critical for:
- **Troubleshooting:** Quick problem identification and resolution
- **Monitoring:** Understanding application health and performance
- **Auditing:** Tracking user actions and system changes
- **Analytics:** Understanding usage patterns and trends

Follow these standards to ensure logs are:
- **Structured:** Easy to query and analyze
- **Consistent:** Predictable format across the application
- **Contextual:** Rich with relevant information
- **Performant:** Minimal impact on application performance
- **Secure:** Protected sensitive information
- **Compliant:** Meeting regulatory requirements

## See Also

- [Monitoring Guide](./monitoring-guide.md)
- [Diagnostics Guide](./diagnostics-guide.md)
- [Structured Logging Examples](../../examples/logging/structured-logging/)
