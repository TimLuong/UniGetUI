# GitHub Copilot Instructions for UniGetUI

This document provides comprehensive guidelines for GitHub Copilot when working on the UniGetUI project. It consolidates all architectural patterns, coding standards, and best practices to ensure consistent, high-quality code generation.

## Project Overview

**UniGetUI** (formerly WingetUI) is a modern Windows desktop application that provides a unified GUI for managing packages across multiple package managers including WinGet, Scoop, Chocolatey, Pip, Npm, .NET Tool, PowerShell Gallery, Cargo, and Vcpkg.

**Key Technologies:**
- C# 12 with .NET 8.0
- WinUI 3 (Windows App SDK 1.7.250606001)
- Target Platform: Windows 10/11 (10.0.19041.0+)
- Architecture: Modular layered desktop application

## Architectural Patterns

### 1. Layered Architecture

The application follows a strict layered architecture with clear separation of concerns:

```
┌─────────────────────────────────────────┐
│   UI Layer (UniGetUI)                   │
│   - XAML views, pages, controls         │
│   - WinUI 3 components                  │
└─────────────────────────────────────────┘
            ↓
┌─────────────────────────────────────────┐
│   Core Services (UniGetUI.Core.*)       │
│   - Settings, Logging, Language         │
│   - IconEngine, Tools, Data             │
└─────────────────────────────────────────┘
            ↓
┌─────────────────────────────────────────┐
│   Package Engine (PackageEngine.*)      │
│   - Unified package management API      │
│   - Operations, Loaders, Classes        │
└─────────────────────────────────────────┘
            ↓
┌─────────────────────────────────────────┐
│   Package Manager Adapters              │
│   - WinGet, Scoop, Chocolatey, etc.     │
│   - CLI command execution & parsing     │
└─────────────────────────────────────────┘
```

**When generating code:**
- Respect layer boundaries - don't mix concerns across layers
- UI code should not directly access package manager implementations
- Use `PEInterface` as the facade for package operations
- Core services should be UI-agnostic and reusable

### 2. Interface-Based Design (Strategy Pattern)

All package managers implement the `IPackageManager` interface:

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

**When creating new package managers:**
- Always implement the `IPackageManager` interface
- Use helper classes to delegate specific responsibilities
- Keep the main manager class focused on coordination
- Implement all interface methods (use fail-safe patterns returning empty collections on error)

### 3. Factory Pattern

Use factories for creating and caching instances:

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

**When implementing factories:**
- Use `ConcurrentDictionary` for thread-safe caching
- Implement singleton or static instance patterns appropriately
- Handle concurrent access scenarios with proper locking or concurrent collections

### 4. Observer Pattern

Use events for state change notifications:

```csharp
public class ObservableQueue<T> : Queue<T>
{
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
}
```

**When implementing observable patterns:**
- Use C# events following the EventHandler pattern
- Use null-conditional operators (`?.`) for safe event invocation
- Create meaningful EventArgs classes with relevant data
- Avoid memory leaks by properly unsubscribing from events

### 5. Task Recycling Pattern (Performance Optimization)

Reduce CPU impact by sharing task results for concurrent identical operations:

```csharp
public static class TaskRecycler<ReturnT>
{
    private static readonly ConcurrentDictionary<int, Task<ReturnT>> _tasks = new();
    
    public static Task<ReturnT> RunOrAttachAsync(Func<ReturnT> method, int cacheTimeSecs = 0)
    {
        int hash = method.GetHashCode();
        if (_tasks.TryGetValue(hash, out Task<ReturnT>? _task))
        {
            return _task;  // Attach to existing task
        }
        return _runTaskAndWait(new Task<ReturnT>(method), hash, cacheTimeSecs);
    }
}
```

**When to use TaskRecycler:**
- CPU-intensive operations that may be called concurrently
- Operations expected to return the same result (e.g., getting installed packages)
- Situations where multiple UI components request the same data simultaneously

## Coding Standards

### Naming Conventions

