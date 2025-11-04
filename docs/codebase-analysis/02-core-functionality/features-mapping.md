# Features Mapping

## Feature Overview

UniGetUI is a unified graphical interface for Windows package managers that consolidates multiple CLI-based package management systems into a single application. The application provides comprehensive package management capabilities including discovery, installation, updates, removal, and backup/restore functionality across multiple package managers (WinGet, Scoop, Chocolatey, Pip, Npm, .NET Tool, PowerShell Gallery, and more).

The codebase is organized into modular components with clear separation of concerns:
- **Core Modules**: Settings, logging, language, icon management, and data handling
- **Package Engine**: Manager implementations, package operations, and serialization
- **User Interface**: WinUI 3-based UI with pages for different functionalities
- **Background Services**: Auto-updater, backup service, and background API for widgets

## Feature Details

### Feature 1: Multi-Package Manager Support
**Description:** Unified interface supporting multiple package managers (WinGet, Scoop, Chocolatey, Pip, Npm, .NET Tool, PowerShell Gallery, and Cargo). Each manager is independently implemented with its own discovery, installation, update, and removal logic.

**Implementation Files:**
- `src/UniGetUI.PackageEngine.PackageManagerClasses/Manager/PackageManager.cs` - Base abstract class for all package managers
- `src/UniGetUI.PackageEngine.Managers.WinGet/` - WinGet package manager implementation
- `src/UniGetUI.PackageEngine.Managers.Scoop/` - Scoop package manager implementation
- `src/UniGetUI.PackageEngine.Managers.Chocolatey/` - Chocolatey package manager implementation
- `src/UniGetUI.PackageEngine.Managers.Pip/` - Python Pip package manager implementation
- `src/UniGetUI.PackageEngine.Managers.Npm/` - Node.js Npm package manager implementation
- `src/UniGetUI.PackageEngine.Managers.Dotnet/` - .NET Tool package manager implementation
- `src/UniGetUI.PackageEngine.Managers.PowerShell/` - PowerShell Gallery implementation
- `src/UniGetUI.PackageEngine.Managers.PowerShell7/` - PowerShell 7 Gallery implementation
- `src/UniGetUI.PackageEngine.Managers.Cargo/` - Rust Cargo package manager implementation
- `src/UniGetUI.PackageEngine.Managers.Vcpkg/` - C++ Vcpkg package manager implementation
- `src/UniGetUI.PackageEngine.PackageManagerClasses/Manager/Helpers/BasePkgOperationHelper.cs` - Base helper for package operations
- `src/UniGetUI.PackageEngine.PackageManagerClasses/Manager/Helpers/BasePkgDetailsHelper.cs` - Base helper for package details
- `src/UniGetUI.PackageEngine.PackageManagerClasses/Manager/Helpers/BaseSourceHelper.cs` - Base helper for package sources

**Dependencies:**
- WindowsPackageManager.Interop (for WinGet COM API)
- System.Diagnostics.Process (for CLI execution)
- UniGetUI.Core.Tools (for process management)

**Entry Point:** `src/UniGetUI.PackageEngine.PackageEngine/PEInterface.cs` - Package Engine interface that initializes and manages all package managers

---

### Feature 2: Package Discovery and Search
**Description:** Browse and search for available packages across all supported package managers with filtering and sorting capabilities. Displays package metadata including name, version, source, and description.

**Implementation Files:**
- `src/UniGetUI/Pages/SoftwarePages/DiscoverSoftwarePage.cs` - Main discover page UI
- `src/UniGetUI/Pages/SoftwarePages/AbstractPackagesPage.xaml.cs` - Base class for package listing pages
- `src/UniGetUI.PackageEngine.PackageLoader/` - Package discovery and loading logic
- `src/UniGetUI.PackageEngine.PackageManagerClasses/Packages/Package.cs` - Package model class
- `src/UniGetUI/Pages/PageInterfaces/ISearchBoxPage.cs` - Search functionality interface

**Dependencies:**
- PackageManager implementations (for querying packages)
- UniGetUI.Core.IconEngine (for package icons)
- UniGetUI.PackageEngine.PackageLoader (for async loading)

**Entry Point:** `src/UniGetUI/Pages/SoftwarePages/DiscoverSoftwarePage.cs`

---

### Feature 3: Package Installation
**Description:** Install packages with customizable installation options including version selection, architecture (32/64-bit), custom switches, and interactive/silent modes. Settings are persisted for future updates.

