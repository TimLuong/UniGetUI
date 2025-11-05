# Configuration Files

## Configuration Overview

UniGetUI uses various configuration files at the repository root level to manage different aspects of development, build process, code quality, dependency management, and deployment. These configuration files can be categorized into:

1. **Code Quality & Analysis**: `.deepsource.toml`, `.whitesource`
2. **Dependency Management**: `.github/dependabot.yml`, `.github/renovate.json`
3. **Project Metadata**: `.github/FUNDING.yml`
4. **Build & Deployment**: `UniGetUI.iss`, `build_release.cmd`, `test_publish.cmd`, `test_publish_nosign.cmd`, `deep_clean.ps1`
5. **Environment Setup**: `configuration.winget/*.winget` files

This document provides detailed information about each configuration file, its purpose, settings, and usage.

---

## Configuration Files

### .deepsource.toml

**Purpose:** Configures DeepSource code analysis and quality checks for the repository. DeepSource automatically analyzes code for bugs, security vulnerabilities, anti-patterns, and code style issues.

**Settings:**

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| version | integer | 1 | DeepSource configuration schema version |
| analyzers | array | - | List of language analyzers to enable |
| analyzers.name | string | - | Analyzer name (python, javascript) |
| analyzers.enabled | boolean | false | Whether the analyzer is enabled |
| analyzers.meta.runtime_version | string | - | Python runtime version to use |
| analyzers.meta.plugins | array | - | JavaScript framework plugins (react, vue, angular) |
| analyzers.meta.environment | array | - | JavaScript runtime environments (nodejs, browser, jest, etc.) |
| analyzers.pylint.enabled | boolean | false | Enable pylint for Python analysis |
| analyzers.pylint.config | string | - | Inline pylint configuration |
| transformers | array | - | Code auto-formatters/transformers |
| transformers.name | string | - | Transformer name (autopep8, black, prettier, etc.) |
| transformers.enabled | boolean | false | Whether the transformer is enabled |

**Example:**
```toml
version = 1

[[analyzers]]
name = "python"
enabled = true

  [analyzers.meta]
  runtime_version = "3.x.x"

  [analyzers.pylint]
  enabled = true
  config = """
  [MESSAGES CONTROL]
  disable = C0111  # Disables missing docstring warnings
  """

[[analyzers]]
name = "javascript"
enabled = true

  [analyzers.meta]
  plugins = ["react", "vue", "angular"]
  environment = ["nodejs", "mocha", "browser", "jest"]

[[transformers]]
name = "prettier"
enabled = false  # Disables prettier transformer
```

**Key Configuration Choices:**
- **Python Analyzer**: Enabled with Python 3.x runtime and pylint integration
- **JavaScript Analyzer**: Enabled with React, Vue, Angular plugins and multiple test frameworks
- **All Transformers Disabled**: Code formatting is handled by other tools (autopep8, black, isort, ruff, standardjs, prettier are all disabled)

---

### .whitesource

**Purpose:** Configures Mend (formerly WhiteSource) security and license compliance scanning. This tool automatically scans dependencies for known vulnerabilities and license compliance issues.

**Settings:**

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| scanSettings.baseBranches | array | [] | Specific branches to scan (empty = all branches) |
| checkRunSettings.vulnerableCheckRunConclusionLevel | string | failure | Check run conclusion for vulnerabilities (failure/success) |
| checkRunSettings.displayMode | string | diff | How to display results (diff/full) |
| checkRunSettings.useMendCheckNames | boolean | false | Use Mend-branded check names |
| issueSettings.minSeverityLevel | string | LOW | Minimum severity to create issues (LOW/MEDIUM/HIGH/CRITICAL) |
| issueSettings.issueType | string | DEPENDENCY | Type of issues to create (DEPENDENCY/SECURITY) |

**Example:**
```json
{
  "scanSettings": {
    "baseBranches": []
  },
  "checkRunSettings": {
    "vulnerableCheckRunConclusionLevel": "failure",
    "displayMode": "diff",
    "useMendCheckNames": true
  },
  "issueSettings": {
    "minSeverityLevel": "LOW",
    "issueType": "DEPENDENCY"
  }
}
```

