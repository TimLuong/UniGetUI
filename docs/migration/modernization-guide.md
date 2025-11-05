# Legacy Code Modernization Guide

## Overview

This guide provides comprehensive strategies for modernizing legacy Windows applications to align with CodingKit framework standards. Whether you're maintaining an older .NET Framework application, a WinForms project, or an application with technical debt, this guide will help you incrementally adopt modern practices.

## Table of Contents

- [Legacy Code Assessment](#legacy-code-assessment)
- [Modernization Strategies](#modernization-strategies)
- [Incremental Adoption](#incremental-adoption)
- [Refactoring Techniques](#refactoring-techniques)
- [Tools and Resources](#tools-and-resources)
- [Migration Checklist](#migration-checklist)

## Legacy Code Assessment

Before beginning modernization, conduct a thorough assessment of your existing codebase.

### Assessment Criteria

#### 1. Current Framework Version
```powershell
# Check .NET version in .csproj file
Select-String -Path "*.csproj" -Pattern "TargetFramework"
```

**Framework Lifecycle:**
- .NET Framework 4.x → Active support ended, security updates only
- .NET Core 3.1 → End of support: December 2022
- .NET 5/6 → End of support: May 2022 / November 2024
- .NET 7 → End of support: May 2024
- **.NET 8 → LTS, supported until November 2026** ✅ (Recommended)

#### 2. Architecture Evaluation

**Questions to Answer:**
- Is the codebase following a clear architectural pattern (Layered, MVVM, MVC)?
- Are concerns properly separated (UI, business logic, data access)?
- How tightly coupled are the components?
- Are dependencies managed through interfaces or concrete implementations?

**Assessment Matrix:**

| Aspect | Poor (Red Flag) | Fair (Needs Work) | Good (Minor Updates) |
|--------|----------------|-------------------|---------------------|
| Separation of Concerns | Everything in code-behind | Some separation | Clear layered architecture |
| Dependency Management | Static classes everywhere | Some DI in places | Consistent DI throughout |
| Error Handling | Try-catch scattered | Some centralized logging | Comprehensive error strategy |
| Testing | No tests | Some unit tests | Good test coverage |
| Documentation | None or outdated | Some inline comments | Comprehensive docs |

#### 3. Code Quality Metrics

**Tools for Assessment:**
```bash
# Install .NET Analyzer
dotnet add package Microsoft.CodeAnalysis.NetAnalyzers

# Run code analysis
dotnet build /p:EnforceCodeStyleInBuild=true
```

**Key Metrics:**
- **Cyclomatic Complexity**: Target < 10 per method
- **Code Coverage**: Target > 70%
- **Maintainability Index**: Target > 60
- **Lines of Code per Class**: Target < 300

#### 4. Technical Debt Analysis

**Common Technical Debt Indicators:**
- Commented-out code blocks
- TODO/HACK/FIXME comments
- Duplicate code (DRY violations)
- God classes (classes doing too much)
- Long methods (> 50 lines)
- Magic numbers and strings
- Missing error handling
- Deprecated API usage

**Debt Classification:**
```csharp
// Example: Categorize technical debt
public enum TechnicalDebtSeverity
{
    Low,        // Code style issues, minor improvements
    Medium,     // Design pattern violations, moderate refactoring needed
    High,       // Security issues, performance problems
    Critical    // Breaking bugs, data loss risks
}
```

## Modernization Strategies

### Strategy 1: Strangler Fig Pattern

**Concept:** Gradually replace legacy systems by building new functionality around the old, eventually "strangling" it.

**Implementation Steps:**

1. **Identify seams** in your legacy application
2. **Create an abstraction layer** around legacy code
3. **Implement new features** using modern patterns
4. **Route traffic** to new implementations
5. **Deprecate and remove** old code incrementally

**Example:**

```csharp
// Step 1: Legacy code
public class LegacyPackageManager
{
    public void InstallPackage(string name)
    {
        // 500 lines of legacy code
    }
}

// Step 2: Create abstraction
public interface IPackageManager
{
    Task InstallPackageAsync(string name, CancellationToken cancellationToken);
}

// Step 3: Adapter for legacy code (temporary)
public class LegacyPackageManagerAdapter : IPackageManager
{
    private readonly LegacyPackageManager _legacy = new();
    
    public async Task InstallPackageAsync(string name, CancellationToken cancellationToken)
    {
        await Task.Run(() => _legacy.InstallPackage(name), cancellationToken);
    }
}

// Step 4: New modern implementation
public class ModernPackageManager : IPackageManager
{
    private readonly ILogger _logger;
    private readonly IPackageOperationHelper _helper;
    
    public ModernPackageManager(ILogger logger, IPackageOperationHelper helper)
    {
        _logger = logger;
        _helper = helper;
    }
    
    public async Task InstallPackageAsync(string name, CancellationToken cancellationToken)
    {
        try
        {
            await _helper.InstallAsync(name, cancellationToken).ConfigureAwait(false);
            _logger.Information($"Package {name} installed successfully");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to install package {name}");
            throw;
        }
    }
}

// Step 5: Router/Facade to control which implementation is used
public class PackageManagerFacade : IPackageManager
{
    private readonly IPackageManager _activeManager;
    
    public PackageManagerFacade(bool useModernImplementation)
    {
        _activeManager = useModernImplementation 
            ? new ModernPackageManager(logger, helper)
            : new LegacyPackageManagerAdapter();
    }
    
    public Task InstallPackageAsync(string name, CancellationToken cancellationToken)
        => _activeManager.InstallPackageAsync(name, cancellationToken);
}
```

**Benefits:**
- ✅ Low risk - old code continues working
- ✅ Incremental progress
- ✅ Business continuity maintained
- ⚠️ Can create complexity during transition

### Strategy 2: Branch by Abstraction

**Concept:** Create abstractions for areas you want to modernize, implement new versions behind the abstraction, and switch when ready.

**Implementation Example:**

```csharp
// Original tightly coupled code
public class SettingsManager
{
    public string GetSetting(string key)
    {
        // Direct file I/O
        return File.ReadAllText($"C:\\Settings\\{key}.txt");
    }
}

// Step 1: Introduce abstraction
public interface ISettingsProvider
{
    string GetSetting(string key);
    void SetSetting(string key, string value);
}

// Step 2: Wrap legacy implementation
public class FileBasedSettingsProvider : ISettingsProvider
{
    public string GetSetting(string key)
    {
        return File.ReadAllText($"C:\\Settings\\{key}.txt");
    }
    
    public void SetSetting(string key, string value)
    {
        File.WriteAllText($"C:\\Settings\\{key}.txt", value);
    }
}

// Step 3: Create modern implementation
public class ModernSettingsProvider : ISettingsProvider
{
    private readonly ConcurrentDictionary<string, string> _cache = new();
    private readonly string _settingsPath;
    private readonly ILogger _logger;
    
    public ModernSettingsProvider(string settingsPath, ILogger logger)
    {
        _settingsPath = settingsPath;
        _logger = logger;
    }
    
    public string GetSetting(string key)
    {
        return _cache.GetOrAdd(key, k =>
        {
            try
            {
                var filePath = Path.Combine(_settingsPath, $"{k}.txt");
                return File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to read setting: {key}");
                return string.Empty;
            }
        });
    }
    
    public void SetSetting(string key, string value)
    {
        try
        {
            var filePath = Path.Combine(_settingsPath, $"{key}.txt");
            File.WriteAllText(filePath, value);
            _cache[key] = value;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to write setting: {key}");
        }
    }
}

// Step 4: Update all callers to use abstraction
public class Application
{
    private readonly ISettingsProvider _settings;
    
    public Application(ISettingsProvider settings)
    {
        _settings = settings;
    }
    
    public void DoSomething()
    {
        var setting = _settings.GetSetting("MySetting");
    }
}

// Step 5: Switch implementation via DI configuration
// In Program.cs or DI container setup:
// Old: services.AddSingleton<ISettingsProvider, FileBasedSettingsProvider>();
// New: services.AddSingleton<ISettingsProvider, ModernSettingsProvider>();
```

### Strategy 3: Feature Toggle for Gradual Rollout

**Concept:** Deploy new code alongside old code and use feature flags to control which version executes.

**Implementation:**

```csharp
public interface IFeatureFlags
{
    bool IsEnabled(string featureName);
}

public class FeatureFlags : IFeatureFlags
{
    private readonly Dictionary<string, bool> _flags = new()
    {
        ["ModernPackageManager"] = false,  // Start with false
        ["NewUIDesign"] = false,
        ["AsyncOperations"] = true
    };
    
    public bool IsEnabled(string featureName)
    {
        return _flags.TryGetValue(featureName, out bool enabled) && enabled;
    }
}

// Usage in application
public class PackageService
{
    private readonly IFeatureFlags _features;
    private readonly IPackageManager _legacyManager;
    private readonly IPackageManager _modernManager;
    
    public PackageService(
        IFeatureFlags features,
        LegacyPackageManager legacyManager,
        ModernPackageManager modernManager)
    {
        _features = features;
        _legacyManager = legacyManager;
        _modernManager = modernManager;
    }
    
    public async Task InstallPackageAsync(string packageName, CancellationToken cancellationToken)
    {
        var manager = _features.IsEnabled("ModernPackageManager") 
            ? _modernManager 
            : _legacyManager;
            
        await manager.InstallPackageAsync(packageName, cancellationToken);
    }
}
```

**Benefits:**
- ✅ Easy rollback if issues occur
- ✅ A/B testing possible
- ✅ Gradual user migration
- ⚠️ Requires maintaining both code paths temporarily

### Strategy 4: Parallel Run

**Concept:** Run both old and new implementations and compare results to verify correctness.

**Implementation:**

```csharp
public class ParallelRunValidator<T>
{
    private readonly ILogger _logger;
    
    public ParallelRunValidator(ILogger logger)
    {
        _logger = logger;
    }
    
    public async Task<T> RunParallelAsync(
        Func<Task<T>> legacyOperation,
        Func<Task<T>> modernOperation,
        bool useModernResult = false)
    {
        T legacyResult = default!;
        T modernResult = default!;
        Exception? legacyException = null;
        Exception? modernException = null;
        
        // Run both implementations
        var legacyTask = Task.Run(async () =>
        {
            try
            {
                legacyResult = await legacyOperation();
            }
            catch (Exception ex)
            {
                legacyException = ex;
                _logger.Error(ex, "Legacy implementation failed");
            }
        });
        
        var modernTask = Task.Run(async () =>
        {
            try
            {
                modernResult = await modernOperation();
            }
            catch (Exception ex)
            {
                modernException = ex;
                _logger.Error(ex, "Modern implementation failed");
            }
        });
        
        await Task.WhenAll(legacyTask, modernTask);
        
        // Compare results
        if (legacyException == null && modernException == null)
        {
            if (!EqualityComparer<T>.Default.Equals(legacyResult, modernResult))
            {
                _logger.Warning("Results differ between legacy and modern implementations");
                _logger.Debug($"Legacy: {legacyResult}, Modern: {modernResult}");
            }
        }
        
        // Return selected result
        if (useModernResult && modernException == null)
        {
            return modernResult;
        }
        
        if (legacyException != null)
        {
            throw legacyException;
        }
        
        return legacyResult;
    }
}
```

## Incremental Adoption

### Phase 1: Foundation (Weeks 1-2)

**Goal:** Establish modern development practices without breaking existing functionality.

#### Tasks:
1. **Update Development Environment**
   ```bash
   # Install .NET 8 SDK
   winget install Microsoft.DotNet.SDK.8
   
   # Install Visual Studio 2022 17.9+
   winget install Microsoft.VisualStudio.2022.Community
   ```

2. **Add Code Analysis**
   ```xml
   <!-- Add to Directory.Build.props -->
   <PropertyGroup>
       <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
       <EnableNETAnalyzers>true</EnableNETAnalyzers>
       <AnalysisLevel>latest</AnalysisLevel>
   </PropertyGroup>
   ```

3. **Create .editorconfig**
   ```ini
   # See reference: src/.editorconfig in UniGetUI repository
   root = true
   
   [*]
   charset = utf-8
   insert_final_newline = true
   trim_trailing_whitespace = true
   
   [*.cs]
   indent_style = space
   indent_size = 4
   ```

4. **Set Up Version Control Properly**
   ```bash
   # Ensure .gitignore is comprehensive
   # Add common ignore patterns
   bin/
   obj/
   .vs/
   *.user
   ```

5. **Establish CI/CD Pipeline**
   ```yaml
   # .github/workflows/dotnet-test.yml
   name: .NET Tests
   on: [push, pull_request]
   jobs:
     test:
       runs-on: windows-latest
       steps:
         - uses: actions/checkout@v4
         - uses: actions/setup-dotnet@v4
           with:
             dotnet-version: '8.0.x'
         - run: dotnet test
   ```

### Phase 2: Architecture Refactoring (Weeks 3-4)

**Goal:** Introduce proper separation of concerns and dependency injection.

#### Tasks:

1. **Introduce Dependency Injection**
   ```csharp
   // Install Microsoft.Extensions.DependencyInjection
   // In Program.cs or App.xaml.cs:
   
   public partial class App : Application
   {
       private ServiceProvider _serviceProvider;
       
       protected override void OnStartup(StartupEventArgs e)
       {
           base.OnStartup(e);
           
           var services = new ServiceCollection();
           ConfigureServices(services);
           _serviceProvider = services.BuildServiceProvider();
           
           var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
           mainWindow.Show();
       }
       
       private void ConfigureServices(IServiceCollection services)
       {
           // Register services
           services.AddSingleton<ILogger, FileLogger>();
           services.AddSingleton<ISettingsProvider, ModernSettingsProvider>();
           services.AddTransient<IPackageManager, ModernPackageManager>();
           
           // Register windows
           services.AddTransient<MainWindow>();
       }
   }
   ```

2. **Extract Interfaces from Concrete Classes**
   ```csharp
   // Before: Concrete dependency
   public class PackageInstaller
   {
       private FileLogger _logger = new();  // ❌ Tight coupling
   }
   
   // After: Interface dependency
   public class PackageInstaller
   {
       private readonly ILogger _logger;  // ✅ Loose coupling
       
       public PackageInstaller(ILogger logger)
       {
           _logger = logger;
       }
   }
   ```

3. **Layer Your Architecture**
   ```
   Solution Structure:
   ├── YourApp.Core           # Core business logic (no dependencies on UI or data)
   ├── YourApp.Data           # Data access layer
   ├── YourApp.Services       # Application services
   └── YourApp.UI             # User interface (WinUI/WPF)
   ```

### Phase 3: Code Modernization (Weeks 5-6)

**Goal:** Update code to use modern C# features and patterns.

#### 1. Adopt Modern C# Features

```csharp
// ❌ Legacy C# 7.x style
public class PackageInfo
{
    private string name;
    private string version;
    
    public PackageInfo(string name, string version)
    {
        this.name = name;
        this.version = version;
    }
    
    public string Name { get { return name; } }
    public string Version { get { return version; } }
}

// ✅ Modern C# 12 style
public record PackageInfo(string Name, string Version);

// or with class and primary constructor
public class PackageInfo(string name, string version)
{
    public string Name { get; } = name;
    public string Version { get; } = version;
}
```

```csharp
// ❌ Legacy null handling
public string GetPackageName(Package package)
{
    if (package == null)
        return null;
    
    if (package.Metadata == null)
        return null;
        
    return package.Metadata.Name;
}

// ✅ Modern null handling
public string? GetPackageName(Package? package)
{
    return package?.Metadata?.Name;
}
```

#### 2. Convert to Async/Await

```csharp
// ❌ Legacy synchronous code
public List<Package> GetInstalledPackages()
{
    Thread.Sleep(2000);  // Simulating slow operation
    var result = LoadFromDatabase();
    return result;
}

// ✅ Modern async code
public async Task<List<Package>> GetInstalledPackagesAsync(CancellationToken cancellationToken = default)
{
    await Task.Delay(2000, cancellationToken);  // Non-blocking delay
    var result = await LoadFromDatabaseAsync(cancellationToken).ConfigureAwait(false);
    return result;
}
```

#### 3. Implement Proper Error Handling

```csharp
// ❌ Legacy error handling
public void InstallPackage(string name)
{
    try
    {
        DoInstall(name);
    }
    catch (Exception)
    {
        // Silent failure
    }
}

// ✅ Modern error handling
public async Task InstallPackageAsync(string name, CancellationToken cancellationToken)
{
    try
    {
        await DoInstallAsync(name, cancellationToken).ConfigureAwait(false);
        _logger.Information($"Package {name} installed successfully");
    }
    catch (OperationCanceledException)
    {
        _logger.Information($"Installation of {name} was cancelled");
        throw;
    }
    catch (PackageNotFoundException ex)
    {
        _logger.Error(ex, $"Package {name} not found");
        throw;
    }
    catch (Exception ex)
    {
        _logger.Error(ex, $"Failed to install package {name}");
        throw new PackageInstallationException($"Installation failed: {name}", ex);
    }
}
```

### Phase 4: Testing Implementation (Weeks 7-8)

**Goal:** Establish comprehensive test coverage.

#### 1. Set Up Testing Infrastructure

```bash
# Create test project
dotnet new xunit -n YourApp.Tests
dotnet add YourApp.Tests reference YourApp.Core

# Add testing packages
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add package Moq
dotnet add package FluentAssertions
```

#### 2. Write Unit Tests

```csharp
public class PackageManagerTests
{
    private readonly Mock<ILogger> _mockLogger;
    private readonly Mock<IPackageOperationHelper> _mockHelper;
    private readonly ModernPackageManager _sut;  // System Under Test
    
    public PackageManagerTests()
    {
        _mockLogger = new Mock<ILogger>();
        _mockHelper = new Mock<IPackageOperationHelper>();
        _sut = new ModernPackageManager(_mockLogger.Object, _mockHelper.Object);
    }
    
    [Fact]
    public async Task InstallPackageAsync_WithValidName_InstallsSuccessfully()
    {
        // Arrange
        const string packageName = "TestPackage";
        _mockHelper
            .Setup(h => h.InstallAsync(packageName, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        // Act
        await _sut.InstallPackageAsync(packageName, CancellationToken.None);
        
        // Assert
        _mockHelper.Verify(h => h.InstallAsync(packageName, It.IsAny<CancellationToken>()), Times.Once);
        _mockLogger.Verify(
            l => l.Information(It.Is<string>(s => s.Contains("installed successfully"))),
            Times.Once);
    }
    
    [Fact]
    public async Task InstallPackageAsync_WhenHelperThrows_LogsErrorAndRethrows()
    {
        // Arrange
        const string packageName = "FailingPackage";
        var expectedException = new IOException("Disk full");
        _mockHelper
            .Setup(h => h.InstallAsync(packageName, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<IOException>(
            () => _sut.InstallPackageAsync(packageName, CancellationToken.None));
        
        _mockLogger.Verify(
            l => l.Error(expectedException, It.Is<string>(s => s.Contains("Failed to install"))),
            Times.Once);
    }
}
```

#### 3. Integration Tests

```csharp
public class PackageManagerIntegrationTests
{
    [Fact]
    public async Task RealPackageManager_CanDiscoverPackages()
    {
        // Arrange
        var logger = new ConsoleLogger();
        var helper = new RealPackageOperationHelper();
        var manager = new ModernPackageManager(logger, helper);
        
        // Act
        var packages = await manager.GetInstalledPackagesAsync(CancellationToken.None);
        
        // Assert
        Assert.NotNull(packages);
        Assert.NotEmpty(packages);
    }
}
```

## Refactoring Techniques

### Technique 1: Extract Method

**When:** Methods are too long or doing multiple things.

**Before:**
```csharp
public void ProcessPackageInstallation(string packageName)
{
    // Validate package name
    if (string.IsNullOrWhiteSpace(packageName))
        throw new ArgumentException("Package name cannot be empty");
    
    // Check if already installed
    var installedPackages = GetInstalledPackages();
    if (installedPackages.Contains(packageName))
    {
        Console.WriteLine("Package already installed");
        return;
    }
    
    // Download package
    var downloadUrl = $"https://packages.com/{packageName}";
    var packageData = DownloadData(downloadUrl);
    
    // Verify signature
    if (!VerifySignature(packageData))
        throw new SecurityException("Invalid package signature");
    
    // Install package
    ExtractAndInstall(packageData);
    
    // Update registry
    AddToInstalledList(packageName);
    
    Console.WriteLine("Package installed successfully");
}
```

**After:**
```csharp
public async Task ProcessPackageInstallationAsync(string packageName, CancellationToken cancellationToken)
{
    ValidatePackageName(packageName);
    
    if (await IsAlreadyInstalledAsync(packageName, cancellationToken))
    {
        _logger.Information($"Package {packageName} is already installed");
        return;
    }
    
    var packageData = await DownloadPackageAsync(packageName, cancellationToken);
    VerifyPackageSignature(packageData);
    await InstallPackageAsync(packageData, cancellationToken);
    await UpdateInstalledPackageRegistryAsync(packageName, cancellationToken);
    
    _logger.Information($"Package {packageName} installed successfully");
}

private void ValidatePackageName(string packageName)
{
    if (string.IsNullOrWhiteSpace(packageName))
        throw new ArgumentException("Package name cannot be empty", nameof(packageName));
}

private async Task<bool> IsAlreadyInstalledAsync(string packageName, CancellationToken cancellationToken)
{
    var installedPackages = await GetInstalledPackagesAsync(cancellationToken);
    return installedPackages.Contains(packageName);
}

private async Task<byte[]> DownloadPackageAsync(string packageName, CancellationToken cancellationToken)
{
    var downloadUrl = $"https://packages.com/{packageName}";
    return await DownloadDataAsync(downloadUrl, cancellationToken);
}

private void VerifyPackageSignature(byte[] packageData)
{
    if (!VerifySignature(packageData))
        throw new SecurityException("Invalid package signature");
}
```

### Technique 2: Replace Conditional with Polymorphism

**When:** You have complex conditional logic based on type.

**Before:**
```csharp
public class PackageProcessor
{
    public void ProcessPackage(Package package)
    {
        if (package.Type == "winget")
        {
            ProcessWinGetPackage(package);
        }
        else if (package.Type == "chocolatey")
        {
            ProcessChocolateyPackage(package);
        }
        else if (package.Type == "scoop")
        {
            ProcessScoopPackage(package);
        }
    }
}
```

**After:**
```csharp
public interface IPackageProcessor
{
    Task ProcessAsync(Package package, CancellationToken cancellationToken);
}

public class WinGetPackageProcessor : IPackageProcessor
{
    public async Task ProcessAsync(Package package, CancellationToken cancellationToken)
    {
        // WinGet-specific processing
    }
}

public class ChocolateyPackageProcessor : IPackageProcessor
{
    public async Task ProcessAsync(Package package, CancellationToken cancellationToken)
    {
        // Chocolatey-specific processing
    }
}

public class ScoopPackageProcessor : IPackageProcessor
{
    public async Task ProcessAsync(Package package, CancellationToken cancellationToken)
    {
        // Scoop-specific processing
    }
}

// Factory pattern to get the right processor
public class PackageProcessorFactory
{
    private readonly Dictionary<string, IPackageProcessor> _processors;
    
    public PackageProcessorFactory(IEnumerable<IPackageProcessor> processors)
    {
        _processors = processors.ToDictionary(p => p.GetType().Name.Replace("PackageProcessor", "").ToLower());
    }
    
    public IPackageProcessor GetProcessor(string packageType)
    {
        return _processors.TryGetValue(packageType.ToLower(), out var processor)
            ? processor
            : throw new NotSupportedException($"Package type {packageType} is not supported");
    }
}
```

### Technique 3: Introduce Parameter Object

**When:** Methods have too many parameters.

**Before:**
```csharp
public void InstallPackage(
    string name, 
    string version, 
    string source, 
    bool skipDependencies, 
    bool force, 
    string installPath, 
    bool silent,
    bool noRestart)
{
    // Implementation
}
```

**After:**
```csharp
public record InstallOptions
{
    public string Name { get; init; } = string.Empty;
    public string? Version { get; init; }
    public string? Source { get; init; }
    public bool SkipDependencies { get; init; }
    public bool Force { get; init; }
    public string? InstallPath { get; init; }
    public bool Silent { get; init; }
    public bool NoRestart { get; init; }
}

public async Task InstallPackageAsync(InstallOptions options, CancellationToken cancellationToken)
{
    // Implementation
}

// Usage
await InstallPackageAsync(new InstallOptions
{
    Name = "MyPackage",
    Version = "1.2.3",
    Force = true,
    Silent = true
}, cancellationToken);
```

### Technique 4: Replace Magic Numbers/Strings with Constants

**Before:**
```csharp
public class PackageManager
{
    public async Task WaitForInstallation()
    {
        await Task.Delay(5000);  // ❌ What does 5000 mean?
    }
    
    public string GetDefaultSource()
    {
        return "https://packages.microsoft.com";  // ❌ Magic string
    }
}
```

**After:**
```csharp
public class PackageManager
{
    private const int InstallationTimeoutMilliseconds = 5000;
    private const string DefaultPackageSource = "https://packages.microsoft.com";
    
    public async Task WaitForInstallation()
    {
        await Task.Delay(InstallationTimeoutMilliseconds);  // ✅ Clear intent
    }
    
    public string GetDefaultSource()
    {
        return DefaultPackageSource;  // ✅ Named constant
    }
}
```

### Technique 5: Decompose God Classes

**When:** A class has too many responsibilities.

**Before:**
```csharp
// ❌ God class doing everything
public class PackageManager
{
    // UI-related
    public void ShowProgressBar() { }
    public void UpdateStatusMessage(string message) { }
    
    // Business logic
    public void InstallPackage(string name) { }
    public void UninstallPackage(string name) { }
    
    // Data access
    public List<Package> LoadFromDatabase() { }
    public void SaveToDatabase(List<Package> packages) { }
    
    // Network
    public byte[] DownloadPackage(string url) { }
    public void UploadTelemetry(string data) { }
    
    // Configuration
    public void LoadSettings() { }
    public void SaveSettings() { }
}
```

**After:**
```csharp
// ✅ Separate concerns into focused classes

// Business logic
public class PackageOperations
{
    private readonly IPackageRepository _repository;
    private readonly IPackageDownloader _downloader;
    private readonly IProgress<InstallProgress> _progress;
    
    public async Task InstallPackageAsync(string name, CancellationToken cancellationToken)
    {
        // Installation logic
    }
}

// Data access
public class PackageRepository : IPackageRepository
{
    public async Task<List<Package>> LoadAsync(CancellationToken cancellationToken) { }
    public async Task SaveAsync(List<Package> packages, CancellationToken cancellationToken) { }
}

// Network operations
public class PackageDownloader : IPackageDownloader
{
    public async Task<byte[]> DownloadAsync(string url, CancellationToken cancellationToken) { }
}

// Configuration
public class SettingsManager : ISettingsManager
{
    public async Task LoadAsync(CancellationToken cancellationToken) { }
    public async Task SaveAsync(CancellationToken cancellationToken) { }
}

// UI coordination
public class PackageManagerViewModel
{
    private readonly PackageOperations _operations;
    
    public async Task InstallPackageCommand(string packageName)
    {
        ShowProgressBar();
        await _operations.InstallPackageAsync(packageName, CancellationToken.None);
        UpdateStatusMessage("Installation complete");
    }
}
```

## Tools and Resources

### Automated Refactoring Tools

#### 1. Visual Studio Built-in Refactorings
- **Access:** Right-click → Quick Actions (Ctrl+.)
- **Common Refactorings:**
  - Extract Method
  - Rename
  - Extract Interface
  - Move to New File
  - Generate Constructor
  - Encapsulate Field

#### 2. ReSharper (JetBrains)
```bash
# Features:
# - 60+ refactorings
# - Code analysis
# - Code cleanup
# - Navigation improvements
```

#### 3. .NET Upgrade Assistant
```bash
# Install
dotnet tool install -g upgrade-assistant

# Analyze project
upgrade-assistant analyze MyApp.csproj

# Upgrade project
upgrade-assistant upgrade MyApp.csproj
```

#### 4. Code Analysis (Built into .NET SDK)
```xml
<!-- Enable in .csproj or Directory.Build.props -->
<PropertyGroup>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <EnableNETAnalyzers>true</EnableNETAnalyzers>
    <AnalysisLevel>latest</AnalysisLevel>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
</PropertyGroup>
```

#### 5. dotnet format
```bash
# Format code according to .editorconfig
dotnet format

# Check without making changes
dotnet format --verify-no-changes

# Format specific file
dotnet format MyFile.cs
```

### Static Analysis Tools

#### 1. SonarQube / SonarCloud
```yaml
# GitHub Action for SonarCloud
- name: SonarCloud Scan
  uses: SonarSource/sonarcloud-github-action@master
  env:
    GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
    SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
```

#### 2. NDepend
- Dependency analysis
- Complexity metrics
- Technical debt estimation
- Architecture diagrams

#### 3. CodeQL (GitHub)
```yaml
# .github/workflows/codeql.yml
name: "CodeQL"
on:
  push:
  pull_request:
  schedule:
    - cron: '0 0 * * 1'
    
jobs:
  analyze:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4
      - uses: github/codeql-action/init@v3
        with:
          languages: csharp
      - uses: github/codeql-action/autobuild@v3
      - uses: github/codeql-action/analyze@v3
```

### Testing Frameworks

#### Unit Testing
```bash
# xUnit (Recommended for .NET)
dotnet add package xunit
dotnet add package xunit.runner.visualstudio

# Mocking
dotnet add package Moq

# Assertions
dotnet add package FluentAssertions
```

#### Coverage Tools
```bash
# Coverlet
dotnet add package coverlet.collector

# Generate coverage report
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# ReportGenerator for HTML reports
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:coverage.opencover.xml -targetdir:coverage
```

### Documentation Tools

#### 1. DocFX
```bash
# Install
dotnet tool install -g docfx

# Generate documentation
docfx init
docfx build
docfx serve _site
```

#### 2. XML Documentation Comments
```csharp
/// <summary>
/// Installs the specified package asynchronously.
/// </summary>
/// <param name="packageName">The name of the package to install.</param>
/// <param name="cancellationToken">Token to cancel the operation.</param>
/// <returns>A task representing the asynchronous operation.</returns>
/// <exception cref="ArgumentException">Thrown when packageName is null or empty.</exception>
/// <exception cref="PackageNotFoundException">Thrown when the package cannot be found.</exception>
public async Task InstallPackageAsync(string packageName, CancellationToken cancellationToken)
{
    // Implementation
}
```

## Migration Checklist

### Pre-Migration Assessment

- [ ] **Document Current State**
  - [ ] Current .NET version identified
  - [ ] Dependencies catalogued
  - [ ] Architecture documented
  - [ ] Pain points identified
  - [ ] Technical debt assessed

- [ ] **Stakeholder Buy-in**
  - [ ] Business case created
  - [ ] Timeline estimated
  - [ ] Resources allocated
  - [ ] Risk mitigation plan created

- [ ] **Infrastructure Preparation**
  - [ ] Development environment updated
  - [ ] CI/CD pipeline configured
  - [ ] Testing environment prepared
  - [ ] Rollback plan documented

### Phase 1: Foundation (✓ Complete When...)

- [ ] **.NET 8 SDK Installed**
  - [ ] All developers have .NET 8
  - [ ] Build servers updated
  - [ ] Visual Studio 2022 17.9+ installed

- [ ] **Code Analysis Enabled**
  - [ ] .editorconfig created
  - [ ] Directory.Build.props configured
  - [ ] Code style rules defined
  - [ ] Build warnings addressed

- [ ] **Version Control Hygiene**
  - [ ] .gitignore updated
  - [ ] Large binary files removed
  - [ ] Branches strategy defined
  - [ ] Commit message standards set

- [ ] **CI/CD Pipeline**
  - [ ] Automated builds working
  - [ ] Unit tests running on CI
  - [ ] Code coverage reporting
  - [ ] Deployment pipeline tested

### Phase 2: Architecture Refactoring (✓ Complete When...)

- [ ] **Dependency Injection Implemented**
  - [ ] DI container configured
  - [ ] Constructor injection used throughout
  - [ ] Service lifetimes defined
  - [ ] Static dependencies eliminated

- [ ] **Layer Separation**
  - [ ] Core project created (no UI dependencies)
  - [ ] Data access layer extracted
  - [ ] UI layer depends only on interfaces
  - [ ] Business logic isolated

- [ ] **Interface Abstractions**
  - [ ] Key services have interfaces
  - [ ] Dependencies injected via interfaces
  - [ ] Implementation swappable
  - [ ] Testing doubles easy to create

### Phase 3: Code Modernization (✓ Complete When...)

- [ ] **Modern C# Features Adopted**
  - [ ] Nullable reference types enabled
  - [ ] Primary constructors used where appropriate
  - [ ] Record types for DTOs
  - [ ] Pattern matching utilized
  - [ ] File-scoped namespaces applied

- [ ] **Async/Await Implementation**
  - [ ] Long-running operations made async
  - [ ] ConfigureAwait(false) used in library code
  - [ ] CancellationToken support added
  - [ ] No blocking on async code (.Result/.Wait())

- [ ] **Error Handling Improved**
  - [ ] Centralized logging implemented
  - [ ] Custom exceptions defined
  - [ ] Try-catch blocks appropriate
  - [ ] Fail-safe methods return safe defaults

- [ ] **Performance Optimization**
  - [ ] Concurrent collections used
  - [ ] Resource pooling implemented
  - [ ] Caching strategies applied
  - [ ] Memory allocations reduced

### Phase 4: Testing (✓ Complete When...)

- [ ] **Test Infrastructure**
  - [ ] Test projects created
  - [ ] xUnit configured
  - [ ] Mocking framework added
  - [ ] Test data management strategy

- [ ] **Test Coverage**
  - [ ] Core business logic > 80% coverage
  - [ ] Critical paths 100% covered
  - [ ] Edge cases tested
  - [ ] Integration tests for key workflows

- [ ] **Test Automation**
  - [ ] Tests run on every commit
  - [ ] Coverage reports generated
  - [ ] Failed tests block merges
  - [ ] Performance tests automated

### Phase 5: Documentation (✓ Complete When...)

- [ ] **Code Documentation**
  - [ ] XML comments on public APIs
  - [ ] Complex algorithms explained
  - [ ] Architecture decisions recorded
  - [ ] README files updated

- [ ] **Migration Documentation**
  - [ ] Migration steps documented
  - [ ] Known issues catalogued
  - [ ] Troubleshooting guide created
  - [ ] Rollback procedures tested

### Phase 6: Validation (✓ Complete When...)

- [ ] **Functional Testing**
  - [ ] All features work as before
  - [ ] New features tested
  - [ ] Edge cases verified
  - [ ] Performance acceptable

- [ ] **Code Quality**
  - [ ] No code analysis warnings
  - [ ] All tests passing
  - [ ] Code coverage targets met
  - [ ] Code review completed

- [ ] **Production Readiness**
  - [ ] Deployment tested
  - [ ] Rollback tested
  - [ ] Monitoring configured
  - [ ] Documentation complete

### Success Criteria

**✅ Migration Complete When:**
- All projects targeting .NET 8+
- Code analysis enabled with zero warnings
- Test coverage > 70%
- CI/CD pipeline green
- All stakeholders approve
- Production deployment successful
- No critical bugs in first week

## Best Practices Summary

### Do's ✅

1. **Start Small**: Pick one feature or module to modernize first
2. **Automate**: Use tools for refactoring, formatting, and testing
3. **Test First**: Ensure existing functionality is tested before changes
4. **Incremental Changes**: Small, frequent commits are better than large rewrites
5. **Monitor Metrics**: Track code quality, coverage, and performance
6. **Document Decisions**: Keep an ADR (Architecture Decision Record)
7. **Pair Program**: Complex refactoring benefits from collaboration
8. **Code Reviews**: All changes should be reviewed

### Don'ts ❌

1. **Big Bang Rewrites**: Avoid rewriting everything at once
2. **Breaking Changes Without Tests**: Never refactor untested code
3. **Ignoring Technical Debt**: Address debt as you encounter it
4. **Over-Engineering**: Keep solutions simple and focused
5. **Skipping Documentation**: Future you will thank present you
6. **Forcing Patterns**: Use patterns that fit the problem
7. **Neglecting Performance**: Measure before and after changes
8. **Going Alone**: Involve the team in decisions

## Related Resources

### Internal Documentation
- [Framework Upgrade Guide](./framework-upgrade.md)
- [UI Migration Guide](./ui-migration.md)
- [Architecture Overview](../codebase-analysis/01-overview/architecture.md)
- [Coding Standards](../codebase-analysis/07-best-practices/patterns-standards.md)

### External Resources
- [.NET Upgrade Assistant](https://dotnet.microsoft.com/platform/upgrade-assistant)
- [Martin Fowler's Refactoring Catalog](https://refactoring.com/catalog/)
- [Microsoft .NET Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [C# Programming Guide](https://learn.microsoft.com/en-us/dotnet/csharp/)

## Conclusion

Legacy code modernization is a journey, not a destination. By following incremental strategies, leveraging automated tools, and maintaining discipline in testing and documentation, you can successfully modernize your codebase while continuing to deliver value to users.

Remember: **The goal is sustainable progress, not perfection.**

Start with small wins, build momentum, and continuously improve. Each step forward is a step toward a more maintainable, performant, and enjoyable codebase.
