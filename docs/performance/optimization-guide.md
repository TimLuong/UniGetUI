# Performance Optimization Guide

## Overview

This guide provides comprehensive performance optimization techniques for UniGetUI and Windows desktop applications built with WinUI 3 and .NET 8. It covers best practices for maximizing application responsiveness, minimizing resource consumption, and delivering a smooth user experience.

## Table of Contents

1. [Async/Await and Parallel Programming](#asyncawait-and-parallel-programming)
2. [UI Thread Optimization](#ui-thread-optimization)
3. [Lazy Loading and Pagination](#lazy-loading-and-pagination)
4. [Application Startup Optimization](#application-startup-optimization)
5. [Task Recycling Pattern](#task-recycling-pattern)
6. [Caching Strategies](#caching-strategies)
7. [Background Processing](#background-processing)
8. [I/O Optimization](#io-optimization)

---

## Async/Await and Parallel Programming

### Best Practices

#### 1. Async All the Way

**DO:** Use async/await throughout the entire call chain
```csharp
// Good: Fully async
public async Task<IEnumerable<IPackage>> GetAvailableUpdatesAsync()
{
    var installed = await GetInstalledPackagesAsync();
    var available = await GetAvailablePackagesAsync();
    return available.Where(p => installed.Contains(p));
}

// Bad: Blocking on async code
public IEnumerable<IPackage> GetAvailableUpdates()
{
    var installed = GetInstalledPackagesAsync().Result; // Deadlock risk!
    return installed;
}
```

**Why:** Blocking on async code with `.Result` or `.Wait()` can cause deadlocks and negatively impacts thread pool efficiency.

#### 2. ConfigureAwait(false) in Library Code

**DO:** Use `ConfigureAwait(false)` for non-UI library code
```csharp
// In library/business logic code
public async Task<string> FetchDataAsync()
{
    var response = await httpClient.GetAsync(url).ConfigureAwait(false);
    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    return content;
}

// In UI code, ConfigureAwait(true) is default and required
private async void Button_Click(object sender, RoutedEventArgs e)
{
    var data = await FetchDataAsync(); // ConfigureAwait(true) implicit
    MyTextBox.Text = data; // Must be on UI thread
}
```

**Why:** `ConfigureAwait(false)` avoids capturing and resuming on the synchronization context, reducing overhead and avoiding potential deadlocks.

**Implementation in UniGetUI:**
```csharp
// From TaskRecycler.cs
private static async Task<ReturnT> _runTaskAndWait(Task<ReturnT> task, int hash, int cacheTimeSecsSecs)
{
    // ... task setup ...
    ReturnT result = await task.ConfigureAwait(false); // Non-UI code
    _removeFromCache(hash, cacheTimeSecsSecs);
    return result;
}
```

#### 3. Parallel Task Execution

**DO:** Execute independent tasks in parallel
```csharp
// Good: Parallel execution
public async Task<(List<IPackage> installed, List<IPackage> updates)> LoadPackageDataAsync()
{
    var installedTask = GetInstalledPackagesAsync();
    var updatesTask = GetAvailableUpdatesAsync();
    
    await Task.WhenAll(installedTask, updatesTask);
    
    return (await installedTask, await updatesTask);
}

// Bad: Sequential execution
public async Task<(List<IPackage> installed, List<IPackage> updates)> LoadPackageDataAsync()
{
    var installed = await GetInstalledPackagesAsync(); // Waits unnecessarily
    var updates = await GetAvailableUpdatesAsync();
    return (installed, updates);
}
```

**Why:** Parallel execution can significantly reduce total wait time when operations are independent.

#### 4. Cancellation Token Support

**DO:** Accept and forward CancellationToken in async methods
```csharp
public async Task<List<IPackage>> SearchPackagesAsync(
    string query, 
    CancellationToken cancellationToken = default)
{
    foreach (var source in sources)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var results = await source.SearchAsync(query, cancellationToken);
        // Process results
    }
    return results;
}
```

**Why:** Enables cancellation of long-running operations, improving responsiveness and resource management.

#### 5. Avoid Task.Run for Short Operations

**DON'T:** Wrap quick operations in Task.Run
```csharp
// Bad: Unnecessary overhead
public Task<int> GetCountAsync()
{
    return Task.Run(() => myList.Count); // Don't do this!
}

// Good: Use ValueTask or just return synchronously
public int GetCount() => myList.Count;
```

**Why:** Task.Run adds overhead. Only use it for CPU-intensive operations that need to run off the UI thread.

#### 6. Use ValueTask for Hot Paths

**DO:** Use ValueTask<T> for frequently-called methods with cached results
```csharp
public ValueTask<Settings> GetSettingsAsync()
{
    if (_cachedSettings != null)
        return new ValueTask<Settings>(_cachedSettings);
    
    return new ValueTask<Settings>(LoadSettingsAsync());
}
```

**Why:** ValueTask avoids allocations when results are immediately available, improving performance in hot paths.

---

## UI Thread Optimization

### Principles

The UI thread (also called the dispatcher thread) must remain responsive. Any operation taking more than 16ms will cause visible UI lag.

### Best Practices

#### 1. Keep UI Thread Free

**DO:** Offload work from the UI thread
```csharp
// Good: Heavy work on background thread
private async void LoadPackages_Click(object sender, RoutedEventArgs e)
{
    LoadingIndicator.IsActive = true;
    
    // Runs on background thread
    var packages = await Task.Run(() => LoadHeavyPackageData());
    
    // Back on UI thread
    PackageList.ItemsSource = packages;
    LoadingIndicator.IsActive = false;
}
```

#### 2. Use Dispatcher for UI Updates from Background Threads

**DO:** Marshal UI updates back to the UI thread
```csharp
// From background thread
await DispatcherQueue.EnqueueAsync(() =>
{
    StatusText.Text = "Package installed successfully";
    ProgressBar.Value = 100;
});
```

**In WinUI 3:**
```csharp
// Get DispatcherQueue from UI element
var dispatcherQueue = PackageList.DispatcherQueue;

await Task.Run(async () =>
{
    var result = DoHeavyWork();
    
    dispatcherQueue.TryEnqueue(() =>
    {
        UpdateUI(result);
    });
});
```

#### 3. Virtualize Large Lists

**DO:** Use virtualization for lists with many items
```xaml
<!-- WinUI 3 - ItemsRepeater with virtualization -->
<ScrollViewer>
    <ItemsRepeater ItemsSource="{x:Bind Packages}">
        <ItemsRepeater.Layout>
            <StackLayout Spacing="8"/>
        </ItemsRepeater.Layout>
        <DataTemplate x:DataType="local:Package">
            <local:PackageCard Package="{x:Bind}"/>
        </DataTemplate>
    </ItemsRepeater>
</ScrollViewer>
```

**Why:** Virtualization only creates UI elements for visible items, dramatically reducing memory and improving performance with large datasets.

#### 4. Defer Expensive UI Creation

**DO:** Load UI elements progressively
```csharp
// Load critical UI first
await LoadMainWindowAsync();

// Defer non-critical UI
_ = Task.Run(async () =>
{
    await Task.Delay(100); // Let main window render first
    await DispatcherQueue.EnqueueAsync(() => LoadSettingsPanel());
});
```

#### 5. Batch UI Updates

**DO:** Batch multiple UI updates together
```csharp
// Bad: Multiple individual updates
foreach (var package in packages)
{
    PackageList.Items.Add(package); // Triggers layout for each item!
}

// Good: Batch update
var tempList = new List<Package>();
foreach (var package in packages)
{
    tempList.Add(package);
}
PackageList.ItemsSource = tempList; // Single layout pass
```

#### 6. Use Incremental Loading for Data-Heavy Operations

**DO:** Implement incremental loading for better perceived performance
```csharp
public class IncrementalPackageCollection : ObservableCollection<IPackage>, ISupportIncrementalLoading
{
    public bool HasMoreItems => _hasMore;

    public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
    {
        return Task.Run(async () =>
        {
            var newItems = await _packageLoader.LoadNextBatchAsync((int)count);
            
            await DispatcherQueue.EnqueueAsync(() =>
            {
                foreach (var item in newItems)
                    Add(item);
            });
            
            return new LoadMoreItemsResult { Count = (uint)newItems.Count };
        }).AsAsyncOperation();
    }
}
```

---

## Lazy Loading and Pagination

### Strategies

#### 1. Lazy Initialization

**DO:** Defer object creation until needed
```csharp
public class PackageManager
{
    private IPackageRepository? _repository;
    
    // Lazy initialization
    private IPackageRepository Repository
    {
        get
        {
            if (_repository == null)
            {
                _repository = new PackageRepository();
            }
            return _repository;
        }
    }
    
    // Or use Lazy<T>
    private readonly Lazy<IPackageRepository> _lazyRepository = 
        new(() => new PackageRepository());
}
```

#### 2. Pagination Pattern

**DO:** Implement server-side pagination for large datasets
```csharp
public class PackageSearchService
{
    private const int PageSize = 50;
    
    public async Task<PagedResult<IPackage>> SearchPackagesAsync(
        string query, 
        int pageNumber,
        CancellationToken cancellationToken = default)
    {
        var skip = pageNumber * PageSize;
        var packages = await _searchProvider
            .SearchAsync(query, skip, PageSize, cancellationToken);
        
        return new PagedResult<IPackage>
        {
            Items = packages,
            PageNumber = pageNumber,
            PageSize = PageSize,
            TotalCount = await _searchProvider.GetTotalCountAsync(query)
        };
    }
}
```

#### 3. Icon Lazy Loading

**DO:** Load icons asynchronously and on-demand
```csharp
// From UniGetUI's icon loading pattern
public async Task<BitmapImage?> GetPackageIconAsync(IPackage package)
{
    // Check cache first
    if (_iconCache.TryGetValue(package.Id, out var cachedIcon))
        return cachedIcon;
    
    // Load asynchronously
    var icon = await Task.Run(() => DownloadIconAsync(package.IconUrl));
    
    // Cache for future use
    _iconCache.TryAdd(package.Id, icon);
    
    return icon;
}
```

#### 4. On-Demand Details Loading

**DO:** Load package details only when needed
```csharp
public class Package : IPackage
{
    private PackageDetails? _details;
    
    public async Task<PackageDetails> GetDetailsAsync()
    {
        if (_details != null)
            return _details;
        
        _details = await _detailsProvider.LoadDetailsAsync(this.Id);
        return _details;
    }
}
```

---

## Application Startup Optimization

### Techniques

#### 1. Minimize Work in Application Constructor

**DO:** Defer initialization to later lifecycle events
```csharp
public partial class App : Application
{
    public App()
    {
        InitializeComponent(); // Required
        // Minimal initialization only
    }
    
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Do heavier initialization here
        m_window = new MainWindow();
        m_window.Activate();
        
        // Start background initialization
        _ = InitializeServicesAsync();
    }
    
    private async Task InitializeServicesAsync()
    {
        await Task.Run(() =>
        {
            // Non-critical services
            InitializeTelemetry();
            CheckForUpdates();
            LoadCachedData();
        });
    }
}
```

#### 2. Parallel Service Initialization

**DO:** Initialize independent services in parallel
```csharp
private async Task InitializeServicesAsync()
{
    var tasks = new[]
    {
        InitializePackageManagersAsync(),
        LoadLanguageResourcesAsync(),
        LoadUserSettingsAsync(),
        InitializeIconCacheAsync()
    };
    
    await Task.WhenAll(tasks);
}
```

#### 3. Splash Screen and Progress Feedback

**DO:** Show immediate feedback during startup
```csharp
protected override void OnLaunched(LaunchActivatedEventArgs args)
{
    // Show window immediately with loading state
    m_window = new MainWindow();
    m_window.ShowLoadingState();
    m_window.Activate();
    
    // Initialize asynchronously
    _ = InitializeAndShowContentAsync();
}

private async Task InitializeAndShowContentAsync()
{
    await InitializeServicesAsync();
    m_window.ShowMainContent();
}
```

#### 4. Delay Non-Critical Features

**DO:** Load non-essential features after the main UI is ready
```csharp
private async Task InitializeServicesAsync()
{
    // Critical: Load immediately
    await LoadSettingsAsync();
    await InitializePackageManagersAsync();
    
    // Show main UI
    m_window.ShowMainContent();
    
    // Non-critical: Load in background
    _ = Task.Run(async () =>
    {
        await Task.Delay(500); // Let UI settle
        await CheckForUpdatesAsync();
        await LoadTelemetryAsync();
        await SyncWithCloudAsync();
    });
}
```

#### 5. Cache Frequently Used Data

**DO:** Cache data between application runs
```csharp
protected override void OnLaunched(LaunchActivatedEventArgs args)
{
    // Load from cache immediately
    var cachedPackages = LoadPackageListFromCache();
    m_window.DisplayPackages(cachedPackages);
    m_window.Activate();
    
    // Refresh in background
    _ = RefreshPackageListAsync();
}
```

---

## Task Recycling Pattern

### UniGetUI's TaskRecycler Implementation

The `TaskRecycler<T>` class is a performance optimization pattern implemented in UniGetUI to reduce CPU impact of expensive operations called concurrently.

#### Use Cases

**When to Use:**
- CPU-intensive operations (e.g., parsing large datasets)
- Operations that query external systems (e.g., package manager CLI calls)
- Operations expected to return the same result when called concurrently
- Heavy I/O operations (e.g., loading large configuration files)

**When NOT to Use:**
- Operations with side effects
- Operations that must execute independently
- Operations with rapidly changing results
- Operations that return different values on each call

#### Implementation Example

```csharp
// Getting installed packages - perfect use case
protected override Task<IEnumerable<IPackage>> GetInstalledPackages_UnSafe()
{
    return TaskRecycler<IEnumerable<IPackage>>.RunOrAttachAsync(
        () => ParseInstalledPackagesFromCLI(), 
        cacheTimeSecs: 0  // No caching after completion
    );
}

// With caching for frequently accessed data
public Task<Settings> GetSettingsAsync()
{
    return TaskRecycler<Settings>.RunOrAttachAsync(
        () => LoadSettingsFromDisk(),
        cacheTimeSecs: 30  // Cache for 30 seconds
    );
}
```

#### Key Benefits

1. **Deduplication**: Multiple concurrent calls execute only once
2. **Memory Efficiency**: Single result shared across callers
3. **CPU Optimization**: Expensive operations run only once
4. **Optional Caching**: Results can be cached for additional performance

#### Important Considerations

⚠️ **Object Instance Sharing**: When TaskRecycler returns a class instance, all callers receive **the same instance**. Modifications by one caller affect all others.

```csharp
// Be careful with mutable objects
var settings1 = await TaskRecycler<Settings>.RunOrAttachAsync(LoadSettings);
var settings2 = await TaskRecycler<Settings>.RunOrAttachAsync(LoadSettings);

// settings1 and settings2 are THE SAME OBJECT!
settings1.Theme = "Dark";
Console.WriteLine(settings2.Theme); // Outputs: "Dark"
```

**Solutions:**
1. Use immutable objects
2. Clone results when needed
3. Use value types or records
4. Design APIs to be thread-safe and handle shared instances

---

## Caching Strategies

### 1. In-Memory Caching

**DO:** Cache frequently accessed data in memory
```csharp
public class PackageIconCache
{
    private readonly ConcurrentDictionary<string, BitmapImage> _cache = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    
    public async Task<BitmapImage?> GetIconAsync(string packageId, string iconUrl)
    {
        // Try cache first
        if (_cache.TryGetValue(packageId, out var cached))
            return cached;
        
        // Prevent duplicate downloads
        await _semaphore.WaitAsync();
        try
        {
            // Double-check after acquiring lock
            if (_cache.TryGetValue(packageId, out cached))
                return cached;
            
            // Download and cache
            var icon = await DownloadIconAsync(iconUrl);
            _cache.TryAdd(packageId, icon);
            return icon;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
```

### 2. Disk-Based Caching

**DO:** Cache data to disk for persistence across app restarts
```csharp
public class PackageListCache
{
    private readonly string _cacheFilePath;
    
    public async Task SaveCacheAsync(List<IPackage> packages)
    {
        var json = JsonSerializer.Serialize(packages);
        await File.WriteAllTextAsync(_cacheFilePath, json);
    }
    
    public async Task<List<IPackage>?> LoadCacheAsync()
    {
        if (!File.Exists(_cacheFilePath))
            return null;
        
        var json = await File.ReadAllTextAsync(_cacheFilePath);
        return JsonSerializer.Deserialize<List<IPackage>>(json);
    }
}
```

### 3. Cache Invalidation

**DO:** Implement cache invalidation strategies
```csharp
public class TimedCache<TKey, TValue> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, CacheEntry<TValue>> _cache = new();
    private readonly TimeSpan _expirationTime;
    
    public void Set(TKey key, TValue value)
    {
        var entry = new CacheEntry<TValue>
        {
            Value = value,
            ExpirationTime = DateTime.UtcNow.Add(_expirationTime)
        };
        _cache[key] = entry;
    }
    
    public bool TryGet(TKey key, out TValue? value)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            if (DateTime.UtcNow < entry.ExpirationTime)
            {
                value = entry.Value;
                return true;
            }
            
            // Remove expired entry
            _cache.TryRemove(key, out _);
        }
        
        value = default;
        return false;
    }
}
```

### 4. Memory-Conscious Caching

**DO:** Implement cache size limits
```csharp
public class LRUCache<TKey, TValue> where TKey : notnull
{
    private readonly int _maxSize;
    private readonly ConcurrentDictionary<TKey, CacheNode<TValue>> _cache;
    private readonly LinkedList<TKey> _lruList = new();
    private readonly object _lock = new();
    
    public void Set(TKey key, TValue value)
    {
        lock (_lock)
        {
            if (_cache.Count >= _maxSize)
            {
                // Remove least recently used
                var lruKey = _lruList.First!.Value;
                _lruList.RemoveFirst();
                _cache.TryRemove(lruKey, out _);
            }
            
            _cache[key] = new CacheNode<TValue> { Value = value };
            _lruList.AddLast(key);
        }
    }
}
```

---

## Background Processing

### Techniques

#### 1. Background Tasks for Periodic Operations

**DO:** Use background tasks for periodic updates
```csharp
public class UpdateCheckService
{
    private readonly PeriodicTimer _timer;
    private Task? _backgroundTask;
    
    public void Start()
    {
        _timer = new PeriodicTimer(TimeSpan.FromHours(4));
        _backgroundTask = CheckForUpdatesLoopAsync();
    }
    
    private async Task CheckForUpdatesLoopAsync()
    {
        while (await _timer.WaitForNextTickAsync())
        {
            try
            {
                await CheckForUpdatesAsync();
            }
            catch (Exception ex)
            {
                Logger.Error("Update check failed", ex);
            }
        }
    }
}
```

#### 2. Queue-Based Background Processing

**DO:** Use queues for background work
```csharp
public class PackageOperationQueue
{
    private readonly Channel<PackageOperation> _queue;
    private Task? _processorTask;
    
    public PackageOperationQueue()
    {
        _queue = Channel.CreateUnbounded<PackageOperation>();
        _processorTask = ProcessQueueAsync();
    }
    
    public async Task EnqueueAsync(PackageOperation operation)
    {
        await _queue.Writer.WriteAsync(operation);
    }
    
    private async Task ProcessQueueAsync()
    {
        await foreach (var operation in _queue.Reader.ReadAllAsync())
        {
            try
            {
                await operation.ExecuteAsync();
            }
            catch (Exception ex)
            {
                Logger.Error($"Operation failed: {operation}", ex);
            }
        }
    }
}
```

#### 3. Responsive Progress Reporting

**DO:** Report progress from background operations
```csharp
public async Task InstallPackageAsync(
    IPackage package, 
    IProgress<InstallProgress> progress,
    CancellationToken cancellationToken = default)
{
    progress?.Report(new InstallProgress { Stage = "Downloading", Percentage = 0 });
    
    await DownloadPackageAsync(package, cancellationToken);
    
    progress?.Report(new InstallProgress { Stage = "Installing", Percentage = 50 });
    
    await InstallPackageFilesAsync(package, cancellationToken);
    
    progress?.Report(new InstallProgress { Stage = "Complete", Percentage = 100 });
}
```

---

## I/O Optimization

### Best Practices

#### 1. Asynchronous File Operations

**DO:** Use async file I/O
```csharp
// Good: Async I/O
public async Task<string> LoadConfigurationAsync()
{
    using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 
                                      bufferSize: 4096, useAsync: true);
    using var reader = new StreamReader(stream);
    return await reader.ReadToEndAsync();
}

// Bad: Synchronous I/O
public string LoadConfiguration()
{
    return File.ReadAllText(path); // Blocks thread
}
```

#### 2. Buffered I/O

**DO:** Use appropriate buffer sizes
```csharp
// Large buffer for bulk operations
const int LargeBufferSize = 81920; // 80KB

using var fileStream = new FileStream(
    path, 
    FileMode.Open, 
    FileAccess.Read,
    FileShare.Read,
    bufferSize: LargeBufferSize,
    useAsync: true);
```

#### 3. Stream Processing

**DO:** Process large files as streams
```csharp
// Good: Stream processing
public async Task<int> CountLinesAsync(string filePath)
{
    int lineCount = 0;
    
    using var stream = File.OpenRead(filePath);
    using var reader = new StreamReader(stream);
    
    while (await reader.ReadLineAsync() != null)
    {
        lineCount++;
    }
    
    return lineCount;
}

// Bad: Load entire file
public int CountLines(string filePath)
{
    var lines = File.ReadAllLines(filePath); // Loads entire file!
    return lines.Length;
}
```

#### 4. Parallel File Processing

**DO:** Process multiple files in parallel when possible
```csharp
public async Task<Dictionary<string, int>> ProcessFilesAsync(IEnumerable<string> filePaths)
{
    var tasks = filePaths.Select(async path =>
    {
        var content = await File.ReadAllTextAsync(path);
        return (path, content.Length);
    });
    
    var results = await Task.WhenAll(tasks);
    return results.ToDictionary(r => r.path, r => r.Length);
}
```

---

## Performance Testing

### Benchmarking

Use BenchmarkDotNet for performance testing:

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

[MemoryDiagnoser]
public class PackageSearchBenchmark
{
    private List<IPackage> _packages;
    
    [GlobalSetup]
    public void Setup()
    {
        _packages = GenerateTestPackages(10000);
    }
    
    [Benchmark]
    public List<IPackage> LinearSearch()
    {
        return _packages.Where(p => p.Name.Contains("test")).ToList();
    }
    
    [Benchmark]
    public List<IPackage> OptimizedSearch()
    {
        return _packages
            .AsParallel()
            .Where(p => p.Name.Contains("test"))
            .ToList();
    }
}

// Run: BenchmarkRunner.Run<PackageSearchBenchmark>();
```

---

## Summary Checklist

**Async/Await:**
- ✅ Use async/await throughout call chains
- ✅ Use ConfigureAwait(false) in library code
- ✅ Execute independent tasks in parallel
- ✅ Support CancellationToken
- ✅ Use ValueTask for hot paths

**UI Thread:**
- ✅ Keep UI thread operations under 16ms
- ✅ Use Task.Run for CPU-intensive work
- ✅ Virtualize large lists
- ✅ Batch UI updates

**Lazy Loading:**
- ✅ Implement pagination for large datasets
- ✅ Load details on-demand
- ✅ Use lazy initialization

**Startup:**
- ✅ Minimize work in constructors
- ✅ Initialize services in parallel
- ✅ Show loading feedback immediately
- ✅ Defer non-critical features

**Caching:**
- ✅ Cache frequently accessed data
- ✅ Implement cache invalidation
- ✅ Consider memory limits
- ✅ Use TaskRecycler for concurrent operations

**Background Processing:**
- ✅ Use background tasks for periodic work
- ✅ Implement work queues
- ✅ Report progress to users

**I/O:**
- ✅ Use async file operations
- ✅ Stream large files
- ✅ Process files in parallel when appropriate

---

## See Also

- [Memory Management Guide](./memory-management.md)
- [Performance Profiling Guide](./profiling-guide.md)
- [Monitoring Strategy](./monitoring-strategy.md)
- [Coding Standards & Best Practices](/docs/codebase-analysis/07-best-practices/patterns-standards.md)
