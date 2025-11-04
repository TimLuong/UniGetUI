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

### Naming Conventions

#### Variables & Functions
```csharp
// camelCase for local variables and parameters
string userName = "John";
int packageCount = 10;

// PascalCase for public methods
public void SearchForUpdates() { }
public Task<string> GetUserDataAsync() { }
```

#### Fields
```csharp
// _camelCase with underscore prefix for private/internal fields
private readonly IPackageManager _manager;
internal string _cachePath;

// PascalCase for public fields (prefer properties)
public string DisplayName { get; set; }
```

#### Classes & Interfaces
```csharp
// PascalCase for classes
public class PackageManager { }
public class SourceFactory { }

// PascalCase with 'I' prefix for interfaces
public interface IPackageManager { }
public interface ISourceFactory { }
```

#### Constants
```csharp
// PascalCase for constants in C#
private const int MaxRetryAttempts = 3;
public const string DefaultLocale = "en";
```

#### Namespaces
```csharp
// PascalCase with dot notation for hierarchical organization
namespace UniGetUI.Core.Classes
namespace UniGetUI.PackageEngine.Interfaces
namespace UniGetUI.PackageEngine.Managers.WinGet
```

#### Files
- **Classes/Interfaces:** `PackageManager.cs`, `IPackageManager.cs` (PascalCase)
- **Tests:** `TaskRecyclerTests.cs`, `ObservableQueueTests.cs` (*Tests.cs suffix)
- **XAML Files:** `SourceManager.xaml` (PascalCase)

### Code Organization

#### File Structure
```csharp
// 1. Using directives
using System.Collections.Concurrent;
using UniGetUI.Core.Logging;

// 2. Namespace (file-scoped preferred)
namespace UniGetUI.Core.Classes;

// 3. Class/Interface definition
public class TaskRecycler<T>
{
    // 4. Private fields
    private static readonly ConcurrentDictionary<int, Task<T>> _tasks = new();
    
    // 5. Public properties
    public string Name { get; set; }
    
    // 6. Constructors
    public TaskRecycler() { }
    
    // 7. Public methods
    public Task<T> RunOrAttachAsync() { }
    
    // 8. Private methods
    private static async Task<T> _runTaskAndWait() { }
}
```

#### Namespace Style
```csharp
// File-scoped namespace declarations (preferred)
namespace UniGetUI.Core.Classes;

public class MyClass { }

// Traditional style (also acceptable)
namespace UniGetUI.Core.Classes
{
    public class MyClass { }
}
```

#### Function Length
- **Guideline:** Keep methods focused and concise
- **Preference:** Smaller, single-purpose functions
- Extract complex logic into private helper methods

### Comments & Documentation

#### XML Documentation Comments
```csharp
/// <summary>
/// Returns the existing source for the given name, or creates a new one if it does not exist.
/// </summary>
/// <param name="name">The name of the source</param>
/// <returns>A valid ManagerSource</returns>
public IManagerSource GetSourceOrDefault(string name)
{
    // Implementation
}
```

#### Inline Comments
```csharp
// Comments should explain "why", not "what"
// Good: Explain intent
// Race condition, an equivalent task got added from another thread
// between the TryGetValue and TryAdd

// Avoid: Stating the obvious
// Bad: i++  // Increment i
```

#### Code Documentation Blocks
```csharp
/*
 * This static class can help reduce the CPU
 * impact of calling a CPU-intensive method
 * that is expected to return the same result when
 * called twice concurrently.
 */
```

### Error Handling

#### Try-Catch Pattern
```csharp
// Always wrap potentially failing operations in try-catch
public void LoadLanguage(string lang)
{
    try
    {
        Locale = "en";
        if (LanguageData.LanguageReference.ContainsKey(lang))
        {
            Locale = lang;
        }
        MainLangDict = LoadLanguageFile(Locale);
    }
    catch (Exception ex)
    {
        Logger.Error($"Could not load language file \"{lang}\"");
        Logger.Error(ex);
    }
}
```

#### Fail-Safe Methods
```csharp
// Methods should return safe defaults on error
// Example from IPackageManager interface:
/// <summary>
/// This method is fail-safe and will return an empty array if an error occurs.
/// </summary>
public IReadOnlyList<IPackage> GetInstalledPackages();
```

#### Logging Errors
```csharp
// Use Logger class for error tracking
try
{
    await riskyOperation();
}
catch (Exception ex)
{
    Logger.Error("Operation failed");
    Logger.Error(ex);
    // Optionally rethrow or return default value
}
```

### Async/Await Standards

#### Async Method Naming
```csharp
// Suffix async methods with 'Async'
public async Task DownloadUpdatedLanguageFile(string langKey)
public async Task<string> GetUserDataAsync()
```

#### ConfigureAwait Usage
```csharp
// Use ConfigureAwait(false) for library code to avoid deadlocks
await task.ConfigureAwait(false);
ReturnT result = await task.ConfigureAwait(false);
```

#### Task Return Types
```csharp
// Use Task for void-like async methods
public async Task RefreshPackageIndexes() { }

// Use Task<T> for async methods with return values
public async Task<string> GetDataAsync() { }
```

### Testing Standards

