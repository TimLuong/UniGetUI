# Diagnostics Guide for Windows Applications

## Overview

This guide provides comprehensive strategies and best practices for diagnosing issues in Windows applications, including distributed tracing, log aggregation, troubleshooting workflows, and debugging techniques.

## Table of Contents

1. [Distributed Tracing](#distributed-tracing)
2. [Log Aggregation and Analysis](#log-aggregation-and-analysis)
3. [Troubleshooting Workflows](#troubleshooting-workflows)
4. [Debugging Techniques](#debugging-techniques)
5. [Performance Diagnostics](#performance-diagnostics)
6. [Memory Diagnostics](#memory-diagnostics)
7. [Network Diagnostics](#network-diagnostics)
8. [Common Failure Patterns](#common-failure-patterns)

## Distributed Tracing

### What is Distributed Tracing?

Distributed tracing tracks requests as they flow through various components and services, providing end-to-end visibility into application behavior.

### Key Concepts

- **Trace:** Complete journey of a request through the system
- **Span:** Individual unit of work within a trace
- **Context Propagation:** Passing trace information between components
- **Trace ID:** Unique identifier for a complete trace
- **Span ID:** Unique identifier for a specific span

### Implementation with System.Diagnostics

#### Basic Activity Tracing

```csharp
using System.Diagnostics;

public class PackageService
{
    private static readonly ActivitySource ActivitySource = 
        new ActivitySource("UniGetUI.PackageService", "1.0.0");
    
    public async Task<PackageResult> InstallPackageAsync(string packageId)
    {
        using var activity = ActivitySource.StartActivity("InstallPackage");
        activity?.SetTag("package.id", packageId);
        activity?.SetTag("package.manager", "winget");
        
        try
        {
            // Download phase
            using (var downloadActivity = ActivitySource.StartActivity("DownloadPackage"))
            {
                downloadActivity?.SetTag("package.id", packageId);
                await DownloadPackageAsync(packageId);
                downloadActivity?.SetTag("download.size", 1024000);
            }
            
            // Install phase
            using (var installActivity = ActivitySource.StartActivity("InstallPackage"))
            {
                installActivity?.SetTag("package.id", packageId);
                await InstallAsync(packageId);
                installActivity?.SetStatus(ActivityStatusCode.Ok);
            }
            
            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.SetTag("operation.success", true);
            
            return new PackageResult { Success = true };
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("operation.success", false);
            activity?.SetTag("error.type", ex.GetType().Name);
            activity?.SetTag("error.message", ex.Message);
            
            throw;
        }
    }
}
```

#### Activity Listener Configuration

```csharp
public class TraceConfiguration
{
    public static void ConfigureTracing()
    {
        // Create activity listener
        var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name.StartsWith("UniGetUI"),
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => 
                ActivitySamplingResult.AllData,
            ActivityStarted = activity =>
            {
                Console.WriteLine($"Activity started: {activity.DisplayName}");
            },
            ActivityStopped = activity =>
            {
                Console.WriteLine($"Activity stopped: {activity.DisplayName}, Duration: {activity.Duration}");
            }
        };
        
        ActivitySource.AddActivityListener(listener);
    }
}
```

#### Context Propagation

```csharp
public class DistributedContext
{
    public static async Task<HttpResponseMessage> CallExternalServiceAsync(
        HttpClient httpClient, 
        string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        
        // Propagate trace context
        var activity = Activity.Current;
        if (activity != null)
        {
            request.Headers.Add("traceparent", activity.Id);
            
            // Add baggage items if needed
            foreach (var baggageItem in activity.Baggage)
            {
                request.Headers.Add($"baggage-{baggageItem.Key}", baggageItem.Value);
            }
        }
        
        return await httpClient.SendAsync(request);
    }
    
    public static Activity? ExtractContext(HttpRequestMessage request)
    {
        if (request.Headers.TryGetValues("traceparent", out var values))
        {
            var traceparent = values.FirstOrDefault();
            if (!string.IsNullOrEmpty(traceparent))
            {
                // Create activity with parent context
                return new Activity("ProcessRequest").SetParentId(traceparent);
            }
        }
        
        return null;
    }
}
```

### OpenTelemetry Integration

```csharp
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

public class OpenTelemetryConfiguration
{
    public static TracerProvider ConfigureOpenTelemetry()
    {
        return Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService("UniGetUI", serviceVersion: "1.0.0"))
            .AddSource("UniGetUI.*")
            .AddHttpClientInstrumentation()
            .AddSqlClientInstrumentation()
            .AddConsoleExporter()
            .AddJaegerExporter(options =>
            {
                options.AgentHost = "localhost";
                options.AgentPort = 6831;
            })
            .AddZipkinExporter(options =>
            {
                options.Endpoint = new Uri("http://localhost:9411/api/v2/spans");
            })
            .Build();
    }
}
```

### Trace Analysis

```csharp
public class TraceAnalyzer
{
    public TraceAnalysis AnalyzeTrace(Activity activity)
    {
        var analysis = new TraceAnalysis
        {
            TraceId = activity.TraceId.ToString(),
            TotalDuration = activity.Duration,
            SpanCount = CountSpans(activity)
        };
        
        // Find slowest span
        var spans = GetAllSpans(activity);
        analysis.SlowestSpan = spans
            .OrderByDescending(s => s.Duration)
            .FirstOrDefault();
        
        // Identify bottlenecks
        analysis.Bottlenecks = spans
            .Where(s => s.Duration > TimeSpan.FromMilliseconds(1000))
            .Select(s => new Bottleneck
            {
                SpanName = s.DisplayName,
                Duration = s.Duration,
                Tags = s.Tags.ToDictionary(t => t.Key, t => t.Value)
            })
            .ToList();
        
        // Check for errors
        analysis.HasErrors = spans.Any(s => s.Status == ActivityStatusCode.Error);
        analysis.Errors = spans
            .Where(s => s.Status == ActivityStatusCode.Error)
            .Select(s => new ErrorInfo
            {
                SpanName = s.DisplayName,
                ErrorMessage = s.StatusDescription,
                Tags = s.Tags.ToDictionary(t => t.Key, t => t.Value)
            })
            .ToList();
        
        return analysis;
    }
    
    private int CountSpans(Activity activity)
    {
        int count = 1;
        foreach (var link in activity.Links)
        {
            // Count linked activities
            count++;
        }
        return count;
    }
    
    private List<Activity> GetAllSpans(Activity activity)
    {
        // In real implementation, would collect all child spans
        return new List<Activity> { activity };
    }
}

public class TraceAnalysis
{
    public string TraceId { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public int SpanCount { get; set; }
    public Activity? SlowestSpan { get; set; }
    public List<Bottleneck> Bottlenecks { get; set; } = new();
    public bool HasErrors { get; set; }
    public List<ErrorInfo> Errors { get; set; } = new();
}

public class Bottleneck
{
    public string SpanName { get; set; }
    public TimeSpan Duration { get; set; }
    public Dictionary<string, object?> Tags { get; set; }
}

public class ErrorInfo
{
    public string SpanName { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object?> Tags { get; set; }
}
```

## Log Aggregation and Analysis

### Centralized Logging Architecture

```
Application Logs → Aggregator → Storage → Analysis/Visualization
                                    ↓
                            Elasticsearch/Azure Monitor
                                    ↓
                              Kibana/Grafana
```

### Structured Log Parsing

```csharp
public class LogParser
{
    public LogEntry ParseLogLine(string logLine)
    {
        // Parse structured log format
        // Example: 2024-01-15 10:30:45.123 [INFO] Package installation {PackageId: "chrome", Duration: 1234}
        
        var parts = logLine.Split(new[] { ' ' }, 4);
        
        var entry = new LogEntry
        {
            Timestamp = DateTime.Parse($"{parts[0]} {parts[1]}"),
            Level = parts[2].Trim('[', ']'),
            Message = parts[3]
        };
        
        // Extract structured properties
        var propertiesMatch = Regex.Match(entry.Message, @"\{([^}]+)\}");
        if (propertiesMatch.Success)
        {
            entry.Properties = ParseProperties(propertiesMatch.Groups[1].Value);
        }
        
        return entry;
    }
    
    private Dictionary<string, string> ParseProperties(string propertiesString)
    {
        var properties = new Dictionary<string, string>();
        var pairs = propertiesString.Split(',');
        
        foreach (var pair in pairs)
        {
            var keyValue = pair.Split(':');
            if (keyValue.Length == 2)
            {
                var key = keyValue[0].Trim().Trim('"');
                var value = keyValue[1].Trim().Trim('"');
                properties[key] = value;
            }
        }
        
        return properties;
    }
}

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; }
    public string Message { get; set; }
    public Dictionary<string, string> Properties { get; set; } = new();
}
```

### ELK Stack Integration (Elasticsearch, Logstash, Kibana)

#### Serilog to Elasticsearch

```csharp
using Serilog;
using Serilog.Sinks.Elasticsearch;

public class ElasticsearchConfiguration
{
    public static void ConfigureElasticsearch()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(
                new Uri("http://localhost:9200"))
            {
                AutoRegisterTemplate = true,
                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                IndexFormat = "unigetui-logs-{0:yyyy.MM.dd}",
                NumberOfShards = 2,
                NumberOfReplicas = 1,
                ModifyConnectionSettings = x => x.BasicAuthentication("user", "password")
            })
            .CreateLogger();
    }
}
```

#### Logstash Configuration

```ruby
# logstash.conf
input {
  file {
    path => "C:/Logs/UniGetUI/app-*.txt"
    start_position => "beginning"
    sincedb_path => "NUL"
    codec => json
  }
}

filter {
  if [level] == "Error" {
    mutate {
      add_tag => ["error"]
    }
  }
  
  if [Properties][CorrelationId] {
    mutate {
      add_field => {
        "correlation_id" => "%{[Properties][CorrelationId]}"
      }
    }
  }
}

output {
  elasticsearch {
    hosts => ["localhost:9200"]
    index => "unigetui-logs-%{+YYYY.MM.dd}"
  }
  
  stdout {
    codec => rubydebug
  }
}
```

### Azure Monitor Integration

```csharp
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

public class AzureMonitorConfiguration
{
    private readonly TelemetryClient _telemetryClient;
    
    public AzureMonitorConfiguration(string instrumentationKey)
    {
        var configuration = new TelemetryConfiguration
        {
            InstrumentationKey = instrumentationKey,
            TelemetryInitializers =
            {
                new OperationCorrelationTelemetryInitializer(),
                new HttpDependenciesParsingTelemetryInitializer()
            }
        };
        
        _telemetryClient = new TelemetryClient(configuration);
    }
    
    public void TrackException(Exception exception, Dictionary<string, string> properties)
    {
        _telemetryClient.TrackException(exception, properties);
    }
    
    public void TrackTrace(string message, SeverityLevel severity, Dictionary<string, string> properties)
    {
        _telemetryClient.TrackTrace(message, severity, properties);
    }
    
    public void TrackDependency(
        string dependencyName,
        string target,
        string operation,
        TimeSpan duration,
        bool success)
    {
        _telemetryClient.TrackDependency(
            dependencyName,
            target,
            operation,
            DateTimeOffset.UtcNow - duration,
            duration,
            success);
    }
}
```

### Log Query and Analysis

```csharp
public class LogAnalyzer
{
    public async Task<List<LogEntry>> FindErrorsAsync(
        DateTime startTime, 
        DateTime endTime,
        string? correlationId = null)
    {
        // Query logs from storage
        var query = $@"
            SELECT * FROM logs 
            WHERE level = 'Error' 
            AND timestamp BETWEEN '{startTime:yyyy-MM-dd HH:mm:ss}' 
                AND '{endTime:yyyy-MM-dd HH:mm:ss}'";
        
        if (!string.IsNullOrEmpty(correlationId))
        {
            query += $" AND correlationId = '{correlationId}'";
        }
        
        query += " ORDER BY timestamp DESC";
        
        // Execute query (pseudo-code)
        return await ExecuteQueryAsync(query);
    }
    
    public async Task<ErrorPattern> AnalyzeErrorPatternsAsync(TimeSpan period)
    {
        var errors = await FindErrorsAsync(
            DateTime.UtcNow - period, 
            DateTime.UtcNow);
        
        var pattern = new ErrorPattern
        {
            TotalErrors = errors.Count,
            ErrorsByType = errors
                .GroupBy(e => GetExceptionType(e.Message))
                .ToDictionary(g => g.Key, g => g.Count()),
            ErrorsByHour = errors
                .GroupBy(e => e.Timestamp.Hour)
                .ToDictionary(g => g.Key, g => g.Count()),
            MostCommonErrors = errors
                .GroupBy(e => e.Message)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => new ErrorSummary
                {
                    Message = g.Key,
                    Count = g.Count(),
                    FirstOccurrence = g.Min(e => e.Timestamp),
                    LastOccurrence = g.Max(e => e.Timestamp)
                })
                .ToList()
        };
        
        return pattern;
    }
    
    private string GetExceptionType(string message)
    {
        var match = Regex.Match(message, @"(\w+Exception)");
        return match.Success ? match.Groups[1].Value : "Unknown";
    }
}

public class ErrorPattern
{
    public int TotalErrors { get; set; }
    public Dictionary<string, int> ErrorsByType { get; set; }
    public Dictionary<int, int> ErrorsByHour { get; set; }
    public List<ErrorSummary> MostCommonErrors { get; set; }
}

public class ErrorSummary
{
    public string Message { get; set; }
    public int Count { get; set; }
    public DateTime FirstOccurrence { get; set; }
    public DateTime LastOccurrence { get; set; }
}
```

## Troubleshooting Workflows

### Systematic Troubleshooting Process

1. **Identify the Problem**
   - What is the symptom?
   - When did it start?
   - Is it reproducible?
   - What changed recently?

2. **Gather Information**
   - Collect logs
   - Check metrics
   - Review traces
   - Check system resources

3. **Form Hypothesis**
   - What could cause this?
   - What are the most likely causes?

4. **Test Hypothesis**
   - Make minimal changes
   - Test one thing at a time
   - Document results

5. **Implement Solution**
   - Apply fix
   - Monitor results
   - Document resolution

### Troubleshooting Checklist

```csharp
public class TroubleshootingChecklist
{
    public async Task<DiagnosticReport> RunDiagnosticsAsync()
    {
        var report = new DiagnosticReport();
        
        // Check application health
        report.ApplicationHealth = await CheckApplicationHealthAsync();
        
        // Check system resources
        report.SystemResources = CheckSystemResources();
        
        // Check dependencies
        report.DependencyStatus = await CheckDependenciesAsync();
        
        // Check recent errors
        report.RecentErrors = await GetRecentErrorsAsync();
        
        // Check configuration
        report.ConfigurationIssues = CheckConfiguration();
        
        // Check network connectivity
        report.NetworkStatus = await CheckNetworkConnectivityAsync();
        
        // Generate recommendations
        report.Recommendations = GenerateRecommendations(report);
        
        return report;
    }
    
    private HealthStatus CheckApplicationHealthAsync()
    {
        // Check if application is running
        // Check if all services are responsive
        // Check if there are any critical errors
        return HealthStatus.Healthy;
    }
    
    private SystemResourceStatus CheckSystemResources()
    {
        var process = Process.GetCurrentProcess();
        
        return new SystemResourceStatus
        {
            CpuUsage = GetCpuUsage(),
            MemoryUsage = process.WorkingSet64,
            DiskSpace = GetDiskSpace(),
            ThreadCount = process.Threads.Count,
            HandleCount = process.HandleCount
        };
    }
    
    private async Task<Dictionary<string, bool>> CheckDependenciesAsync()
    {
        var dependencies = new Dictionary<string, bool>();
        
        // Check database
        dependencies["Database"] = await TestDatabaseConnectionAsync();
        
        // Check external APIs
        dependencies["PackageAPI"] = await TestApiConnectionAsync("https://api.example.com");
        
        // Check file system
        dependencies["FileSystem"] = TestFileSystemAccess();
        
        return dependencies;
    }
    
    private async Task<List<LogEntry>> GetRecentErrorsAsync()
    {
        // Get errors from last hour
        var analyzer = new LogAnalyzer();
        return await analyzer.FindErrorsAsync(
            DateTime.UtcNow.AddHours(-1),
            DateTime.UtcNow);
    }
    
    private List<string> CheckConfiguration()
    {
        var issues = new List<string>();
        
        // Check required configuration values
        if (string.IsNullOrEmpty(Config.DatabaseConnection))
        {
            issues.Add("Database connection string not configured");
        }
        
        // Check file paths
        if (!Directory.Exists(Config.LogDirectory))
        {
            issues.Add($"Log directory does not exist: {Config.LogDirectory}");
        }
        
        return issues;
    }
    
    private List<string> GenerateRecommendations(DiagnosticReport report)
    {
        var recommendations = new List<string>();
        
        if (report.SystemResources.MemoryUsage > 1024 * 1024 * 1024) // > 1GB
        {
            recommendations.Add("High memory usage detected. Consider restarting the application.");
        }
        
        if (report.RecentErrors.Count > 100)
        {
            recommendations.Add("High error rate detected. Check logs for patterns.");
        }
        
        if (!report.DependencyStatus.Values.All(v => v))
        {
            recommendations.Add("One or more dependencies are unavailable. Check network and service status.");
        }
        
        return recommendations;
    }
}

public class DiagnosticReport
{
    public HealthStatus ApplicationHealth { get; set; }
    public SystemResourceStatus SystemResources { get; set; }
    public Dictionary<string, bool> DependencyStatus { get; set; }
    public List<LogEntry> RecentErrors { get; set; }
    public List<string> ConfigurationIssues { get; set; }
    public NetworkStatus NetworkStatus { get; set; }
    public List<string> Recommendations { get; set; }
}
```

### Root Cause Analysis

```csharp
public class RootCauseAnalyzer
{
    public async Task<RootCauseAnalysis> AnalyzeAsync(string correlationId)
    {
        var analysis = new RootCauseAnalysis { CorrelationId = correlationId };
        
        // Get all logs for this correlation ID
        var logs = await GetLogsForCorrelationIdAsync(correlationId);
        analysis.Timeline = BuildTimeline(logs);
        
        // Get trace information
        var trace = await GetTraceAsync(correlationId);
        analysis.TraceAnalysis = AnalyzeTrace(trace);
        
        // Identify failure point
        analysis.FailurePoint = IdentifyFailurePoint(logs);
        
        // Find related errors
        analysis.RelatedErrors = await FindRelatedErrorsAsync(
            analysis.FailurePoint.Timestamp,
            TimeSpan.FromMinutes(5));
        
        // Determine probable cause
        analysis.ProbableCause = DetermineProbableCause(analysis);
        
        return analysis;
    }
    
    private List<TimelineEvent> BuildTimeline(List<LogEntry> logs)
    {
        return logs
            .OrderBy(l => l.Timestamp)
            .Select(l => new TimelineEvent
            {
                Timestamp = l.Timestamp,
                Level = l.Level,
                Message = l.Message,
                Properties = l.Properties
            })
            .ToList();
    }
    
    private FailurePoint IdentifyFailurePoint(List<LogEntry> logs)
    {
        var errorLog = logs.FirstOrDefault(l => l.Level == "Error");
        if (errorLog != null)
        {
            return new FailurePoint
            {
                Timestamp = errorLog.Timestamp,
                Message = errorLog.Message,
                Component = errorLog.Properties.GetValueOrDefault("Component", "Unknown"),
                StackTrace = errorLog.Properties.GetValueOrDefault("StackTrace", "")
            };
        }
        
        return null;
    }
    
    private string DetermineProbableCause(RootCauseAnalysis analysis)
    {
        if (analysis.FailurePoint?.Message?.Contains("Timeout") == true)
        {
            return "Network timeout or slow external service";
        }
        
        if (analysis.FailurePoint?.Message?.Contains("OutOfMemory") == true)
        {
            return "Memory exhaustion";
        }
        
        if (analysis.FailurePoint?.Message?.Contains("Access denied") == true)
        {
            return "Permission or authentication issue";
        }
        
        return "Unknown - manual investigation required";
    }
}

public class RootCauseAnalysis
{
    public string CorrelationId { get; set; }
    public List<TimelineEvent> Timeline { get; set; }
    public TraceAnalysis TraceAnalysis { get; set; }
    public FailurePoint FailurePoint { get; set; }
    public List<LogEntry> RelatedErrors { get; set; }
    public string ProbableCause { get; set; }
}

public class TimelineEvent
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; }
    public string Message { get; set; }
    public Dictionary<string, string> Properties { get; set; }
}

public class FailurePoint
{
    public DateTime Timestamp { get; set; }
    public string Message { get; set; }
    public string Component { get; set; }
    public string StackTrace { get; set; }
}
```

## Debugging Techniques

### Debug Logging

```csharp
public class DebugLogger
{
    private readonly ILogger _logger;
    private readonly bool _isDebugEnabled;
    
    public DebugLogger(ILogger logger, bool isDebugEnabled)
    {
        _logger = logger;
        _isDebugEnabled = isDebugEnabled;
    }
    
    public void LogMethodEntry(
        [CallerMemberName] string methodName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        params object[] parameters)
    {
        if (_isDebugEnabled)
        {
            _logger.LogDebug(
                "Entering {MethodName} at {FilePath}:{LineNumber}, Parameters: {Parameters}",
                methodName, Path.GetFileName(filePath), lineNumber, 
                string.Join(", ", parameters));
        }
    }
    
    public void LogMethodExit(
        object? returnValue = null,
        [CallerMemberName] string methodName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (_isDebugEnabled)
        {
            _logger.LogDebug(
                "Exiting {MethodName} at {FilePath}:{LineNumber}, Return: {ReturnValue}",
                methodName, Path.GetFileName(filePath), lineNumber, returnValue);
        }
    }
    
    public void LogState(object state, string description = "")
    {
        if (_isDebugEnabled)
        {
            _logger.LogDebug("State {Description}: {State}", 
                description, JsonSerializer.Serialize(state));
        }
    }
}
```

### Conditional Compilation

```csharp
public class ConditionalDebugging
{
    [Conditional("DEBUG")]
    public static void DebugLog(string message)
    {
        Console.WriteLine($"[DEBUG] {message}");
    }
    
    [Conditional("TRACE")]
    public static void TraceLog(string message)
    {
        Console.WriteLine($"[TRACE] {message}");
    }
    
    public void ProcessData(Data data)
    {
        DebugLog($"Processing data: {data.Id}");
        
        // Processing logic
        
        TraceLog($"Data processed successfully");
    }
}
```

### Breakpoint Logging

```csharp
public class BreakpointLogger
{
    private readonly ILogger _logger;
    
    public void LogBreakpoint(
        string condition,
        [CallerMemberName] string methodName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        _logger.LogWarning(
            "Breakpoint hit in {MethodName} at line {LineNumber}: {Condition}",
            methodName, lineNumber, condition);
        
        // Could trigger debugger
        if (Debugger.IsAttached)
        {
            Debugger.Break();
        }
    }
}
```

## Performance Diagnostics

### Performance Profiling

```csharp
public class PerformanceProfiler
{
    private readonly Dictionary<string, List<TimeSpan>> _measurements = new();
    
    public IDisposable Profile(string operationName)
    {
        return new ProfileScope(operationName, this);
    }
    
    private void RecordMeasurement(string operationName, TimeSpan duration)
    {
        if (!_measurements.ContainsKey(operationName))
        {
            _measurements[operationName] = new List<TimeSpan>();
        }
        
        _measurements[operationName].Add(duration);
    }
    
    public PerformanceReport GenerateReport()
    {
        var report = new PerformanceReport();
        
        foreach (var kvp in _measurements)
        {
            var measurements = kvp.Value;
            report.Operations[kvp.Key] = new OperationStats
            {
                Count = measurements.Count,
                TotalDuration = TimeSpan.FromTicks(measurements.Sum(m => m.Ticks)),
                AverageDuration = TimeSpan.FromTicks((long)measurements.Average(m => m.Ticks)),
                MinDuration = measurements.Min(),
                MaxDuration = measurements.Max()
            };
        }
        
        return report;
    }
    
    private class ProfileScope : IDisposable
    {
        private readonly string _operationName;
        private readonly PerformanceProfiler _profiler;
        private readonly Stopwatch _stopwatch;
        
        public ProfileScope(string operationName, PerformanceProfiler profiler)
        {
            _operationName = operationName;
            _profiler = profiler;
            _stopwatch = Stopwatch.StartNew();
        }
        
        public void Dispose()
        {
            _stopwatch.Stop();
            _profiler.RecordMeasurement(_operationName, _stopwatch.Elapsed);
        }
    }
}

public class PerformanceReport
{
    public Dictionary<string, OperationStats> Operations { get; set; } = new();
}

public class OperationStats
{
    public int Count { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public TimeSpan AverageDuration { get; set; }
    public TimeSpan MinDuration { get; set; }
    public TimeSpan MaxDuration { get; set; }
}
```

### Slow Query Detection

```csharp
public class SlowQueryDetector
{
    private readonly TimeSpan _threshold;
    private readonly ILogger _logger;
    
    public SlowQueryDetector(TimeSpan threshold, ILogger logger)
    {
        _threshold = threshold;
        _logger = logger;
    }
    
    public async Task<T> ExecuteWithMonitoringAsync<T>(
        Func<Task<T>> operation,
        string operationName)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var result = await operation();
            stopwatch.Stop();
            
            if (stopwatch.Elapsed > _threshold)
            {
                _logger.LogWarning(
                    "Slow operation detected: {OperationName}, Duration: {Duration}ms",
                    operationName, stopwatch.ElapsedMilliseconds);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "Operation failed: {OperationName}, Duration: {Duration}ms",
                operationName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
```

## Memory Diagnostics

### Memory Leak Detection

```csharp
public class MemoryMonitor
{
    private readonly Timer _timer;
    private readonly ILogger _logger;
    private long _previousMemory;
    
    public MemoryMonitor(ILogger logger)
    {
        _logger = logger;
        _previousMemory = GC.GetTotalMemory(false);
        _timer = new Timer(CheckMemory, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }
    
    private void CheckMemory(object? state)
    {
        var currentMemory = GC.GetTotalMemory(false);
        var memoryDelta = currentMemory - _previousMemory;
        var memoryDeltaMB = memoryDelta / (1024.0 * 1024.0);
        
        _logger.LogInformation(
            "Memory: {CurrentMemory}MB, Delta: {DeltaMB}MB, Gen0: {Gen0}, Gen1: {Gen1}, Gen2: {Gen2}",
            currentMemory / (1024.0 * 1024.0),
            memoryDeltaMB,
            GC.CollectionCount(0),
            GC.CollectionCount(1),
            GC.CollectionCount(2));
        
        if (memoryDeltaMB > 100) // More than 100MB increase
        {
            _logger.LogWarning(
                "Potential memory leak detected: Memory increased by {DeltaMB}MB",
                memoryDeltaMB);
        }
        
        _previousMemory = currentMemory;
    }
    
    public void Dispose()
    {
        _timer?.Dispose();
    }
}
```

### Memory Snapshot Analysis

```csharp
public class MemorySnapshot
{
    public long TotalMemory { get; set; }
    public long Gen0Collections { get; set; }
    public long Gen1Collections { get; set; }
    public long Gen2Collections { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<Type, int> ObjectCounts { get; set; } = new();
}

public class MemoryAnalyzer
{
    public MemorySnapshot TakeSnapshot()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        return new MemorySnapshot
        {
            TotalMemory = GC.GetTotalMemory(true),
            Gen0Collections = GC.CollectionCount(0),
            Gen1Collections = GC.CollectionCount(1),
            Gen2Collections = GC.CollectionCount(2),
            Timestamp = DateTime.UtcNow
        };
    }
    
    public MemoryComparison CompareSnapshots(MemorySnapshot before, MemorySnapshot after)
    {
        return new MemoryComparison
        {
            MemoryIncrease = after.TotalMemory - before.TotalMemory,
            Gen0CollectionsIncrease = after.Gen0Collections - before.Gen0Collections,
            Gen1CollectionsIncrease = after.Gen1Collections - before.Gen1Collections,
            Gen2CollectionsIncrease = after.Gen2Collections - before.Gen2Collections,
            Duration = after.Timestamp - before.Timestamp
        };
    }
}

public class MemoryComparison
{
    public long MemoryIncrease { get; set; }
    public long Gen0CollectionsIncrease { get; set; }
    public long Gen1CollectionsIncrease { get; set; }
    public long Gen2CollectionsIncrease { get; set; }
    public TimeSpan Duration { get; set; }
}
```

## Network Diagnostics

### Network Connectivity Testing

```csharp
public class NetworkDiagnostics
{
    private readonly ILogger _logger;
    
    public async Task<NetworkStatus> DiagnoseNetworkAsync()
    {
        var status = new NetworkStatus();
        
        // Check internet connectivity
        status.InternetConnected = await TestInternetConnectivityAsync();
        
        // Check specific endpoints
        status.EndpointStatus = await TestEndpointsAsync(new[]
        {
            "https://api.github.com",
            "https://www.google.com",
            "https://packages.microsoft.com"
        });
        
        // Check DNS resolution
        status.DnsWorking = await TestDnsResolutionAsync("www.google.com");
        
        // Get network adapter info
        status.NetworkAdapters = GetNetworkAdapters();
        
        return status;
    }
    
    private async Task<bool> TestInternetConnectivityAsync()
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var response = await client.GetAsync("https://www.google.com");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
    
    private async Task<Dictionary<string, bool>> TestEndpointsAsync(string[] endpoints)
    {
        var results = new Dictionary<string, bool>();
        
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        
        foreach (var endpoint in endpoints)
        {
            try
            {
                var response = await client.GetAsync(endpoint);
                results[endpoint] = response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to connect to {Endpoint}", endpoint);
                results[endpoint] = false;
            }
        }
        
        return results;
    }
    
    private async Task<bool> TestDnsResolutionAsync(string hostname)
    {
        try
        {
            var addresses = await Dns.GetHostAddressesAsync(hostname);
            return addresses.Length > 0;
        }
        catch
        {
            return false;
        }
    }
    
    private List<NetworkAdapterInfo> GetNetworkAdapters()
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .Select(adapter => new NetworkAdapterInfo
            {
                Name = adapter.Name,
                Description = adapter.Description,
                Status = adapter.OperationalStatus.ToString(),
                Speed = adapter.Speed,
                Type = adapter.NetworkInterfaceType.ToString()
            })
            .ToList();
    }
}

public class NetworkStatus
{
    public bool InternetConnected { get; set; }
    public Dictionary<string, bool> EndpointStatus { get; set; }
    public bool DnsWorking { get; set; }
    public List<NetworkAdapterInfo> NetworkAdapters { get; set; }
}

public class NetworkAdapterInfo
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
    public long Speed { get; set; }
    public string Type { get; set; }
}
```

## Common Failure Patterns

### Pattern Recognition

```csharp
public class FailurePatternDetector
{
    public List<FailurePattern> DetectPatterns(List<LogEntry> errors)
    {
        var patterns = new List<FailurePattern>();
        
        // Pattern 1: Cascading failures
        var cascadingFailures = DetectCascadingFailures(errors);
        if (cascadingFailures != null)
        {
            patterns.Add(cascadingFailures);
        }
        
        // Pattern 2: Intermittent failures
        var intermittentFailures = DetectIntermittentFailures(errors);
        if (intermittentFailures != null)
        {
            patterns.Add(intermittentFailures);
        }
        
        // Pattern 3: Timeout patterns
        var timeoutPatterns = DetectTimeoutPatterns(errors);
        if (timeoutPatterns != null)
        {
            patterns.Add(timeoutPatterns);
        }
        
        return patterns;
    }
    
    private FailurePattern? DetectCascadingFailures(List<LogEntry> errors)
    {
        // Look for multiple errors in quick succession
        var groupedByTime = errors
            .GroupBy(e => new DateTime(e.Timestamp.Year, e.Timestamp.Month, 
                e.Timestamp.Day, e.Timestamp.Hour, e.Timestamp.Minute / 5 * 5, 0))
            .Where(g => g.Count() > 10)
            .ToList();
        
        if (groupedByTime.Any())
        {
            return new FailurePattern
            {
                Type = "CascadingFailure",
                Description = "Multiple failures detected in short time period",
                Severity = "High",
                Recommendation = "Check for root cause that triggered cascade"
            };
        }
        
        return null;
    }
    
    private FailurePattern? DetectIntermittentFailures(List<LogEntry> errors)
    {
        // Look for the same error occurring sporadically
        var errorGroups = errors
            .GroupBy(e => e.Message)
            .Where(g => g.Count() >= 3);
        
        foreach (var group in errorGroups)
        {
            var timestamps = group.Select(e => e.Timestamp).OrderBy(t => t).ToList();
            var intervals = new List<TimeSpan>();
            
            for (int i = 1; i < timestamps.Count; i++)
            {
                intervals.Add(timestamps[i] - timestamps[i - 1]);
            }
            
            // If intervals vary significantly, it's intermittent
            var avgInterval = TimeSpan.FromTicks((long)intervals.Average(i => i.Ticks));
            var maxDeviation = intervals.Max(i => Math.Abs((i - avgInterval).Ticks));
            
            if (maxDeviation > avgInterval.Ticks)
            {
                return new FailurePattern
                {
                    Type = "IntermittentFailure",
                    Description = $"Intermittent failure: {group.Key}",
                    Severity = "Medium",
                    Recommendation = "Investigate timing and external dependencies"
                };
            }
        }
        
        return null;
    }
    
    private FailurePattern? DetectTimeoutPatterns(List<LogEntry> errors)
    {
        var timeoutErrors = errors
            .Where(e => e.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase))
            .ToList();
        
        if (timeoutErrors.Count > 5)
        {
            return new FailurePattern
            {
                Type = "TimeoutPattern",
                Description = $"{timeoutErrors.Count} timeout errors detected",
                Severity = "High",
                Recommendation = "Check network connectivity and external service performance"
            };
        }
        
        return null;
    }
}

public class FailurePattern
{
    public string Type { get; set; }
    public string Description { get; set; }
    public string Severity { get; set; }
    public string Recommendation { get; set; }
}
```

## Best Practices

1. **Always correlate:** Use correlation IDs to track related events
2. **Log context:** Include sufficient context in log entries
3. **Structured data:** Use structured logging for easier analysis
4. **Automate collection:** Automatically collect diagnostics on errors
5. **Monitor trends:** Look for patterns, not just individual events
6. **Document patterns:** Maintain runbooks for common issues
7. **Test diagnostics:** Regularly verify diagnostic tools work correctly
8. **Protect sensitive data:** Sanitize logs and traces

## Summary

Effective diagnostics requires:
- **Comprehensive logging:** Capture all relevant events
- **Distributed tracing:** Track requests across components
- **Log aggregation:** Centralize logs for analysis
- **Systematic approach:** Follow structured troubleshooting workflows
- **Pattern recognition:** Identify common failure modes
- **Proactive monitoring:** Detect issues before they impact users

## See Also

- [Logging Standards](./logging-standards.md)
- [Monitoring Guide](./monitoring-guide.md)
- [Structured Logging Examples](../../examples/logging/structured-logging/)
