# Performance Profiling Guide

## Overview

This guide covers performance profiling tools, techniques, and methodologies for identifying and resolving performance bottlenecks in UniGetUI and .NET 8 Windows applications.

## Table of Contents

1. [Profiling Tools Overview](#profiling-tools-overview)
2. [Visual Studio Profiler](#visual-studio-profiler)
3. [dotTrace and dotMemory](#dottrace-and-dotmemory)
4. [PerfView](#perfview)
5. [Application Insights](#application-insights)
6. [BenchmarkDotNet](#benchmarkdotnet)
7. [Profiling Methodologies](#profiling-methodologies)
8. [Common Performance Patterns](#common-performance-patterns)

---

## Profiling Tools Overview

### Tool Selection Matrix

| Tool | Best For | Cost | Platform | Learning Curve |
|------|----------|------|----------|----------------|
| Visual Studio Profiler | CPU, Memory, Database | Free (VS Community) | Windows | Low |
| dotTrace | CPU, Timeline Analysis | Paid | Windows, macOS, Linux | Medium |
| dotMemory | Memory Leaks, Snapshots | Paid | Windows, macOS, Linux | Medium |
| PerfView | Deep .NET Analysis, GC | Free | Windows | High |
| Application Insights | Production Monitoring | Paid (Free tier) | Cloud-based | Medium |
| BenchmarkDotNet | Micro-benchmarks | Free | Cross-platform | Low |

### When to Use Each Tool

**Visual Studio Profiler:**
- Quick CPU and memory profiling during development
- Debugging performance issues in Visual Studio
- Database query analysis
- First-line performance investigation

**dotTrace/dotMemory:**
- Production profiling
- Complex memory leak investigation
- Timeline-based analysis
- Sampling vs. tracing profiling

**PerfView:**
- GC analysis and tuning
- Allocation analysis
- JIT compilation issues
- Advanced .NET diagnostics

**Application Insights:**
- Production monitoring
- Distributed tracing
- User behavior analysis
- Performance regression detection

**BenchmarkDotNet:**
- Comparing algorithm implementations
- Micro-optimization validation
- Performance regression testing
- Accurate measurement of small operations

---

## Visual Studio Profiler

### Getting Started

#### 1. CPU Usage Profiling

**Steps:**
1. In Visual Studio: Debug → Performance Profiler (Alt+F2)
2. Select "CPU Usage"
3. Click "Start"
4. Exercise the slow code path
5. Click "Stop Collection"
6. Analyze the results

**Reading Results:**
```
Function                          Total CPU    Self CPU    Module
─────────────────────────────────────────────────────────────────
SearchPackagesAsync               2,847ms      15ms        UniGetUI.exe
├─ LoadPackagesFromCLI            2,650ms      1,200ms     UniGetUI.exe
│  ├─ Process.Start               850ms        850ms       System.dll
│  └─ ParseOutput                 600ms        600ms       UniGetUI.exe
└─ FilterResults                  182ms        182ms       UniGetUI.exe
```

**Key Metrics:**
- **Total CPU**: Time including called functions
- **Self CPU**: Time in the function itself
- **Call Count**: Number of times function was called

**Hot Path Indicators:**
- High Self CPU = optimization target
- High Total CPU with low Self CPU = investigate callees
- High call count with moderate CPU = reduce calls or optimize

#### 2. Memory Usage Profiling

**Steps:**
1. Debug → Performance Profiler
2. Select ".NET Object Allocation Tracking"
3. Start profiling
4. Take snapshots at different points
5. Compare snapshots to find leaks

**Key Views:**

**Heap Size View:**
```
Type                              Instances    Size      Delta
─────────────────────────────────────────────────────────────
Package                           12,450       2.1 MB    +1,200
BitmapImage                       3,200        45.3 MB   +450
Dictionary<string, Package>       1            850 KB    +0
```

**Allocation View:**
```
Function                          Allocations    Size
────────────────────────────────────────────────────
LoadPackageIcons                  3,200          45.3 MB
CreatePackageList                 12,450         2.1 MB
```

**Leak Detection:**
1. Take snapshot at baseline
2. Perform operation
3. Force GC (Debug → Windows → Memory → Collect Garbage)
4. Take another snapshot
5. Compare: Objects that remain after GC may indicate a leak

#### 3. Database Profiling (Entity Framework)

**Steps:**
1. Select "Database" profiler
2. Start profiling
3. Review slow queries
4. Analyze query execution plans

**Common Issues:**
- N+1 query problems
- Missing indexes
- Excessive data retrieval
- Inefficient joins

### Example: Profiling Package Search

```csharp
// Before profiling - suspected slow code
public async Task<List<IPackage>> SearchPackagesAsync(string query)
{
    var allPackages = await GetAllPackagesAsync(); // Suspect: loads everything
    
    var results = new List<IPackage>();
    foreach (var package in allPackages)
    {
        if (package.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            results.Add(package);
        }
    }
    
    return results;
}
```

**Profiling reveals:**
- GetAllPackagesAsync: 2,650ms (95% of time)
- String operations: 120ms
- List operations: 30ms

**After optimization:**
```csharp
public async Task<List<IPackage>> SearchPackagesAsync(string query)
{
    // Push search to package manager's native search
    return await _packageManager.SearchAsync(query);
}
```

---

## dotTrace and dotMemory

### dotTrace - CPU Profiling

#### Profiling Modes

**1. Sampling Mode**
- Low overhead (~5%)
- Best for getting overview
- May miss very fast methods
- Good for production

**2. Tracing Mode**
- Higher overhead (~30%)
- Captures all method calls
- Accurate timing
- Use for development

**3. Timeline Mode**
- Shows time-based view
- See threads and their activity
- Identify blocking and contention
- Excellent for async code analysis

#### Example Workflow

**Step 1: Attach to Process**
```bash
# Command line
dotTrace attach <pid> --save-to=profile.dtp --profiling-type=Sampling

# Or use GUI
1. Launch dotTrace
2. Attach to UniGetUI.exe
3. Select Sampling mode
4. Start profiling
```

**Step 2: Analyze Call Tree**
```
Call Tree View:
┌─ SearchPackagesAsync (2,847ms, 100%)
│  ├─ GetInstalledPackagesAsync (1,850ms, 65%)
│  │  ├─ Process.Start (850ms, 30%)
│  │  └─ ParseJsonOutput (1,000ms, 35%)
│  └─ FilterByQuery (997ms, 35%)
```

**Step 3: Hot Spots View**
```
Method                           Time      % Time
───────────────────────────────────────────────
ParseJsonOutput                  1,000ms   35%
Process.Start                    850ms     30%
FilterByQuery                    997ms     35%
```

**Step 4: Optimize**

### dotMemory - Memory Profiling

#### Taking Snapshots

```csharp
// Programmatic snapshots
using JetBrains.Profiler.Api;

[Test]
public void TestMemoryUsage()
{
    MemoryProfiler.CollectAllocations(true);
    MemoryProfiler.GetSnapshot("Before");
    
    // Code to test
    for (int i = 0; i < 1000; i++)
    {
        LoadPackages();
    }
    
    MemoryProfiler.GetSnapshot("After");
}
```

#### Analyzing Snapshots

**1. Object Retention:**
```
Package instances: 12,450
├─ Held by: PackageCache (static)
│  └─ Dictionary entry
├─ Held by: PackageListView._items
│  └─ ObservableCollection
└─ Held by: SearchResults._packages
   └─ List<T>
```

**2. Comparing Snapshots:**
```
Object Type              Snapshot 1    Snapshot 2    Delta
──────────────────────────────────────────────────────────
Package                  10,000        12,450        +2,450
BitmapImage              2,000         3,200         +1,200
EventHandler             150           450           +300 ⚠️
```

⚠️ Growing EventHandler count indicates potential memory leak

#### Memory Traffic Analysis

```
Type                     Allocations    Total Size    Avg Lifetime
───────────────────────────────────────────────────────────────────
byte[]                   450,000        2.3 GB        Short
string                   125,000        150 MB        Mixed
Package                  12,450         2.1 MB        Long
```

High allocation count with short lifetime = GC pressure

---

## PerfView

### Getting Started

PerfView is a powerful tool for advanced .NET performance analysis.

#### Basic Usage

**1. Collect Data:**
```bash
# Collect CPU samples
PerfView collect -DataFile:trace.etl -MaxCollectSec:30

# Collect with GC events
PerfView collect -GCCollectOnly -DataFile:gc.etl
```

**2. In Application:**
```bash
# Start PerfView
PerfView.exe

# Start Collection
1. Click "Collect" → "Collect"
2. Configure collection
3. Start UniGetUI.exe
4. Exercise scenario
5. Stop collection
```

#### Analyzing CPU Performance

**1. CPU Stacks View:**
```
Exclusive %    Inclusive %    Name
───────────────────────────────────────────────
35.2%          35.2%         System.Text.Json.JsonSerializer.Deserialize
30.5%          88.3%         UniGetUI.PackageEngine.LoadPackagesAsync
25.1%          25.1%         System.Diagnostics.Process.WaitForExit
```

**2. Flame Graph:**
```
                LoadPackagesAsync (100%)
        ┌────────────┴───────────────────┐
  ExecuteCommand        ParseJsonOutput
    (45%)                   (55%)
    ┌──┴──┐              ┌────┴─────┐
Process.Start  WaitFor  JsonSerializer.Deserialize
  (30%)        (15%)      (35%)       (20%)
```

**Understanding the Graph:**
- Width = time spent
- Height = call depth
- Wide boxes = optimization opportunities

#### GC Analysis

**Key Metrics:**

**1. GC Stats View:**
```
Generation    Collections    Total Pause    Max Pause
────────────────────────────────────────────────────
Gen 0         450           250ms          12ms
Gen 1         45            180ms          25ms
Gen 2         5             520ms          125ms
```

**2. Allocation Analysis:**
```
Type                Size (MB)    % of Total    Gen 2 %
───────────────────────────────────────────────────
byte[]              856          45%           35%
string              342          18%           20%
Package             125          7%            65%
```

High Gen 2 % = long-lived objects

**3. GC Heap Stats:**
```
Metric                  Value
────────────────────────────
Gen 0 Size              15 MB
Gen 1 Size              8 MB
Gen 2 Size              145 MB
LOH Size                67 MB
Total                   235 MB
```

Large LOH = potential fragmentation issue

#### Identifying Allocation Hot Paths

```bash
# In PerfView
1. Open .GCStats.html
2. View "Allocated Bytes by Type"
3. Click on type (e.g., "byte[]")
4. View "Allocating Call Stacks"
```

**Example Output:**
```
byte[] allocations: 856 MB
├─ ReadFileAsync → 450 MB
│  └─ FileStream.Read → 450 MB
├─ DownloadPackageAsync → 306 MB
│  └─ HttpClient.GetByteArrayAsync → 306 MB
└─ Others → 100 MB
```

#### Using PerfView Programmatically

```csharp
// Trigger ETW events for PerfView
using System.Diagnostics.Tracing;

[EventSource(Name = "UniGetUI-Performance")]
public sealed class PerformanceEventSource : EventSource
{
    public static PerformanceEventSource Log = new();
    
    [Event(1, Level = EventLevel.Informational)]
    public void PackageSearchStart(string query)
    {
        WriteEvent(1, query);
    }
    
    [Event(2, Level = EventLevel.Informational)]
    public void PackageSearchEnd(string query, int resultCount, long durationMs)
    {
        WriteEvent(2, query, resultCount, durationMs);
    }
}

// Usage
public async Task<List<IPackage>> SearchPackagesAsync(string query)
{
    PerformanceEventSource.Log.PackageSearchStart(query);
    var sw = Stopwatch.StartNew();
    
    var results = await PerformSearchAsync(query);
    
    sw.Stop();
    PerformanceEventSource.Log.PackageSearchEnd(query, results.Count, sw.ElapsedMilliseconds);
    
    return results;
}
```

View events in PerfView: Events → Filter → "UniGetUI-Performance"

---

## Application Insights

### Integration Setup

#### 1. Install Package

```xml
<!-- In your .csproj -->
<PackageReference Include="Microsoft.ApplicationInsights.WindowsDesktop" Version="2.22.0" />
```

#### 2. Initialize

```csharp
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

public partial class App : Application
{
    private TelemetryClient? _telemetryClient;
    
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Initialize Application Insights
        var config = TelemetryConfiguration.CreateDefault();
        config.InstrumentationKey = "YOUR_INSTRUMENTATION_KEY";
        
        _telemetryClient = new TelemetryClient(config);
        _telemetryClient.Context.Component.Version = GetVersion();
        
        // Rest of initialization
        InitializeWindow();
    }
}
```

#### 3. Track Operations

```csharp
public class PackageSearchService
{
    private readonly TelemetryClient _telemetry;
    
    public async Task<List<IPackage>> SearchPackagesAsync(string query)
    {
        using var operation = _telemetry.StartOperation<RequestTelemetry>("SearchPackages");
        operation.Telemetry.Properties["query"] = query;
        
        try
        {
            var sw = Stopwatch.StartNew();
            var results = await PerformSearchAsync(query);
            sw.Stop();
            
            operation.Telemetry.Properties["resultCount"] = results.Count.ToString();
            operation.Telemetry.Properties["duration"] = sw.ElapsedMilliseconds.ToString();
            operation.Telemetry.Success = true;
            
            // Track metrics
            _telemetry.TrackMetric("SearchDuration", sw.ElapsedMilliseconds);
            _telemetry.TrackMetric("SearchResults", results.Count);
            
            return results;
        }
        catch (Exception ex)
        {
            operation.Telemetry.Success = false;
            _telemetry.TrackException(ex);
            throw;
        }
    }
}
```

#### 4. Custom Metrics

```csharp
public class PerformanceTracker
{
    private readonly TelemetryClient _telemetry;
    
    public void TrackPackageInstallation(Package package, TimeSpan duration, bool success)
    {
        var properties = new Dictionary<string, string>
        {
            ["PackageId"] = package.Id,
            ["PackageManager"] = package.Manager.Name,
            ["Success"] = success.ToString()
        };
        
        var metrics = new Dictionary<string, double>
        {
            ["Duration"] = duration.TotalSeconds
        };
        
        _telemetry.TrackEvent("PackageInstalled", properties, metrics);
    }
    
    public void TrackMemoryUsage()
    {
        var memoryMB = GC.GetTotalMemory(false) / 1024.0 / 1024.0;
        _telemetry.TrackMetric("MemoryUsage", memoryMB);
    }
}
```

### Querying Application Insights

**KQL Queries:**

**1. Average Search Duration:**
```kql
customEvents
| where name == "SearchPackages"
| extend duration = todouble(customMeasurements.SearchDuration)
| summarize avg(duration), percentile(duration, 95) by bin(timestamp, 1h)
```

**2. Failed Operations:**
```kql
requests
| where success == false
| summarize count() by operation_Name, resultCode
| order by count_ desc
```

**3. Performance Trends:**
```kql
customMetrics
| where name == "SearchDuration"
| summarize avg(value), percentile(value, 95) by bin(timestamp, 1d)
| render timechart
```

---

## BenchmarkDotNet

### Creating Benchmarks

#### 1. Setup

```xml
<PackageReference Include="BenchmarkDotNet" Version="0.13.12" />
```

#### 2. Simple Benchmark

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 5)]
public class PackageSearchBenchmark
{
    private List<Package> _packages;
    private const string SearchQuery = "python";
    
    [GlobalSetup]
    public void Setup()
    {
        _packages = GenerateTestPackages(10000);
    }
    
    [Benchmark(Baseline = true)]
    public List<Package> LinearSearch()
    {
        return _packages
            .Where(p => p.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
    
    [Benchmark]
    public List<Package> ParallelSearch()
    {
        return _packages
            .AsParallel()
            .Where(p => p.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
    
    [Benchmark]
    public List<Package> OptimizedSearch()
    {
        var results = new List<Package>(_packages.Count / 10);
        var querySpan = SearchQuery.AsSpan();
        
        foreach (var package in _packages)
        {
            if (package.Name.AsSpan().Contains(querySpan, StringComparison.OrdinalIgnoreCase))
            {
                results.Add(package);
            }
        }
        
        return results;
    }
}

// Run benchmarks
public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<PackageSearchBenchmark>();
    }
}
```

#### 3. Reading Results

```
|           Method |      Mean |    Error |   StdDev | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
|----------------- |----------:|---------:|---------:|------:|-------:|-------:|----------:|------------:|
|     LinearSearch | 1.234 ms  | 0.024 ms | 0.021 ms |  1.00 | 31.250 | 15.625 |   256 KB  |        1.00 |
|   ParallelSearch | 0.456 ms  | 0.009 ms | 0.008 ms |  0.37 | 62.500 | 31.250 |   512 KB  |        2.00 |
| OptimizedSearch  | 0.789 ms  | 0.015 ms | 0.013 ms |  0.64 | 15.625 |  7.812 |   128 KB  |        0.50 |
```

**Analysis:**
- **OptimizedSearch** is 56% faster than baseline
- Uses 50% less memory
- ParallelSearch is fastest but uses 2x memory (not worth it for this size)

#### 4. Advanced Benchmarking

```csharp
[MemoryDiagnoser]
[ThreadingDiagnoser]
[ExceptionDiagnoser]
public class TaskRecyclerBenchmark
{
    private const int Iterations = 1000;
    
    [Benchmark]
    public async Task WithoutTaskRecycler()
    {
        var tasks = new Task<int>[Iterations];
        for (int i = 0; i < Iterations; i++)
        {
            tasks[i] = Task.Run(() => ExpensiveOperation());
        }
        await Task.WhenAll(tasks);
    }
    
    [Benchmark]
    public async Task WithTaskRecycler()
    {
        var tasks = new Task<int>[Iterations];
        for (int i = 0; i < Iterations; i++)
        {
            tasks[i] = TaskRecycler<int>.RunOrAttachAsync(ExpensiveOperation);
        }
        await Task.WhenAll(tasks);
    }
    
    private int ExpensiveOperation()
    {
        Thread.Sleep(10);
        return 42;
    }
}
```

---

## Profiling Methodologies

### 1. Top-Down Profiling

**Approach:** Start with the biggest time consumers

```
1. Profile entire application
2. Identify methods with highest inclusive time
3. Drill down into those methods
4. Continue until you find the actual bottleneck
```

**Best for:** Finding obvious performance issues

### 2. Bottom-Up Profiling

**Approach:** Find hot spots (highest self time)

```
1. Sort by "Self Time" or "Exclusive Time"
2. Optimize highest self-time methods first
3. Re-profile to measure improvement
4. Repeat
```

**Best for:** CPU-bound performance issues

### 3. Comparative Profiling

**Approach:** Compare before/after optimization

```
1. Profile baseline
2. Make optimization
3. Profile again
4. Compare results
5. Verify improvement
```

**Best for:** Validating optimizations

### 4. Production Profiling

**Approach:** Profile in production environment

```
1. Use lightweight sampling profiler
2. Collect data over time
3. Analyze aggregated results
4. Identify patterns
```

**Best for:** Real-world performance issues

---

## Common Performance Patterns

### Pattern 1: N+1 Query Problem

**Symptom:** Many small database/API calls instead of one large call

**Detection:**
```
GetPackageDetails called 1,000 times
├─ ExecuteQuery: 0.5ms × 1,000 = 500ms
```

**Solution:**
```csharp
// Bad: N+1 queries
foreach (var packageId in packageIds)
{
    var details = await GetPackageDetailsAsync(packageId);
}

// Good: Single batch query
var allDetails = await GetPackageDetailsBatchAsync(packageIds);
```

### Pattern 2: Excessive Allocations

**Symptom:** High Gen 0/1 collection count, GC pauses

**Detection:**
```
Gen 0: 5,000 collections
byte[] allocations: 2.5 GB
```

**Solution:**
```csharp
// Use ArrayPool, Span<T>, StringBuilder, object pooling
```

### Pattern 3: Lock Contention

**Symptom:** Threads waiting, low CPU usage

**Detection:**
```
Thread 1: Waiting on lock (1,500ms)
Thread 2: Waiting on lock (1,200ms)
Thread 3: Has lock (50ms of work)
```

**Solution:**
```csharp
// Use concurrent collections, reduce lock scope, use finer-grained locks
```

### Pattern 4: Blocking Async Code

**Symptom:** Thread pool starvation, deadlocks

**Detection:**
```
Task.Result called
├─ Thread blocked: 2,000ms
```

**Solution:**
```csharp
// Use await instead of .Result
var result = await GetDataAsync(); // Not: var result = GetDataAsync().Result;
```

---

## Profiling Checklist

Before deploying performance improvements:

- [ ] Profile baseline performance
- [ ] Identify specific bottlenecks (not guesses)
- [ ] Make targeted optimizations
- [ ] Profile again to verify improvement
- [ ] Check for memory regressions
- [ ] Test under realistic load
- [ ] Monitor in production (if applicable)
- [ ] Document performance characteristics

---

## See Also

- [Performance Optimization Guide](./optimization-guide.md)
- [Memory Management Guide](./memory-management.md)
- [Monitoring Strategy](./monitoring-strategy.md)