**Key Configuration Choices:**
- **All Branches Scanned**: Empty baseBranches array means all branches are scanned
- **Fail on Vulnerabilities**: Check runs fail when vulnerabilities are found
- **Low Severity Threshold**: Even low-severity issues are tracked
- **Diff Display Mode**: Only shows changes in dependencies between scans

---

### .github/dependabot.yml

**Purpose:** Configures GitHub Dependabot for automated dependency update pull requests. Dependabot monitors dependencies and automatically creates PRs when updates are available.

**Settings:**

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| version | integer | 2 | Dependabot configuration file version |
| updates | array | - | List of update configurations |
| updates.package-ecosystem | string | - | Package manager ecosystem (github-actions, npm, nuget, etc.) |
| updates.directory | string | - | Directory containing dependencies |
| updates.schedule.interval | string | - | Update check frequency (daily/weekly/monthly) |
| updates.groups | object | - | Dependency grouping configuration |
| updates.groups.{name}.patterns | array | - | Patterns to match for grouping dependencies |

**Example:**
```yaml
version: 2
updates:
  - package-ecosystem: "github-actions"
    directory: "/"
    schedule:
      interval: "weekly"
    groups:
      actions-deps:
        patterns:
          - "*"
```

**Key Configuration Choices:**
- **GitHub Actions Only**: Currently only monitoring GitHub Actions dependencies
- **Weekly Updates**: Checks for updates once per week
- **Grouped Updates**: All GitHub Actions updates are grouped into a single PR

**Additional Ecosystem Examples:**
```yaml
# Example for .NET/NuGet dependencies
- package-ecosystem: "nuget"
  directory: "/src"
  schedule:
    interval: "weekly"
  groups:
    nuget-deps:
      patterns:
        - "*"

# Example for Python dependencies
- package-ecosystem: "pip"
  directory: "/"
  schedule:
    interval: "weekly"
```

---

### .github/renovate.json

**Purpose:** Configures Renovate Bot for automated dependency updates. Renovate is an alternative to Dependabot with more advanced features and customization options.

**Settings:**

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| $schema | string | - | JSON schema URL for validation |
| extends | array | - | Preset configurations to extend from |
| enabledManagers | array | - | Package managers to enable (nuget, npm, etc.) |
| dependencyDashboard | boolean | false | Enable/disable dependency dashboard issue |

**Example:**
```json
{
  "$schema": "https://docs.renovatebot.com/renovate-schema.json",
  "extends": [
    "config:recommended"
  ],
  "enabledManagers": [
    "nuget"
  ],
  "dependencyDashboard": true
}
```

**Key Configuration Choices:**
- **Recommended Config**: Uses Renovate's recommended best practices preset
- **NuGet Only**: Only monitors NuGet (.NET) dependencies
- **Dashboard Enabled**: Creates and maintains a GitHub issue with dependency status

**Common Preset Options:**
- `config:recommended` - Renovate's recommended settings
- `config:js-app` - Settings optimized for JavaScript applications
- `schedule:weekly` - Run updates weekly instead of immediately

---

### .github/FUNDING.yml

**Purpose:** Configures GitHub Sponsors and funding links displayed on the repository's main page. This helps contributors support the project maintainer.

**Settings:**

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| github | string/array | - | GitHub Sponsors username(s) |
| patreon | string | - | Patreon username |
| open_collective | string | - | Open Collective username |
| ko_fi | string | - | Ko-fi username |
| tidelift | string | - | Tidelift package identifier |
| community_bridge | string | - | Community Bridge project name |
| liberapay | string | - | Liberapay username |
| issuehunt | string | - | IssueHunt username |
| otechie | string | - | Otechie username |
| lfx_crowdfunding | string | - | LFX Crowdfunding project name |
| custom | array | - | Custom sponsorship URLs |

