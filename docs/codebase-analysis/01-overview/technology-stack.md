# Technology Stack

## Programming Languages
| Language | Version | Usage |
|----------|---------|-------|
| C# | 12 (latest) | Main application language for all components |
| Python | 3.x | Build scripts, translations, integrity generation |
| PowerShell | 5.1+ / 7+ | Build automation, secret generation, utility scripts |
| Batch Script | Windows CMD | Build orchestration, installer scripts |
| XAML | - | UI markup for WinUI 3 interfaces |

## Frameworks & Libraries
| Technology | Version | Purpose | Documentation Link |
|------------|---------|---------|-------------------|
| .NET | 8.0 | Application framework (net8.0-windows10.0.26100.0) | [https://learn.microsoft.com/en-us/dotnet/](https://learn.microsoft.com/en-us/dotnet/) |
| Windows App SDK | 1.7.250606001 | Modern Windows application platform | [https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/) |
| WinUI 3 | (via Windows App SDK) | Modern Windows UI framework | [https://learn.microsoft.com/en-us/windows/apps/winui/](https://learn.microsoft.com/en-us/windows/apps/winui/) |
| CommunityToolkit.WinUI | 8.2.250402 | UI controls and helpers (Animations, Segmented, SettingsControls, etc.) | [https://learn.microsoft.com/en-us/dotnet/communitytoolkit/](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/) |
| CommunityToolkit.Common | 8.4.0 | Common utilities and helpers | [https://learn.microsoft.com/en-us/dotnet/communitytoolkit/](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/) |
| Microsoft.Windows.CsWinRT | 2.2.0 | C#/WinRT projection for Windows Runtime | [https://github.com/microsoft/CsWinRT](https://github.com/microsoft/CsWinRT) |
| H.NotifyIcon.WinUI | 2.3.0 | System tray icon support for WinUI | [https://github.com/HavenDV/H.NotifyIcon](https://github.com/HavenDV/H.NotifyIcon) |
| YamlDotNet | 16.3.0 | YAML parsing and serialization | [https://github.com/aaubry/YamlDotNet](https://github.com/aaubry/YamlDotNet) |
| PhotoSauce.MagicScaler | 0.15.0 | High-performance image processing | [https://github.com/saucecontrol/PhotoSauce](https://github.com/saucecontrol/PhotoSauce) |
| IdentityModel.OidcClient | 6.0.0 | OpenID Connect and OAuth 2.0 authentication | [https://github.com/IdentityModel/IdentityModel.OidcClient](https://github.com/IdentityModel/IdentityModel.OidcClient) |
| Octokit | 14.0.0 | GitHub API client | [https://github.com/octokit/octokit.net](https://github.com/octokit/octokit.net) |
| System.Drawing.Common | 9.0.8 | Image manipulation support | [https://learn.microsoft.com/en-us/dotnet/api/system.drawing](https://learn.microsoft.com/en-us/dotnet/api/system.drawing) |
| xUnit | 2.9.3 | Unit testing framework | [https://xunit.net/](https://xunit.net/) |
| Microsoft.NET.Test.Sdk | 17.14.1 | Test platform for running tests | [https://github.com/microsoft/vstest](https://github.com/microsoft/vstest) |
| coverlet.collector | 6.0.4 | Code coverage collection | [https://github.com/coverlet-coverage/coverlet](https://github.com/coverlet-coverage/coverlet) |

## Development Tools
| Tool | Version | Purpose |
|------|---------|---------|
| Visual Studio | 2022 (17.9+) | Primary IDE for development |
| .NET SDK | 8.0.407 | Build and runtime tooling |
| Windows SDK | 10.0.26100.4948 | Windows platform development tools |
| Inno Setup | 6.x | Creating Windows installers |
| 7-Zip | - | Archive creation for releases |
| PowerShell | Core 7+ | Build and automation scripts |
| Python | 3.x | Build utilities and scripts |
| Git | - | Version control |

## Infrastructure & Deployment
| Technology | Purpose |
|------------|---------|
| GitHub Actions | CI/CD automation (tests, builds, releases) |
| Microsoft Store | Primary distribution channel |
| WinGet | Package distribution |
| Scoop | Package distribution |
| Chocolatey | Package distribution |
| Dependabot | Dependency updates automation |
| Azure SignTool | Code signing for executables |

## Package Management
- Package manager: **NuGet**
- Configuration files:
  - `*.csproj` - Project files containing package references
  - `*.sln` - Solution file (UniGetUI.sln)
  - `Directory.Build.props` - Shared build properties across projects
  - `.github/dependabot.yml` - Dependency update automation

## Build & Release Configuration
- **Target Framework**: net8.0-windows10.0.26100.0
- **Minimum Platform Version**: 10.0.19041.0 (Windows 10, version 2004)
- **Runtime Identifiers**: win-x64, win-arm64
- **Platform**: x64 (primary)
- **Build Configuration**: Debug, Release
- **Self-Contained Deployment**: Yes
- **Trimming**: Enabled (partial mode)
- **Ready To Run**: Enabled (Release builds)
- **Whole Program Optimization**: Enabled (Release builds)

## Project Structure
The solution contains 41+ C# projects organized into the following categories:
- **UniGetUI.Core**: Core functionality (Data, Settings, Logging, Tools, Classes, IconEngine, LanguageEngine, SecureSettings)
- **UniGetUI.PackageEngine**: Package management engine (Interfaces, Classes, Operations, PackageLoaders, Serializable)
  - **Managers**: WinGet, Scoop, Chocolatey, PowerShell, PowerShell7, Pip, Npm, Dotnet, Cargo, Vcpkg, Generic.NuGet
- **UniGetUI.Interface**: UI components (Enums, BackgroundApi, Telemetry)
- **ExternalLibraries**: Third-party integrations (Clipboard, FilePickers, WindowsPackageManager.Interop)
- **Tests**: Unit tests for core components

## Continuous Integration
The project uses GitHub Actions workflows for:
- **.NET Tests** (`dotnet-test.yml`): Runs xUnit tests on every push/PR
- **CodeQL Analysis** (`codeql.yml`): Security and code quality scanning
- **Translation Tests** (`translations-test.yml`): Validates translation files
- **WinGet Releases** (`winget-prerelease.yml`, `winget-stable.yml`): Automated package publishing
- **Icon Updates** (`update-icons.yaml`): Updates application icons
- **Translation Updates** (`update-tolgee.yml`): Syncs translations from Tolgee service
