# Package Engine

> **Note**: This document describes the unified package management system in UniGetUI. For implementation details, see the source code in `src/UniGetUI.PackageEngine.*`

## Overview

The **Package Engine** is the core system that provides a unified API for managing packages across multiple package managers. It abstracts away the differences between WinGet, Scoop, Chocolatey, and other package managers, presenting a consistent interface to the UI layer.

## Key Components

### PEInterface (Package Engine Interface)

The `PEInterface` static class serves as the main entry point and facade for all package management operations. It provides access to:

- **Package Managers**: `PEInterface.WinGet`, `PEInterface.Scoop`, etc.
- **Package Loaders**: `DiscoverablePackagesLoader`, `InstalledPackagesLoader`, `UpgradablePackagesLoader`
- **Utility Functions**: Helper methods for common operations

### IPackageManager Interface

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

### Package Loaders

Package loaders manage the lifecycle of package data:

- **DiscoverablePackagesLoader**: Loads packages available for installation
- **InstalledPackagesLoader**: Tracks installed packages
- **UpgradablePackagesLoader**: Monitors packages with available updates
- **PackageBundlesLoader**: Manages package bundles for import/export

### Package Operations

The operations system handles package actions:
- Install operations
- Update operations  
- Uninstall operations
- Operation queuing and dependency management

## Architecture

For detailed architecture information, see:
- [System Architecture](../02-architecture/system-architecture.md#layer-3-package-engine-layer)
- [Data Flow](./data-flow.md) - How data moves through the package engine

## Supported Package Managers

Currently supported:
- **WinGet** - Windows Package Manager
- **Scoop** - Command-line installer for Windows
- **Chocolatey** - Package manager for Windows
- **Pip** - Python package installer
- **Npm** - Node.js package manager
- **.NET Tool** - .NET global tools
- **PowerShell Gallery** - PowerShell modules
- **Cargo** - Rust package manager
- **Vcpkg** - C++ library manager

## Adding New Package Managers

See [Adding Package Managers](../12-extending/adding-package-managers.md) for a guide on integrating new package managers into the engine.

## Related Documentation

- [Data Flow](./data-flow.md)
- [System Architecture](../02-architecture/system-architecture.md)
- [Adding Package Managers](../12-extending/adding-package-managers.md)