**Implementation Files:**
- `src/UniGetUI.PackageEngine.Operations/PackageOperations.cs` - Core package installation operations
- `src/UniGetUI.PackageEngine.Operations/AbstractOperation.cs` - Base operation class with execution logic
- `src/UniGetUI.PackageEngine.Operations/AbstractProcessOperation.cs` - Process-based operation execution
- `src/UniGetUI.PackageEngine.PackageManagerClasses/Packages/Classes/InstallOptionsFactory.cs` - Factory for installation options
- `src/UniGetUI.PackageEngine.Serializable/InstallOptions.cs` - Serializable installation options
- `src/UniGetUI/Pages/DialogPages/DialogHelper_Packages.cs` - UI dialogs for installation options

**Dependencies:**
- PackageManager.OperationHelper (manager-specific operations)
- UniGetUI.Core.Settings (for persisting options)
- System.Diagnostics.Process (for executing installers)

**Entry Point:** `src/UniGetUI.PackageEngine.Operations/PackageOperations.cs` - `Install()` method

---

### Feature 4: Package Updates Management
**Description:** Monitor available updates, configure automatic updates, bulk update packages, skip versions, and ignore specific packages. Includes system tray notifications for available updates.

**Implementation Files:**
- `src/UniGetUI/Pages/SoftwarePages/SoftwareUpdatesPage.cs` - Updates page UI
- `src/UniGetUI.PackageEngine.PackageManagerClasses/Packages/Classes/IgnoredUpdatesDatabase.cs` - Database for ignored updates
- `src/UniGetUI.PackageEngine.Serializable/SerializableUpdatesOptions.cs` - Update options serialization
- `src/UniGetUI/Pages/SettingsPages/GeneralPages/Updates.xaml.cs` - Update settings page
- `src/UniGetUI/MainWindow.xaml.cs` - System tray integration for update notifications

**Dependencies:**
- PackageManager (for checking updates)
- UniGetUI.Core.Settings (for update preferences)
- H.NotifyIcon (for system tray notifications)

**Entry Point:** `src/UniGetUI/Pages/SoftwarePages/SoftwareUpdatesPage.cs`

---

### Feature 5: Installed Packages Management
**Description:** View and manage installed packages with capabilities to update, reinstall, uninstall, or modify installation options for installed software.

**Implementation Files:**
- `src/UniGetUI/Pages/SoftwarePages/InstalledPackagesPage.cs` - Installed packages page UI
- `src/UniGetUI.PackageEngine.Operations/PackageOperations.cs` - Uninstall and update operations
- `src/UniGetUI/Pages/SoftwarePages/AbstractPackagesPage.xaml.cs` - Base class for package listing

**Dependencies:**
- PackageManager (for listing installed packages)
- PackageOperations (for uninstall/update)
- UniGetUI.PackageEngine.PackageLoader (for loading installed packages)

**Entry Point:** `src/UniGetUI/Pages/SoftwarePages/InstalledPackagesPage.cs`

---

### Feature 6: Package Bundles (Import/Export)
**Description:** Export package lists with custom installation parameters to JSON/YAML/XML formats. Import bundles to quickly set up machines or share configurations. Supports multiple bundle formats.

**Implementation Files:**
- `src/UniGetUI/Pages/SoftwarePages/PackageBundlesPage.cs` - Bundle management page
- `src/UniGetUI.PackageEngine.Serializable/SerializableBundle.cs` - Bundle serialization model
- `src/UniGetUI.PackageEngine.Serializable/SerializablePackage.cs` - Package serialization model
- `src/UniGetUI.PackageEngine.Serializable/SerializableComponent.cs` - Component serialization
- `src/UniGetUI.PackageEngine.PackageManagerClasses/Packages/ImportedPackage.cs` - Imported package model
- `src/UniGetUI.PackageEngine.PackageManagerClasses/Packages/InvalidImportedPackage.cs` - Invalid package handling

**Dependencies:**
- System.Text.Json (for JSON serialization)
- YamlDotNet (for YAML serialization)
- System.Xml (for XML serialization)

**Entry Point:** `src/UniGetUI/Pages/SoftwarePages/PackageBundlesPage.cs`

---

### Feature 7: Cloud Backup to GitHub
**Description:** Backup installed packages to GitHub Gists for cloud storage and easy recovery across machines. Integrates with GitHub OAuth for authentication.

**Implementation Files:**
- `src/UniGetUI/Services/GitHubBackupService.cs` - GitHub Gist backup service
- `src/UniGetUI/Pages/SettingsPages/GeneralPages/Backup.xaml.cs` - Backup settings page
- `src/UniGetUI/Services/GitHubAuthService.cs` - GitHub OAuth authentication (implied)

**Dependencies:**
- Octokit (GitHub API client)
- SerializableBundle (for bundle creation)
- UniGetUI.Core.Settings (for storing credentials)

