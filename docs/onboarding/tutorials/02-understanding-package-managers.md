# Tutorial 2: Understanding Package Managers

**Difficulty:** Intermediate  
**Time:** 2-3 hours  
**Prerequisites:** Completed Tutorial 1, Basic understanding of interfaces and inheritance

## 📖 Overview

This tutorial will teach you how package manager integration works in UniGetUI. You'll understand:
- The `IPackageManager` interface
- How different package managers are implemented
- The helper pattern for operations
- How to read and understand an existing package manager implementation

## 🎯 Learning Objectives

By the end of this tutorial, you'll be able to:
- Explain the package manager architecture
- Navigate package manager implementations
- Understand how package operations work
- Identify where to make changes for package manager features
- Prepare for adding a new package manager (Tutorial 4)

## 📚 Package Manager Architecture

### The Core Interface

UniGetUI abstracts all package managers through a common interface:

```csharp
public interface IPackageManager
{
    // Properties
    public ManagerProperties Properties { get; }
    public ManagerCapabilities Capabilities { get; }
    public ManagerStatus Status { get; }
    
    // Helpers for specific operations
    public IPackageDetailsHelper DetailsHelper { get; }
    public IPackageOperationHelper OperationHelper { get; }
    public IMultiSourceHelper SourcesHelper { get; }
    
    // Core operations
    public IReadOnlyList<IPackage> FindPackages(string query);
    public IReadOnlyList<IPackage> GetAvailableUpdates();
    public IReadOnlyList<IPackage> GetInstalledPackages();
}
```

### Why This Design?

**Benefits:**
- **Uniform Interface:** All package managers look the same to the UI
- **Easy Addition:** New package managers just implement this interface
- **Testability:** Can mock package managers for testing
- **Maintainability:** Changes to one manager don't affect others

## 📋 Step 1: Explore an Existing Implementation

### Choose a Package Manager to Study

Let's examine the **WinGet** package manager as it's comprehensive and well-documented.

**Location:** `src/UniGetUI.PackageEngine.Managers.WinGet/`

### List the Files

```bash
cd src/UniGetUI.PackageEngine.Managers.WinGet/
ls -la
```

You'll typically see:
- `WinGet.cs` - Main implementation
- `WinGetOperationHelper.cs` - Handles install/update/uninstall
- `WinGetDetailsHelper.cs` - Fetches package details
- `WinGetSourceHelper.cs` - Manages package sources

### Open the Main File

Open `WinGet.cs` and examine its structure:

```bash
code WinGet.cs
```

## 📋 Step 2: Understanding the Main Class

### Class Declaration

```csharp
public class WinGet : PackageManager
{
    // Constructor initializes properties and capabilities
    public WinGet()
    {
        // Define what this manager can do
        Capabilities = new ManagerCapabilities
        {
            CanRunAsAdmin = true,
            CanSkipIntegrityChecks = true,
            SupportsCustomVersions = true,
            SupportsCustomSources = true,
            // ... more capabilities
        };
        
        // Define manager metadata
        Properties = new ManagerProperties
        {
            Name = "WinGet",
            DisplayName = "WinGet",
            Description = "Microsoft's official package manager...",
            IconId = IconType.WinGet,
            // ... more properties
        };
        
        // Set up helper classes
        DetailsHelper = new WinGetDetailsHelper(this);
        OperationHelper = new WinGetOperationHelper(this);
        SourcesHelper = new WinGetSourceHelper(this);
    }
}
```

### Key Concepts

**ManagerCapabilities:** What operations are supported?
- Can it run as administrator?
- Can it skip integrity checks?
- Does it support custom versions?
- Does it support parallel operations?

**ManagerProperties:** Metadata about the manager
- Display name
- Description
- Icons
- Known sources (repositories)

**Helper Classes:** Separate concerns
- `DetailsHelper`: Gets package information
- `OperationHelper`: Performs install/update/uninstall
- `SourcesHelper`: Manages repositories/sources

## 📋 Step 3: The Helper Pattern

### Why Helpers?

Instead of putting all logic in one giant class, UniGetUI uses helpers to separate concerns:

```
PackageManager (WinGet)
├── DetailsHelper      → Get package details, screenshots, info
├── OperationHelper    → Install, update, uninstall packages
└── SourcesHelper      → Add, remove, list sources
```

This follows the **Single Responsibility Principle**.

### Example: OperationHelper

