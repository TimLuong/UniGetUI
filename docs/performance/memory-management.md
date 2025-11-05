# Memory Management Best Practices

## Overview

This guide covers memory management best practices for UniGetUI and .NET 8 Windows desktop applications. Proper memory management is critical for application performance, stability, and resource efficiency.

## Table of Contents

1. [Resource Cleanup and Disposal](#resource-cleanup-and-disposal)
2. [Memory Leak Prevention](#memory-leak-prevention)
3. [Object Lifecycle Management](#object-lifecycle-management)
4. [Collection Management](#collection-management)
5. [Large Object Handling](#large-object-handling)
6. [Memory Profiling](#memory-profiling)

---

## Resource Cleanup and Disposal

### IDisposable Pattern

#### 1. Implementing IDisposable

**DO:** Implement IDisposable for classes that own unmanaged resources or expensive managed resources

```csharp
public class PackageDownloader : IDisposable
{
    private HttpClient? _httpClient;
    private FileStream? _downloadStream;
    private bool _disposed = false;
    
    public PackageDownloader()
    {
        _httpClient = new HttpClient();
    }
    
    // Public dispose method
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    // Protected dispose pattern
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        
        if (disposing)
        {
            // Dispose managed resources
            _httpClient?.Dispose();
            _downloadStream?.Dispose();
        }
        
        // Free unmanaged resources (if any)
        // ...
        
        _disposed = true;
    }
    
    // Finalizer (only if you have unmanaged resources)
    ~PackageDownloader()
    {
        Dispose(false);
    }
    
    // Helper method to check if disposed
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().Name);
    }
}
```

#### 2. Using Statement Pattern

**DO:** Use `using` statements for automatic disposal

```csharp
// Traditional using statement
public async Task<string> DownloadPackageAsync(string url)
{
    using (var client = new HttpClient())
    {
        return await client.GetStringAsync(url);
    }
    // client.Dispose() is called automatically
}

// C# 8+ using declaration (preferred)
public async Task<string> DownloadPackageAsync(string url)
{
    using var client = new HttpClient();
    return await client.GetStringAsync(url);
    // client.Dispose() is called at end of method
}
```

#### 3. Multiple Disposables

**DO:** Properly handle multiple disposable objects

```csharp
// Good: Each resource properly disposed
public async Task ProcessFileAsync(string inputPath, string outputPath)
{
    using var inputStream = File.OpenRead(inputPath);
    using var outputStream = File.OpenWrite(outputPath);
    using var reader = new StreamReader(inputStream);
    using var writer = new StreamWriter(outputStream);
    
    string? line;
    while ((line = await reader.ReadLineAsync()) != null)
    {
        await writer.WriteLineAsync(line.ToUpper());
    }
}

// Handle exceptions properly
public async Task ProcessFileAsync(string inputPath, string outputPath)
{
    FileStream? inputStream = null;
    FileStream? outputStream = null;
    
    try
    {
        inputStream = File.OpenRead(inputPath);
        outputStream = File.OpenWrite(outputPath);
        await ProcessStreamsAsync(inputStream, outputStream);
    }
    finally
    {
        inputStream?.Dispose();
        outputStream?.Dispose();
    }
}
```

#### 4. Async Disposal

**DO:** Implement IAsyncDisposable for async cleanup

```csharp
public class AsyncPackageProcessor : IAsyncDisposable
{
    private readonly HttpClient _httpClient = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    
    public async ValueTask DisposeAsync()
    {
        // Async cleanup
        await FlushPendingOperationsAsync();
        
        // Dispose managed resources
        _httpClient?.Dispose();
        _semaphore?.Dispose();
        
        GC.SuppressFinalize(this);
    }
    
    private async Task FlushPendingOperationsAsync()
    {
        // Perform async cleanup operations
        await Task.CompletedTask;
    }
}

// Usage
await using var processor = new AsyncPackageProcessor();
await processor.ProcessPackagesAsync();
// DisposeAsync() called automatically
```

---

## Memory Leak Prevention

### Common Leak Patterns and Solutions

#### 1. Event Handler Leaks

**PROBLEM:** Event handlers prevent garbage collection of subscribers

```csharp
// Bad: Creates memory leak
public class PackageListView
{
    public PackageListView(PackageManager manager)
    {
        // This creates a strong reference from manager to PackageListView
        manager.PackageInstalled += OnPackageInstalled;
    }
    
    private void OnPackageInstalled(object sender, PackageEventArgs e)
    {
        // Handle event
    }
    
    // If PackageListView is closed, it won't be garbage collected
    // because PackageManager still holds a reference via the event
}
```

**SOLUTION:** Always unsubscribe from events

```csharp
// Good: Proper cleanup
public class PackageListView : IDisposable
{
    private readonly PackageManager _manager;
    
    public PackageListView(PackageManager manager)
    {
        _manager = manager;
        _manager.PackageInstalled += OnPackageInstalled;
    }
    
    public void Dispose()
    {
        // Unsubscribe to prevent memory leak
        _manager.PackageInstalled -= OnPackageInstalled;
    }
    
    private void OnPackageInstalled(object sender, PackageEventArgs e)
    {
        // Handle event
    }
}
```

**ALTERNATIVE:** Use weak event patterns

```csharp
// Using weak references for event handlers
public class WeakEventManager<TEventArgs> where TEventArgs : EventArgs
{
    private readonly List<WeakReference<EventHandler<TEventArgs>>> _handlers = new();
    
    public void AddHandler(EventHandler<TEventArgs> handler)
    {
        _handlers.Add(new WeakReference<EventHandler<TEventArgs>>(handler));
    }
    
    public void Raise(object sender, TEventArgs args)
    {
        var deadHandlers = new List<WeakReference<EventHandler<TEventArgs>>>();
        
        foreach (var weakHandler in _handlers)
        {
            if (weakHandler.TryGetTarget(out var handler))
            {
                handler?.Invoke(sender, args);
            }
            else
            {
                deadHandlers.Add(weakHandler);
            }
        }
        
        // Clean up dead handlers
        foreach (var deadHandler in deadHandlers)
        {
            _handlers.Remove(deadHandler);
        }
    }
}
```

#### 2. Timer Leaks

**PROBLEM:** Timers keep objects alive

```csharp
// Bad: Timer prevents garbage collection
public class UpdateChecker
{
    private Timer _timer;
    
    public UpdateChecker()
    {
        _timer = new Timer(CheckForUpdates, null, TimeSpan.Zero, TimeSpan.FromHours(1));
    }
    
    private void CheckForUpdates(object? state)
    {
        // Check for updates
    }
}
// UpdateChecker won't be garbage collected because Timer keeps it alive
```

**SOLUTION:** Dispose timers properly

```csharp
// Good: Proper timer disposal
public class UpdateChecker : IDisposable
{
    private Timer? _timer;
    
    public UpdateChecker()
    {
        _timer = new Timer(CheckForUpdates, null, TimeSpan.Zero, TimeSpan.FromHours(1));
    }
    
    public void Dispose()
    {
        _timer?.Dispose();
        _timer = null;
    }
    
    private void CheckForUpdates(object? state)
    {
        // Check for updates
    }
}
```

#### 3. Static References

**PROBLEM:** Static fields prevent garbage collection

```csharp
// Bad: Static collection grows indefinitely
public static class PackageCache
{
    private static readonly Dictionary<string, Package> _cache = new();
    
    public static void AddPackage(Package package)
    {
        _cache[package.Id] = package; // Never removed!
    }
}
```

**SOLUTION:** Implement cache cleanup or use weak references

```csharp
// Good: Implements cleanup
public static class PackageCache
{
    private static readonly Dictionary<string, Package> _cache = new();
    private static readonly Timer _cleanupTimer;
    
    static PackageCache()
    {
        _cleanupTimer = new Timer(CleanupOldEntries, null, 
            TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }
    
    public static void AddPackage(Package package)
    {
        _cache[package.Id] = package;
    }
    
    private static void CleanupOldEntries(object? state)
    {
        var oldEntries = _cache
            .Where(kvp => kvp.Value.IsExpired)
            .Select(kvp => kvp.Key)
            .ToList();
        
        foreach (var key in oldEntries)
        {
            _cache.Remove(key);
        }
    }
}

// Or use ConditionalWeakTable for automatic cleanup
public static class PackageCache
{
    private static readonly ConditionalWeakTable<string, Package> _cache = new();
    
    public static void AddPackage(string id, Package package)
    {
        _cache.AddOrUpdate(id, package);
    }
    
    public static bool TryGetPackage(string id, out Package? package)
    {
        return _cache.TryGetValue(id, out package);
    }
}
```

#### 4. Closure Captures

**PROBLEM:** Closures capture context and prevent garbage collection

```csharp
// Bad: Closure captures entire class instance
public class PackageInstaller
{
    private readonly List<Package> _packages = new();
    
    public void InstallAllAsync()
    {
        foreach (var package in _packages)
        {
            // This lambda captures 'this', keeping entire PackageInstaller alive
            Task.Run(async () =>
            {
                await InstallPackageAsync(package);
            });
        }
    }
}
```

**SOLUTION:** Capture only what you need

```csharp
// Good: Capture only necessary data
public class PackageInstaller
{
    private readonly List<Package> _packages = new();
    
    public void InstallAllAsync()
    {
        foreach (var package in _packages)
        {
            // Capture only the package, not the entire class
            var packageCopy = package;
            Task.Run(async () =>
            {
                await InstallPackageInternalAsync(packageCopy);
            });
        }
    }
    
    private static async Task InstallPackageInternalAsync(Package package)
    {
        // Static method doesn't capture 'this'
        await Task.Delay(100);
        // Install package
    }
}
```

---

## Object Lifecycle Management

### Best Practices

#### 1. Object Pooling

**DO:** Use object pooling for frequently created/disposed objects

```csharp
public class PackageProcessorPool
{
    private readonly ObjectPool<PackageProcessor> _pool;
    
    public PackageProcessorPool()
    {
        var policy = new DefaultPooledObjectPolicy<PackageProcessor>();
        _pool = new DefaultObjectPool<PackageProcessor>(policy, maxRetained: 10);
    }
    
    public async Task ProcessPackageAsync(Package package)
    {
        var processor = _pool.Get();
        try
        {
            await processor.ProcessAsync(package);
        }
        finally
        {
            _pool.Return(processor);
        }
    }
}

// Custom policy for complex initialization
public class PackageProcessorPoolPolicy : IPooledObjectPolicy<PackageProcessor>
{
    public PackageProcessor Create()
    {
        return new PackageProcessor();
    }
    
    public bool Return(PackageProcessor obj)
    {
        // Reset state before returning to pool
        obj.Reset();
        return true; // Return true to return to pool
    }
}
```

#### 2. ArrayPool for Temporary Buffers

**DO:** Use ArrayPool for temporary arrays

```csharp
public async Task<byte[]> ProcessDataAsync(Stream input)
{
    // Rent buffer from pool
    var buffer = ArrayPool<byte>.Shared.Rent(4096);
    try
    {
        int bytesRead = await input.ReadAsync(buffer, 0, buffer.Length);
        
        // Create result array with actual size
        var result = new byte[bytesRead];
        Array.Copy(buffer, result, bytesRead);
        return result;
    }
    finally
    {
        // Return buffer to pool
        ArrayPool<byte>.Shared.Return(buffer);
    }
}
```

#### 3. Struct vs Class

**DO:** Use structs for small, immutable data

```csharp
// Good: Small immutable data as struct
public readonly struct PackageVersion : IEquatable<PackageVersion>
{
    public int Major { get; init; }
    public int Minor { get; init; }
    public int Patch { get; init; }
    
    public PackageVersion(int major, int minor, int patch)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
    }
    
    public bool Equals(PackageVersion other)
    {
        return Major == other.Major && Minor == other.Minor && Patch == other.Patch;
    }
}

// Bad: Large struct (creates copies)
public struct PackageDetails // Don't do this if struct is large!
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<string> Dependencies { get; set; }
    // ... many more fields
}
```

**Guidelines:**
- Structs should be < 16 bytes
- Structs should be immutable
- Structs should be logically a single value
- Use classes for complex objects

#### 4. Records for Immutable Data

**DO:** Use records for immutable reference types

```csharp
// Immutable package information
public record PackageInfo(string Id, string Name, string Version)
{
    // Records are ideal for DTOs and immutable data
    public string FullName => $"{Name} v{Version}";
}

// With immutable collections
public record PackageSearchResult(
    string Query,
    ImmutableArray<PackageInfo> Packages,
    int TotalCount);
```

---

## Collection Management

### Best Practices

#### 1. Choose the Right Collection

```csharp
// List<T>: Random access, fast add to end
private List<Package> _packages = new();

// Dictionary<K,V>: Fast lookup by key
private Dictionary<string, Package> _packageById = new();

// HashSet<T>: Fast membership testing, no duplicates
private HashSet<string> _installedPackageIds = new();

// Queue<T>: FIFO operations
private Queue<PackageOperation> _operationQueue = new();

// ConcurrentDictionary<K,V>: Thread-safe dictionary
private ConcurrentDictionary<string, Package> _sharedCache = new();
```

#### 2. Initialize with Capacity

**DO:** Specify capacity if size is known

```csharp
// Good: Pre-allocate capacity
public List<Package> LoadPackages(int expectedCount)
{
    var packages = new List<Package>(capacity: expectedCount);
    // Avoids multiple array reallocations
    return packages;
}

// Dictionary with capacity
var packageMap = new Dictionary<string, Package>(capacity: 1000);
```

#### 3. Clear vs New Instance

**DO:** Reuse collections by clearing instead of creating new instances

```csharp
// Good: Reuse collection
private readonly List<Package> _tempPackages = new();

public void ProcessBatch(IEnumerable<Package> packages)
{
    _tempPackages.Clear(); // Reuses backing array
    _tempPackages.AddRange(packages);
    // Process...
}

// Bad: Creates garbage
public void ProcessBatch(IEnumerable<Package> packages)
{
    var tempPackages = new List<Package>(); // New allocation each time
    tempPackages.AddRange(packages);
    // Process...
}
```

#### 4. Avoid LINQ in Hot Paths

**DON'T:** Use LINQ in performance-critical code

```csharp
// Bad: Creates intermediate collections
public List<Package> GetInstalledPackagesByName()
{
    return _packages
        .Where(p => p.IsInstalled)
        .OrderBy(p => p.Name)
        .ToList(); // Creates multiple intermediate collections
}

// Good: Manual loop is more efficient
public List<Package> GetInstalledPackagesByName()
{
    var result = new List<Package>(_packages.Count);
    
    foreach (var package in _packages)
    {
        if (package.IsInstalled)
            result.Add(package);
    }
    
    result.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
    return result;
}
```

#### 5. Use Span<T> for Array Slicing

**DO:** Use Span<T> to avoid allocations

```csharp
// Bad: Creates new array
public byte[] GetHeader(byte[] data)
{
    var header = new byte[16];
    Array.Copy(data, 0, header, 0, 16);
    return header;
}

// Good: No allocation
public ReadOnlySpan<byte> GetHeader(byte[] data)
{
    return data.AsSpan(0, 16);
}
```

---

## Large Object Handling

### Best Practices

#### 1. Understand Large Object Heap (LOH)

Objects ≥ 85,000 bytes go to LOH:
- Not compacted by default (causes fragmentation)
- Collected only during Gen 2 GC
- Can cause memory fragmentation

**DO:** Avoid creating many large objects

```csharp
// Bad: Frequent large allocations
public void ProcessFiles(IEnumerable<string> files)
{
    foreach (var file in files)
    {
        // Each allocation creates LOH pressure
        var buffer = new byte[100_000];
        ProcessFile(file, buffer);
    }
}

// Good: Reuse buffer or use pooling
private readonly byte[] _reusableBuffer = new byte[100_000];

public void ProcessFiles(IEnumerable<string> files)
{
    foreach (var file in files)
    {
        ProcessFile(file, _reusableBuffer);
    }
}
```

#### 2. Streaming for Large Data

**DO:** Stream large data instead of loading into memory

```csharp
// Bad: Loads entire file
public async Task<string> ProcessLargeFileAsync(string path)
{
    var content = await File.ReadAllTextAsync(path); // Could be gigabytes!
    return content.ToUpper();
}

// Good: Stream processing
public async Task ProcessLargeFileAsync(string inputPath, string outputPath)
{
    using var input = File.OpenRead(inputPath);
    using var output = File.OpenWrite(outputPath);
    using var reader = new StreamReader(input);
    using var writer = new StreamWriter(output);
    
    string? line;
    while ((line = await reader.ReadLineAsync()) != null)
    {
        await writer.WriteLineAsync(line.ToUpper());
    }
}
```

#### 3. Pagination for Large Collections

**DO:** Process large collections in batches

```csharp
public async Task ProcessLargePackageListAsync(List<Package> packages)
{
    const int batchSize = 100;
    
    for (int i = 0; i < packages.Count; i += batchSize)
    {
        var batch = packages.Skip(i).Take(batchSize);
        await ProcessBatchAsync(batch);
        
        // Optional: Allow GC between batches
        if (i % 1000 == 0)
        {
            GC.Collect(0, GCCollectionMode.Optimized);
        }
    }
}
```

#### 4. Memory-Mapped Files for Very Large Data

**DO:** Use memory-mapped files for very large datasets

```csharp
public class LargeFileProcessor : IDisposable
{
    private MemoryMappedFile? _mmf;
    private MemoryMappedViewAccessor? _accessor;
    
    public void OpenFile(string path)
    {
        var fileInfo = new FileInfo(path);
        _mmf = MemoryMappedFile.CreateFromFile(
            path, 
            FileMode.Open, 
            null, 
            fileInfo.Length,
            MemoryMappedFileAccess.Read);
        
        _accessor = _mmf.CreateViewAccessor(
            0, 
            fileInfo.Length, 
            MemoryMappedFileAccess.Read);
    }
    
    public byte ReadByte(long position)
    {
        return _accessor!.ReadByte(position);
    }
    
    public void Dispose()
    {
        _accessor?.Dispose();
        _mmf?.Dispose();
    }
}
```

---

## Memory Profiling

### Detecting Memory Issues

#### 1. Monitor GC Statistics

```csharp
public class MemoryMonitor
{
    public void LogMemoryStats()
    {
        var gen0 = GC.CollectionCount(0);
        var gen1 = GC.CollectionCount(1);
        var gen2 = GC.CollectionCount(2);
        var memory = GC.GetTotalMemory(forceFullCollection: false) / 1024 / 1024;
        
        Logger.Info($"GC Stats - Gen0: {gen0}, Gen1: {gen1}, Gen2: {gen2}, Memory: {memory}MB");
    }
    
    public GCMemoryInfo GetMemoryInfo()
    {
        return GC.GetGCMemoryInfo();
    }
}
```

#### 2. Track Allocations

```csharp
public class AllocationTracker : IDisposable
{
    private readonly long _startAllocations;
    private readonly long _startMemory;
    
    public AllocationTracker()
    {
        _startAllocations = GC.GetTotalAllocatedBytes();
        _startMemory = GC.GetTotalMemory(false);
    }
    
    public void Dispose()
    {
        var endAllocations = GC.GetTotalAllocatedBytes();
        var endMemory = GC.GetTotalMemory(false);
        
        var allocated = (endAllocations - _startAllocations) / 1024;
        var memoryChange = (endMemory - _startMemory) / 1024;
        
        Logger.Info($"Allocated: {allocated}KB, Memory Change: {memoryChange}KB");
    }
}

// Usage
using (new AllocationTracker())
{
    // Code to measure
    ProcessPackages();
}
```

#### 3. Use Diagnostic Tools

**Visual Studio:**
- Memory Usage profiler
- .NET Object Allocation Tracking
- Heap snapshots and comparison

**dotMemory (JetBrains):**
- Memory snapshot analysis
- Memory leak detection
- Object retention paths

**PerfView:**
- GC event analysis
- Allocation profiling
- Memory investigation

#### 4. WeakReference for Caches

**DO:** Use WeakReference for optional caches

```csharp
public class ImageCache
{
    private readonly ConcurrentDictionary<string, WeakReference<BitmapImage>> _cache = new();
    
    public BitmapImage? TryGetImage(string key)
    {
        if (_cache.TryGetValue(key, out var weakRef))
        {
            if (weakRef.TryGetTarget(out var image))
            {
                return image; // Still alive
            }
            
            // Was collected, remove from cache
            _cache.TryRemove(key, out _);
        }
        
        return null;
    }
    
    public void AddImage(string key, BitmapImage image)
    {
        _cache[key] = new WeakReference<BitmapImage>(image);
    }
}
```

---

## Memory Best Practices Summary

### DO:
✅ Implement IDisposable for resource-owning types  
✅ Use `using` statements for automatic disposal  
✅ Unsubscribe from events to prevent leaks  
✅ Dispose timers and other long-lived objects  
✅ Use object pooling for frequently created objects  
✅ Choose appropriate collection types  
✅ Pre-allocate collections when size is known  
✅ Use Span<T> and Memory<T> to avoid allocations  
✅ Stream large files instead of loading into memory  
✅ Monitor GC statistics in production  

### DON'T:
❌ Keep static references to disposable objects  
❌ Create large objects frequently  
❌ Use LINQ in performance-critical loops  
❌ Block on async code with .Result or .Wait()  
❌ Capture more than necessary in closures  
❌ Load entire large files into memory  
❌ Create new collections when you can reuse  
❌ Use structs for large or mutable data  

---

## Code Review Checklist

When reviewing code for memory issues:

- [ ] Are IDisposable objects properly disposed?
- [ ] Are event handlers unsubscribed?
- [ ] Are timers disposed when no longer needed?
- [ ] Are large objects reused instead of recreated?
- [ ] Are collections initialized with appropriate capacity?
- [ ] Are temporary buffers rented from ArrayPool?
- [ ] Are closures capturing only necessary data?
- [ ] Is streaming used for large data?
- [ ] Are weak references used for optional caches?
- [ ] Are static collections properly managed?

---

## See Also

- [Performance Optimization Guide](./optimization-guide.md)
- [Performance Profiling Guide](./profiling-guide.md)
- [Monitoring Strategy](./monitoring-strategy.md)
- [TaskRecycler Documentation](/docs/codebase-analysis/04-code-understanding/class-taskrecycler.md)