**Example:**
```yaml
# These are supported funding model platforms
github: # Replace with up to 4 GitHub Sponsors-enabled usernames e.g., [user1, user2]
patreon: # Replace with a single Patreon username
open_collective: # Replace with a single Open Collective username
ko_fi: martinet101
tidelift: # Replace with a single Tidelift platform-name/package-name e.g., npm/babel
community_bridge: # Replace with a single Community Bridge project-name e.g., cloud-foundry
liberapay: # Replace with a single Liberapay username
issuehunt: # Replace with a single IssueHunt username
otechie: # Replace with a single Otechie username
lfx_crowdfunding: # Replace with a single LFX Crowdfunding project name e.g., cloud-foundry
custom: # Replace with up to 4 custom sponsorship URLs e.g., ['link1', 'link2']
```

**Key Configuration:**
- **Ko-fi**: Currently configured with username `martinet101` for one-time donations and memberships

---

### UniGetUI.iss

**Purpose:** Inno Setup script for creating the Windows installer for UniGetUI. Defines installer behavior, files to include, registry entries, shortcuts, and uninstaller configuration.

**Settings:**

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| MyAppVersion | define | 3.3.6 | Application version number |
| MyAppName | define | UniGetUI | Application display name |
| MyAppPublisher | define | Martí Climent | Publisher name |
| MyAppURL | define | - | Application website URL |
| MyAppExeName | define | UniGetUI.exe | Main executable name |
| AppId | GUID | - | Unique application identifier for installation |
| DefaultDirName | string | {autopf64}\UniGetUI | Default installation directory |
| DisableProgramGroupPage | boolean | yes | Skip program group selection page |
| DisableDirPage | boolean | no | Show/hide directory selection page |
| PrivilegesRequired | string | lowest | Installation privilege level (lowest/admin) |
| OutputBaseFilename | string | - | Output installer filename |
| OutputDir | string | . | Directory for generated installer |
| SignTool | string | azsign | Code signing tool to use |
| SignedUninstaller | boolean | yes | Sign the uninstaller executable |
| MinVersion | string | 10.0 | Minimum Windows version (10.0 = Windows 10) |
| SetupIconFile | string | - | Path to installer icon file |
| UninstallDisplayIcon | string | - | Path to uninstaller icon |
| Compression | string | lzma | Compression algorithm (lzma/zip/bzip) |
| SolidCompression | boolean | yes | Use solid compression for smaller size |
| WizardStyle | string | classic | Installer wizard style (classic/modern) |
| WizardImageFile | string | - | Side image for classic wizard |
| WizardSmallImageFile | string | - | Small header image |

**Example:**
```iss
#define MyAppVersion "3.3.6"
#define MyAppName "UniGetUI"
#define MyAppPublisher "Martí Climent"
#define MyAppURL "https://github.com/marticliment/UniGetUI"
#define MyAppExeName "UniGetUI.exe"

[Setup]
AppId={{889610CC-4337-4BDB-AC3B-4F21806C0BDE}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName="{autopf64}\UniGetUI"
DisableProgramGroupPage=yes
OutputBaseFilename=UniGetUI Installer
SignTool=azsign
MinVersion=10.0
Compression=lzma
SolidCompression=yes
```

**Key Configuration Choices:**
- **64-bit Installation**: Uses `{autopf64}` for Program Files (x64)
- **Windows 10+**: Minimum version set to 10.0
- **Code Signing**: Configured with Azure SignTool (azsign)
- **LZMA Compression**: High compression ratio with solid compression
- **PrivilegesRequired**: Can be set to `lowest` for per-user installation or requires admin override dialog

**Environment Variables:**
- `SignTool` command configured in build environment is used via `azsign` identifier
- Inno Setup must be installed at: `%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe`

---

### configuration.winget/unigetui-min.winget

**Purpose:** WinGet DSC (Desired State Configuration) file for minimal UniGetUI runtime installation. Installs only the essential dependencies required to run UniGetUI.

**Settings:**

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| $schema | string | - | DSC schema URL for validation |
| metadata.name | string | - | Configuration name/title |
| metadata.description | string | - | Configuration description |
| metadata.author | string | - | Configuration author |
| resources | array | - | List of packages to install |
| resources.name | string | - | Human-readable resource name |
| resources.type | string | - | Resource type (Microsoft.WinGet/Package) |
| resources.properties.id | string | - | WinGet package ID |
| resources.properties.source | string | winget | Package source to use |

