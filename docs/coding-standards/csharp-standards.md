# C# Coding Standards

This document outlines the C# coding standards and best practices for the UniGetUI project.

## Table of Contents

1. [Naming Conventions](#naming-conventions)
2. [Formatting Standards](#formatting-standards)
3. [Async/Await Patterns](#asyncawait-patterns)
4. [File Organization](#file-organization)
5. [Code Quality](#code-quality)
6. [Anti-Patterns to Avoid](#anti-patterns-to-avoid)
7. [Common Pitfalls](#common-pitfalls)

## Naming Conventions

### General Rules

- **PascalCase** for class names, method names, properties, and public fields
- **camelCase** for local variables and method parameters
- **PascalCase** for constants (instead of UPPER_CASE)
- **_camelCase** for private fields (with underscore prefix) - only when necessary
- Use meaningful, descriptive names that convey intent

### Classes and Interfaces

```csharp
// ✅ Good
public class PackageManager { }
public interface IPackageRepository { }

// ❌ Bad
public class pkgMgr { }
public interface PackageRepository { } // Missing 'I' prefix for interface
```

### Methods

```csharp
// ✅ Good
public async Task<Package> GetPackageByIdAsync(string packageId)
{
    // Implementation
}

// ❌ Bad
public async Task<Package> get_package(string id) // snake_case is not C# convention
{
    // Implementation
}
```

### Properties and Fields

```csharp
// ✅ Good
public string PackageName { get; set; }
private readonly ILogger _logger;
private int _retryCount;

// ❌ Bad
public string packageName { get; set; } // Should be PascalCase
private ILogger logger; // Private field should have underscore prefix
```

### Local Variables and Parameters

```csharp
// ✅ Good
public void ProcessPackage(string packageId, bool forceUpdate)
{
    var downloadPath = GetDownloadPath();
    int retryAttempts = 3;
}

// ❌ Bad
public void ProcessPackage(string PackageId, bool ForceUpdate) // Parameters should be camelCase
{
    var DownloadPath = GetDownloadPath(); // Local variable should be camelCase
}
```

### Namespaces

- Use meaningful, hierarchical namespace names
- Follow the pattern: `CompanyName.ProductName.Feature.SubFeature`

```csharp
// ✅ Good
namespace UniGetUI.PackageEngine.Managers.WinGet
{
    // Implementation
}

// ❌ Bad
namespace WinGet // Too generic
{
    // Implementation
}
```

## Formatting Standards

### Indentation and Spacing

- Use **4 spaces** for indentation (not tabs)
- Place opening braces `{` on a new line (Allman style)
- Add one blank line between method definitions
- Add spaces around operators

```csharp
// ✅ Good
public class PackageManager
{
    public async Task<bool> InstallPackageAsync(string packageId)
    {
        if (string.IsNullOrEmpty(packageId))
        {
            throw new ArgumentNullException(nameof(packageId));
        }

        var result = await DownloadPackageAsync(packageId);
        return result.IsSuccess;
    }

    public void UninstallPackage(string packageId)
    {
        // Implementation
    }
}

// ❌ Bad
public class PackageManager {
    public async Task<bool> InstallPackageAsync(string packageId) {
        if(string.IsNullOrEmpty(packageId)){throw new ArgumentNullException(nameof(packageId));}
        var result=await DownloadPackageAsync(packageId);
        return result.IsSuccess;
    }
    public void UninstallPackage(string packageId){
        // Implementation
    }
}
```

### File Organization

```csharp
// 1. Using statements (sorted alphabetically)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniGetUI.Core.Classes;

// 2. Namespace declaration
namespace UniGetUI.PackageEngine.Managers
{
    // 3. Class documentation (if needed)
    /// <summary>
    /// Manages package installation and updates.
    /// </summary>
    public class PackageManager
    {
        // 4. Constants
        private const int MaxRetryAttempts = 3;

        // 5. Private fields
        private readonly ILogger _logger;
        private int _retryCount;

        // 6. Constructors
        public PackageManager(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // 7. Properties
        public string ManagerName { get; set; }
        public bool IsEnabled { get; set; }

        // 8. Public methods
        public async Task<bool> InstallAsync(string packageId)
        {
            // Implementation
        }

        // 9. Private methods
        private async Task<bool> DownloadAsync(string url)
        {
            // Implementation
        }
    }
}
```

### Line Length

- Keep lines under **120 characters** when possible
- Break long lines at logical points (after commas, before operators)

```csharp
// ✅ Good
var package = await _packageRepository.GetPackageAsync(
    packageId,
    includeMetadata: true,
    includeVersionHistory: true);

// ❌ Bad
var package = await _packageRepository.GetPackageAsync(packageId, includeMetadata: true, includeVersionHistory: true, includeDownloadStats: true);
```

## Async/Await Patterns

### Method Naming

- Always suffix async methods with `Async`
- Return `Task` or `Task<T>` for async methods

```csharp
// ✅ Good
public async Task<Package> GetPackageAsync(string packageId)
{
    return await _repository.FindAsync(packageId);
}

// ❌ Bad
public async Task<Package> GetPackage(string packageId) // Missing 'Async' suffix
{
    return await _repository.FindAsync(packageId);
}
```

### ConfigureAwait

- Use `ConfigureAwait(false)` in library code to avoid deadlocks
- Avoid `ConfigureAwait(false)` in UI code where synchronization context is needed

```csharp
// ✅ Good (library code)
public async Task<Package> GetPackageAsync(string packageId)
{
    var result = await _httpClient.GetAsync(packageId).ConfigureAwait(false);
    return await ParsePackageAsync(result).ConfigureAwait(false);
}

// ✅ Good (UI code)
public async Task UpdateUIAsync()
{
    var data = await GetDataAsync(); // No ConfigureAwait(false)
    UpdateDisplay(data); // Runs on UI thread
}
```

### Avoid Async Void

- Never use `async void` except for event handlers
- Use `async Task` instead

```csharp
// ✅ Good
public async Task ProcessDataAsync()
{
    await FetchDataAsync();
    await SaveDataAsync();
}

// Event handler - async void is acceptable
private async void OnButtonClick(object sender, EventArgs e)
{
    await ProcessDataAsync();
}

// ❌ Bad
public async void ProcessDataAsync() // Don't use async void for non-event handlers
{
    await FetchDataAsync();
}
```

### Cancellation Tokens

- Always accept `CancellationToken` for long-running operations
- Pass cancellation tokens through the call chain

```csharp
// ✅ Good
public async Task<Package> DownloadPackageAsync(
    string packageId,
    CancellationToken cancellationToken = default)
{
    var response = await _httpClient.GetAsync(packageId, cancellationToken);
    return await ParseResponseAsync(response, cancellationToken);
}

// ❌ Bad
public async Task<Package> DownloadPackageAsync(string packageId)
{
    // No way to cancel this operation
    var response = await _httpClient.GetAsync(packageId);
    return await ParseResponseAsync(response);
}
```

### Parallel Operations

```csharp
// ✅ Good - Run operations in parallel
public async Task<IEnumerable<Package>> GetMultiplePackagesAsync(IEnumerable<string> packageIds)
{
    var tasks = packageIds.Select(id => GetPackageAsync(id));
    return await Task.WhenAll(tasks);
}

// ❌ Bad - Sequential operations
public async Task<IEnumerable<Package>> GetMultiplePackagesAsync(IEnumerable<string> packageIds)
{
    var packages = new List<Package>();
    foreach (var id in packageIds)
    {
        packages.Add(await GetPackageAsync(id)); // Awaiting in a loop
    }
    return packages;
}
```

## File Organization

### One Class Per File

- Each class should be in its own file
- File name should match the class name
- Exception: Nested classes and small helper classes

```csharp
// File: PackageManager.cs
namespace UniGetUI.PackageEngine.Managers
{
    public class PackageManager
    {
        // Implementation
    }
}
```

### Using Statements

- Place `using` statements at the top of the file
- Sort alphabetically
- Remove unused using statements
- Use file-scoped namespaces (C# 10+)

```csharp
// ✅ Good
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniGetUI.Core.Classes;

namespace UniGetUI.PackageEngine.Managers;

public class PackageManager
{
    // Implementation
}

// ❌ Bad
namespace UniGetUI.PackageEngine.Managers
{
    using System.Collections.Generic;
    using System;
    using UniGetUI.Core.Classes;
    using System.Linq; // Unused

    public class PackageManager
    {
        // Implementation
    }
}
```

## Code Quality

### Null Checking

- Use null-coalescing operators (`??`, `??=`)
- Use null-conditional operators (`?.`, `?[]`)
- Enable nullable reference types

```csharp
// ✅ Good
public void ProcessPackage(Package? package)
{
    var name = package?.Name ?? "Unknown";
    package?.Dependencies?.ForEach(d => ProcessDependency(d));
}

// ❌ Bad
public void ProcessPackage(Package package)
{
    string name;
    if (package != null && package.Name != null)
    {
        name = package.Name;
    }
    else
    {
        name = "Unknown";
    }
}
```

### Exception Handling

- Catch specific exceptions, not general `Exception`
- Don't swallow exceptions silently
- Use `throw;` to rethrow, not `throw ex;`

```csharp
// ✅ Good
public async Task<Package> GetPackageAsync(string packageId)
{
    try
    {
        return await _repository.FindAsync(packageId);
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "Failed to fetch package {PackageId}", packageId);
        throw; // Preserves stack trace
    }
}

// ❌ Bad
public async Task<Package> GetPackageAsync(string packageId)
{
    try
    {
        return await _repository.FindAsync(packageId);
    }
    catch (Exception ex) // Too broad
    {
        // Silent failure - bad!
        return null;
    }
}
```

### LINQ Usage

- Use LINQ for collection operations
- Prefer method syntax over query syntax
- Avoid unnecessary intermediate collections

```csharp
// ✅ Good
var installedPackages = packages
    .Where(p => p.IsInstalled)
    .OrderBy(p => p.Name)
    .ToList();

// ✅ Also acceptable
var installedPackages = packages
    .Where(p => p.IsInstalled)
    .OrderBy(p => p.Name)
    .Take(10)
    .ToList();

// ❌ Bad - Multiple enumerations
var tempList = packages.Where(p => p.IsInstalled).ToList();
var orderedList = tempList.OrderBy(p => p.Name).ToList();
var finalList = orderedList.Take(10).ToList();
```

### Dispose Pattern

- Implement `IDisposable` for classes managing unmanaged resources
- Use `using` statements or declarations for disposable objects

```csharp
// ✅ Good
public async Task ProcessFileAsync(string filePath)
{
    using var stream = File.OpenRead(filePath);
    await ProcessStreamAsync(stream);
} // stream is automatically disposed

// ✅ Also good
public async Task ProcessFileAsync(string filePath)
{
    using (var stream = File.OpenRead(filePath))
    {
        await ProcessStreamAsync(stream);
    }
}

// ❌ Bad
public async Task ProcessFileAsync(string filePath)
{
    var stream = File.OpenRead(filePath);
    await ProcessStreamAsync(stream);
    // stream is never disposed - resource leak!
}
```

## Anti-Patterns to Avoid

### 1. God Classes

```csharp
// ❌ Bad - Class does too many things
public class PackageManager
{
    public void InstallPackage() { }
    public void UninstallPackage() { }
    public void UpdatePackage() { }
    public void SearchPackages() { }
    public void DownloadPackage() { }
    public void ValidatePackage() { }
    public void LogActivity() { }
    public void SendNotification() { }
    public void UpdateUI() { }
    public void ManageDatabase() { }
}

// ✅ Good - Single responsibility
public class PackageInstaller
{
    public async Task InstallAsync(Package package) { }
}

public class PackageSearchService
{
    public async Task<IEnumerable<Package>> SearchAsync(string query) { }
}
```

### 2. Magic Numbers

```csharp
// ❌ Bad
if (retryCount > 3)
{
    throw new Exception("Too many retries");
}

// ✅ Good
private const int MaxRetryAttempts = 3;

if (retryCount > MaxRetryAttempts)
{
    throw new InvalidOperationException("Maximum retry attempts exceeded");
}
```

### 3. String Concatenation in Loops

```csharp
// ❌ Bad
string result = "";
foreach (var package in packages)
{
    result += package.Name + ", ";
}

// ✅ Good
var result = string.Join(", ", packages.Select(p => p.Name));

// ✅ Also good for complex scenarios
var builder = new StringBuilder();
foreach (var package in packages)
{
    builder.Append(package.Name).Append(", ");
}
string result = builder.ToString();
```

### 4. Ignoring Async Warnings

```csharp
// ❌ Bad
public void ProcessData()
{
    var result = GetDataAsync().Result; // Blocks thread, can cause deadlocks
}

// ✅ Good
public async Task ProcessDataAsync()
{
    var result = await GetDataAsync();
}
```

## Common Pitfalls

### 1. Not Using Nullable Reference Types

```csharp
// ❌ Bad - Possible null reference
public string GetPackageName(Package package)
{
    return package.Name; // What if package is null?
}

// ✅ Good
public string GetPackageName(Package? package)
{
    return package?.Name ?? "Unknown";
}
```

### 2. Mutating Collections During Iteration

```csharp
// ❌ Bad
foreach (var package in packages)
{
    if (package.IsObsolete)
    {
        packages.Remove(package); // Throws InvalidOperationException
    }
}

// ✅ Good
var obsoletePackages = packages.Where(p => p.IsObsolete).ToList();
foreach (var package in obsoletePackages)
{
    packages.Remove(package);
}

// ✅ Better
packages.RemoveAll(p => p.IsObsolete);
```

### 3. Not Checking for Empty Collections

```csharp
// ❌ Bad
var firstPackage = packages.First(); // Throws if empty

// ✅ Good
var firstPackage = packages.FirstOrDefault();
if (firstPackage != null)
{
    // Process package
}

// ✅ Also good
if (packages.Any())
{
    var firstPackage = packages.First();
    // Process package
}
```

### 4. Overusing Inheritance

```csharp
// ❌ Bad - Deep inheritance hierarchy
public class Animal { }
public class Mammal : Animal { }
public class Primate : Mammal { }
public class Ape : Primate { }
public class Human : Ape { }

// ✅ Good - Prefer composition
public interface IMovable
{
    void Move();
}

public interface IFeedable
{
    void Feed();
}

public class Animal : IMovable, IFeedable
{
    public void Move() { }
    public void Feed() { }
}
```

### 5. Not Using CancellationToken

```csharp
// ❌ Bad
public async Task<Data> FetchLargeDatasetAsync()
{
    // No way to cancel this long-running operation
    return await _service.GetDataAsync();
}

// ✅ Good
public async Task<Data> FetchLargeDatasetAsync(CancellationToken cancellationToken = default)
{
    return await _service.GetDataAsync(cancellationToken);
}
```

## Additional Resources

- [C# Coding Conventions (Microsoft)](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [Framework Design Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/)
- [Async/Await Best Practices](https://docs.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming)
