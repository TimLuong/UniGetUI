# Adding Package Managers

## Overview

This guide walks you through the process of integrating a new package manager into UniGetUI. The modular architecture makes it straightforward to add support for new package managers.

## Prerequisites

Before implementing a new package manager:
1. The package manager must have a CLI interface
2. You should understand how the package manager works
3. Familiarize yourself with UniGetUI's architecture
4. Review existing package manager implementations

## Implementation Steps

### Step 1: Create Package Manager Class

Create a new file in `src/UniGetUI.PackageEngine.Managers.{ManagerName}/`

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
        // Implement package search
    }
    
    public IReadOnlyList<IPackage> GetAvailableUpdates()
    {
        // Implement update checking
    }
    
    public IReadOnlyList<IPackage> GetInstalledPackages()
    {
        // Implement installed package enumeration
    }
}
```

### Step 2: Implement Helper Classes

Create the three required helpers:
- `MyPackageDetailsHelper` - Handle package metadata
- `MyPackageOperationHelper` - Handle install/uninstall operations
- `MyPackageSourcesHelper` - Handle package sources/repositories

### Step 3: Register with PEInterface

Add your manager to `PEInterface`:

```csharp
public static MyPackageManager MyPackageManager { get; } = new();
```

### Step 4: Add Detection Logic

Implement logic to detect if the package manager is installed:

```csharp
public bool IsAvailable()
{
    // Check if CLI tool exists in PATH
    // Return true if package manager is installed
}
```

### Step 5: Implement CLI Execution

Use the CLI execution helpers to run package manager commands:

```csharp
private async Task<List<IPackage>> LoadPackagesAsync()
{
    var process = new Process();
    process.StartInfo.FileName = "mypm";
    process.StartInfo.Arguments = "list --json";
    // Execute and parse output
}
```

### Step 6: Parse Output

Parse the package manager's output format into `IPackage` objects:

```csharp
private List<IPackage> ParsePackages(string output)
{
    // Parse JSON/XML/text output
    // Create Package objects
    // Return list
}
```

### Step 7: Add Tests

Create test file `MyPackageManagerTests.cs` in test project.

### Step 8: Update Documentation

Add your package manager to:
- README.md
- This documentation
- Technology Stack documentation

## Example: Minimal Implementation

See existing implementations for examples:
- **WinGet** - Most complex, uses COM API
- **Scoop** - JSON-based, good reference
- **Pip** - Simple text parsing

## Best Practices

- Implement fail-safe patterns (return empty lists on error)
- Use TaskRecycler for expensive operations
- Log operations using Core.Logger
- Handle missing package manager gracefully
- Support offline operation when possible

## Testing

Test your implementation with:
- Package search
- Package installation
- Package updates
- Package uninstallation
- Source management

## Related Documentation

- [Package Engine](../04-core-systems/package-engine.md)
- [Design Patterns](../03-development-standards/design-patterns.md)
- [Coding Standards](../03-development-standards/coding-standards.md)
