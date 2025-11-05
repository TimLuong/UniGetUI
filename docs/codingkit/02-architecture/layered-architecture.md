# Layered Architecture

> **Note**: This document provides detailed information about UniGetUI's layered architecture pattern. For high-level architecture overview, see [System Architecture](./system-architecture.md).

## Overview

UniGetUI follows a strict **layered architecture** pattern where each layer has specific responsibilities and can only communicate with adjacent layers. This architectural style promotes:

- **Separation of Concerns**: Each layer handles a specific aspect of the application
- **Maintainability**: Changes in one layer have minimal impact on others
- **Testability**: Layers can be tested independently
- **Scalability**: New features can be added without affecting the entire system

## Architecture Layers

### Layer 1: Presentation Layer (UI)
**Namespace**: `UniGetUI`  
**Technology**: WinUI 3 with XAML  
**Responsibility**: User interface and user interaction

**Key Components**:
- MainWindow and MainView
- Pages (Discover, Installed, Updates, Settings)
- Custom Controls
- System tray integration

**Dependencies**:
- Can access: Core Services Layer, Package Engine Layer
- Cannot access: Package Manager Adapters directly

**Communication**:
- Uses `PEInterface` to interact with package management
- Uses Core services for settings, logging, language
- Observes events from Package Loaders for UI updates

### Layer 2: Core Services Layer
**Namespace**: `UniGetUI.Core.*`  
**Technology**: .NET 8.0 C# libraries  
**Responsibility**: Application-wide services and utilities

**Modules**:
- **Core.Settings**: Configuration management
- **Core.LanguageEngine**: Internationalization (i18n)
- **Core.Logger**: Centralized logging
- **Core.IconStore**: Icon caching and management
- **Core.Tools**: Utility functions
- **Core.Classes**: Shared data structures
- **Core.Data**: Application data

**Dependencies**:
- UI-agnostic (no WinUI/XAML dependencies)
- Self-contained or depends only on other Core modules
- Does not depend on Package Engine

**Communication**:
- Provides services to all layers via static classes or singletons
- Fires events for state changes (e.g., settings updated)

### Layer 3: Package Engine Layer
**Namespace**: `UniGetUI.PackageEngine.*`  
**Technology**: .NET 8.0 C# libraries  
**Responsibility**: Unified package management abstraction

**Components**:
- **PEInterface**: Facade providing access to all package managers
- **PackageClasses**: Core package data models (`IPackage`, `Package`)
- **Operations**: Package operations (install, update, uninstall)
- **Loaders**: Package loading and caching
  - DiscoverablePackagesLoader
  - InstalledPackagesLoader
  - UpgradablePackagesLoader
- **Interfaces**: Contracts (`IPackageManager`, `IPackage`)
- **Serializable**: Import/export functionality

**Dependencies**:
- Uses Core Services for logging, settings
- Manages Package Manager Adapters
- UI-agnostic

**Communication**:
- Exposes unified API through `PEInterface`
- Delegates to specific package managers
- Fires events when package states change

### Layer 4: Package Manager Adapters
**Namespace**: `UniGetUI.PackageEngine.Managers.*`  
**Technology**: .NET 8.0 C# + CLI integration  
**Responsibility**: Package manager-specific implementations

**Adapters**:
- WinGet Manager
- Scoop Manager
- Chocolatey Manager
- Pip Manager
- Npm Manager
- .NET Tool Manager
- PowerShell Gallery Manager
- Cargo Manager
- Vcpkg Manager

**Dependencies**:
- Implements `IPackageManager` interface
- Uses Core.Logger for logging
- Interacts with external CLI tools via process execution

**Communication**:
- Accessed only through Package Engine Layer
- Executes CLI commands to external package managers
- Parses CLI output into `IPackage` objects

### Layer 5: External Integration Layer
**Namespace**: `UniGetUI.Interface.*`  
**Technology**: .NET 8.0 with REST API  
**Responsibility**: External services and integrations

**Components**:
- **BackgroundApi**: REST API for widgets and external tools
- **Telemetry**: Anonymous usage analytics
- **GitHubAuthService**: GitHub OAuth integration

**Dependencies**:
- Uses Core Services
- Uses Package Engine for package operations
- Provides external-facing APIs

**Communication**:
- REST endpoints on `localhost:7058`
- Token-based authentication
- CORS-enabled for widget access

## Layer Communication Rules

### Allowed Communication Paths

```
┌─────────────────────────┐
│   UI Layer              │ ◄─── User Interactions
└────────┬────────────────┘
         │ Uses
         ▼
┌─────────────────────────┐
│   Core Services         │ ◄─── Settings, Logging, Language
└────────┬────────────────┘
         │
         ▼
┌─────────────────────────┐
│   Package Engine        │ ◄─── Unified Package API
└────────┬────────────────┘
         │ Manages
         ▼
┌─────────────────────────┐
│   Package Managers      │ ◄─── CLI Integration
└────────┬────────────────┘
         │ Calls
         ▼
┌─────────────────────────┐
│   External Package      │ ◄─── WinGet, Scoop, etc.
│   Repositories          │
└─────────────────────────┘
```

