# Health Check Example

This example demonstrates comprehensive application health monitoring with multiple health check implementations.

## Features Demonstrated

- **Health Check Interface**: Generic interface for implementing custom checks
- **Multiple Check Types**: Memory, file system, process, database, and external service checks
- **Health Status Levels**: Healthy, Degraded, and Unhealthy states
- **Aggregated Reports**: Overall health based on individual check results
- **Diagnostic Data**: Additional context included with each check
- **Performance Tracking**: Duration measurement for each check

## Running the Example

```bash
dotnet restore
dotnet run
```

## Health Checks Implemented

### 1. Memory Health Check
Monitors application memory usage:
- **Healthy**: < 500 MB
- **Degraded**: 500-1000 MB
- **Unhealthy**: > 1000 MB

Includes GC collection counts for memory analysis.

### 2. File System Health Check
Monitors disk space availability:
- **Healthy**: > 20% free space
- **Degraded**: 10-20% free space
- **Unhealthy**: < 10% free space

Reports free space in both percentage and GB.

### 3. Process Health Check
Monitors process resource usage:
- Tracks thread count
- Monitors handle count
- Reports working set memory

### 4. Database Health Check (Simulated)
Simulates database connectivity check:
- Connection timeout detection
- Response time tracking
- Connection pool status

### 5. External Service Health Check (Simulated)
Simulates external API health check:
- Service availability
- Response time monitoring
- Degraded performance detection

## Output Example

```
[10:30:45 INF] === Health Check Run 1 ===
[10:30:45 INF] Overall Health: Healthy
[10:30:45 INF] Check Duration: 156ms
[10:30:45 INF]   MemoryHealthCheck: ✓ HEALTHY (2ms) - Memory usage: 45.23 MB
[10:30:45 DBG]     MemoryMB: 45.23
[10:30:45 DBG]     Gen0Collections: 2
[10:30:45 INF]   FileSystemHealthCheck: ✓ HEALTHY (15ms) - Disk C:\: 45.5% free (250.00 GB)
[10:30:45 INF]   ProcessHealthCheck: ✓ HEALTHY (3ms) - Process: 15 threads, 245 handles
[10:30:45 INF]   SimulatedDatabaseHealthCheck: ✓ HEALTHY (45ms) - Database is accessible
[10:30:45 INF]   SimulatedExternalServiceHealthCheck: ⚠ DEGRADED (91ms) - External service responding slowly
```

## Integration Patterns

### Periodic Health Checks

```csharp
var timer = new Timer(async _ =>
{
    var report = await healthCheckService.CheckHealthAsync();
    if (report.Status != HealthStatus.Healthy)
    {
        Log.Warning("Application health degraded: {Status}", report.Status);
        await SendAlertAsync(report);
    }
}, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
```

### Health Endpoint for Monitoring

```csharp
app.MapGet("/health", async (HealthCheckService healthService) =>
{
    var report = await healthService.CheckHealthAsync();
    
    var statusCode = report.Status switch
    {
        HealthStatus.Healthy => 200,
        HealthStatus.Degraded => 200,
        HealthStatus.Unhealthy => 503,
        _ => 500
    };
    
    return Results.Json(report, statusCode: statusCode);
});
```

### Custom Health Checks

Implement `IHealthCheck` for domain-specific checks:

```csharp
public class PackageManagerHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Check if winget is available
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "winget",
                Arguments = "--version",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
            
            await process.WaitForExitAsync(cancellationToken);
            stopwatch.Stop();
            
            if (process.ExitCode == 0)
            {
                return new HealthCheckResult
                {
                    Status = HealthStatus.Healthy,
                    Description = "Package manager is available",
                    Duration = stopwatch.Elapsed
                };
            }
            
            return new HealthCheckResult
            {
                Status = HealthStatus.Unhealthy,
                Description = "Package manager not found",
                Duration = stopwatch.Elapsed
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new HealthCheckResult
            {
                Status = HealthStatus.Unhealthy,
                Description = $"Health check failed: {ex.Message}",
                Duration = stopwatch.Elapsed
            };
        }
    }
}
```

## Best Practices

1. ✅ Keep health checks fast (< 5 seconds)
2. ✅ Use timeouts to prevent hanging
3. ✅ Include diagnostic data in results
4. ✅ Check all critical dependencies
5. ✅ Use three-state status (Healthy/Degraded/Unhealthy)
6. ✅ Run checks periodically, not on every request
7. ✅ Alert on unhealthy status
8. ✅ Log health check failures

## Alerting Integration

Health checks can trigger alerts:

```csharp
if (report.Status == HealthStatus.Unhealthy)
{
    // Send alert via email, Slack, etc.
    await SendCriticalAlert(report);
}
else if (report.Status == HealthStatus.Degraded)
{
    // Send warning
    await SendWarningAlert(report);
}
```

## See Also

- [Monitoring Guide](../../../../docs/observability/monitoring-guide.md)
- [Diagnostics Guide](../../../../docs/observability/diagnostics-guide.md)