```csharp
public class WinGetOperationHelper : BasePackageOperationHelper
{
    public WinGetOperationHelper(PackageManager manager) : base(manager) { }
    
    protected override string[] _getOperationParameters(
        Package package,
        IInstallationOptions options,
        OperationType operation)
    {
        List<string> parameters = new();
        
        // Build command based on operation type
        switch (operation)
        {
            case OperationType.Install:
                parameters.Add("install");
                break;
            case OperationType.Update:
                parameters.Add("upgrade");
                break;
            case OperationType.Uninstall:
                parameters.Add("uninstall");
                break;
        }
        
        // Add package identifier
        parameters.Add("--id");
        parameters.Add(package.Id);
        
        // Add options
        if (options.RunAsAdministrator)
            parameters.Add("--admin");
            
        if (options.SkipHashCheck)
            parameters.Add("--ignore-security-hash");
        
        return parameters.ToArray();
    }
}
```

### Understanding the Code

1. **Inherits from base:** `BasePackageOperationHelper` provides common functionality
2. **Takes manager reference:** Each helper knows which manager it belongs to
3. **Builds CLI arguments:** Translates user options into command-line parameters
4. **Returns string array:** These become the actual CLI arguments

## 📋 Step 4: Package Operations Flow

### How Installation Works

Let's trace an installation from start to finish:

```
1. User clicks "Install" in UI
   ↓
2. UI creates InstallationOptions
   ↓
3. UI calls PackageManager.Install(package, options)
   ↓
4. PackageManager delegates to OperationHelper
   ↓
5. OperationHelper builds CLI arguments
   ↓
6. OperationHelper executes the CLI command
   ↓
7. OperationHelper monitors output
   ↓
8. OperationHelper reports progress/completion
   ↓
9. UI updates to show installation complete
```

### Example: Installation Code

```csharp
// Somewhere in the UI layer
var options = new InstallationOptions
{
    RunAsAdministrator = adminCheckbox.IsChecked,
    SkipHashCheck = skipHashCheckbox.IsChecked,
    // ... other options
};

// Initiate installation
await packageManager.Install(package, options);
```

The PackageManager then:
1. Uses `OperationHelper` to build the command
2. Executes: `winget install --id Package.Name --admin`
3. Monitors the process output
4. Reports status back to UI

## 📋 Step 5: Getting Package Details

### DetailsHelper Purpose

When a user clicks on a package, they want to see:
- Full description
- Version information
- Download size
- Screenshots (if available)
- Homepage URL
- License information

### Example: DetailsHelper Implementation

```csharp
public class WinGetDetailsHelper : BasePackageDetailsHelper
{
    public WinGetDetailsHelper(PackageManager manager) : base(manager) { }
    
    protected override async Task<PackageDetails> GetPackageDetails_UnSafe(Package package)
    {
        var details = new PackageDetails
        {
            Name = package.Name,
            Id = package.Id,
        };
        
        // Execute: winget show --id PackageName
        Process process = new Process();
        process.StartInfo.FileName = "winget";
        process.StartInfo.Arguments = $"show --id {package.Id}";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        
        process.Start();
        string output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        
        // Parse output to extract details
        details.Description = ParseDescription(output);
        details.Version = ParseVersion(output);
        details.Homepage = ParseHomepage(output);
        // ... more parsing
        
        return details;
    }
    
    private string ParseDescription(string output)
    {
        // Parse CLI output to extract description
        // Implementation varies by package manager
    }
}
```

### Key Takeaways

- Each package manager has different CLI output formats
- Helpers parse CLI output to extract information
- Async methods for I/O operations (process execution)
- Error handling for when commands fail

## 📋 Step 6: Managing Sources

### What Are Sources?

Sources (or repositories) are where packages come from:
- **WinGet:** winget, msstore
- **Scoop:** main, extras, versions, etc.
- **Chocolatey:** chocolatey.org

### SourcesHelper Example

```csharp
public class WinGetSourceHelper : BaseSourceHelper
{
    protected override async Task<List<ManagerSource>> GetSources_UnSafe()
    {
        var sources = new List<ManagerSource>();
        
        // Execute: winget source list
        string output = await RunCommand("winget", "source list");
        
        // Parse output to extract source names and URLs
        foreach (var line in output.Split('\n'))
        {
            if (IsSourceLine(line))
            {
                var source = ParseSourceLine(line);
                sources.Add(source);
            }
        }
        
        return sources;
    }
    
    protected override async Task AddSource_UnSafe(ManagerSource source)
    {
        // Execute: winget source add --name SourceName --arg URL
        await RunCommand("winget", $"source add --name {source.Name} --arg {source.Url}");
    }
}
```

## 📋 Step 7: Hands-On Exercise

### Exercise 1: Find Package Manager Capabilities

Pick three package managers and compare their capabilities:

```bash
cd src
grep -r "CanRunAsAdmin" UniGetUI.PackageEngine.Managers.*/
grep -r "SupportsCustomVersions" UniGetUI.PackageEngine.Managers.*/
```

