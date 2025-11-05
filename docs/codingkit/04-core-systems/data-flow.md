# Data Flow

## Overview

UniGetUI is a Windows desktop application that serves as a unified graphical interface for multiple package managers (WinGet, Scoop, Chocolatey, Pip, Npm, .NET Tool, PowerShell Gallery, Cargo, and Vcpkg). The application follows a layered architecture where data flows from user interactions through the UI layer, into the Package Engine layer, and finally to the underlying CLI package managers.

The primary data flow involves:
1. **User Input** → UI Pages (WinUI 3/XAML)
2. **UI Pages** → Package Engine Interface (PEInterface)
3. **Package Engine** → Package Managers (WinGet, Scoop, etc.)
4. **Package Managers** → CLI Tools (via process execution)
5. **CLI Output** → Package Operations
6. **Package Operations** → Package Loaders
7. **Package Loaders** → UI (via event notifications)

Data is primarily stored in-memory using concurrent collections and observable collections, with persistent settings stored locally. The application also exposes a local REST API for external integrations like Windows Widgets.

## Data Flow Diagram

```mermaid
graph TB
    A[User Input] --> B[WinUI 3 Pages]
    B --> C[MainWindow Controller]
    C --> D[PEInterface - Package Engine Entry Point]
    D --> E[Package Loaders]
    E --> F{Loader Type}
    F -->|Discover| G[DiscoverablePackagesLoader]
    F -->|Installed| H[InstalledPackagesLoader]
    F -->|Updates| I[UpgradablePackagesLoader]
    F -->|Bundles| J[PackageBundlesLoader]
    
    G --> K[Package Managers]
    H --> K
    I --> K
    
    K --> L{Manager Type}
    L -->|WinGet| M[WinGet Manager]
    L -->|Scoop| N[Scoop Manager]
    L -->|Chocolatey| O[Chocolatey Manager]
    L -->|Others| P[Pip/Npm/DotNet/etc]
    
    M --> Q[CLI Process Execution]
    N --> Q
    O --> Q
    P --> Q
    
    Q --> R[Package Operations]
    R --> S[Operation Results]
    S --> T[UI Update Events]
    T --> B
    
    U[Background API] --> C
    V[System Tray] --> C
    W[CLI Arguments] --> C
    
    X[Settings Engine] <--> C
    Y[Icon Store] <--> B
    Z[Logger] --> AA[Log Files]
    
    C --> Z
    R --> Z
```

## Data Lifecycle

### 1. Data Entry Points

**Primary User Interface Inputs:**
- **MainWindow.xaml.cs**: Main application window handling user interactions
  - Navigation between pages (Discover, Installed, Updates, Settings)
  - System tray interactions
  - Window state management
  
- **UI Pages** (in `src/UniGetUI/Pages/`):
  - Package discovery and search requests
  - Package installation/update/uninstall commands
  - Settings configuration changes
  - Import/export bundle operations

**External Integration Inputs:**
- **Background API** (`BackgroundApi.cs`): RESTful API running on `http://localhost:7058`
  - Widget integration requests (`/widgets/v1/*` and `/widgets/v2/*` endpoints)
  - Package sharing links (`/v2/show-package`)
  - External application integrations
  
- **CLI Arguments** (`CLIHandler.cs`): Command-line parameter processing
  - Silent operations
  - Direct package operations
  - Daemon mode activation

**Automated Inputs:**
- **AutoUpdater.cs**: Periodic update checks for packages and the application itself
- **Package Loaders**: Automated background refresh of package lists

### 2. Data Transformations

**User Request to Package Operation:**
1. **UI Input Capture**: User actions captured in XAML pages
2. **Command Translation**: UI commands translated to `InstallOptions` and operation parameters
3. **Package Object Creation**: `IPackage` instances created with metadata
4. **Operation Queue**: Operations added to `AbstractOperation` queue with dependencies

**CLI Output to Package Data:**
1. **Process Execution**: CLI commands executed via `AbstractProcessOperation`
2. **Output Parsing**: Manager-specific helpers parse CLI output
   - WinGet: JSON output parsing (`WinGet.cs`)
   - Scoop: JSON manifest parsing
   - Chocolatey: XML/text output parsing
3. **Package Metadata Extraction**: 
   - Package ID, name, version
   - Source, publisher information
   - Installation status, update availability
4. **Object Hydration**: `Package` objects populated with parsed data

**Settings Transformation:**
- **Settings Engine**: Reads/writes settings to persistent storage
- **Configuration Mapping**: Maps UI settings to package manager CLI arguments
- **Installation Options**: Converts user preferences to `InstallOptions` objects
  - Admin rights requirements
  - Interactive installation mode
  - Hash check settings
  - Installation scope (user/system)

**Icon and Resource Transformations:**
- **Icon Engine**: Fetches and caches package icons
- **Image Processing**: Converts icon formats for UI rendering
- **Resource Localization**: Language-specific strings loaded from `LanguageEngine`

### 3. Data Storage