**Example:**
```yaml
$schema: https://raw.githubusercontent.com/PowerShell/DSC/main/schemas/2024/04/config/document.json

metadata:
  name: UniGetUI Minimal Runtime Dependencies
  description: Installs the minimal runtime dependencies required for UniGetUI
  author: Martí Climent

resources:
# Basic dependencies 
- name: Install Microsoft Edge WebView2
  type: Microsoft.WinGet/Package
  properties:
    id: Microsoft.EdgeWebView2Runtime
    source: winget

- name: Install Microsoft Visual C++ 2015-2022 Redistributable
  type: Microsoft.WinGet/Package
  properties:
    id: Microsoft.VCRedist.2015+.x64
    source: winget

- name: Install UniGetUI
  type: Microsoft.WinGet/Package
  properties:
    id: MartiCliment.UniGetUI
    source: winget
```

**Installed Components:**
- Microsoft Edge WebView2 Runtime (for UI rendering)
- Microsoft Visual C++ 2015-2022 Redistributable (required libraries)
- UniGetUI application

**Usage:**
```cmd
# Apply the configuration with WinGet
winget configure configuration.winget/unigetui-min.winget
```

---

### configuration.winget/unigetui-all.winget

**Purpose:** WinGet DSC configuration for complete UniGetUI environment setup. Installs UniGetUI along with all supported package managers for full functionality.

**Example:**
```yaml
$schema: https://raw.githubusercontent.com/PowerShell/DSC/main/schemas/2024/04/config/document.json

metadata:
  name: UniGetUI Complete Environment
  description: Installs all dependencies and tools for UniGetUI development and runtime
  author: Martí Climent

resources:
# Basic dependencies 
- name: Install Microsoft Edge WebView2
  type: Microsoft.WinGet/Package
  properties:
    id: Microsoft.EdgeWebView2Runtime
    source: winget

- name: Install Microsoft Visual C++ 2015-2022 Redistributable
  type: Microsoft.WinGet/Package
  properties:
    id: Microsoft.VCRedist.2015+.x64
    source: winget

- name: Install UniGetUI
  type: Microsoft.WinGet/Package
  properties:
    id: MartiCliment.UniGetUI
    source: winget

# Package Managers (for testing UniGetUI functionality)
- name: Install Chocolatey
  type: Microsoft.WinGet/Package
  properties:
    id: Chocolatey.Chocolatey
    source: winget

- name: Install Python
  type: Microsoft.WinGet/Package
  properties:
    id: Python.Python.3.13
    source: winget

- name: Install PowerShell 7
  type: Microsoft.WinGet/Package
  properties:
    id: Microsoft.PowerShell
    source: winget

- name: Install NodeJS
  type: Microsoft.WinGet/Package
  properties:
    id: OpenJS.NodeJS
    source: winget

- name: Install Rust (Cargo)
  type: Microsoft.WinGet/Package
  properties:
    id: Rustlang.Rustup
    source: winget

- name: Install .NET 8 SDK
  type: Microsoft.WinGet/Package
  properties:
    id: Microsoft.DotNet.SDK.8
    source: winget
```

**Installed Components:**
- All components from `unigetui-min.winget`
- Chocolatey package manager
- Python 3.13 (enables Pip package manager)
- PowerShell 7 (enables PowerShell Gallery)
- Node.js (enables Npm package manager)
- Rust/Rustup (enables Cargo package manager)
- .NET 8 SDK (enables .NET Tool package manager)

**Note:** vcpkg and Scoop require manual installation as they are not available via WinGet

**Usage:**
```cmd
winget configure configuration.winget/unigetui-all.winget
```

---

### configuration.winget/develop-unigetui.winget

**Purpose:** WinGet DSC configuration for setting up a complete UniGetUI development environment. Installs all runtime dependencies, package managers, and development tools.

