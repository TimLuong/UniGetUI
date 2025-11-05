# Monitoring Guide for Windows Applications

## Overview

This guide provides comprehensive standards and best practices for monitoring Windows applications, including metrics collection, application health checks, performance monitoring, and alerting strategies.

## Table of Contents

1. [Monitoring Fundamentals](#monitoring-fundamentals)
2. [Application Health Checks](#application-health-checks)
3. [Metrics Collection](#metrics-collection)
4. [Performance Counters](#performance-counters)
5. [Custom Metrics](#custom-metrics)
6. [Monitoring Tools and Platforms](#monitoring-tools-and-platforms)
7. [Alerting Strategies](#alerting-strategies)
8. [Dashboard Design](#dashboard-design)

## Monitoring Fundamentals

### The Four Golden Signals

Monitor these key signals for effective observability:

1. **Latency:** Time taken to service a request
2. **Traffic:** Demand on your system
3. **Errors:** Rate of failed requests
4. **Saturation:** How "full" your service is

### Key Metrics Categories

#### Application Metrics
- Request rate and duration
- Error rates and types
- Success rates
- Active operations

#### System Metrics
- CPU usage
- Memory consumption
- Disk I/O
- Network I/O

#### Business Metrics
- User activity
- Feature usage
- Transaction volumes
- Conversion rates

## Application Health Checks

### Health Check Implementation

Health checks verify that an application and its dependencies are functioning correctly.

#### Basic Health Check Pattern

```csharp
public interface IHealthCheck
{
    Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default);
}

public class HealthCheckResult
{
    public HealthStatus Status { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
    public TimeSpan Duration { get; set; }
}

public enum HealthStatus
{
    Healthy,
    Degraded,
    Unhealthy
}
```

#### Database Health Check

```csharp
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IDbConnection _connection;
    private readonly ILogger<DatabaseHealthCheck> _logger;
    
    public DatabaseHealthCheck(IDbConnection connection, ILogger<DatabaseHealthCheck> logger)
    {
        _connection = connection;
        _logger = logger;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            await _connection.OpenAsync(cancellationToken);
            await _connection.ExecuteScalarAsync<int>("SELECT 1", cancellationToken);
            
            stopwatch.Stop();
            
            return new HealthCheckResult
            {
                Status = HealthStatus.Healthy,
                Description = "Database is accessible",
                Duration = stopwatch.Elapsed,
                Data = { ["ResponseTime"] = stopwatch.ElapsedMilliseconds }
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Database health check failed");
            
            return new HealthCheckResult
            {
                Status = HealthStatus.Unhealthy,
                Description = $"Database is not accessible: {ex.Message}",
                Duration = stopwatch.Elapsed,
                Data = { ["Error"] = ex.Message }
            };
        }
        finally
        {
            if (_connection.State == ConnectionState.Open)
            {
                await _connection.CloseAsync();
            }
        }
    }
}
```

#### File System Health Check

```csharp
public class FileSystemHealthCheck : IHealthCheck
{
    private readonly string _path;
    private readonly ILogger<FileSystemHealthCheck> _logger;
    
    public FileSystemHealthCheck(string path, ILogger<FileSystemHealthCheck> logger)
    {
        _path = path;
        _logger = logger;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var driveInfo = new DriveInfo(Path.GetPathRoot(_path));
            var freeSpacePercent = (driveInfo.AvailableFreeSpace * 100.0) / driveInfo.TotalSize;
            
            stopwatch.Stop();
            
            var status = freeSpacePercent < 10 ? HealthStatus.Unhealthy :
                        freeSpacePercent < 20 ? HealthStatus.Degraded :
                        HealthStatus.Healthy;
            
            return new HealthCheckResult
            {
                Status = status,
                Description = $"File system has {freeSpacePercent:F1}% free space",
                Duration = stopwatch.Elapsed,
                Data =
                {
                    ["FreeSpacePercent"] = freeSpacePercent,
                    ["FreeSpaceGB"] = driveInfo.AvailableFreeSpace / (1024.0 * 1024 * 1024),
                    ["TotalSpaceGB"] = driveInfo.TotalSize / (1024.0 * 1024 * 1024)
                }
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "File system health check failed");
            
            return new HealthCheckResult
            {
                Status = HealthStatus.Unhealthy,
                Description = $"File system check failed: {ex.Message}",
                Duration = stopwatch.Elapsed,
                Data = { ["Error"] = ex.Message }
            };
        }
    }
}
```

#### External Service Health Check

```csharp
public class ExternalServiceHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly string _serviceUrl;
    private readonly ILogger<ExternalServiceHealthCheck> _logger;
    
    public ExternalServiceHealthCheck(
        HttpClient httpClient, 
        string serviceUrl, 
        ILogger<ExternalServiceHealthCheck> logger)
    {
        _httpClient = httpClient;
        _serviceUrl = serviceUrl;
        _logger = logger;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var response = await _httpClient.GetAsync(_serviceUrl, cancellationToken);
            stopwatch.Stop();
            
            var status = response.IsSuccessStatusCode ? HealthStatus.Healthy : HealthStatus.Unhealthy;
            
            return new HealthCheckResult
            {
                Status = status,
                Description = $"Service returned {response.StatusCode}",
                Duration = stopwatch.Elapsed,
                Data =
                {
                    ["StatusCode"] = (int)response.StatusCode,
                    ["ResponseTime"] = stopwatch.ElapsedMilliseconds
                }
            };
        }
        catch (TaskCanceledException)
        {
            stopwatch.Stop();
            _logger.LogWarning("External service health check timed out");
            
            return new HealthCheckResult
            {
                Status = HealthStatus.Unhealthy,
                Description = "Service health check timed out",
                Duration = stopwatch.Elapsed,
                Data = { ["Timeout"] = true }
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "External service health check failed");
            
            return new HealthCheckResult
            {
                Status = HealthStatus.Unhealthy,
                Description = $"Service is not accessible: {ex.Message}",
                Duration = stopwatch.Elapsed,
                Data = { ["Error"] = ex.Message }
            };
        }
    }
}
```

#### Health Check Coordinator

```csharp
public class HealthCheckService
{
    private readonly IEnumerable<IHealthCheck> _healthChecks;
    private readonly ILogger<HealthCheckService> _logger;
    
    public HealthCheckService(
        IEnumerable<IHealthCheck> healthChecks,
        ILogger<HealthCheckService> logger)
    {
        _healthChecks = healthChecks;
        _logger = logger;
    }
    
    public async Task<HealthReport> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<string, HealthCheckResult>();
        var stopwatch = Stopwatch.StartNew();
        
        foreach (var healthCheck in _healthChecks)
        {
            var checkName = healthCheck.GetType().Name;
            
            try
            {
                var result = await healthCheck.CheckHealthAsync(cancellationToken);
                results[checkName] = result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check {CheckName} threw an exception", checkName);
                results[checkName] = new HealthCheckResult
                {
                    Status = HealthStatus.Unhealthy,
                    Description = $"Health check failed with exception: {ex.Message}",
                    Data = { ["Exception"] = ex.ToString() }
                };
            }
        }
        
        stopwatch.Stop();
        
        var overallStatus = results.Values.Any(r => r.Status == HealthStatus.Unhealthy)
            ? HealthStatus.Unhealthy
            : results.Values.Any(r => r.Status == HealthStatus.Degraded)
                ? HealthStatus.Degraded
                : HealthStatus.Healthy;
        
        return new HealthReport
        {
            Status = overallStatus,
            Results = results,
            TotalDuration = stopwatch.Elapsed,
            Timestamp = DateTime.UtcNow
        };
    }
}

public class HealthReport
{
    public HealthStatus Status { get; set; }
    public Dictionary<string, HealthCheckResult> Results { get; set; } = new();
    public TimeSpan TotalDuration { get; set; }
    public DateTime Timestamp { get; set; }
}
```

### Health Check Best Practices

1. **Keep checks fast:** Health checks should complete in < 5 seconds
2. **Use timeouts:** Prevent hanging checks with appropriate timeouts
3. **Check dependencies:** Verify all critical external dependencies
4. **Return details:** Include diagnostic information in results
5. **Schedule regularly:** Run health checks at appropriate intervals (30s - 5min)
6. **Don't affect performance:** Health checks should not impact application performance

## Metrics Collection

### Metrics Types

#### Counter
Tracks cumulative values that only increase:
```csharp
public class MetricsCollector
{
    private long _requestCount = 0;
    private long _errorCount = 0;
    
    public void IncrementRequestCount() => Interlocked.Increment(ref _requestCount);
    public void IncrementErrorCount() => Interlocked.Increment(ref _errorCount);
    
    public long RequestCount => _requestCount;
    public long ErrorCount => _errorCount;
}
```

#### Gauge
Tracks values that can increase or decrease:
```csharp
public class GaugeMetrics
{
    private long _activeConnections = 0;
    private long _queueSize = 0;
    
    public void IncrementActiveConnections() => Interlocked.Increment(ref _activeConnections);
    public void DecrementActiveConnections() => Interlocked.Decrement(ref _activeConnections);
    
    public long ActiveConnections => _activeConnections;
}
```

#### Histogram
Tracks distribution of values:
```csharp
public class HistogramMetric
{
    private readonly ConcurrentBag<double> _observations = new();
    
    public void Observe(double value)
    {
        _observations.Add(value);
    }
    
    public HistogramSnapshot GetSnapshot()
    {
        var sorted = _observations.OrderBy(x => x).ToArray();
        return new HistogramSnapshot
        {
            Count = sorted.Length,
            Min = sorted.FirstOrDefault(),
            Max = sorted.LastOrDefault(),
            Mean = sorted.Any() ? sorted.Average() : 0,
            P50 = GetPercentile(sorted, 0.50),
            P95 = GetPercentile(sorted, 0.95),
            P99 = GetPercentile(sorted, 0.99)
        };
    }
    
    private static double GetPercentile(double[] sorted, double percentile)
    {
        if (sorted.Length == 0) return 0;
        
        int index = (int)Math.Ceiling(percentile * sorted.Length) - 1;
        return sorted[Math.Max(0, index)];
    }
}
```

#### Timer/Duration
Tracks timing information:
```csharp
public class TimerMetric
{
    private readonly HistogramMetric _histogram = new();
    
    public IDisposable Time()
    {
        var stopwatch = Stopwatch.StartNew();
        return new Timer(stopwatch, this);
    }
    
    private void Record(TimeSpan duration)
    {
        _histogram.Observe(duration.TotalMilliseconds);
    }
    
    private class Timer : IDisposable
    {
        private readonly Stopwatch _stopwatch;
        private readonly TimerMetric _metric;
        
        public Timer(Stopwatch stopwatch, TimerMetric metric)
        {
            _stopwatch = stopwatch;
            _metric = metric;
        }
        
        public void Dispose()
        {
            _stopwatch.Stop();
            _metric.Record(_stopwatch.Elapsed);
        }
    }
}

// Usage
using (metrics.PackageInstallDuration.Time())
{
    await InstallPackageAsync(packageId);
}
```

### Application Metrics

Define key application metrics:

```csharp
public class ApplicationMetrics
{
    // Request metrics
    public Counter TotalRequests { get; } = new Counter();
    public Counter SuccessfulRequests { get; } = new Counter();
    public Counter FailedRequests { get; } = new Counter();
    
    // Operation metrics
    public Counter PackageInstalls { get; } = new Counter();
    public Counter PackageUpdates { get; } = new Counter();
    public Counter PackageUninstalls { get; } = new Counter();
    
    // Performance metrics
    public TimerMetric RequestDuration { get; } = new TimerMetric();
    public TimerMetric PackageInstallDuration { get; } = new TimerMetric();
    
    // Resource metrics
    public Gauge ActiveOperations { get; } = new Gauge();
    public Gauge QueuedOperations { get; } = new Gauge();
    
    // Error metrics
    public Counter DatabaseErrors { get; } = new Counter();
    public Counter NetworkErrors { get; } = new Counter();
    public Counter ValidationErrors { get; } = new Counter();
    
    // Business metrics
    public Counter UniqueUsers { get; } = new Counter();
    public Counter PackageSearches { get; } = new Counter();
}
```

### Metrics Collection Patterns

#### Decorator Pattern

```csharp
public interface IPackageService
{
    Task<PackageResult> InstallPackageAsync(string packageId);
}

public class MetricsPackageServiceDecorator : IPackageService
{
    private readonly IPackageService _inner;
    private readonly ApplicationMetrics _metrics;
    private readonly ILogger _logger;
    
    public MetricsPackageServiceDecorator(
        IPackageService inner,
        ApplicationMetrics metrics,
        ILogger<MetricsPackageServiceDecorator> logger)
    {
        _inner = inner;
        _metrics = metrics;
        _logger = logger;
    }
    
    public async Task<PackageResult> InstallPackageAsync(string packageId)
    {
        _metrics.PackageInstalls.Increment();
        _metrics.ActiveOperations.Increment();
        
        using (_metrics.PackageInstallDuration.Time())
        {
            try
            {
                var result = await _inner.InstallPackageAsync(packageId);
                
                if (result.Success)
                {
                    _metrics.SuccessfulRequests.Increment();
                }
                else
                {
                    _metrics.FailedRequests.Increment();
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _metrics.FailedRequests.Increment();
                _logger.LogError(ex, "Package installation failed for {PackageId}", packageId);
                throw;
            }
            finally
            {
                _metrics.ActiveOperations.Decrement();
            }
        }
    }
}
```

#### Middleware Pattern (for HTTP)

```csharp
public class MetricsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ApplicationMetrics _metrics;
    
    public MetricsMiddleware(RequestDelegate next, ApplicationMetrics metrics)
    {
        _next = next;
        _metrics = metrics;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        _metrics.TotalRequests.Increment();
        
        using (_metrics.RequestDuration.Time())
        {
            try
            {
                await _next(context);
                
                if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
                {
                    _metrics.SuccessfulRequests.Increment();
                }
                else if (context.Response.StatusCode >= 400)
                {
                    _metrics.FailedRequests.Increment();
                }
            }
            catch
            {
                _metrics.FailedRequests.Increment();
                throw;
            }
        }
    }
}
```

## Performance Counters

### Windows Performance Counters

#### Reading Performance Counters

```csharp
public class PerformanceMetricsCollector : IDisposable
{
    private readonly PerformanceCounter _cpuCounter;
    private readonly PerformanceCounter _memoryCounter;
    private readonly PerformanceCounter _diskCounter;
    private readonly Timer _timer;
    
    public PerformanceMetricsCollector()
    {
        _cpuCounter = new PerformanceCounter(
            "Processor", 
            "% Processor Time", 
            "_Total");
        
        _memoryCounter = new PerformanceCounter(
            "Memory", 
            "Available MBytes");
        
        _diskCounter = new PerformanceCounter(
            "PhysicalDisk", 
            "% Disk Time", 
            "_Total");
        
        _timer = new Timer(CollectMetrics, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
    }
    
    private void CollectMetrics(object? state)
    {
        try
        {
            var cpu = _cpuCounter.NextValue();
            var memory = _memoryCounter.NextValue();
            var disk = _diskCounter.NextValue();
            
            // Record metrics
            Logger.Information("Performance Metrics: CPU={CPU}%, Memory={Memory}MB, Disk={Disk}%",
                cpu, memory, disk);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to collect performance metrics");
        }
    }
    
    public void Dispose()
    {
        _timer?.Dispose();
        _cpuCounter?.Dispose();
        _memoryCounter?.Dispose();
        _diskCounter?.Dispose();
    }
}
```

#### Creating Custom Performance Counters

```csharp
public class CustomPerformanceCounters
{
    private const string CategoryName = "UniGetUI";
    
    public static void CreateCounters()
    {
        if (!PerformanceCounterCategory.Exists(CategoryName))
        {
            var counters = new CounterCreationDataCollection
            {
                new CounterCreationData
                {
                    CounterName = "Packages Installed/sec",
                    CounterType = PerformanceCounterType.RateOfCountsPerSecond32,
                    CounterHelp = "Number of packages installed per second"
                },
                new CounterCreationData
                {
                    CounterName = "Active Operations",
                    CounterType = PerformanceCounterType.NumberOfItems32,
                    CounterHelp = "Number of currently active operations"
                },
                new CounterCreationData
                {
                    CounterName = "Average Operation Duration",
                    CounterType = PerformanceCounterType.AverageTimer32,
                    CounterHelp = "Average duration of package operations"
                },
                new CounterCreationData
                {
                    CounterName = "Average Operation Duration Base",
                    CounterType = PerformanceCounterType.AverageBase,
                    CounterHelp = "Base counter for average operation duration"
                }
            };
            
            PerformanceCounterCategory.Create(
                CategoryName,
                "UniGetUI Application Counters",
                PerformanceCounterCategoryType.SingleInstance,
                counters);
        }
    }
    
    public PerformanceCounter PackagesInstalledCounter { get; }
    public PerformanceCounter ActiveOperationsCounter { get; }
    public PerformanceCounter AvgDurationCounter { get; }
    public PerformanceCounter AvgDurationBaseCounter { get; }
    
    public CustomPerformanceCounters()
    {
        PackagesInstalledCounter = new PerformanceCounter(
            CategoryName, "Packages Installed/sec", false);
        
        ActiveOperationsCounter = new PerformanceCounter(
            CategoryName, "Active Operations", false);
        
        AvgDurationCounter = new PerformanceCounter(
            CategoryName, "Average Operation Duration", false);
        
        AvgDurationBaseCounter = new PerformanceCounter(
            CategoryName, "Average Operation Duration Base", false);
    }
}
```

### Process Metrics

```csharp
public class ProcessMetrics
{
    private readonly Process _currentProcess;
    
    public ProcessMetrics()
    {
        _currentProcess = Process.GetCurrentProcess();
    }
    
    public ProcessMetricsSnapshot GetSnapshot()
    {
        return new ProcessMetricsSnapshot
        {
            WorkingSet = _currentProcess.WorkingSet64,
            PrivateMemory = _currentProcess.PrivateMemorySize64,
            VirtualMemory = _currentProcess.VirtualMemorySize64,
            ThreadCount = _currentProcess.Threads.Count,
            HandleCount = _currentProcess.HandleCount,
            TotalProcessorTime = _currentProcess.TotalProcessorTime,
            UserProcessorTime = _currentProcess.UserProcessorTime
        };
    }
}

public class ProcessMetricsSnapshot
{
    public long WorkingSet { get; set; }
    public long PrivateMemory { get; set; }
    public long VirtualMemory { get; set; }
    public int ThreadCount { get; set; }
    public int HandleCount { get; set; }
    public TimeSpan TotalProcessorTime { get; set; }
    public TimeSpan UserProcessorTime { get; set; }
}
```

## Custom Metrics

### Business Metrics

Track business-critical operations:

```csharp
public class BusinessMetrics
{
    private readonly ApplicationMetrics _metrics;
    private readonly ILogger _logger;
    
    public void TrackPackageInstallation(string packageId, bool success, TimeSpan duration)
    {
        _metrics.PackageInstalls.Increment();
        
        if (success)
        {
            _logger.Information(
                "Package installation succeeded: {PackageId}, Duration: {Duration}ms",
                packageId, duration.TotalMilliseconds);
        }
        else
        {
            _metrics.FailedRequests.Increment();
            _logger.Warning(
                "Package installation failed: {PackageId}, Duration: {Duration}ms",
                packageId, duration.TotalMilliseconds);
        }
    }
    
    public void TrackUserActivity(string userId, string action)
    {
        _logger.Information(
            "User activity: {UserId}, Action: {Action}",
            userId, action);
    }
    
    public void TrackFeatureUsage(string featureName)
    {
        _logger.Information("Feature used: {FeatureName}", featureName);
    }
}
```

### SLA Metrics

Track Service Level Agreement metrics:

```csharp
public class SlaMetrics
{
    private readonly TimerMetric _responseTime = new();
    private readonly Counter _totalRequests = new();
    private readonly Counter _successfulRequests = new();
    
    public void RecordRequest(TimeSpan duration, bool success)
    {
        _responseTime.Observe(duration.TotalMilliseconds);
        _totalRequests.Increment();
        
        if (success)
        {
            _successfulRequests.Increment();
        }
    }
    
    public SlaReport GenerateReport(TimeSpan period)
    {
        var snapshot = _responseTime.GetSnapshot();
        var successRate = _totalRequests.Value > 0
            ? (_successfulRequests.Value * 100.0) / _totalRequests.Value
            : 0;
        
        return new SlaReport
        {
            Period = period,
            TotalRequests = _totalRequests.Value,
            SuccessRate = successRate,
            AverageResponseTime = snapshot.Mean,
            P95ResponseTime = snapshot.P95,
            P99ResponseTime = snapshot.P99
        };
    }
}

public class SlaReport
{
    public TimeSpan Period { get; set; }
    public long TotalRequests { get; set; }
    public double SuccessRate { get; set; }
    public double AverageResponseTime { get; set; }
    public double P95ResponseTime { get; set; }
    public double P99ResponseTime { get; set; }
    
    public bool MeetsSla(double targetSuccessRate, double targetP95ResponseTime)
    {
        return SuccessRate >= targetSuccessRate && P95ResponseTime <= targetP95ResponseTime;
    }
}
```

## Monitoring Tools and Platforms

### Azure Monitor

```csharp
public class AzureMonitorMetrics
{
    private readonly TelemetryClient _telemetryClient;
    
    public AzureMonitorMetrics(string instrumentationKey)
    {
        var config = new TelemetryConfiguration
        {
            InstrumentationKey = instrumentationKey
        };
        
        _telemetryClient = new TelemetryClient(config);
    }
    
    public void TrackMetric(string name, double value, IDictionary<string, string>? properties = null)
    {
        _telemetryClient.TrackMetric(name, value, properties);
    }
    
    public void TrackEvent(string eventName, IDictionary<string, string>? properties = null)
    {
        _telemetryClient.TrackEvent(eventName, properties);
    }
    
    public void TrackDependency(
        string dependencyName,
        string commandName,
        DateTimeOffset startTime,
        TimeSpan duration,
        bool success)
    {
        _telemetryClient.TrackDependency(
            dependencyName,
            commandName,
            startTime,
            duration,
            success);
    }
}
```

### Prometheus

```csharp
public class PrometheusMetrics
{
    private static readonly Counter RequestCounter = Metrics
        .CreateCounter("app_requests_total", "Total number of requests");
    
    private static readonly Histogram RequestDuration = Metrics
        .CreateHistogram("app_request_duration_seconds", "Request duration in seconds");
    
    private static readonly Gauge ActiveOperations = Metrics
        .CreateGauge("app_active_operations", "Number of active operations");
    
    public void RecordRequest(TimeSpan duration, bool success)
    {
        RequestCounter.Inc();
        RequestDuration.Observe(duration.TotalSeconds);
    }
    
    public IDisposable TrackOperation()
    {
        ActiveOperations.Inc();
        return new OperationTracker(() => ActiveOperations.Dec());
    }
}
```

### Windows Event Log

```csharp
public class EventLogMonitor
{
    private const string SourceName = "UniGetUI";
    private const string LogName = "Application";
    
    public static void Initialize()
    {
        if (!EventLog.SourceExists(SourceName))
        {
            EventLog.CreateEventSource(SourceName, LogName);
        }
    }
    
    public void LogCriticalEvent(string message, int eventId = 1000)
    {
        EventLog.WriteEntry(SourceName, message, EventLogEntryType.Error, eventId);
    }
    
    public void LogWarning(string message, int eventId = 2000)
    {
        EventLog.WriteEntry(SourceName, message, EventLogEntryType.Warning, eventId);
    }
    
    public void LogInformation(string message, int eventId = 3000)
    {
        EventLog.WriteEntry(SourceName, message, EventLogEntryType.Information, eventId);
    }
}
```

## Alerting Strategies

### Alert Definitions

```csharp
public class AlertRule
{
    public string Name { get; set; }
    public string Description { get; set; }
    public AlertSeverity Severity { get; set; }
    public Func<MetricsSnapshot, bool> Condition { get; set; }
    public TimeSpan EvaluationPeriod { get; set; }
    public int ConsecutiveFailures { get; set; }
    public List<string> NotificationChannels { get; set; } = new();
}

public enum AlertSeverity
{
    Info,
    Warning,
    Critical
}

public class AlertingService
{
    private readonly List<AlertRule> _rules = new();
    private readonly Dictionary<string, int> _consecutiveFailures = new();
    private readonly INotificationService _notificationService;
    
    public void AddRule(AlertRule rule)
    {
        _rules.Add(rule);
    }
    
    public async Task EvaluateAlertsAsync(MetricsSnapshot snapshot)
    {
        foreach (var rule in _rules)
        {
            var triggered = rule.Condition(snapshot);
            
            if (triggered)
            {
                _consecutiveFailures.TryGetValue(rule.Name, out var failures);
                _consecutiveFailures[rule.Name] = failures + 1;
                
                if (_consecutiveFailures[rule.Name] >= rule.ConsecutiveFailures)
                {
                    await SendAlertAsync(rule, snapshot);
                }
            }
            else
            {
                _consecutiveFailures[rule.Name] = 0;
            }
        }
    }
    
    private async Task SendAlertAsync(AlertRule rule, MetricsSnapshot snapshot)
    {
        var alert = new Alert
        {
            RuleName = rule.Name,
            Description = rule.Description,
            Severity = rule.Severity,
            Timestamp = DateTime.UtcNow,
            Metrics = snapshot
        };
        
        await _notificationService.SendAlertAsync(alert, rule.NotificationChannels);
    }
}
```

### Common Alert Rules

```csharp
public static class CommonAlertRules
{
    public static AlertRule HighErrorRate => new AlertRule
    {
        Name = "HighErrorRate",
        Description = "Error rate exceeds 5%",
        Severity = AlertSeverity.Critical,
        Condition = snapshot => 
            (snapshot.FailedRequests * 100.0 / snapshot.TotalRequests) > 5.0,
        EvaluationPeriod = TimeSpan.FromMinutes(5),
        ConsecutiveFailures = 2
    };
    
    public static AlertRule SlowResponseTime => new AlertRule
    {
        Name = "SlowResponseTime",
        Description = "P95 response time exceeds 1000ms",
        Severity = AlertSeverity.Warning,
        Condition = snapshot => snapshot.P95ResponseTime > 1000,
        EvaluationPeriod = TimeSpan.FromMinutes(5),
        ConsecutiveFailures = 3
    };
    
    public static AlertRule HighMemoryUsage => new AlertRule
    {
        Name = "HighMemoryUsage",
        Description = "Memory usage exceeds 80%",
        Severity = AlertSeverity.Warning,
        Condition = snapshot => snapshot.MemoryUsagePercent > 80,
        EvaluationPeriod = TimeSpan.FromMinutes(5),
        ConsecutiveFailures = 5
    };
    
    public static AlertRule DiskSpaceLow => new AlertRule
    {
        Name = "DiskSpaceLow",
        Description = "Free disk space below 10%",
        Severity = AlertSeverity.Critical,
        Condition = snapshot => snapshot.DiskFreeSpacePercent < 10,
        EvaluationPeriod = TimeSpan.FromMinutes(15),
        ConsecutiveFailures = 1
    };
}
```

### Notification Channels

```csharp
public interface INotificationService
{
    Task SendAlertAsync(Alert alert, List<string> channels);
}

public class NotificationService : INotificationService
{
    private readonly IEmailService _emailService;
    private readonly ISlackService _slackService;
    private readonly ILogger _logger;
    
    public async Task SendAlertAsync(Alert alert, List<string> channels)
    {
        var tasks = new List<Task>();
        
        foreach (var channel in channels)
        {
            tasks.Add(channel.ToLower() switch
            {
                "email" => _emailService.SendAlertEmailAsync(alert),
                "slack" => _slackService.SendAlertToSlackAsync(alert),
                "eventlog" => Task.Run(() => LogToEventLog(alert)),
                _ => Task.CompletedTask
            });
        }
        
        await Task.WhenAll(tasks);
    }
    
    private void LogToEventLog(Alert alert)
    {
        var eventType = alert.Severity switch
        {
            AlertSeverity.Critical => EventLogEntryType.Error,
            AlertSeverity.Warning => EventLogEntryType.Warning,
            _ => EventLogEntryType.Information
        };
        
        EventLog.WriteEntry("UniGetUI", 
            $"Alert: {alert.RuleName}\n{alert.Description}", 
            eventType);
    }
}
```

## Dashboard Design

### Key Dashboard Principles

1. **At-a-glance visibility:** Most important metrics prominently displayed
2. **Hierarchical information:** Overview → Details → Diagnostics
3. **Actionable insights:** Clear indication of what requires attention
4. **Real-time updates:** Fresh data for current system state
5. **Historical context:** Trends over time for comparison

### Dashboard Components

```csharp
public class DashboardData
{
    // Health Status
    public HealthStatus OverallHealth { get; set; }
    public Dictionary<string, HealthCheckResult> ComponentHealth { get; set; }
    
    // Key Metrics
    public long TotalRequests { get; set; }
    public double RequestsPerSecond { get; set; }
    public double ErrorRate { get; set; }
    public double AverageResponseTime { get; set; }
    public double P95ResponseTime { get; set; }
    
    // Resource Usage
    public double CpuUsagePercent { get; set; }
    public double MemoryUsagePercent { get; set; }
    public double DiskUsagePercent { get; set; }
    
    // Active Operations
    public int ActiveOperations { get; set; }
    public int QueuedOperations { get; set; }
    
    // Recent Errors
    public List<ErrorSummary> RecentErrors { get; set; }
    
    // Performance Trends
    public List<DataPoint> ResponseTimeTrend { get; set; }
    public List<DataPoint> RequestRateTrend { get; set; }
}
```

## Best Practices

1. **Monitor what matters:** Focus on metrics that impact users and business
2. **Set meaningful thresholds:** Based on SLAs and historical data
3. **Reduce noise:** Avoid alert fatigue with appropriate thresholds
4. **Context is key:** Include relevant context in metrics and alerts
5. **Regular reviews:** Periodically review and adjust monitoring strategy
6. **Automate responses:** Where possible, automate remediation actions
7. **Document runbooks:** Provide clear steps for responding to alerts

## Summary

Effective monitoring provides:
- **Visibility:** Understanding of system behavior and health
- **Early warning:** Detection of issues before they impact users
- **Diagnostics:** Information to troubleshoot problems quickly
- **Trends:** Historical data for capacity planning
- **Accountability:** Metrics for SLA compliance

## See Also

- [Logging Standards](./logging-standards.md)
- [Diagnostics Guide](./diagnostics-guide.md)
- [Structured Logging Examples](../../examples/logging/structured-logging/)