#### Test File Organization
- **Location:** Separate test projects with `.Tests` suffix
  - `UniGetUI.Core.Classes.Tests`
  - `UniGetUI.Core.Settings.Tests`
- **Naming:** `{ClassName}Tests.cs` pattern
  - `TaskRecyclerTests.cs`
  - `ObservableQueueTests.cs`

#### Test Framework
- **Framework:** xUnit
- **Test attribute:** `[Fact]` for simple tests

#### Test Method Structure
```csharp
[Fact]
public async Task TestTaskRecycler_Static_Int()
{
    // Arrange
    var task1 = TaskRecycler<int>.RunOrAttachAsync(MySlowMethod1);
    var task2 = TaskRecycler<int>.RunOrAttachAsync(MySlowMethod1);
    
    // Act
    int result1 = await task1;
    int result2 = await task2;
    
    // Assert
    Assert.Equal(result1, result2);
}
```

#### Test Naming
```csharp
// Pattern: Test{MethodName}_{Scenario}_{ExpectedResult}
[Fact]
public async Task TestTaskRecycler_Static_Int()

[Fact]
public void TestEnqueue_AddsItem_RaisesEvent()
```

### Code Style

#### Linter & Formatter
- **Configuration:** `.editorconfig` in src directory
- **Base:** Derived from dotnet/aspnetcore standards
- **Code Analysis:** Enabled in build via `EnforceCodeStyleInBuild`

#### Key Style Rules
```csharp
// 1. Indent with 4 spaces
// 2. Use UTF-8 encoding
// 3. Trim trailing whitespace
// 4. Insert final newline

// 5. Braces on new line for all constructs
if (condition)
{
    DoSomething();
}

// 6. Sort using directives with System first
using System;
using System.Collections.Generic;
using UniGetUI.Core.Classes;

// 7. File-scoped namespaces preferred
namespace UniGetUI.Core.Classes;

// 8. Use var when type is apparent
var manager = new PackageManager();
string explicitType = GetString();  // Not apparent
```

#### Modifier Order
```csharp
// Standard order: public, private, protected, internal, static, extern, 
// new, virtual, abstract, sealed, override, readonly, unsafe, volatile, async
public static readonly string DefaultValue = "test";
private async Task DoWorkAsync() { }
```

### Nullable Reference Types

#### Null-Safety
```csharp
// Nullable reference types are enabled project-wide
<Nullable>enable</Nullable>

// Use NotNull attribute for guarantees
[NotNull]
public string? Locale { get; private set; }

// Null-conditional operators
ItemEnqueued?.Invoke(this, new EventArgs(item));
```

### Git Commit Messages

#### Format from CONTRIBUTING.md
```
Brief description of changes

Optional longer description explaining the changes in more detail.
Can span multiple lines if needed.

Related issue: TimLuong/UniGetUI#123
```

#### Best Practices
- Commits should be focused on a single feature or section
- Code in each commit should be executable
- Commit names must be self-explanatory
- Reference related issues when applicable

#### Examples
```
Add TaskRecycler class for CPU optimization

Implemented a thread-safe task caching mechanism to reduce
CPU impact when calling expensive methods concurrently.

Related issue: TimLuong/UniGetUI#45
```

```
Fix language file loading error handling

Improved error handling in LoadLanguage method to prevent
crashes when language files are missing or corrupted.
```

## Performance Best Practices

### Task Recycling
- Use `TaskRecycler<T>` for CPU-intensive operations that may be called concurrently
- Reduces redundant computation when multiple callers request the same data simultaneously
- Example: Getting installed packages, loading package details

### Concurrent Collections
- Use `ConcurrentDictionary<TKey, TValue>` for thread-safe caching
- Provides better performance than lock-based Dictionary in multi-threaded scenarios
- Used in SourceFactory and TaskRecycler implementations

### Async/Await Best Practices
- Use `ConfigureAwait(false)` in library code to avoid context switching overhead
- Prefer async all the way (don't block on async code with `.Result` or `.Wait()`)
- Use `Task.Delay()` instead of `Thread.Sleep()` in async methods

### Memory Optimization
- Reuse object instances through factories where appropriate
- Use readonly fields for immutable data
- Leverage span-based APIs for string operations when possible (per CA diagnostics)

## Security Best Practices

### Input Validation
- Validate user inputs before processing
- Use fail-safe methods that return safe defaults on error
- Check for null/empty strings before operations

### Error Information Disclosure
- Log detailed errors internally using Logger class
- Avoid exposing sensitive information in error messages to users
- Use generic error messages for user-facing scenarios

### Async Operations Safety
- Always wrap async operations in try-catch blocks
- Use CancellationToken for long-running operations (per CA2016)
- Forward CancellationTokens through async call chains

### Code Analysis
- Project uses extensive CA (Code Analysis) rules from Microsoft
- Build enforces code style and security diagnostics
- Examples include:
  - CA2007: ConfigureAwait usage
  - CA2016: Forward CancellationToken parameters
  - CA1822: Mark members static when possible
  - CA1510-1513: Use modern exception throw helpers

### Dependency Management
- Dependencies defined in `Directory.Build.props`
- Target framework: .NET 8.0 (Windows 10.0.26100.0)
- Platform minimum version: Windows 10 (10.0.19041.0)
