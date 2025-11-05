using System.Diagnostics;
using Serilog;

namespace HealthCheckExample;

/// <summary>
/// Demonstrates application health check implementation
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        // Configure logging
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            Log.Information("Health Check Example starting");

            var healthCheckService = new HealthCheckService();

            // Run health checks periodically
            for (int i = 0; i < 3; i++)
            {
                Log.Information("=== Health Check Run {RunNumber} ===", i + 1);
                
                var report = await healthCheckService.CheckHealthAsync();
                DisplayHealthReport(report);

                if (i < 2)
                {
                    await Task.Delay(5000);
                }
            }

            Log.Information("Health Check Example completed");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    static void DisplayHealthReport(HealthReport report)
    {
        Log.Information("Overall Health: {Status}", report.Status);
        Log.Information("Check Duration: {Duration}ms", report.TotalDuration.TotalMilliseconds);

        foreach (var check in report.Results)
        {
            var status = check.Value.Status switch
            {
                HealthStatus.Healthy => "✓ HEALTHY",
                HealthStatus.Degraded => "⚠ DEGRADED",
                HealthStatus.Unhealthy => "✗ UNHEALTHY",
                _ => "? UNKNOWN"
            };

            Log.Information("  {CheckName}: {Status} ({Duration}ms) - {Description}",
                check.Key,
                status,
                check.Value.Duration.TotalMilliseconds,
                check.Value.Description);

            if (check.Value.Data.Any())
            {
                foreach (var data in check.Value.Data)
                {
                    Log.Debug("    {Key}: {Value}", data.Key, data.Value);
                }
            }
        }
    }
}

// Health Check Infrastructure

public enum HealthStatus
{
    Healthy,
    Degraded,
    Unhealthy
}

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

public class HealthReport
{
    public HealthStatus Status { get; set; }
    public Dictionary<string, HealthCheckResult> Results { get; set; } = new();
    public TimeSpan TotalDuration { get; set; }
    public DateTime Timestamp { get; set; }
}

// Health Check Service

public class HealthCheckService
{
    private readonly IEnumerable<IHealthCheck> _healthChecks;