**Entry Point:** `src/UniGetUI/Services/GitHubBackupService.cs` - `UploadPackageBundle()` method

---

### Feature 8: Auto-Update for UniGetUI
**Description:** Built-in auto-updater that checks for new versions of UniGetUI, downloads updates, and prompts users to install. Supports both stable and beta channels.

**Implementation Files:**
- `src/UniGetUI/AutoUpdater.cs` - Auto-update logic and update checking
- `src/UniGetUI/Pages/SettingsPages/GeneralPages/Updates.xaml.cs` - Update settings UI

**Dependencies:**
- System.Net.Http (for version checking)
- System.Security.Cryptography (for checksum verification)
- Microsoft.Windows.AppNotifications (for update notifications)

**Entry Point:** `src/UniGetUI/AutoUpdater.cs` - `UpdateCheckLoop()` method

---

### Feature 9: Multi-Language Support
**Description:** Internationalization support with 50+ languages. Provides dynamic language switching and translation management.

**Implementation Files:**
- `src/UniGetUI.Core.LanguageEngine/LanguageEngine.cs` - Translation engine
- `src/UniGetUI.Core.LanguageEngine/LanguageData.cs` - Language data management
- `src/UniGetUI.Core.Data/Assets/Languages/` - Language resource files (implied)
- `src/UniGetUI.Core.Tools/CoreTools.cs` - `Translate()` utility method

**Dependencies:**
- Language resource files (JSON/XML)
- UniGetUI.Core.Settings (for language preference)

**Entry Point:** `src/UniGetUI.Core.LanguageEngine/LanguageEngine.cs`

---

### Feature 10: Package Details and Metadata
**Description:** Display comprehensive package information including description, publisher, version, download size, homepage, license, and screenshots before installation.

**Implementation Files:**
- `src/UniGetUI.PackageEngine.PackageManagerClasses/Packages/Classes/PackageDetails.cs` - Package details model
- `src/UniGetUI.PackageEngine.PackageManagerClasses/Manager/Helpers/BasePkgDetailsHelper.cs` - Base details helper
- Each manager's `Helpers/*DetailsHelper.cs` - Manager-specific details retrieval
- `src/UniGetUI/Pages/DialogPages/DialogHelper_Packages.cs` - Package details UI dialog

**Dependencies:**
- PackageManager implementations (for fetching details)
- System.Net.Http (for web requests)
- Manager-specific APIs

**Entry Point:** `BasePkgDetailsHelper` implementations in each manager

---

### Feature 11: System Tray Integration
**Description:** System tray icon showing available updates count with quick access to common operations. Provides context menu for quick package management.

**Implementation Files:**
- `src/UniGetUI/MainWindow.xaml.cs` - Tray icon initialization and management (`LoadTrayMenu()` method)
- `src/UniGetUI/Controls/OperationWidgets/` - Operation widgets for tray menu

**Dependencies:**
- H.NotifyIcon (system tray library)
- Microsoft.Windows.AppNotifications (for notifications)

**Entry Point:** `src/UniGetUI/MainWindow.xaml.cs` - `LoadTrayMenu()` method

---

### Feature 12: Widget and API Integration
**Description:** Background HTTP API for integration with Windows Widgets and Dev Home. Provides package information and allows remote operations.

**Implementation Files:**
- `src/UniGetUI.Interface.BackgroundApi/BackgroundApi.cs` - HTTP API server and endpoints
- `src/UniGetUI.Interface.BackgroundApi/` - Widget API implementation

**Dependencies:**
- Microsoft.AspNetCore (for HTTP API)
- System.Net.Http (for HTTP handling)
- UniGetUI.PackageEngine (for package operations)