**In-Memory Storage:**
- **ConcurrentDictionary&lt;long, IPackage&gt;**: Primary package collection in `AbstractPackageLoader`
  - Thread-safe concurrent access
  - Key: Package hash (ID + Manager + Source)
  - Value: `IPackage` instances
  
- **ObservableQueue&lt;string&gt;**: Command-line parameters queue in `MainWindow`
- **Observable Collections**: UI-bound collections for real-time updates

**Persistent Storage:**
- **Settings Storage** (via `SettingsEngine`):
  - File location: Local application data directory
  - Format: Key-value pairs
  - Content: User preferences, API tokens, cached configurations
  
- **Icon Cache** (`IconStore`):
  - File location: Local cache directory
  - Format: Image files (PNG/ICO)
  - Content: Downloaded package icons
  
- **Log Files** (`Logger`):
  - File location: Application data directory
  - Format: Text logs with timestamps
  - Content: Operation history, errors, debug information

**No Traditional Database:**
- UniGetUI does not use SQL databases or document stores
- All package data is ephemeral and fetched from package managers on demand
- State is maintained in-memory with periodic refreshes

### 4. Data Output

**UI Rendering:**
- **Package Lists**: Rendered in WinUI 3 DataGrids and ListViews
- **Status Updates**: Progress bars, notifications, and status messages
- **Package Details**: Detail panels with metadata, version history, and actions
- **Visual Feedback**: Icons, badges, and state indicators

**API Responses:**
- **Background API JSON Responses**:
  ```json
  {
    "version": "3.x.x",
    "updates": [...],
    "status": "success"
  }
  ```
- **CORS-enabled** for cross-origin widget requests
- **Token-based authentication** for secure external access

**External Notifications:**
- **System Notifications**: Windows toast notifications for updates and operations
- **System Tray Updates**: Icon badge and context menu updates
- **Widget Updates**: Data pushed to Windows Widgets panel

**Process Outputs:**
- **CLI Command Execution**: Standard output/error captured from package managers
- **Operation Results**: Success/failure status with detailed messages
- **Log Entries**: Structured logging to files for debugging

## Critical Data Paths

### Path 1: Package Installation Flow

```mermaid
sequenceDiagram
    participant User
    participant UI as UI Page
    participant MW as MainWindow
    participant PE as PEInterface
    participant PM as Package Manager
    participant CLI as CLI Process
    participant PO as PackageOperation
    participant PL as PackageLoader
    
    User->>UI: Click Install Button
    UI->>MW: Create InstallOptions
    MW->>PE: Get Manager Instance
    PE->>PM: WinGet.Install(package, options)
    PM->>PO: Create InstallOperation
    PO->>PO: Enqueue Operation
    PO->>CLI: Execute "winget install ..."
    CLI-->>PO: Output Stream
    PO->>PO: Parse Output
    PO-->>PM: Success/Failure Result
    PM->>PL: Notify Package Status Change
    PL->>UI: PackagesChanged Event
    UI->>User: Update UI State
```

**Key Components:**
1. **InstallOptions**: Configuration object with admin rights, interactive mode, hash check settings
2. **PackageOperation**: Abstract operation handling pre/post operations and error retry logic
3. **CLI Process**: Spawned process with captured stdout/stderr
4. **Event System**: Observer pattern for UI updates without direct coupling

### Path 2: Package Discovery and Loading

```mermaid
sequenceDiagram
    participant User
    participant UI as Discover Page
    participant Loader as DiscoverablePackagesLoader
    participant Managers as Package Managers
    participant CLI as CLI Tools
    participant Cache as In-Memory Cache
    
    User->>UI: Navigate to Discover Tab
    UI->>Loader: ReloadPackages()
    Loader->>Loader: Check IsLoading Flag
    Loader->>Managers: Query All Managers
    
    par Parallel Manager Queries
        Managers->>CLI: winget search
        Managers->>CLI: scoop search
        Managers->>CLI: choco search
    end
    
    CLI-->>Managers: JSON/Text Output
    Managers->>Managers: Parse to Package Objects
    Managers-->>Loader: List<IPackage>
    Loader->>Cache: Store in ConcurrentDictionary
    Loader->>UI: PackagesChanged Event
    UI->>User: Display Package List
```

**Key Features:**
- **Parallel Execution**: Multiple package managers queried simultaneously
- **Lazy Loading**: Packages loaded on-demand when page is accessed
- **Deduplication**: Same package from multiple sources handled by hash-based dictionary
- **Event-Driven Updates**: UI automatically refreshes when package data changes

### Path 3: Update Check and Notification

```mermaid
sequenceDiagram
    participant Timer as AutoUpdater Timer
    participant Loader as UpgradablePackagesLoader
    participant Managers as Package Managers
    participant CLI as CLI Tools
    participant Cache as Package Cache
    participant UI as Main Window
    participant Notif as System Notification
    
    Timer->>Loader: Periodic Update Check
    Loader->>Managers: Get Upgradable Packages
    
    par Check All Managers
        Managers->>CLI: winget upgrade
        Managers->>CLI: scoop status
        Managers->>CLI: choco outdated
    end
    
    CLI-->>Managers: Package Lists with Versions
    Managers->>Managers: Compare Installed vs Available
    Managers-->>Loader: List<IPackage> with Updates
    Loader->>Cache: Update Package References
    Loader->>UI: PackagesChanged Event
    UI->>UI: Update Badge Count
    UI->>Notif: Show Update Notification
    Notif->>User: Toast Notification
```