**Questions:**
1. Which package managers support running as admin?
2. Which support custom versions?
3. Which support parallel installations?

### Exercise 2: Trace an Installation

Open these files and trace the installation process:
1. `src/UniGetUI.PackageEngine.Managers.WinGet/WinGetOperationHelper.cs`
2. Find the `_getOperationParameters` method
3. List all the command-line arguments it can generate

**Challenge:** What happens if a user selects:
- Install
- Skip hash check
- Install to custom location: "C:\MyApps"

What would the final command look like?

### Exercise 3: Compare Two Managers

Compare WinGet and Scoop implementations:

```bash
# Open both side-by-side
code src/UniGetUI.PackageEngine.Managers.WinGet/WinGet.cs
code src/UniGetUI.PackageEngine.Managers.Scoop/Scoop.cs
```

**Questions:**
1. How do their capabilities differ?
2. How do they find packages differently?
3. Which one has more complex installation logic?

## 📋 Step 8: Understanding Package Objects

### The IPackage Interface

Every package, regardless of source, implements:

```csharp
public interface IPackage
{
    public string Id { get; }           // Unique identifier
    public string Name { get; }         // Display name
    public string Version { get; }      // Current version
    public string NewVersion { get; }   // Available version (for updates)
    public IPackageManager Manager { get; } // Which manager owns this
    public IManagerSource Source { get; }   // Where it came from
}
```

### Package Lifecycle

```
Discovery:
User searches → Manager.FindPackages() → List<IPackage>

Installation:
User clicks Install → Package + Options → Manager.Install()

Updates:
Background check → Manager.GetAvailableUpdates() → List<IPackage> with NewVersion

Removal:
User clicks Uninstall → Package → Manager.Uninstall()
```

## 📋 Step 9: Error Handling

### Fail-Safe Design

Package manager operations can fail for many reasons:
- Package manager not installed
- Network issues
- Permission denied
- Package not found

UniGetUI uses fail-safe methods:

```csharp
/// <summary>
/// This method is fail-safe and will return an empty array if an error occurs.
/// </summary>
public IReadOnlyList<IPackage> GetInstalledPackages()
{
    try
    {
        return GetInstalledPackages_UnSafe();
    }
    catch (Exception ex)
    {
        Logger.Error($"Failed to get installed packages: {ex.Message}");
        return Array.Empty<IPackage>();
    }
}
```

### Best Practices

1. **Wrap external calls in try-catch**
2. **Log errors for debugging**
3. **Return safe defaults** (empty lists, null objects)
4. **Don't crash the application** if one manager fails

## 🎓 What You Learned

Congratulations! You now understand:
- ✅ The `IPackageManager` interface and why it exists
- ✅ How package managers are structured (main class + helpers)
- ✅ How package operations work (install, update, uninstall)
- ✅ How package details are fetched and displayed
- ✅ How sources/repositories are managed
- ✅ The package lifecycle from discovery to removal
- ✅ Error handling patterns

## 🚀 Next Steps

### Immediate Practice
1. **Pick a simple package manager** like Scoop
2. **Read through its implementation** completely
3. **Trace a package installation** from start to finish
4. **Identify potential improvements** (don't implement yet!)

### Continue Learning
- [Tutorial 3: Working with WinUI 3](03-working-with-winui3.md) - Learn the UI layer
- [Tutorial 4: Adding a New Package Manager](04-adding-package-manager.md) - Put it all together
- [Adding Features Guide](../../codebase-analysis/08-extension/adding-features.md) - Full reference

### Challenge Yourself
- Compare all package manager implementations
- Find code duplication across managers
- Think about how you'd add a new manager
- Identify which helpers could be more reusable

## 📚 Additional Resources

- [IPackageManager Interface](../../codebase-analysis/02-core-functionality/entry-point.md)
- [Design Patterns Used](../../codebase-analysis/07-best-practices/patterns-standards.md)
- [Package Engine Architecture](../../codebase-analysis/01-overview/architecture.md)

## ✅ Self-Assessment

Can you answer these questions?

- [ ] What is the purpose of the `IPackageManager` interface?
- [ ] Why are helpers used instead of one big class?
- [ ] How does an install operation flow from UI to CLI?
- [ ] What information does a `PackageDetails` contain?
- [ ] How are sources different from packages?
- [ ] Why do methods return empty lists instead of throwing exceptions?

If yes to all, you're ready for Tutorial 3! 🎉

## 🆘 Need Help?

- Review the [Adding Features Guide](../../codebase-analysis/08-extension/adding-features.md)
- Ask questions in [GitHub Discussions](https://github.com/marticliment/UniGetUI/discussions)
- Study more [existing implementations](../../codebase-analysis/04-code-understanding/)

Happy learning! 📚