**Example:**
```yaml
$schema: https://raw.githubusercontent.com/PowerShell/DSC/main/schemas/2024/04/config/document.json

metadata:
  name: UniGetUI Development Environment
  description: Sets up the development environment for UniGetUI
  author: Martí Climent

resources:
# (All resources from unigetui-all.winget plus:)

# Build and deployment tools
- name: Install Git for version control
  type: Microsoft.WinGet/Package
  properties:
    id: Git.Git
    source: winget

- name: Install Visual Studio 2022 Community
  type: Microsoft.WinGet/Package
  properties:
    id: Microsoft.VisualStudio.2022.Community
    source: winget

- name: Install Windows App SDK
  type: Microsoft.WinGet/Package
  properties:
    id: Microsoft.WindowsAppRuntime.1.7
    source: winget

- name: Install Windows SDK
  type: Microsoft.WinGet/Package
  properties:
    id: Microsoft.WindowsSDK.10.0.19041
    source: winget

- name: Install Inno Setup (for installer creation)
  type: Microsoft.WinGet/Package
  properties:
    id: JRSoftware.InnoSetup
    source: winget

- name: Install 7-Zip (for archive handling)
  type: Microsoft.WinGet/Package
  properties:
    id: 7zip.7zip
    source: winget

- name: Install gsudo (sudo for Windows)
  type: Microsoft.WinGet/Package
  properties:
    id: gerardog.gsudo
    source: winget
```

**Additional Development Tools:**
- Git (version control)
- Visual Studio 2022 Community (IDE)
- Windows App SDK 1.7 (WinUI development)
- Windows SDK 10.0.19041 (Windows API development)
- Inno Setup (installer creation)
- 7-Zip (archive compression)
- gsudo (elevated permissions tool)

**Usage:**
```cmd
winget configure configuration.winget/develop-unigetui.winget
```

**Post-Installation:**
After applying this configuration, you can:
1. Clone the repository: `git clone https://github.com/marticliment/UniGetUI.git`
2. Open the solution in Visual Studio 2022
3. Build and run the project

---

## Build & Deployment Scripts

### build_release.cmd

**Purpose:** Batch script to build a release version of UniGetUI, including running tests, building the executable, signing code, and creating installer packages.

**Key Steps:**
1. Updates version resources (`scripts/apply_versions.py`)
2. Kills any running UniGetUI instances
3. Runs unit tests (`dotnet test`)
4. Cleans and publishes the .NET project for x64 Release
5. Optionally signs executables and DLLs
6. Generates integrity tree
7. Creates ZIP archive
8. Builds Inno Setup installer
9. Outputs SHA256 hashes

**Environment Variables:**
- `%signcommand%` - Code signing command configured in environment
- `%INSTALLATOR%` - Path to Inno Setup compiler (default: `%localappdata%\Programs\Inno Setup 6\ISCC.exe`)

**Configuration Options:**
```cmd
set /p signfiles="Do you want to sign the files? [Y/n]: "
```
Interactive prompt to skip code signing (useful for testing)

**Requirements:**
- .NET 8 SDK
- Python 3 (for scripts)
- 7-Zip (for archive creation)
- Inno Setup 6 (for installer creation)
- Code signing certificate (optional)

---

### test_publish.cmd

**Purpose:** Publishes a signed test build of UniGetUI for verification before final release.

**Key Steps:**
1. Builds release configuration
2. Creates test publish directory
3. Copies files from build output
4. Signs executables and DLLs
5. Creates test archive

**Usage:**
```cmd
test_publish.cmd
```

---

### test_publish_nosign.cmd

**Purpose:** Same as `test_publish.cmd` but skips the code signing step for faster iteration during testing.

**Usage:**
```cmd
test_publish_nosign.cmd
```

---

### deep_clean.ps1

**Purpose:** PowerShell script to remove all build artifacts, temporary files, and clean the repository to a pristine state.

**Usage:**
```powershell
.\deep_clean.ps1
```

**Cleans:**
- Build output directories (`bin`, `obj`)
- Published artifacts
- NuGet package cache
- Temporary files

---

## Environment-Specific Configurations

### Development Environment

For local development, use the `develop-unigetui.winget` configuration or manually install:

**Required Tools:**
```powershell
# Install development tools
winget install Git.Git
winget install Microsoft.VisualStudio.2022.Community
winget install Microsoft.DotNet.SDK.8
winget install Microsoft.WindowsAppRuntime.1.7

# Optional: Package managers for testing
winget install Chocolatey.Chocolatey
winget install Python.Python.3.13
winget install OpenJS.NodeJS
```

**Visual Studio Configuration:**
- Open `src/UniGetUI.sln` in Visual Studio 2022
- Build configuration: Debug or Release
- Platform: x64
- Framework: .NET 8.0

**Launch Settings** (`src/UniGetUI/Properties/launchSettings.json`):
```json
{
  "profiles": {
    "UniGetUI (Package)": {
      "commandName": "MsixPackage"
    },
    "UniGetUI (Unpackaged)": {
      "commandName": "Project",
      "commandLineArgs": ""
    }
  }
}
```

---

### Production Environment

For production deployments:

**Installation:**
```powershell
# Via Microsoft Store (Recommended)
# Install from: https://apps.microsoft.com/detail/xpfftq032ptphf

# Or via WinGet
winget install --exact --id MartiCliment.UniGetUI --source winget

# Or via downloaded installer
.\UniGetUI.Installer.exe
```

**Runtime Requirements:**
- Windows 10 (version 10.0) or Windows 11
- Microsoft Edge WebView2 Runtime
- Microsoft Visual C++ 2015-2022 Redistributable (x64)

**Configuration Location:**
User-specific settings are stored in:
- `%LOCALAPPDATA%\UniGetUI\` - Application data and settings
- `%APPDATA%\UniGetUI\` - User preferences and cache

---

## Configuration Best Practices

### Code Quality & Security

1. **Keep analyzers enabled**: The `.deepsource.toml` and `.whitesource` configurations help catch issues early
2. **Address security alerts promptly**: Mend/WhiteSource and DeepSource provide actionable security findings
3. **Review dependency updates**: Don't auto-merge Dependabot/Renovate PRs without testing

### Dependency Management

1. **Use grouped updates**: Group related dependencies (e.g., all GitHub Actions) to reduce PR noise
2. **Enable dependency dashboards**: Renovate's dependency dashboard provides a consolidated view
3. **Monitor both tools**: Dependabot (GitHub Actions) and Renovate (NuGet) complement each other
4. **Test updates in CI/CD**: Always run tests before merging dependency updates

### Build Configuration

1. **Version management**: Update `MyAppVersion` in `UniGetUI.iss` for each release
2. **Code signing**: Always sign release builds for security and user trust
3. **Test before release**: Use `test_publish_nosign.cmd` for quick iteration, then `build_release.cmd` for final build
4. **Verify installers**: Run installers in clean VM environments before distribution

### WinGet Configurations

1. **Use appropriate configuration level**:
   - `unigetui-min.winget` - End users wanting minimal installation
   - `unigetui-all.winget` - End users wanting full package manager support
   - `develop-unigetui.winget` - Contributors and developers
2. **Keep configurations synchronized**: When adding new dependencies, update all relevant .winget files
3. **Document manual steps**: Some package managers (vcpkg, Scoop) require manual installation

### Documentation

1. **Keep configuration files commented**: Inline comments help future maintainers
2. **Update this document**: When adding new configuration files, document them here
3. **Version configuration changes**: Configuration changes should be tracked in version control with clear commit messages
4. **Test configurations**: Verify configuration changes don't break builds or deployments

### Security Considerations

1. **Never commit secrets**: Use environment variables for sensitive data
2. **Rotate signing certificates**: Maintain valid code signing certificates
3. **Review third-party actions**: Keep GitHub Actions dependencies up to date and from trusted sources
4. **Enable security scanning**: Keep WhiteSource/Mend and DeepSource enabled and configured
5. **Use least privilege**: Set `PrivilegesRequired=lowest` in installer when possible

### Maintenance

1. **Regular updates**: Review and update dependencies monthly
2. **Monitor CI/CD**: Keep build scripts and workflows functional
3. **Validate configurations**: Use schema validation for YAML/JSON config files
4. **Archive old configurations**: Remove obsolete configuration files, don't just disable them
5. **Document breaking changes**: When configuration changes affect build process, document in release notes
