# Design Patterns & Coding Standards

## Design Patterns

### Pattern 1: Factory Pattern
- **Type:** Creational
- **Purpose:** Creates instances of objects while encapsulating the creation logic and maintaining a cache of created instances
- **Implementation Location:** `src/UniGetUI.PackageEngine.PackageManagerClasses/Manager/Classes/SourceFactory.cs`

**Example:**
```csharp
public class SourceFactory : ISourceFactory
{
    private readonly ConcurrentDictionary<string, IManagerSource> __reference;
    
    public IManagerSource GetSourceOrDefault(string name)
    {
        if (__reference.TryGetValue(name, out IManagerSource? source))
        {
            return source;
        }
        
        ManagerSource new_source = new(__manager, name, __default_uri);
        __reference.TryAdd(name, new_source);
        return new_source;
    }
}
```

**Benefits:**
- Centralizes object creation logic
- Provides caching mechanism for reusing instances
- Decouples client code from concrete implementations
- Thread-safe using ConcurrentDictionary

### Pattern 2: Singleton Pattern (Static Class Variant)
- **Type:** Creational
- **Purpose:** Provides a single point of access to shared functionality and state across the application
- **Implementation Location:** `src/UniGetUI.Core.Classes/TaskRecycler.cs`

**Example:**
```csharp
public static class TaskRecycler<ReturnT>
{
    private static readonly ConcurrentDictionary<int, Task<ReturnT>> _tasks = new();
    
    public static Task<ReturnT> RunOrAttachAsync(Func<ReturnT> method, int cacheTimeSecs = 0)
    {
        int hash = method.GetHashCode();
        return _runTaskAndWait(new Task<ReturnT>(method), hash, cacheTimeSecs);
    }
}
```

**Benefits:**
- Ensures single instance of cached tasks across application
- Reduces memory usage by sharing task results
- Thread-safe implementation using concurrent collections

### Pattern 3: Observer Pattern
- **Type:** Behavioral
- **Purpose:** Allows objects to notify subscribers when their state changes
- **Implementation Location:** `src/UniGetUI.Core.Classes/ObservableQueue.cs`

**Example:**
```csharp
public class ObservableQueue<T> : Queue<T>
{
    // Custom EventArgs using C# 12 primary constructor
    public class EventArgs(T item)
    {
        public readonly T Item = item;
    }

    public event EventHandler<EventArgs>? ItemEnqueued;
    public event EventHandler<EventArgs>? ItemDequeued;

    public new void Enqueue(T item)
    {
        base.Enqueue(item);
        ItemEnqueued?.Invoke(this, new EventArgs(item));
    }

    public new T Dequeue()
    {
        T item = base.Dequeue();
        ItemDequeued?.Invoke(this, new EventArgs(item));
        return item;
    }
}
```

**Benefits:**
- Decouples event producers from consumers
- Allows multiple subscribers to react to state changes
- Follows .NET event pattern conventions

### Pattern 4: Strategy Pattern
- **Type:** Behavioral
- **Purpose:** Defines a family of algorithms (package manager operations) and makes them interchangeable through interfaces
- **Implementation Location:** `src/UniGetUI.PAckageEngine.Interfaces/IPackageManager.cs`

**Example:**
```csharp
public interface IPackageManager
{
    public IPackageDetailsHelper DetailsHelper { get; }
    public IPackageOperationHelper OperationHelper { get; }
    public IMultiSourceHelper SourcesHelper { get; }
    
    public IReadOnlyList<IPackage> FindPackages(string query);
    public IReadOnlyList<IPackage> GetAvailableUpdates();
    public IReadOnlyList<IPackage> GetInstalledPackages();
}
```

**Benefits:**
- Allows different package managers (WinGet, Cargo, Dotnet) to have different implementations
- Enables runtime selection of package manager strategy
- Promotes code reusability through interfaces

### Pattern 5: Helper Pattern (Delegation)
- **Type:** Structural
- **Purpose:** Delegates specific responsibilities to helper classes to maintain single responsibility principle
- **Implementation Location:** Various `*Helper.cs` files in package manager implementations

**Example:**
```csharp
// Package managers delegate specific operations to helper classes
public interface IPackageManager
{
    public IPackageDetailsHelper DetailsHelper { get; }      // Handles package details
    public IPackageOperationHelper OperationHelper { get; }  // Handles install/uninstall
    public IMultiSourceHelper SourcesHelper { get; }         // Handles package sources
}
```

**Benefits:**
- Separates concerns into focused, maintainable classes
- Makes code easier to test
- Reduces complexity of main classes

### Pattern 6: Flyweight Pattern (Task Recycling)
- **Type:** Structural
- **Purpose:** Reduces CPU overhead by reusing results from identical concurrent operations
- **Implementation Location:** `src/UniGetUI.Core.Classes/TaskRecycler.cs`

**Example:**
```csharp
// Attaches to existing task if the same operation is already running
public static Task<ReturnT> RunOrAttachAsync(Func<ReturnT> method, int cacheTimeSecs = 0)
{
    int hash = method.GetHashCode();
    if (_tasks.TryGetValue(hash, out Task<ReturnT>? _task))
    {
        return _task;  // Reuse existing task
    }
    // Create new task only if needed
}
```

**Benefits:**
- Reduces CPU usage by avoiding duplicate work
- Improves performance for expensive operations
- Provides optional caching of results

## Coding Standards
