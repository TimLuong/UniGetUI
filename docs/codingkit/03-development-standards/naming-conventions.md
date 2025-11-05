# Naming Conventions

## Overview

This guide provides comprehensive naming conventions for all code artifacts in UniGetUI. Consistent naming improves code readability, maintainability, and reduces cognitive load when navigating the codebase.

## General Principles

1. **Be Descriptive**: Names should clearly convey purpose and intent
2. **Be Consistent**: Follow the same patterns throughout the codebase
3. **Avoid Abbreviations**: Use full words unless the abbreviation is well-known (e.g., URL, HTTP, API)
4. **Use Domain Language**: Prefer terminology from the package management domain

## Variables and Parameters

### Local Variables
**Convention**: camelCase

```csharp
// Good examples
string userName = "John";
int packageCount = 10;
List<IPackage> installedPackages = new();
bool isUpdateAvailable = false;

// Bad examples
string UserName;  // PascalCase is for public members
string usrnm;     // Unclear abbreviation
int x;            // Non-descriptive
```

### Method Parameters
**Convention**: camelCase

```csharp
// Good examples
public void InstallPackage(string packageId, string version, bool forceReinstall)
{
    // Implementation
}

// Bad examples
public void InstallPackage(string PackageId, string V, bool b)
{
    // Bad: PascalCase, abbreviation, non-descriptive
}
```

### Boolean Variables
**Prefix**: Use is, has, can, should, or similar prefixes

```csharp
// Good examples
bool isInstalled = true;
bool hasUpdates = false;
bool canUninstall = CheckPermissions();
bool shouldRetry = true;

// Bad examples
bool installed;   // Unclear if it's a state or action
bool updates;     // Could be noun or verb
```

## Fields

### Private Fields
**Convention**: _camelCase (underscore prefix)

```csharp
public class PackageManager
{
    // Good examples
    private readonly IPackageManager _manager;
    private string _cachePath;
    private int _retryCount;
    private bool _isInitialized;
    
    // Bad examples
    private string cachePath;     // Missing underscore
    private string _CachePath;    // PascalCase after underscore
}
```

### Internal Fields
**Convention**: _camelCase (underscore prefix)

```csharp
internal string _configurationPath;
internal List<IPackage> _packageCache;
```

### Public Fields
**Convention**: PascalCase (but prefer properties)

```csharp
// Acceptable for public readonly fields
public readonly string DefaultSourceUrl = "https://api.example.com";

// Better: Use properties instead
public string DefaultSourceUrl { get; } = "https://api.example.com";
```

### Static Fields
**Convention**: _camelCase for private, PascalCase for public

```csharp
private static readonly ConcurrentDictionary<int, Task> _tasks = new();
public static readonly string Version = "3.0.0";
```

### Constants
**Convention**: PascalCase

```csharp
// Good examples
private const int MaxRetryAttempts = 3;
private const string DefaultLocale = "en";
public const int TimeoutSeconds = 30;

// Bad examples
private const int MAX_RETRY_ATTEMPTS = 3;  // SCREAMING_SNAKE_CASE (not C# convention)
private const int maxRetryAttempts = 3;    // camelCase (not for constants)
```

## Methods

### Public Methods
**Convention**: PascalCase, descriptive verb phrases

```csharp
// Good examples
public void RefreshPackageList()
public async Task<List<IPackage>> GetInstalledPackagesAsync()
public bool ValidatePackageId(string packageId)
public void ClearCache()

// Bad examples
public void refresh();          // Not PascalCase
public void Get();              // Not descriptive
public void DoStuff();          // Too vague
```

### Private Methods
**Convention**: PascalCase or _camelCase (project uses _camelCase for private helpers)

```csharp
// Project standard for private helper methods
private void _initializePackageManagers()
private async Task<string> _downloadManifestAsync()
private bool _validateConfiguration()

// Also acceptable (standard C#)
private void InitializePackageManagers()
private async Task<string> DownloadManifestAsync()
```

### Async Methods
**Convention**: Suffix with "Async"

```csharp
// Good examples
public async Task LoadPackagesAsync()
public async Task<string> GetDataAsync()
public async Task DownloadFileAsync(string url)

// Bad examples
public async Task LoadPackages()       // Missing Async suffix
public async Task LoadPackagesTask()   // Don't use "Task" suffix
```

### Event Handlers
**Convention**: On{EventName} or {Object}_{Event}

```csharp
// Good examples
private void OnPackageInstalled(object sender, EventArgs e)
private void InstallButton_Click(object sender, RoutedEventArgs e)
private void PackageLoader_PackagesChanged(object sender, EventArgs e)

// Bad examples
private void HandleClick()             // Doesn't follow pattern
private void ButtonClick()             // Missing object name
```

