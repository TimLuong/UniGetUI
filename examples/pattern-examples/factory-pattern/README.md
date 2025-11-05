# Factory Pattern Example

This example demonstrates the Factory Pattern as implemented in UniGetUI, showing how to create object instances while encapsulating creation logic and maintaining a cache.

## Pattern Overview

The Factory Pattern is a **Creational Pattern** that:
- Centralizes object creation logic
- Provides caching mechanism for reusing instances
- Decouples client code from concrete implementations
- Ensures thread-safety using `ConcurrentDictionary`

## Implementation Location in UniGetUI

**Primary Implementation:**
- `src/UniGetUI.PackageEngine.PackageManagerClasses/Manager/Classes/SourceFactory.cs`

## Benefits

1. **Centralized Creation**: All object creation logic in one place
2. **Instance Caching**: Reuse objects instead of creating duplicates
3. **Thread-Safe**: Safe for concurrent access from multiple threads
4. **Loose Coupling**: Clients don't need to know about concrete classes
5. **Easy Testing**: Can inject mock factories for testing

## Code Example

### Interface Definition
```csharp
/// <summary>
/// Interface for package services
/// </summary>
public interface IPackageService
{
    string ServiceName { get; }
    Task<List<Package>> GetAvailablePackagesAsync();
    Task<bool> InstallPackageAsync(Package package);
}
```

### Factory Implementation
```csharp
/// <summary>
/// Factory for creating and caching package service instances
/// </summary>
public class PackageServiceFactory
{
    // Thread-safe cache of created instances
    private readonly ConcurrentDictionary<string, IPackageService> _services = new();
    
    /// <summary>
    /// Gets an existing service or creates a new one
    /// </summary>
    public IPackageService GetService(string serviceType)
    {
        // Try to get existing instance from cache
        if (_services.TryGetValue(serviceType, out IPackageService? service))
        {
            return service; // Return cached instance
        }
        
        // Create new instance based on service type
        IPackageService newService = serviceType.ToLowerInvariant() switch
        {
            "winget" => new WinGetService(),
            "scoop" => new ScoopService(),
            "chocolatey" => new ChocolateyService(),
            _ => throw new ArgumentException($"Unknown service type: {serviceType}")
        };
        
        // Try to add to cache (thread-safe)
        if (_services.TryAdd(serviceType, newService))
        {
            return newService;
        }
        
        // Race condition - another thread added between TryGetValue and TryAdd
        // Return the instance added by the other thread
        return _services[serviceType];
    }
}
```

### Concrete Implementations
```csharp
/// <summary>
/// WinGet implementation
/// </summary>
public class WinGetService : IPackageService
{
    public string ServiceName => "WinGet";
    
    public async Task<List<Package>> GetAvailablePackagesAsync()
    {
        // Implementation specific to WinGet
        // Execute winget commands, parse output, return packages
    }
    
    public async Task<bool> InstallPackageAsync(Package package)
    {
        // WinGet-specific installation logic
    }
}

/// <summary>
/// Scoop implementation
/// </summary>
public class ScoopService : IPackageService
{
    public string ServiceName => "Scoop";
    
    public async Task<List<Package>> GetAvailablePackagesAsync()
    {
        // Implementation specific to Scoop
    }
    
    public async Task<bool> InstallPackageAsync(Package package)
    {
        // Scoop-specific installation logic
    }
}
```

## Usage Examples

### Basic Usage
```csharp
// Get factory instance
var factory = new PackageServiceFactory();

// Get a service (creates new instance)
IPackageService wingetService = factory.GetService("winget");

// Get same service again (returns cached instance)
IPackageService sameService = factory.GetService("winget");

// Verify it's the same instance
Console.WriteLine(ReferenceEquals(wingetService, sameService)); // True
```

### Concurrent Usage
```csharp
// Multiple threads requesting the same service
var factory = new PackageServiceFactory();

var tasks = Enumerable.Range(0, 10).Select(i => Task.Run(() =>
{
    // All threads will get the same cached instance
    var service = factory.GetService("winget");
    Console.WriteLine($"Thread {i}: Got service {service.GetHashCode()}");
}));

await Task.WhenAll(tasks);
// All threads log the same hash code - proving they got the same instance
```

### Using with Dependency Injection
```csharp
// Register factory in DI container
services.AddSingleton<PackageServiceFactory>();

// Inject and use in a class
public class PackageManager
{
    private readonly PackageServiceFactory _factory;
    
    public PackageManager(PackageServiceFactory factory)
    {
        _factory = factory;
    }
    
    public async Task LoadPackages(string source)
    {
        var service = _factory.GetService(source);
        var packages = await service.GetAvailablePackagesAsync();
        // Process packages...
    }
}
```

## Thread Safety Analysis