```csharp
// Variables and parameters: camelCase
string userName = "John";
int packageCount = 10;

// Methods: PascalCase
public void SearchForUpdates() { }
public async Task<string> GetUserDataAsync() { }

// Private fields: _camelCase with underscore prefix
private readonly IPackageManager _manager;
internal string _cachePath;

// Public properties: PascalCase
public string DisplayName { get; set; }

// Classes: PascalCase
public class PackageManager { }

// Interfaces: PascalCase with 'I' prefix
public interface IPackageManager { }

// Constants: PascalCase (C# convention)
private const int MaxRetryAttempts = 3;
public const string DefaultLocale = "en";

// Namespaces: PascalCase with dot notation
namespace UniGetUI.Core.Classes
namespace UniGetUI.PackageEngine.Managers.WinGet
```

### File Organization

```csharp
// 1. Using directives (System first, then alphabetically)
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UniGetUI.Core.Logging;

// 2. File-scoped namespace (preferred)
namespace UniGetUI.Core.Classes;

// 3. Class definition
public class MyClass
{
    // 4. Private fields
    private static readonly ConcurrentDictionary<int, Task> _tasks = new();
    
    // 5. Public properties
    public string Name { get; set; }
    
    // 6. Constructors
    public MyClass() { }
    
    // 7. Public methods
    public void DoSomething() { }
    
    // 8. Private methods
    private void _helperMethod() { }
}
```

### Modern C# Features