## Properties

### Public Properties
**Convention**: PascalCase

```csharp
// Good examples
public string DisplayName { get; set; }
public int PackageCount { get; private set; }
public bool IsInitialized { get; }
public List<IPackage> Packages { get; init; }

// Bad examples
public string displayName { get; set; }    // Not PascalCase
public string name { get; set; }           // Too generic
```

### Boolean Properties
**Convention**: Use is, has, can prefixes

```csharp
// Good examples
public bool IsRunning { get; set; }
public bool HasErrors { get; private set; }
public bool CanExecute { get; }

// Bad examples
public bool Running { get; set; }          // Missing prefix
public bool Errors { get; set; }           // Ambiguous
```

## Classes and Interfaces

### Classes
**Convention**: PascalCase, noun or noun phrase

```csharp
// Good examples
public class PackageManager
public class SourceFactory
public class InstallOperation
public class TaskRecycler<T>

// Bad examples
public class packageManager         // Not PascalCase
public class Install                // Too generic (verb)
public class PMgr                   // Unclear abbreviation
```

### Interfaces
**Convention**: PascalCase with 'I' prefix

```csharp
// Good examples
public interface IPackageManager
public interface ISourceFactory
public interface IPackageLoader

// Bad examples
public interface PackageManager     // Missing 'I' prefix
public interface IpackageManager    // Not PascalCase after 'I'
```

### Abstract Classes
**Convention**: PascalCase, often with "Base" or "Abstract" prefix/suffix

```csharp
// Good examples
public abstract class BasePackageManager
public abstract class AbstractOperation
public abstract class PackageLoaderBase

// Bad examples
public abstract class PackageManager    // Should indicate it's abstract/base
```

## Enumerations

### Enum Types
**Convention**: PascalCase, singular noun

```csharp
// Good examples
public enum PackageStatus
public enum InstallationScope
public enum ManagerType

// Bad examples
public enum PackageStatuses         // Should be singular
public enum packageStatus           // Not PascalCase
```

### Enum Values
**Convention**: PascalCase

```csharp
// Good examples
public enum PackageStatus
{
    NotInstalled,
    Installing,
    Installed,
    UpdateAvailable,
    Failed
}

// Bad examples
public enum PackageStatus
{
    NOT_INSTALLED,                  // SCREAMING_SNAKE_CASE
    installing,                     // camelCase
}
```

## Namespaces

### Convention
**Convention**: PascalCase with dot notation

```csharp
// Good examples
namespace UniGetUI.Core.Classes
namespace UniGetUI.PackageEngine.Interfaces
namespace UniGetUI.PackageEngine.Managers.WinGet
namespace UniGetUI.Interface.Enums

// Bad examples
namespace unigetui.core             // Not PascalCase
namespace UniGetUI_Core             // Use dots, not underscores
namespace Core                      // Too generic, missing project prefix
```

### Structure
- Start with project name: `UniGetUI`
- Follow with module: `Core`, `PackageEngine`, `Interface`
- Then sub-modules: `Classes`, `Managers`, etc.
- Specific implementations last: `WinGet`, `Scoop`, etc.

## Files

### Class Files
**Convention**: {ClassName}.cs

```csharp
// Good examples
PackageManager.cs
IPackageManager.cs
SourceFactory.cs
TaskRecycler.cs

// Bad examples
package_manager.cs              // Use PascalCase, not snake_case
packagemanager.cs               // Should match class case exactly
PackageManagerClass.cs          // Don't add "Class" suffix
```

### Test Files
**Convention**: {ClassName}Tests.cs

```csharp
// Good examples
PackageManagerTests.cs
TaskRecyclerTests.cs
SourceFactoryTests.cs

// Bad examples
PackageManagerTest.cs           // Use plural "Tests"
TestPackageManager.cs           // Don't prefix with "Test"
PackageManager_Tests.cs         // Don't use underscores
```

### XAML Files
**Convention**: PascalCase, matching code-behind

```csharp
// Good examples
MainWindow.xaml / MainWindow.xaml.cs
SettingsPage.xaml / SettingsPage.xaml.cs
PackageDetailsControl.xaml / PackageDetailsControl.xaml.cs

// Bad examples
mainwindow.xaml                 // Not PascalCase
Main_Window.xaml                // Don't use underscores
```

## Generic Type Parameters

### Convention
**Convention**: Single uppercase letter (T, TKey, TValue) or descriptive with 'T' prefix

```csharp
// Good examples - Single letter for simple cases
public class Repository<T>
public class Dictionary<TKey, TValue>
public class TaskRecycler<ReturnT>

// Good examples - Descriptive for complex cases
public class Cache<TItem, TKey>
public interface IConverter<TSource, TDestination>

// Bad examples
public class Repository<Item>           // Should use T prefix
public class Cache<t>                   // Should be uppercase
```