### Why Thread-Safe?
```csharp
// ConcurrentDictionary provides thread-safe operations:
// 1. TryGetValue - Thread-safe read
// 2. TryAdd - Atomic add operation
// 3. Indexer access - Thread-safe read

// Potential race condition is handled:
if (_services.TryGetValue(serviceType, out var service))
{
    return service; // Safe
}

// Between here, another thread might add the same key...

if (_services.TryAdd(serviceType, newService))
{
    return newService; // We added it
}

// If TryAdd fails, another thread added it first
return _services[serviceType]; // Return their instance
```

## Performance Considerations

### Memory Usage
- **Benefit**: Reduced memory usage by reusing instances
- **Trade-off**: Factory holds references, preventing garbage collection
- **Solution**: Implement `ClearCache()` method if needed

### Creation Overhead
- **First Request**: Full object creation cost
- **Subsequent Requests**: Dictionary lookup only (O(1))
- **Net Benefit**: Significant for expensive object creation

## Testing

### Unit Tests
```csharp
[Fact]
public void GetService_SameType_ReturnsSameInstance()
{
    // Arrange
    var factory = new PackageServiceFactory();
    
    // Act
    var service1 = factory.GetService("winget");
    var service2 = factory.GetService("winget");
    
    // Assert
    Assert.Same(service1, service2);
}

[Fact]
public void GetService_DifferentTypes_ReturnsDifferentInstances()
{
    // Arrange
    var factory = new PackageServiceFactory();
    
    // Act
    var wingetService = factory.GetService("winget");
    var scoopService = factory.GetService("scoop");
    
    // Assert
    Assert.NotSame(wingetService, scoopService);
}

[Fact]
public async Task GetService_ConcurrentAccess_ReturnsSameInstance()
{
    // Arrange
    var factory = new PackageServiceFactory();
    var services = new ConcurrentBag<IPackageService>();
    
    // Act
    var tasks = Enumerable.Range(0, 100).Select(_ => Task.Run(() =>
    {
        services.Add(factory.GetService("winget"));
    }));
    
    await Task.WhenAll(tasks);
    
    // Assert
    var firstService = services.First();
    Assert.All(services, service => Assert.Same(firstService, service));
}
```

### Mock Factory for Testing
```csharp
public class MockPackageServiceFactory : PackageServiceFactory
{
    private readonly Dictionary<string, IPackageService> _mocks = new();
    
    public void RegisterMock(string serviceType, IPackageService mockService)
    {
        _mocks[serviceType] = mockService;
    }
    
    public override IPackageService GetService(string serviceType)
    {
        return _mocks.ContainsKey(serviceType) 
            ? _mocks[serviceType] 
            : base.GetService(serviceType);
    }
}
```

## Related Patterns

- **Singleton Pattern**: Factory itself can be a singleton
- **Strategy Pattern**: Created objects often implement strategy interfaces
- **Dependency Injection**: Factory can be registered in DI container

## Common Mistakes

### ❌ Creating New Instances Every Time
```csharp
// Wrong: No caching, creates duplicate instances
public IPackageService GetService(string serviceType)
{
    return serviceType switch
    {
        "winget" => new WinGetService(), // New instance every time!
        "scoop" => new ScoopService(),
        _ => throw new ArgumentException()
    };
}
```

### ✅ Correct: Cache and Reuse
```csharp
// Correct: Cache instances in ConcurrentDictionary
public IPackageService GetService(string serviceType)
{
    if (_services.TryGetValue(serviceType, out var service))
    {
        return service; // Return cached instance
    }
    // Create and cache new instance...
}
```

### ❌ Not Thread-Safe
```csharp
// Wrong: Regular Dictionary is not thread-safe
private readonly Dictionary<string, IPackageService> _services = new();

public IPackageService GetService(string serviceType)
{
    if (!_services.ContainsKey(serviceType))
    {
        _services[serviceType] = CreateService(serviceType); // Race condition!
    }
    return _services[serviceType];
}
```

### ✅ Correct: Use Concurrent Collections
```csharp
// Correct: ConcurrentDictionary handles thread safety
private readonly ConcurrentDictionary<string, IPackageService> _services = new();
```

## Real-World Usage in UniGetUI

In UniGetUI, the Factory Pattern is used for:

1. **SourceFactory**: Creates and caches package source instances
2. **Manager Creation**: Creates package manager instances
3. **Helper Creation**: Creates helper class instances

Example from `SourceFactory.cs`:
```csharp
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
```

## Further Reading

- [Factory Pattern - Gang of Four](https://en.wikipedia.org/wiki/Factory_method_pattern)
- [C# Design Patterns](https://refactoring.guru/design-patterns/factory-method/csharp/example)
- [Concurrent Collections](https://learn.microsoft.com/en-us/dotnet/standard/collections/thread-safe/)
- [UniGetUI Architecture](../../docs/codebase-analysis/01-overview/architecture.md)

## Building This Example

```bash
cd pattern-examples/factory-pattern
dotnet build
dotnet run
```

## License

Part of the UniGetUI project examples.