**Use file-scoped namespaces (C# 10):**
```csharp
namespace UniGetUI.Core.Classes;

public class MyClass { }
```

**Use primary constructors (C# 12) when appropriate:**
```csharp
public class EventArgs(T item)
{
    public readonly T Item = item;
}
```

**Use target-typed new expressions (C# 9):**
```csharp
ConcurrentDictionary<int, Task> _tasks = new();
ManagerSource source = new(__manager, name, __default_uri);
```

**Use nullable reference types:**
```csharp
// Enabled project-wide: <Nullable>enable</Nullable>
public string? Locale { get; private set; }

// Use null-conditional operators
ItemEnqueued?.Invoke(this, eventArgs);

// Use null-coalescing for defaults
string value = dict.GetValueOrDefault(key) ?? "";
```

### Async/Await Standards

```csharp
// Suffix async methods with 'Async'
public async Task DownloadFileAsync(string url) { }
public async Task<string> GetDataAsync() { }

// Use ConfigureAwait(false) in library code
await task.ConfigureAwait(false);
ReturnT result = await task.ConfigureAwait(false);

// Return Task/Task<T> appropriately
public async Task RefreshPackagesAsync() { }
public async Task<string> FetchDataAsync() { }
```

### Error Handling

```csharp
// Always wrap potentially failing operations
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

// Methods should return safe defaults on error
public IReadOnlyList<IPackage> GetInstalledPackages()
{
    try
    {
        return LoadPackages();
    }
    catch (Exception ex)
    {
        Logger.Error("Failed to get installed packages");
        Logger.Error(ex);
        return Array.Empty<IPackage>();  // Fail-safe
    }
}
```

### Comments and Documentation

```csharp
/// <summary>
/// Returns the existing source for the given name, or creates a new one if it does not exist.
/// </summary>
/// <param name="name">The name of the source</param>
/// <returns>A valid ManagerSource instance</returns>
public IManagerSource GetSourceOrDefault(string name)
{
    // Race condition - an equivalent task got added from another thread
    // between the TryGetValue and TryAdd
    if (__reference.TryGetValue(name, out IManagerSource? source))
    {
        return source;
    }
    // Implementation continues...
}
```

**Guidelines:**
- Use XML documentation comments for public APIs
- Inline comments should explain "why", not "what"
- Avoid stating the obvious in comments
- Document complex algorithms and business logic

## Database and Data Patterns

### File-Based Storage System

UniGetUI uses a file-based persistence system instead of traditional databases:

**Storage Types:**
1. **Boolean Settings**: Empty files (presence = true, absence = false)
2. **Value Settings**: Text files containing string values
3. **Structured Data**: JSON files for dictionaries and lists

**Location:**
- Settings: `%LOCALAPPDATA%\UniGetUI\Configuration\`
- Icon Cache: `%LOCALAPPDATA%\UniGetUI\CachedMetadata\`
- Legacy: `~/.wingetui/`

### Settings Management

```csharp
// Read boolean setting
bool autoUpdate = Settings.Get(Settings.K.AutomaticallyUpdatePackages);

// Write boolean setting
Settings.Set(Settings.K.AutomaticallyUpdatePackages, true);

// Read string value
string proxyUrl = Settings.GetValue(Settings.K.ProxyURL);

// Write string value
Settings.SetValue(Settings.K.ProxyURL, "http://proxy:8080");

// Read dictionary
var ignoredUpdates = Settings.GetDictionary<string, string>(Settings.K.IgnoredPackageUpdates);

// Write dictionary
Settings.SetDictionary(Settings.K.IgnoredPackageUpdates, updatesDict);
```

**When working with settings:**
- All settings are cached in-memory using `ConcurrentDictionary`
- Thread-safe by default - no manual locking required
- Use typed settings keys from `Settings.K` enum
- Settings persist automatically on write

### Ignored Updates Database

```csharp
// Add package to ignored updates
string ignoredId = IgnoredUpdatesDatabase.GetIgnoredIdForPackage(package);
IgnoredUpdatesDatabase.Add(ignoredId, "*");  // Ignore all versions

// Check if updates are ignored
bool isIgnored = IgnoredUpdatesDatabase.HasUpdatesIgnored(ignoredId, version);

// Remove from ignored list
IgnoredUpdatesDatabase.Remove(ignoredId);
```

**Key Format:** `{manager.Name.ToLower()}\\{package.Id}`

**Value Options:**
- `"*"` - All versions ignored permanently
- `"1.2.3"` - Specific version ignored
- `"<2025-12-31"` - Ignored until date (temporary)

### Icon Database

```csharp
// Get icon URL for package
string? iconUrl = IconDatabase.Instance.GetIconUrlForId(packageId);

// Get screenshot URLs
string[] screenshots = IconDatabase.Instance.GetScreenshotsUrlForId(packageId);
```

**Implementation details:**
- Downloads from GitHub on startup
- Falls back to local cache if download fails
- Singleton pattern ensures single instance
- In-memory dictionary for O(1) lookups

## UI/UX Guidelines

### WinUI 3 Best Practices

**XAML Structure:**
```xml
<Page
    x:Class="UniGetUI.Pages.DiscoverPackagesPage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    mc:Ignorable="d">
    
    <Grid>
        <!-- Content here -->
    </Grid>
</Page>
```

**Code-behind:**
```csharp
namespace UniGetUI.Pages;

public sealed partial class DiscoverPackagesPage : Page
{
    public DiscoverPackagesPage()
    {
        InitializeComponent();
    }
}
```

**When creating UI components:**
- Use WinUI 3 controls from Microsoft.UI.Xaml
- Leverage CommunityToolkit.WinUI for additional controls
- Follow MVVM-inspired patterns (View and ViewModel separation)
- Keep UI responsive - use async operations for long-running tasks
- Use proper accessibility attributes (AutomationProperties)

### Localization

```csharp
// Get translated string
string text = CoreTools.Translate("Settings");

// Translate with formatting
string message = CoreTools.Translate("Package {0} installed successfully", packageName);
```

**When adding new strings:**
- All user-facing strings must be translatable
- Use `CoreTools.Translate()` for all UI text
- Never hard-code English strings in UI code
- Add translation keys to language files via translation management system

## Security Standards

### Input Validation

```csharp
// Validate user input before processing
public bool ValidatePackageId(string packageId)
{
    if (string.IsNullOrWhiteSpace(packageId))
    {
        Logger.Warn("Package ID cannot be empty");
        return false;
    }
    
    // Additional validation logic
    return true;
}
```

### Secure Settings

```csharp
// Use SecureSettings for sensitive data
SecureSettings.SetValue("ApiKey", encryptedKey);
string apiKey = SecureSettings.GetValue("ApiKey");
```

**When handling sensitive data:**
- Use `UniGetUI.Core.SecureSettings` for credentials and API keys
- Never log sensitive information
- Use encrypted storage for passwords and tokens
- Validate and sanitize all external input

### Command Execution Safety

```csharp
// When executing CLI commands
// - Validate all parameters
// - Use proper escaping for command arguments
// - Log command execution for debugging (without sensitive data)
// - Handle process timeouts appropriately
// - Capture and handle stderr output
```

### Code Analysis

The project enforces security through code analysis rules:
- CA2007: ConfigureAwait usage
- CA2016: Forward CancellationToken parameters
- CA1822: Mark members static when possible
- CA1510-1513: Use modern exception throw helpers

**Always address code analysis warnings** - they often indicate potential bugs or security issues.

## Testing Standards

### Test Organization

```
src/
├── UniGetUI.Core.Classes/
│   └── TaskRecycler.cs
├── UniGetUI.Core.Classes.Tests/
│   └── TaskRecyclerTests.cs
```

**Naming:** `{ClassName}Tests.cs`

### Test Framework: xUnit

```csharp
using Xunit;

namespace UniGetUI.Core.Classes.Tests;

public class TaskRecyclerTests
{
    [Fact]
    public async Task TestTaskRecycler_Static_Int()
    {
        // Arrange
        var task1 = TaskRecycler<int>.RunOrAttachAsync(MySlowMethod);
        var task2 = TaskRecycler<int>.RunOrAttachAsync(MySlowMethod);
        
        // Act
        int result1 = await task1;
        int result2 = await task2;
        
        // Assert
        Assert.Equal(result1, result2);
    }
    
    private static int MySlowMethod()
    {
        Thread.Sleep(1000);
        return 42;
    }
}
```

**Test Structure:**
- Arrange: Set up test data and dependencies
- Act: Execute the operation being tested
- Assert: Verify expected outcomes

**Test Naming:** `Test{MethodName}_{Scenario}_{ExpectedResult}`

**When writing tests:**
- Cover both positive and negative cases
- Test edge cases and boundary conditions
- Use async/await for asynchronous operations
- Keep tests isolated and independent
- Mock external dependencies appropriately

## Performance Considerations

### Task Recycling

Use `TaskRecycler<T>` for CPU-intensive operations:

```csharp
// Avoid duplicate work when called concurrently
var packages = await TaskRecycler<List<IPackage>>.RunOrAttachAsync(
    () => LoadInstalledPackages(),
    cacheTimeSecs: 60
);
```

### Concurrent Collections

```csharp
// Thread-safe caching without locks
private readonly ConcurrentDictionary<string, IManagerSource> _cache = new();

// Thread-safe operations
_cache.TryAdd(key, value);
_cache.TryGetValue(key, out var value);
```

### Async Best Practices

```csharp
// Use ConfigureAwait(false) in library code
var result = await operation.ConfigureAwait(false);

// Don't block on async code
// BAD: var result = task.Result;
// BAD: task.Wait();
// GOOD: var result = await task;

// Use CancellationToken for long-running operations
public async Task ProcessPackagesAsync(CancellationToken cancellationToken)
{
    foreach (var package in packages)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await ProcessPackageAsync(package, cancellationToken);
    }
}
```

### Memory Optimization

```csharp
// Reuse instances through factories
var source = _sourceFactory.GetSourceOrDefault(sourceName);

// Use readonly fields for immutable data
private readonly IPackageManager _manager;

// Leverage span-based APIs where appropriate
ReadOnlySpan<char> span = text.AsSpan();
```

## Logging Patterns

### Using the Logger

```csharp
using UniGetUI.Core.Logging;

// Log levels
Logger.Debug("Detailed diagnostic information");
Logger.Info("General informational messages");
Logger.Warn("Warning messages for potentially harmful situations");
Logger.Error("Error messages for error events");
Logger.Error(exception);  // Log exception with stack trace

// Formatted logging
Logger.Info($"Package {packageName} version {version} installed");
```

**When to log:**
- **Debug**: Detailed diagnostic information useful for debugging
- **Info**: General operational information (startup, configuration, major operations)
- **Warn**: Potentially harmful situations that don't prevent operation
- **Error**: Error events that might still allow the application to continue
- **Exception objects**: Always log exceptions with full stack traces

**What not to log:**
- Sensitive information (passwords, API keys, personal data)
- Excessive detail in production (avoid log spam)
- Information already available through other means

## Configuration Management

### Application Settings

```csharp
// Settings are managed through the Settings class
Settings.Set(Settings.K.DisableAutoUpdateCheck, true);
bool value = Settings.Get(Settings.K.DisableAutoUpdateCheck);

// Value settings
Settings.SetValue(Settings.K.PreferredLanguage, "en");
string language = Settings.GetValue(Settings.K.PreferredLanguage);
```

### Package Manager Configuration

```csharp
// Disable/enable package managers
var disabledManagers = Settings.GetDictionary<string, bool>(Settings.K.DisabledManagers);
disabledManagers["winget"] = true;
Settings.SetDictionary(Settings.K.DisabledManagers, disabledManagers);

// Custom package manager paths
var managerPaths = Settings.GetDictionary<string, string>(Settings.K.ManagerPaths);
managerPaths["scoop"] = @"C:\CustomPath\scoop.cmd";
Settings.SetDictionary(Settings.K.ManagerPaths, managerPaths);
```

### Portable Mode

When `ForceUniGetUIPortable` file exists in executable directory:
- All data stored in `.\Settings\` relative to executable
- Enables USB drive installations
- Automatic fallback to user directory if not writable

## File and Folder Structure

```
/UniGetUI
├── /src                              # Main source code
│   ├── UniGetUI/                     # Main application
│   │   ├── Pages/                    # XAML pages
│   │   ├── Controls/                 # Custom controls
│   │   ├── Services/                 # UI services
│   │   └── App.xaml                  # Application entry point
│   ├── UniGetUI.Core.*/              # Core services
│   │   ├── Classes/                  # Shared classes
│   │   ├── Data/                     # Data layer
│   │   ├── IconStore/                # Icon management
│   │   ├── LanguageEngine/           # i18n support
│   │   ├── Logger/                   # Logging
│   │   ├── Settings/                 # Configuration
│   │   └── Tools/                    # Utilities
│   ├── UniGetUI.PackageEngine.*/     # Package management
│   │   ├── Interfaces/               # Contracts
│   │   ├── PackageManagerClasses/    # Base classes
│   │   ├── Operations/               # Operations
│   │   ├── PackageLoader/            # Package loading
│   │   └── Managers.*/               # Manager implementations
│   └── *.Tests/                      # Unit tests
├── /docs                             # Documentation
├── /scripts                          # Build scripts
├── /media                            # Assets
├── /.github                          # GitHub configuration
└── [Configuration files]             # Root config files
```

**When adding new files:**
- Follow existing namespace patterns
- Place files in appropriate layer/module
- Create corresponding test files in test projects
- Use PascalCase for file names matching class names

## Code Generation Examples

### Creating a New Package Manager

```csharp
namespace UniGetUI.PackageEngine.Managers.MyPackageManager;

public class MyPackageManager : IPackageManager
{
    public string Name => "MyPackageManager";
    public string DisplayName => "My Package Manager";
    
    public IPackageDetailsHelper DetailsHelper { get; }
    public IPackageOperationHelper OperationHelper { get; }
    public IMultiSourceHelper SourcesHelper { get; }
    
    public MyPackageManager()
    {
        DetailsHelper = new MyPackageDetailsHelper(this);
        OperationHelper = new MyPackageOperationHelper(this);
        SourcesHelper = new MyPackageSourcesHelper(this);
    }
    
    public IReadOnlyList<IPackage> FindPackages(string query)
    {
        try
        {
            // Execute CLI command to search packages
            // Parse output
            // Return list of packages
            return packages;
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to find packages in {Name}");
            Logger.Error(ex);
            return Array.Empty<IPackage>();  // Fail-safe
        }
    }
    
    public IReadOnlyList<IPackage> GetAvailableUpdates()
    {
        // Implementation
        return Array.Empty<IPackage>();
    }
    
    public IReadOnlyList<IPackage> GetInstalledPackages()
    {
        // Implementation
        return Array.Empty<IPackage>();
    }
}
```

### Creating a Settings Page

```xml
<!-- SettingsPage.xaml -->
<Page
    x:Class="UniGetUI.Pages.SettingsPage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:controls="using:CommunityToolkit.WinUI.Controls">
    
    <ScrollViewer>
        <StackPanel Spacing="8" Margin="16">
            <controls:SettingsCard 
                Header="Auto-update packages"
                Description="Automatically update packages when available">
                <ToggleSwitch x:Name="AutoUpdateToggle"
                             Toggled="AutoUpdateToggle_Toggled"/>
            </controls:SettingsCard>
        </StackPanel>
    </ScrollViewer>
</Page>
```

```csharp
// SettingsPage.xaml.cs
namespace UniGetUI.Pages;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
        LoadSettings();
    }
    
    private void LoadSettings()
    {
        AutoUpdateToggle.IsOn = Settings.Get(Settings.K.AutomaticallyUpdatePackages);
    }
    
    private void AutoUpdateToggle_Toggled(object sender, RoutedEventArgs e)
    {
        Settings.Set(Settings.K.AutomaticallyUpdatePackages, AutoUpdateToggle.IsOn);
    }
}
```

### Creating an Async Operation

```csharp
public async Task<List<IPackage>> LoadPackagesAsync(CancellationToken cancellationToken = default)
{
    try
    {
        // Use TaskRecycler for concurrent calls
        return await TaskRecycler<List<IPackage>>.RunOrAttachAsync(
            () => LoadPackagesInternal(cancellationToken),
            cacheTimeSecs: 60
        );
    }
    catch (OperationCanceledException)
    {
        Logger.Info("Package loading was cancelled");
        return new List<IPackage>();
    }
    catch (Exception ex)
    {
        Logger.Error("Failed to load packages");
        Logger.Error(ex);
        return new List<IPackage>();
    }
}

