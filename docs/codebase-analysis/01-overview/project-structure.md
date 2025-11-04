# Project Structure

## Directory Tree
```
/UniGetUI
├── /src - Main source code containing all C# projects
├── /scripts - Python automation scripts for build and translation management
├── /docs - Documentation files
├── /media - Images, icons, and promotional materials
├── /InstallerExtras - Dependencies and resources for Inno Setup installer
├── /WebBasedData - Screenshot database and URL validation scripts
├── /configuration.winget - WinGet configuration files for various installation scenarios
├── /.github - GitHub-specific configuration files (workflows, issue templates, etc.)
└── [Root level configuration files]
```

## Directory Descriptions

### /src - Source Code
The main source code directory containing the entire UniGetUI application written in C# using .NET 8 and WinUI 3. The codebase is organized into multiple projects with clear separation of concerns:

- **UniGetUI** - Main application project containing the UI layer (XAML pages, controls, themes), entry point, and application lifecycle management
- **UniGetUI.Core.*** - Core functionality modules:
  - **Classes** - Core data structures and utility classes
  - **Data** - Data layer for managing application data
  - **IconEngine/IconStore** - Icon management and caching system
  - **LanguageEngine** - Internationalization and localization engine supporting 50+ languages
  - **Logger** - Logging infrastructure
  - **SecureSettings** - Secure credential and settings storage
  - **Settings** - Application settings management
  - **Tools** - Common utilities and helper functions
- **UniGetUI.PackageEngine.*** - Package management architecture:
  - **PackageEngine/PackageLoader/PackageManagerClasses** - Core package management logic
  - **Operations** - Package operation handlers (install, update, uninstall)
  - **Managers.*** - Individual package manager implementations:
    - **WinGet** - Windows Package Manager integration
    - **Scoop** - Scoop package manager support
    - **Chocolatey** - Chocolatey integration
    - **Pip** - Python package manager support
    - **Npm** - Node.js package manager support
    - **Dotnet** - .NET Tool support
    - **PowerShell/PowerShell7** - PowerShell Gallery integration
    - **Cargo** - Rust package manager support
    - **Vcpkg** - C++ package manager support
    - **Generic.NuGet** - Generic NuGet package support
  - **Enums** - Enumerations for package-related types
  - **Interfaces** - Package manager interface definitions
  - **Serializable** - Package data serialization for import/export
- **UniGetUI.Interface.*** - Interface-related modules:
  - **BackgroundApi** - Background operations and system tray integration
  - **Enums** - UI-related enumerations
  - **Telemetry** - Anonymous usage statistics collection
- **ExternalLibraries.*** - External library integrations:
  - **Clipboard** - Clipboard operations
  - **FilePickers** - File picker implementations
- **WindowsPackageManager.Interop** - COM interop with Windows Package Manager
- **Test Projects** - Multiple test projects (*.Tests) for unit testing various components

The solution file `UniGetUI.sln` orchestrates all these projects. The architecture follows a modular design with clear boundaries between the UI layer, business logic, package management engines, and core utilities.

### /scripts - Automation Scripts
Python scripts for automating various development and maintenance tasks:

- **apply_versions.py** - Updates version numbers across the project files
- **download_translations.py** - Downloads latest translations from translation service
- **translation_*.py** - Various translation management utilities (verification, purging unused strings)
- **generate_integrity_tree.py** - Generates integrity checksums for build artifacts
- **generate_json_from_excel.py** - Converts Excel data to JSON format
- **get_contributors.py** - Retrieves contributor information
- **tolgee_*.py** - Integration with Tolgee translation management platform
- **BuildNumber** - File containing the current build number
- **Languages** - Directory with language-related resources

These scripts are used during the build process and for maintaining translations across 50+ supported languages.

### /docs - Documentation
Documentation files organized for the project:

- **codebase-analysis/** - Developer documentation analyzing the codebase
  - **01-overview/** - High-level overview documentation including repository overview and project structure
  
Additional documentation files may be added here for developer guides, architecture decisions, and other technical documentation.

### /media - Media Assets
Images, icons, and promotional materials for UniGetUI:

- **icon.png/svg/ai** - Application icon in various formats
- **UniGetUI_*.png** - Screenshots and promotional images (10 numbered screenshots)
- **banner.png** - Repository banner image
- **socialicon.png** - Social media icon
- **store.png** - Microsoft Store listing image
- **main.webp** - Main promotional image
- **Icon sizes** - Directory containing different icon size variants
- **UniGetUI Media.pptx** - PowerPoint presentation with media assets

These assets are used in the application, documentation, and promotional materials.

### /InstallerExtras - Installer Resources
Dependencies, resources, and configuration files required by the Inno Setup installer:

- **CodeDependencies.iss** - Inno Setup script for managing installer dependencies
- **CustomMessages.iss** - Custom localized messages for the installer
- **INSTALLER.BMP** - Installer background image
- **appsdk.exe** - Windows App SDK runtime installer
- **netcorecheck_x64.exe** - .NET runtime checker and installer
- **ForceUniGetUIPortable** - Marker for portable installation mode
- **MsiCreator/** - MSI package creation tools and wrapper

The Inno Setup script (`UniGetUI.iss` in the root) uses these files to create the UniGetUI installer that handles runtime dependencies and proper installation on Windows systems.

### /WebBasedData - Web-Based Data Assets
Data and scripts for managing external web-based resources:

- **screenshot-database-v2.json** - Database mapping packages to their screenshots and icons
- **screenshot-database.json** - Legacy screenshot database
- **test_urls.py** - Script to validate URLs in the screenshot database
- **invalid_urls.txt** - Log of invalid or broken URLs

This data is used to enhance the package browsing experience by showing screenshots and custom icons for packages from various sources.

### /configuration.winget - WinGet Configuration Files
WinGet DSC (Desired State Configuration) files for automated UniGetUI setup:

- **configurations.md** - Documentation explaining each configuration file
- **unigetui-min.winget** - Minimal installation (UniGetUI + dependencies only)
- **unigetui-all.winget** - Full installation (includes all package managers except vcpkg and Scoop)
- **develop-unigetui.winget** - Development setup (includes all tools + repository clone)

These configuration files allow users to set up UniGetUI and its dependencies using `winget configure` for different use cases.

### /.github - GitHub Configuration
GitHub-specific configuration files for repository management:

- **workflows/** - GitHub Actions CI/CD workflows for automated testing, building, and deployment
- **ISSUE_TEMPLATE/** - Issue templates for bug reports and feature requests
- **PULL_REQUEST_TEMPLATE.md** - Pull request template
- **dependabot.yml** - Dependabot configuration for automated dependency updates
- **renovate.json** - Renovate bot configuration for dependency management
- **FUNDING.yml** - GitHub Sponsors configuration

These files configure the development workflow, issue tracking, and automated maintenance processes.

## Special Files

### Root Level Configuration Files

- **UniGetUI.iss** - Inno Setup installer script that creates the Windows installer executable. Defines installation behavior, dependencies, registry entries, and file placement
- **UniGetUI.sln** - Visual Studio solution file that organizes all C# projects in the src/ directory
- **build_release.cmd** - Windows batch script that orchestrates the complete build process: running tests, building the solution, signing binaries, creating ZIP archives, and generating the installer
- **test_publish.cmd** / **test_publish_nosign.cmd** - Scripts for testing the publish process with and without code signing
- **deep_clean.ps1** - PowerShell script for cleaning build artifacts and temporary files
- **README.md** - Main project documentation with features, installation instructions, and screenshots
- **LICENSE** - MIT License for the project
- **CODE_OF_CONDUCT.md** - Community code of conduct guidelines
- **CONTRIBUTING.md** - Guidelines for contributors including coding standards and PR process
- **SECURITY.md** - Security policy and vulnerability disclosure information
- **cli-arguments.md** - Documentation of command-line arguments and deep link protocols supported by UniGetUI
- **.gitignore** - Git ignore rules for build artifacts and temporary files
- **.gitattributes** - Git attributes for line ending normalization
- **.deepsource.toml** - DeepSource static analysis configuration
- **.whitesource** - WhiteSource security scanning configuration

### Build and Project Configuration

- **src/Directory.Build.props** - MSBuild properties shared across all projects
- **src/Solution.props** - Additional solution-wide MSBuild properties
- **src/SharedAssemblyInfo.cs** - Shared assembly metadata across projects
- **src/.editorconfig** - Editor configuration for consistent code formatting

### Main Application Files

- **src/UniGetUI/UniGetUI.csproj** - Main application project file
- **src/UniGetUI/App.xaml** - WinUI 3 application definition with resource dictionaries and styling
- **src/UniGetUI/MainWindow.xaml** - Main application window layout
- **src/UniGetUI/Package.appxmanifest** - Windows application manifest for MSIX packaging
- **src/UniGetUI/app.manifest** - Application manifest defining compatibility and UAC requirements