### Prohibited Communication

❌ **UI Layer → Package Manager Adapters**
- UI should never directly instantiate or call package managers
- Use `PEInterface` instead

❌ **Core Services → Package Engine**
- Core services must remain independent
- Package Engine can use Core Services, not vice versa

❌ **Package Managers → UI Layer**
- Package managers should not know about UI
- Communication via events through Package Engine

## Layer Boundaries

### How to Maintain Boundaries

1. **Use Interfaces**: Define contracts at layer boundaries
2. **Avoid Circular Dependencies**: Upper layers depend on lower, not vice versa
3. **Event-Driven Communication**: Use events for upward communication
4. **Facade Pattern**: Use `PEInterface` as the entry point to Package Engine
5. **Dependency Injection**: Pass dependencies explicitly (constructor injection)

### Example: Correct Layer Usage

```csharp
// ❌ WRONG: UI directly accessing package manager
public class DiscoverPage : Page
{
    public void LoadPackages()
    {
        var winget = new WinGetManager();  // Direct access!
        var packages = winget.FindPackages("browser");
    }
}

// ✅ CORRECT: UI using Package Engine facade
public class DiscoverPage : Page
{
    public void LoadPackages()
    {
        var packages = PEInterface.WinGet.FindPackages("browser");
    }
}
```

### Example: Core Services Independence

```csharp
// ❌ WRONG: Core service depending on Package Engine
namespace UniGetUI.Core.Tools
{
    public class CoreHelper
    {
        public void DoSomething()
        {
            var packages = PEInterface.GetInstalledPackages();  // Violates independence!
        }
    }
}

// ✅ CORRECT: Core service remains independent
namespace UniGetUI.Core.Tools
{
    public class CoreHelper
    {
        public void DoSomething()
        {
            // Only use other core services or framework code
            var setting = Settings.Get(Settings.K.SomeSetting);
        }
    }
}
```

## Data Flow Across Layers

### Downward Flow (Command)
1. User action in UI Layer
2. UI calls Package Engine via `PEInterface`
3. Package Engine delegates to appropriate Package Manager
4. Package Manager executes CLI command
5. Result bubbles back up through layers

### Upward Flow (Event)
1. Package Manager completes operation
2. Updates internal state
3. Fires event to Package Loader
4. Package Loader updates cache
5. Fires event to UI Layer
6. UI updates display

## Benefits of Layered Architecture

### Testability
- Each layer can be unit tested independently
- Mock interfaces at layer boundaries
- No need for UI to test business logic

### Maintainability
- Clear responsibilities reduce complexity
- Changes isolated to specific layers
- Easy to locate code by functionality

### Flexibility
- Swap implementations without affecting other layers
- Example: Replace WinUI 3 with another UI framework
- Example: Add new package manager without touching UI

### Scalability
- Layers can evolve independently
- Performance optimizations can target specific layers
- New features fit naturally into existing structure

## Common Pitfalls

### ❌ Layer Violation
**Problem**: Bypassing layers for "convenience"
```csharp
// UI directly calling package manager
var packages = new WinGetManager().GetInstalledPackages();
```

**Solution**: Always use the designated layer interface
```csharp
var packages = PEInterface.WinGet.GetInstalledPackages();
```

### ❌ Circular Dependencies
**Problem**: Lower layer depending on upper layer
```csharp
// Package Manager trying to access UI
public class WinGetManager
{
    public void ShowNotification()
    {
        MainWindow.Instance.ShowToast("Update available");  // ❌
    }
}
```

**Solution**: Use events or callbacks
```csharp
public class WinGetManager
{
    public event EventHandler<string> NotificationNeeded;
    
    public void CheckUpdates()
    {
        NotificationNeeded?.Invoke(this, "Update available");  // ✅
    }
}
```

### ❌ Monolithic Services
**Problem**: One service doing too much
```csharp
// "God class" handling everything
public class PackageService
{
    public void InstallPackage() { }
    public void LogError() { }
    public void SaveSettings() { }
    public void ShowUI() { }
    // ... everything
}
```

**Solution**: Separate concerns into appropriate layers
```csharp
// Package Engine handles packages
PEInterface.WinGet.InstallPackage(...);

// Core handles logging
Logger.Error("Error message");

// Core handles settings
Settings.Set(key, value);

// UI handles display
ShowNotification("Installed");
```

## Related Documentation

- [System Architecture](./system-architecture.md) - High-level architecture overview
- [Project Structure](./project-structure.md) - Physical file organization
- [Data Flow](../04-core-systems/data-flow.md) - How data moves through layers
- [Design Patterns](../03-development-standards/design-patterns.md) - Patterns used at layer boundaries