    public HealthCheckService()
    {
        _healthChecks = new List<IHealthCheck>
        {
            new MemoryHealthCheck(),
            new FileSystemHealthCheck("C:\\"),
            new ProcessHealthCheck(),
            new SimulatedDatabaseHealthCheck(),
            new SimulatedExternalServiceHealthCheck()
        };
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
                Log.Error(ex, "Health check {CheckName} threw an exception", checkName);
                results[checkName] = new HealthCheckResult
                {
                    Status = HealthStatus.Unhealthy,
                    Description = $"Health check failed: {ex.Message}",
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

// Specific Health Checks

public class MemoryHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var totalMemory = GC.GetTotalMemory(false);
            var totalMemoryMB = totalMemory / (1024.0 * 1024.0);

            stopwatch.Stop();

            var status = totalMemoryMB > 500 ? HealthStatus.Degraded :
                        totalMemoryMB > 1000 ? HealthStatus.Unhealthy :
                        HealthStatus.Healthy;

            return Task.FromResult(new HealthCheckResult
            {
                Status = status,
                Description = $"Memory usage: {totalMemoryMB:F2} MB",
                Duration = stopwatch.Elapsed,
                Data =
                {
                    ["MemoryMB"] = totalMemoryMB,
                    ["Gen0Collections"] = GC.CollectionCount(0),
                    ["Gen1Collections"] = GC.CollectionCount(1),
                    ["Gen2Collections"] = GC.CollectionCount(2)
                }
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return Task.FromResult(new HealthCheckResult
            {
                Status = HealthStatus.Unhealthy,
                Description = $"Memory check failed: {ex.Message}",
                Duration = stopwatch.Elapsed
            });
        }
    }
}

public class FileSystemHealthCheck : IHealthCheck
{
    private readonly string _path;

    public FileSystemHealthCheck(string path)
    {
        _path = path;
    }

    public Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var driveInfo = new DriveInfo(Path.GetPathRoot(_path) ?? _path);
            var freeSpacePercent = (driveInfo.AvailableFreeSpace * 100.0) / driveInfo.TotalSize;
            var freeSpaceGB = driveInfo.AvailableFreeSpace / (1024.0 * 1024 * 1024);

            stopwatch.Stop();

            var status = freeSpacePercent < 10 ? HealthStatus.Unhealthy :
                        freeSpacePercent < 20 ? HealthStatus.Degraded :
                        HealthStatus.Healthy;

            return Task.FromResult(new HealthCheckResult
            {
                Status = status,
                Description = $"Disk {driveInfo.Name}: {freeSpacePercent:F1}% free ({freeSpaceGB:F2} GB)",
                Duration = stopwatch.Elapsed,
                Data =
                {
                    ["DriveName"] = driveInfo.Name,
                    ["FreeSpacePercent"] = freeSpacePercent,
                    ["FreeSpaceGB"] = freeSpaceGB,
                    ["TotalSpaceGB"] = driveInfo.TotalSize / (1024.0 * 1024 * 1024)
                }
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return Task.FromResult(new HealthCheckResult
            {
                Status = HealthStatus.Unhealthy,
                Description = $"File system check failed: {ex.Message}",
                Duration = stopwatch.Elapsed
            });
        }
    }
}

public class ProcessHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var process = Process.GetCurrentProcess();
            var threadCount = process.Threads.Count;
            var handleCount = process.HandleCount;

            stopwatch.Stop();

            var status = threadCount > 100 || handleCount > 1000 ? HealthStatus.Degraded :
                        threadCount > 200 || handleCount > 2000 ? HealthStatus.Unhealthy :
                        HealthStatus.Healthy;

            return Task.FromResult(new HealthCheckResult
            {
                Status = status,
                Description = $"Process: {threadCount} threads, {handleCount} handles",
                Duration = stopwatch.Elapsed,
                Data =
                {
                    ["ThreadCount"] = threadCount,
                    ["HandleCount"] = handleCount,
                    ["WorkingSetMB"] = process.WorkingSet64 / (1024.0 * 1024)
                }
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return Task.FromResult(new HealthCheckResult
            {
                Status = HealthStatus.Unhealthy,
                Description = $"Process check failed: {ex.Message}",
                Duration = stopwatch.Elapsed
            });
        }
    }
}

public class SimulatedDatabaseHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Simulate database connection
            await Task.Delay(Random.Shared.Next(10, 100), cancellationToken);

            stopwatch.Stop();

            // Simulate occasional failures
            if (Random.Shared.Next(0, 10) == 0)
            {
                return new HealthCheckResult
                {
                    Status = HealthStatus.Unhealthy,
                    Description = "Database connection timeout",
                    Duration = stopwatch.Elapsed
                };
            }

            return new HealthCheckResult
            {
                Status = HealthStatus.Healthy,
                Description = "Database is accessible",
                Duration = stopwatch.Elapsed,
                Data =
                {
                    ["ResponseTimeMs"] = stopwatch.ElapsedMilliseconds,
                    ["ConnectionPool"] = "Available"
                }
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new HealthCheckResult
            {
                Status = HealthStatus.Unhealthy,
                Description = $"Database check failed: {ex.Message}",
                Duration = stopwatch.Elapsed
            };
        }
    }
}

public class SimulatedExternalServiceHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Simulate external service call
            await Task.Delay(Random.Shared.Next(50, 200), cancellationToken);

            stopwatch.Stop();

            // Simulate degraded service
            if (Random.Shared.Next(0, 5) == 0)
            {
                return new HealthCheckResult
                {
                    Status = HealthStatus.Degraded,
                    Description = "External service responding slowly",
                    Duration = stopwatch.Elapsed,
                    Data = { ["ResponseTimeMs"] = stopwatch.ElapsedMilliseconds }
                };
            }

            return new HealthCheckResult
            {
                Status = HealthStatus.Healthy,
                Description = "External service is accessible",
                Duration = stopwatch.Elapsed,
                Data =
                {
                    ["ResponseTimeMs"] = stopwatch.ElapsedMilliseconds,
                    ["Endpoint"] = "https://api.example.com"
                }
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new HealthCheckResult
            {
                Status = HealthStatus.Unhealthy,
                Description = $"External service check failed: {ex.Message}",
                Duration = stopwatch.Elapsed
            };
        }
    }
}