private List<IPackage> LoadPackagesInternal(CancellationToken cancellationToken)
{
    var packages = new List<IPackage>();
    
    // Perform operation
    foreach (var item in items)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // Process item
    }
    
    return packages;
}
```

## Anti-Patterns to Avoid

### ❌ Don't: Block on Async Code

```csharp
// BAD: Blocks thread, can cause deadlocks
var result = asyncMethod().Result;
asyncMethod().Wait();

// GOOD: Await properly
var result = await asyncMethod();
```

### ❌ Don't: Ignore Exceptions

```csharp
// BAD: Swallows exceptions
try { 
    RiskyOperation(); 
} 
catch { }

// GOOD: Log and handle appropriately
try 
{ 
    RiskyOperation(); 
}
catch (Exception ex)
{
    Logger.Error("Operation failed");
    Logger.Error(ex);
    // Take appropriate action
}
```

### ❌ Don't: Hard-Code UI Strings

```csharp
// BAD: Not translatable
MessageBox.Show("Operation completed successfully");

// GOOD: Use translation system
MessageBox.Show(CoreTools.Translate("OperationCompletedSuccessfully"));
```

### ❌ Don't: Mix Concerns Across Layers

```csharp
// BAD: UI code directly accessing package manager
public class MyPage : Page
{
    public void OnButtonClick()
    {
        var packages = new WinGetManager().GetInstalledPackages();
    }
}

