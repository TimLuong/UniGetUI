using System.Collections.Concurrent;
using System.Diagnostics;
using Serilog;

namespace MetricsExample;

class Program
{
    static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            Log.Information("Metrics Collection Example starting");

            var metrics = new ApplicationMetrics();
            
            // Simulate application operations
            await SimulateOperations(metrics);
            
            // Display metrics report
            DisplayMetricsReport(metrics);

            Log.Information("Metrics Collection Example completed");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    static async Task SimulateOperations(ApplicationMetrics metrics)
    {
        var tasks = new List<Task>();
        
        // Simulate multiple concurrent package operations
        for (int i = 0; i < 20; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                await SimulatePackageInstall(metrics);
            }));
        }

        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                await SimulatePackageUpdate(metrics);
            }));
        }

        await Task.WhenAll(tasks);
    }

    static async Task SimulatePackageInstall(ApplicationMetrics metrics)
    {
        metrics.TotalRequests.Increment();
        metrics.PackageInstalls.Increment();
        metrics.ActiveOperations.Increment();

        using (metrics.PackageInstallDuration.Time())
        {
            try
            {
                await Task.Delay(Random.Shared.Next(100, 500));
                
                if (Random.Shared.Next(0, 10) == 0)
                {
                    metrics.FailedRequests.Increment();
                    throw new Exception("Installation failed");
                }
                
                metrics.SuccessfulRequests.Increment();
            }
            catch
            {
                // Exception already counted
            }
            finally
            {
                metrics.ActiveOperations.Decrement();
            }
        }
    }

    static async Task SimulatePackageUpdate(ApplicationMetrics metrics)
    {
        metrics.TotalRequests.Increment();
        metrics.PackageUpdates.Increment();
        metrics.ActiveOperations.Increment();

        using (metrics.PackageInstallDuration.Time())
        {
            try
            {
                await Task.Delay(Random.Shared.Next(200, 600));
                metrics.SuccessfulRequests.Increment();
            }
            finally
            {
                metrics.ActiveOperations.Decrement();
            }
        }
    }

    static void DisplayMetricsReport(ApplicationMetrics metrics)
    {
        Log.Information("=== Metrics Report ===");
        
        Log.Information("Request Metrics:");
        Log.Information("  Total Requests: {Total}", metrics.TotalRequests.Value);
        Log.Information("  Successful: {Success}", metrics.SuccessfulRequests.Value);
        Log.Information("  Failed: {Failed}", metrics.FailedRequests.Value);
        Log.Information("  Success Rate: {Rate:F2}%", 
            metrics.TotalRequests.Value > 0 
                ? (metrics.SuccessfulRequests.Value * 100.0 / metrics.TotalRequests.Value) 
                : 0);

        Log.Information("Operation Metrics:");
        Log.Information("  Package Installs: {Installs}", metrics.PackageInstalls.Value);
        Log.Information("  Package Updates: {Updates}", metrics.PackageUpdates.Value);

        var installStats = metrics.PackageInstallDuration.GetSnapshot();
        Log.Information("Duration Metrics:");
        Log.Information("  Count: {Count}", installStats.Count);
        Log.Information("  Average: {Avg:F2}ms", installStats.Mean);
        Log.Information("  Min: {Min:F2}ms", installStats.Min);
        Log.Information("  Max: {Max:F2}ms", installStats.Max);
        Log.Information("  P50: {P50:F2}ms", installStats.P50);
        Log.Information("  P95: {P95:F2}ms", installStats.P95);
        Log.Information("  P99: {P99:F2}ms", installStats.P99);
    }
}

// Metrics Infrastructure

public class Counter
{
    private long _value;
    public long Value => _value;
    public void Increment() => Interlocked.Increment(ref _value);
    public void Decrement() => Interlocked.Decrement(ref _value);
    public void Add(long amount) => Interlocked.Add(ref _value, amount);
}

public class Gauge
{
    private long _value;
    public long Value => _value;
    public void Set(long value) => Interlocked.Exchange(ref _value, value);
    public void Increment() => Interlocked.Increment(ref _value);
    public void Decrement() => Interlocked.Decrement(ref _value);
}

public class TimerMetric
{
    private readonly HistogramMetric _histogram = new();
    
    public IDisposable Time() => new Timer(Stopwatch.StartNew(), this);
    
    private void Record(TimeSpan duration) => _histogram.Observe(duration.TotalMilliseconds);
    
    public HistogramSnapshot GetSnapshot() => _histogram.GetSnapshot();
    
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

public class HistogramMetric
{
    private readonly ConcurrentBag<double> _observations = new();
    
    public void Observe(double value) => _observations.Add(value);
    
    public HistogramSnapshot GetSnapshot()
    {
        var sorted = _observations.OrderBy(x => x).ToArray();
        return new HistogramSnapshot
        {
            Count = sorted.Length,
            Min = sorted.Length > 0 ? sorted.First() : 0,
            Max = sorted.Length > 0 ? sorted.Last() : 0,
            Mean = sorted.Length > 0 ? sorted.Average() : 0,
            P50 = GetPercentile(sorted, 0.50),
            P95 = GetPercentile(sorted, 0.95),
            P99 = GetPercentile(sorted, 0.99)
        };
    }
    
    private static double GetPercentile(double[] sorted, double percentile)
    {
        if (sorted.Length == 0) return 0;
        int index = (int)Math.Ceiling(percentile * sorted.Length) - 1;
        return sorted[Math.Max(0, Math.Min(index, sorted.Length - 1))];
    }
}

public class HistogramSnapshot
{
    public int Count { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public double Mean { get; set; }
    public double P50 { get; set; }
    public double P95 { get; set; }
    public double P99 { get; set; }
}

public class ApplicationMetrics
{
    public Counter TotalRequests { get; } = new();
    public Counter SuccessfulRequests { get; } = new();
    public Counter FailedRequests { get; } = new();
    
    public Counter PackageInstalls { get; } = new();
    public Counter PackageUpdates { get; } = new();
    public Counter PackageUninstalls { get; } = new();
    
    public TimerMetric PackageInstallDuration { get; } = new();
    
    public Gauge ActiveOperations { get; } = new();
    public Gauge QueuedOperations { get; } = new();
}
