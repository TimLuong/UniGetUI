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

### Modern C# Features

#### Primary Constructors (C# 12)
```csharp
// Used for concise class definitions with constructor parameters
public class EventArgs(T item)
{
    public readonly T Item = item;
}

// Traditional equivalent:
public class EventArgs
{
    public readonly T Item;
    public EventArgs(T item) { this.Item = item; }
}
```

#### File-Scoped Namespaces (C# 10)
```csharp
// Preferred: Reduces indentation
namespace UniGetUI.Core.Classes;

public class MyClass { }
```

#### Target-Typed New (C# 9)
```csharp
// Use when type is obvious from context
ConcurrentDictionary<int, Task> _tasks = new();
ManagerSource new_source = new(__manager, name, __default_uri);
```

### Nullable Reference Types

#### Null-Safety
```csharp
// Nullable reference types are enabled project-wide
<Nullable>enable</Nullable>

// Use NotNull attribute for guarantees
[NotNull]
public string? Locale { get; private set; }

// Null-conditional operators for safe event invocation
ItemEnqueued?.Invoke(this, eventArgs);

// Null-coalescing for defaults
string value = dict.GetValueOrDefault(key) ?? "";
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