// GOOD: Use the package engine interface
public class MyPage : Page
{
    public void OnButtonClick()
    {
        var packages = PEInterface.WinGet.GetInstalledPackages();
    }
}
```

### ❌ Don't: Create Leaky Abstractions

```csharp
// BAD: Exposes implementation details
public string GetRawCliOutput() 
{ 
    return _cliOutput; 
}

// GOOD: Return abstracted data
public IReadOnlyList<IPackage> GetPackages() 
{ 
    return ParseCliOutput(_cliOutput); 
}
```

### ❌ Don't: Forget ConfigureAwait in Library Code

```csharp
// BAD: Can cause deadlocks in some contexts
var data = await FetchDataAsync();

// GOOD: Use ConfigureAwait(false) in library code
var data = await FetchDataAsync().ConfigureAwait(false);
```

### ❌ Don't: Use Improper Locking

```csharp
// BAD: Manual locking with potential for deadlocks
private readonly object _lock = new();
private Dictionary<string, string> _cache;

public string GetValue(string key)
{
    lock (_lock) { return _cache[key]; }
}

// GOOD: Use concurrent collections
private readonly ConcurrentDictionary<string, string> _cache = new();

public string GetValue(string key)
{
    return _cache.GetValueOrDefault(key);
}
```

### ❌ Don't: Return Null for Collections

```csharp
// BAD: Caller must check for null
public List<IPackage>? GetPackages()
{
    if (error) return null;
    return packages;
}