## Acronyms and Abbreviations

### General Rule
- 2 letters: All caps (e.g., `IO`, `UI`)
- 3+ letters: Only first letter caps (e.g., `Http`, `Xml`, `Json`)

```csharp
// Good examples
public class HttpClient
public class XmlParser
public class JsonConverter
public string ToHtmlString()
public void ProcessIOOperations()

// Bad examples
public class HTTPClient              // Should be Http
public class XMLParser               // Should be Xml
public string ToHTMLString()         // Should be Html
```

### Common Acronyms
- **API** → Api (in identifiers)
- **URL** → Url
- **HTTP** → Http
- **JSON** → Json
- **XML** → Xml
- **CLI** → Cli
- **GUI** → Gui
- **ID** → Id (not ID)
- **IO** → IO (exception: 2 letters)
- **UI** → UI (exception: 2 letters)

## Event Names

### Events
**Convention**: PascalCase, past or present tense

```csharp
// Good examples - Past tense (preferred for "after" events)
public event EventHandler PackageInstalled;
public event EventHandler UpdateCompleted;

// Good examples - Present progressive (for "during" events)
public event EventHandler PackageInstalling;
public event EventHandler ProgressChanged;

// Bad examples
public event EventHandler OnPackageInstalled;    // Don't prefix with "On"
public event EventHandler InstallPackage;        // Should be past/progressive
```

### Event Args
**Convention**: {EventName}EventArgs

```csharp
// Good examples
public class PackageInstalledEventArgs : EventArgs
public class ProgressChangedEventArgs : EventArgs

// Bad examples
public class PackageInstalled                   // Missing EventArgs suffix
public class PackageEventArgs                   // Not specific enough
```

## XAML Naming

### Control Names (x:Name)
**Convention**: PascalCase with type suffix

```csharp
// Good examples
<Button x:Name="InstallButton" />
<TextBox x:Name="SearchTextBox" />
<ListView x:Name="PackagesListView" />
<Grid x:Name="MainContentGrid" />

// Bad examples
<Button x:Name="btnInstall" />          // Hungarian notation
<TextBox x:Name="searchBox" />          // Not PascalCase
<ListView x:Name="Packages" />          // Missing type suffix
```

## Special Cases

### Factory Methods
**Convention**: Create{Type} or Get{Type}

```csharp
// Good examples
public IPackageManager CreatePackageManager()
public IManagerSource GetSourceOrDefault(string name)

// Bad examples
public IPackageManager NewPackageManager()      // Use "Create"
public IManagerSource Source(string name)       // Not descriptive
```

### Extension Methods
**Convention**: Descriptive verb, first parameter should be `this`

```csharp
// Good examples
public static string ToTitleCase(this string value)
public static bool IsInstalled(this IPackage package)

// Bad examples
public static string TitleCase(string value)    // Missing "To" verb
```

### Helper Methods
**Convention**: Verb + object

```csharp
// Good examples
private void ValidateInput()
private string FormatPackageName(string name)
private bool CheckPermissions()

// Bad examples
private void Validate()                 // What is being validated?
private string Format(string name)      // Too generic
```

## Summary

### Quick Reference

| Item | Convention | Example |
|------|------------|---------|
| Local variables | camelCase | `userName`, `packageCount` |
| Parameters | camelCase | `packageId`, `version` |
| Private fields | _camelCase | `_manager`, `_cachePath` |
| Properties | PascalCase | `DisplayName`, `IsEnabled` |
| Methods | PascalCase | `LoadPackages()`, `GetDataAsync()` |
| Classes | PascalCase | `PackageManager`, `InstallOperation` |
| Interfaces | IPascalCase | `IPackageManager`, `ISourceFactory` |
| Enums | PascalCase | `PackageStatus`, `ManagerType` |
| Enum values | PascalCase | `NotInstalled`, `Installing` |
| Namespaces | PascalCase.Dot | `UniGetUI.Core.Classes` |
| Files | PascalCase.ext | `PackageManager.cs` |
| Constants | PascalCase | `MaxRetryAttempts`, `DefaultTimeout` |
| Type parameters | TPascalCase | `T`, `TKey`, `ReturnT` |

### When in Doubt

1. **Check existing code** - Look for similar patterns in the codebase
2. **Be consistent** - Follow the same pattern as surrounding code
3. **Prioritize clarity** - Choose readability over brevity
4. **Follow C# conventions** - When not specified, use standard C# conventions

## Related Documentation

- [Coding Standards](./coding-standards.md) - Overall coding conventions
- [Design Patterns](./design-patterns.md) - Common patterns used in the codebase
- [Error Handling](./error-handling.md) - Exception handling standards