**Entry Point:** `src/UniGetUI.Interface.BackgroundApi/BackgroundApi.cs` - `Start()` method (runs on http://localhost:7058)

---

### Feature 13: Package Icon Management
**Description:** Caches and manages package icons with automatic downloading and local storage. Provides icon retrieval for UI display.

**Implementation Files:**
- `src/UniGetUI.Core.IconStore/IconCacheEngine.cs` - Icon caching engine
- `src/UniGetUI.Core.IconStore/IconDatabase.cs` - Icon database management
- `src/UniGetUI.Core.IconStore/Serializable.cs` - Icon data serialization

**Dependencies:**
- System.Net.Http (for downloading icons)
- System.IO (for file storage)
- Microsoft.UI.Xaml.Media.Imaging (for image loading)

**Entry Point:** `src/UniGetUI.Core.IconStore/IconCacheEngine.cs`

---

### Feature 14: Settings and Configuration Management
**Description:** Comprehensive settings system with UI preferences, manager-specific settings, and secure settings stored with encryption. Supports import/export of settings.

**Implementation Files:**
- `src/UniGetUI.Core.Settings/SettingsEngine.cs` - Core settings engine (implied)
- `src/UniGetUI.Core.Settings/SettingsEngine_ImportExport.cs` - Settings import/export
- `src/UniGetUI.Core.SecureSettings/` - Secure settings with encryption
- `src/UniGetUI/Pages/SettingsPages/` - Settings UI pages
- `src/UniGetUI/Controls/SettingsWidgets/` - Settings UI widgets

**Dependencies:**
- System.Security.Cryptography (for encryption)
- System.Text.Json (for serialization)
- Windows Registry (for storage)

**Entry Point:** `src/UniGetUI.Core.Settings/SettingsEngine.cs`

---

### Feature 15: Operation History and Logging
**Description:** Tracks all package operations (install, update, uninstall) with detailed logs. Provides operation history viewer and manager-specific logs.

**Implementation Files:**
- `src/UniGetUI/Pages/LogPages/OperationHistoryPage.cs` - Operation history UI
- `src/UniGetUI/Pages/LogPages/ManagerLogsPage.cs` - Manager-specific logs UI
- `src/UniGetUI/Pages/LogPages/UniGetUILogPage.cs` - UniGetUI application logs
- `src/UniGetUI/Pages/LogPages/LogPage.xaml.cs` - Base log page
- `src/UniGetUI.Core.Logger/` - Logging infrastructure
- `src/UniGetUI.PackageEngine.PackageManagerClasses/Manager/Classes/ManagerLogger.cs` - Manager-specific logger

**Dependencies:**
- UniGetUI.Core.Logging (for log management)
- System.IO (for log files)

**Entry Point:** `src/UniGetUI/Pages/LogPages/OperationHistoryPage.cs`

---

### Feature 16: Package Source Management
**Description:** Manage package sources/repositories for each package manager. Add, remove, enable/disable custom sources.

**Implementation Files:**
- `src/UniGetUI.PackageEngine.Operations/SourceOperations.cs` - Source management operations
- `src/UniGetUI.PackageEngine.PackageManagerClasses/Manager/Classes/ManagerSource.cs` - Source model
- `src/UniGetUI.PackageEngine.PackageManagerClasses/Manager/Classes/SourceFactory.cs` - Source factory
- `src/UniGetUI.PackageEngine.PackageManagerClasses/Manager/Helpers/BaseSourceHelper.cs` - Base source helper
- Each manager's `Helpers/*SourceHelper.cs` - Manager-specific source operations

**Dependencies:**
- PackageManager implementations
- UniGetUI.Core.Settings (for source preferences)

**Entry Point:** `src/UniGetUI.PackageEngine.Operations/SourceOperations.cs`

---

### Feature 17: Command-Line Interface Support
**Description:** Support for command-line arguments including daemon mode, update automation, settings management, and deep link handling (unigetui:// protocol).

**Implementation Files:**
- `src/UniGetUI/CLIHandler.cs` - Command-line argument parsing and handling
- `src/UniGetUI/EntryPoint.cs` - Application entry point
- `cli-arguments.md` - CLI documentation

**Dependencies:**
- UniGetUI.Core.Settings (for CLI-based settings modification)
- Deep link protocol registration

**Entry Point:** `src/UniGetUI/EntryPoint.cs` and `src/UniGetUI/CLIHandler.cs`

---

### Feature 18: Crash Handling and Error Reporting
**Description:** Crash detection, error logging, and error report generation for troubleshooting and debugging.

**Implementation Files:**
- `src/UniGetUI/CrashHandler.cs` - Crash detection and reporting
- `src/UniGetUI.Core.Logger/` - Error logging infrastructure

**Dependencies:**
- UniGetUI.Core.Logging (for error logs)
- System.Diagnostics (for crash detection)

**Entry Point:** `src/UniGetUI/CrashHandler.cs`

---

### Feature 19: Process and Operation Management
**Description:** Manages concurrent package operations with queue management, process monitoring, and operation lifecycle handling.

**Implementation Files:**
- `src/UniGetUI.PackageEngine.Operations/AbstractOperation.cs` - Base operation class
- `src/UniGetUI.PackageEngine.Operations/AbstractOperation_Auxiliaries.cs` - Operation auxiliaries
- `src/UniGetUI.PackageEngine.Operations/AbstractProcessOperation.cs` - Process-based operations
- `src/UniGetUI.PackageEngine.Operations/KillProcessOperation.cs` - Process termination
- `src/UniGetUI.Core.Classes/TaskRecycler.cs` - Task queue and recycling
- `src/UniGetUI.Core.Classes/ObservableQueue.cs` - Observable queue implementation

**Dependencies:**
- System.Diagnostics.Process (for process management)
- System.Threading.Tasks (for async operations)

**Entry Point:** `src/UniGetUI.PackageEngine.Operations/AbstractOperation.cs`

---

### Feature 20: Desktop Shortcuts Management
**Description:** Tracks and manages desktop shortcuts for installed packages, allowing users to see which packages create shortcuts.

**Implementation Files:**
- `src/UniGetUI.PackageEngine.PackageManagerClasses/Packages/Classes/DesktopShortcutsDatabase.cs` - Desktop shortcuts database

**Dependencies:**
- System.IO (for file system access)
- Windows Shell API (for shortcut detection)

**Entry Point:** `src/UniGetUI.PackageEngine.PackageManagerClasses/Packages/Classes/DesktopShortcutsDatabase.cs`

---

## Feature Dependency Matrix

| Feature | Depends On | Used By |
|---------|-----------|---------|
| Multi-Package Manager Support | Core Settings, Core Logging, Core Tools | Package Discovery, Package Installation, Package Updates, Installed Packages, Package Details |
| Package Discovery and Search | Multi-Package Manager Support, Icon Management | User Interface |
| Package Installation | Multi-Package Manager Support, Settings Management, Operation Management | User Interface, CLI Support |
| Package Updates Management | Multi-Package Manager Support, Settings Management, System Tray Integration | User Interface, Auto-Update, Widget Integration |
| Installed Packages Management | Multi-Package Manager Support, Package Installation | User Interface |
| Package Bundles (Import/Export) | Package Installation, Settings Management | Cloud Backup, CLI Support |
| Cloud Backup to GitHub | Package Bundles, Settings Management | User Interface |
| Auto-Update for UniGetUI | Settings Management, Logging | System Tray Integration, User Interface |
| Multi-Language Support | Settings Management | All UI Features |
| Package Details and Metadata | Multi-Package Manager Support | Package Discovery, Package Installation |
| System Tray Integration | Package Updates Management, Settings Management | User Interface |
| Widget and API Integration | Multi-Package Manager Support, Icon Management, Package Updates | External Widgets, Dev Home |
| Package Icon Management | Multi-Package Manager Support | Package Discovery, Widget Integration, User Interface |
| Settings and Configuration Management | Core Logging | All Features |
| Operation History and Logging | Operation Management, Multi-Package Manager Support | User Interface, Debugging |
| Package Source Management | Multi-Package Manager Support, Settings Management | Package Discovery, User Interface |
| Command-Line Interface Support | Settings Management, Package Installation, Package Updates | User Interface |
| Crash Handling and Error Reporting | Logging | User Interface |
| Process and Operation Management | Core Classes, Logging | Package Installation, Package Updates, Package Source Management |
| Desktop Shortcuts Management | Installed Packages Management | User Interface |

---

## Architecture Overview

The application follows a layered architecture:

1. **Core Layer** (UniGetUI.Core.*): Foundation services including settings, logging, language, icons, and data
2. **Package Engine Layer** (UniGetUI.PackageEngine.*): Package manager implementations, operations, and serialization
3. **Interface Layer** (UniGetUI.*): User interface, pages, controls, and services
4. **Background Services Layer**: Auto-updater, backup service, background API

Key architectural patterns:
- **Strategy Pattern**: Each package manager implements the `IPackageManager` interface
- **Factory Pattern**: Used for creating package sources and installation options
- **Observer Pattern**: ObservableQueue and event-based notifications
- **Repository Pattern**: Database classes for persistent storage
- **Command Pattern**: Operations encapsulate package management commands

## Core Dependencies

**External Libraries:**
- **WinUI 3** (Microsoft.UI.Xaml): Modern Windows UI framework
- **H.NotifyIcon**: System tray icon implementation
- **Octokit**: GitHub API client for cloud backup
- **YamlDotNet**: YAML serialization
- **Microsoft.AspNetCore**: Background HTTP API
- **WindowsPackageManager.Interop**: WinGet COM API
- **Microsoft.Windows.AppNotifications**: Windows notifications

**Internal Modules:**
- **UniGetUI.Core.Tools**: Utility functions and helpers
- **UniGetUI.Core.Settings**: Settings persistence
- **UniGetUI.Core.Logging**: Logging infrastructure
- **UniGetUI.Core.LanguageEngine**: Internationalization
- **UniGetUI.Core.IconEngine**: Icon management
- **UniGetUI.PackageEngine**: Package management core