// GOOD: Return empty collection
public IReadOnlyList<IPackage> GetPackages()
{
    if (error) return Array.Empty<IPackage>();
    return packages;
}
```

### ❌ Don't: Ignore Thread Safety

```csharp
// BAD: Not thread-safe
private static int _counter = 0;
public void Increment() { _counter++; }

// GOOD: Use thread-safe operations
private static int _counter = 0;
public void Increment() { Interlocked.Increment(ref _counter); }

// OR BETTER: Use concurrent collections when appropriate
```

## Additional Guidelines

### Git Commit Messages

```
Brief description of changes (50 chars or less)

Optional longer description explaining the changes in more detail.
Can span multiple lines if needed.

Related issue: TimLuong/UniGetUI#123
```

**Best practices:**
- Keep commits focused on a single feature or fix
- Code in each commit should be executable
- Commit names must be self-explanatory
- Reference related issues when applicable

### Code Review Checklist

Before submitting code, ensure:
- [ ] Follows architectural patterns and layer boundaries
- [ ] Adheres to naming conventions and code style
- [ ] Includes appropriate error handling and logging
- [ ] Uses async/await correctly with ConfigureAwait
- [ ] Includes XML documentation for public APIs
- [ ] Has corresponding unit tests (if applicable)
- [ ] Doesn't hard-code strings (uses translation system)
- [ ] Thread-safe when accessed concurrently
- [ ] Returns fail-safe defaults on error
- [ ] Addresses all code analysis warnings

### Performance Guidelines

- Use `TaskRecycler<T>` for expensive concurrent operations
- Use `ConcurrentDictionary` for thread-safe caching
- Leverage `ConfigureAwait(false)` to avoid context switching
- Return early when possible to avoid unnecessary work
- Use `IReadOnlyList` and `ReadOnlySpan` for read-only data
- Cache frequently accessed data appropriately
- Use lazy loading where appropriate

### Documentation Requirements

- Public APIs must have XML documentation comments
- Complex algorithms need explanatory comments
- Architectural decisions should be documented
- Breaking changes must be clearly documented
- Update README.md for user-facing features

## Project-Specific Context

### Package Manager Support

Currently supported package managers:
- **WinGet**: Windows Package Manager (native COM API + CLI)
- **Scoop**: Command-line installer for Windows
- **Chocolatey**: Package manager for Windows
- **Pip**: Python package installer
- **Npm**: Node.js package manager
- **DotNet**: .NET Tool package manager
- **PowerShell/PowerShell7**: PowerShell Gallery
- **Cargo**: Rust package manager
- **Vcpkg**: C++ library manager

### Multi-Language Support

The application supports 50+ languages through the `LanguageEngine`. All UI strings must be translatable using `CoreTools.Translate()`.

### Build and Release

- **Build Tool**: .NET SDK 8.0, Visual Studio 2022
- **Package Format**: MSIX (Microsoft Store), Inno Setup installer
- **Distribution**: Microsoft Store, WinGet, Scoop, Chocolatey, direct download
- **CI/CD**: GitHub Actions for automated testing and builds

### Testing Infrastructure

- **Framework**: xUnit
- **Coverage**: Core classes, settings, package engine
- **CI**: Automated tests run on every PR via GitHub Actions

### Key Dependencies

- Windows App SDK 1.7.250606001
- CommunityToolkit.WinUI 8.2.250402
- YamlDotNet 16.3.0
- Octokit 14.0.0 (GitHub API)
- H.NotifyIcon.WinUI 2.3.0 (system tray)

---

## Quick Reference

### Common Tasks

**Load packages:**
```csharp
var packages = await PEInterface.WinGet.GetInstalledPackagesAsync();
```

**Get settings:**
```csharp
bool value = Settings.Get(Settings.K.SettingName);
string text = Settings.GetValue(Settings.K.SettingName);
```

**Log messages:**
```csharp
Logger.Info("Information message");
Logger.Error(exception);
```

**Translate strings:**
```csharp
string text = CoreTools.Translate("TranslationKey");
```

**Use TaskRecycler:**
```csharp
var result = await TaskRecycler<T>.RunOrAttachAsync(() => ExpensiveOperation());
```

### Important Classes

- `PEInterface` - Package engine facade
- `Settings` - Configuration management
- `Logger` - Logging system
- `CoreTools` - Translation and utilities
- `TaskRecycler<T>` - Performance optimization
- `IconDatabase` - Package icons and screenshots

### Key Interfaces

- `IPackageManager` - Package manager contract
- `IPackage` - Package representation
- `IManagerSource` - Package source/repository
- `IPackageDetailsHelper` - Package metadata
- `IPackageOperationHelper` - Install/uninstall operations

---

This document consolidates all standards and patterns for UniGetUI development. When generating code, always refer to these guidelines to ensure consistency with the project's architecture and coding standards.

For more detailed information, see:
- `/docs/codebase-analysis/` - Detailed codebase documentation
- `/CONTRIBUTING.md` - Contribution guidelines
- `/docs/copilot/customization-guide.md` - Copilot customization instructions