**Key Behaviors:**
- **Background Polling**: Periodic checks without blocking UI
- **Version Comparison**: Semantic versioning comparison for update detection
- **User Preferences**: Respects auto-update settings and ignored packages
- **Notification Throttling**: Prevents notification spam

### Path 4: Background API Integration

```mermaid
sequenceDiagram
    participant Widget as Windows Widget
    participant API as BackgroundApi (Port 7058)
    participant Auth as Token Auth
    participant PE as PEInterface
    participant Loader as UpgradablePackagesLoader
    participant UI as MainWindow
    
    Widget->>API: GET /widgets/v1/get_updates?token=xxx
    API->>Auth: AuthenticateToken(token)
    Auth-->>API: Token Valid
    API->>Loader: Get Packages()
    Loader-->>API: List<IPackage>
    API->>API: Serialize to JSON
    API-->>Widget: JSON Response
    
    Widget->>API: GET /widgets/v1/update_all_packages?token=xxx
    API->>Auth: AuthenticateToken(token)
    API->>PE: Trigger UpdateAll Event
    PE->>UI: OnUpgradeAll Event
    UI->>UI: Execute Update Operations
    API-->>Widget: 200 OK
```

**Security Features:**
- **Token Authentication**: Random 64-character token generated per session
- **Local-Only Access**: API bound to `localhost:7058` only
- **CORS Enabled**: Allows widget cross-origin requests
- **Session-Based**: Token regenerated on application restart

### Path 5: Settings Persistence

```mermaid
sequenceDiagram
    participant User
    participant Settings as Settings Page
    participant Engine as SettingsEngine
    participant Storage as Local Storage
    participant PE as PEInterface
    participant Managers as Package Managers
    
    User->>Settings: Change Setting
    Settings->>Engine: SetValue(key, value)
    Engine->>Storage: Write to File
    Storage-->>Engine: Confirm Write
    Engine->>PE: Notify Settings Change
    PE->>Managers: Apply Configuration
    Managers->>Managers: Update Runtime Behavior
    Engine-->>Settings: Setting Updated
    Settings->>User: Show Confirmation
```

**Configuration Types:**
- **Boolean Flags**: Auto-updates, admin elevation, interactive installs
- **String Values**: Proxy settings, language preferences, API tokens
- **Lists**: Ignored packages, preferred sources, custom CLI arguments
- **Dictionaries**: Per-package override options

### Path 6: Package Import/Export (Bundles)

```mermaid
sequenceDiagram
    participant User
    participant UI as Backup Page
    participant Bundle as PackageBundlesLoader
    participant Installed as InstalledPackagesLoader
    participant File as File System
    participant Import as Import Operation
    participant PE as PEInterface
    
    User->>UI: Export Packages
    UI->>Installed: Get All Packages
    Installed-->>UI: List<IPackage>
    UI->>Bundle: AddPackagesAsync(packages)
    Bundle->>Bundle: Serialize to ImportedPackage
    Bundle->>File: Write JSON/XML Bundle
    File-->>User: Download Bundle File
    
    User->>UI: Import Packages
    UI->>File: Read Bundle File
    File-->>UI: Package Data
    UI->>Bundle: LoadFromFile(data)
    Bundle->>Bundle: Deserialize Packages
    Bundle->>Import: Create InstallOperations
    Import->>PE: Queue Install Operations
    PE->>UI: Operations Queued
    UI->>User: Show Import Progress
```

**Bundle Features:**
- **Serialization Format**: JSON-based package metadata
- **Version Preservation**: Saves specific versions and installation options
- **Cross-Machine Support**: Bundles portable across machines
- **Validation**: Invalid packages marked during import for user review

## Data Flow Characteristics

### Concurrency Model
- **Thread-Safe Collections**: `ConcurrentDictionary` for package storage
- **Async/Await Pattern**: Heavy use of async operations for I/O
- **Parallel Manager Loading**: Multiple package managers initialized concurrently
- **UI Thread Marshaling**: Observable collections ensure UI updates on correct thread

### Error Handling
- **Operation Retry Logic**: Failed operations can retry with modified settings
- **Graceful Degradation**: Missing package managers don't block the application
- **Detailed Logging**: All operations logged with context for debugging
- **User Notifications**: Errors surfaced through UI with actionable messages

### Performance Optimizations
- **Lazy Loading**: Package details loaded on-demand
- **Icon Caching**: Package icons cached locally to reduce network requests
- **Debounced Updates**: UI updates batched to prevent excessive redraws
- **Background Loading**: Package lists refreshed in background without blocking UI

### Data Validation
- **Token Authentication**: API requests validated with session tokens
- **Package Hash Verification**: Optional integrity checking for installations
- **Version Validation**: Semantic version parsing and comparison
- **Input Sanitization**: User inputs sanitized before CLI execution
