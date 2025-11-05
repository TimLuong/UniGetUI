# Approved Packages

## Overview

This document maintains a comprehensive list of all approved NuGet packages used in the UniGetUI project. Each package is documented with its purpose, version, license, and usage guidelines.

**Last Updated**: 2025-11-05

## Package Categories

- [Core Framework Packages](#core-framework-packages)
- [UI and Controls](#ui-and-controls)
- [Image Processing](#image-processing)
- [Authentication and Security](#authentication-and-security)
- [Data and Serialization](#data-and-serialization)
- [GitHub Integration](#github-integration)
- [Testing Frameworks](#testing-frameworks)
- [Build and Development Tools](#build-and-development-tools)

---

## Core Framework Packages

### Microsoft.WindowsAppSDK
- **Version**: 1.7.250606001
- **License**: MIT
- **Purpose**: Provides core Windows App SDK functionality including WinUI 3, windowing, and platform APIs
- **Usage**: Foundation for the entire application UI and Windows integration
- **Maintainer**: Microsoft
- **Documentation**: https://docs.microsoft.com/en-us/windows/apps/windows-app-sdk/
- **Status**: ✅ Active, Required
- **Dependencies**: ~30 transitive packages
- **Security**: Regularly updated by Microsoft, no known vulnerabilities

### Microsoft.Windows.SDK.BuildTools
- **Version**: 10.0.26100.4948
- **License**: Microsoft Software License
- **Purpose**: Provides Windows SDK build tools and APIs for Windows 10/11 development
- **Usage**: Build-time dependency for Windows API access and code generation
- **Maintainer**: Microsoft
- **Documentation**: https://docs.microsoft.com/en-us/windows/win32/
- **Status**: ✅ Active, Required
- **Notes**: Version must be compatible with TargetFramework in Directory.Build.props

### Microsoft.Windows.CsWinRT
- **Version**: 2.2.0
- **License**: MIT
- **Purpose**: C#/WinRT language projection for Windows Runtime APIs
- **Usage**: Enables C# code to consume Windows Runtime components
- **Maintainer**: Microsoft
- **Documentation**: https://github.com/microsoft/CsWinRT
- **Status**: ✅ Active, Required
- **Security**: Signed by Microsoft, regularly updated

### Microsoft.Windows.CsWin32
- **Version**: 0.3.183
- **License**: MIT
- **Purpose**: Source generator for Win32 API access in C#
- **Usage**: Provides type-safe access to Win32 APIs without P/Invoke
- **Maintainer**: Microsoft
- **Documentation**: https://github.com/microsoft/CsWin32
- **Status**: ✅ Active, Required
- **Build-time**: Yes (source generator)
- **Attributes**: PrivateAssets=all, IncludeAssets=runtime;build;native;contentfiles;analyzers;buildtransitive

---

## UI and Controls

### CommunityToolkit.Common
- **Version**: 8.4.0
- **License**: MIT
- **Purpose**: Common utilities and helpers used across Community Toolkit packages
- **Usage**: Foundation for other Community Toolkit packages
- **Maintainer**: .NET Foundation (Community Toolkit)
- **Documentation**: https://docs.microsoft.com/en-us/dotnet/communitytoolkit/
- **Status**: ✅ Active, Required
- **Dependencies**: Minimal

### CommunityToolkit.WinUI.Animations
- **Version**: 8.2.250402
- **License**: MIT
- **Purpose**: Provides animation helpers and effects for WinUI 3 applications
- **Usage**: UI animations and transitions
- **Maintainer**: .NET Foundation (Community Toolkit)
- **Documentation**: https://docs.microsoft.com/en-us/dotnet/communitytoolkit/windows/animations/
- **Status**: ✅ Active
- **Use Cases**: Smooth UI transitions, loading animations, visual effects

### CommunityToolkit.WinUI.Controls.Primitives
- **Version**: 8.2.250402
- **License**: MIT
- **Purpose**: Base controls and primitives for building custom WinUI controls
- **Usage**: Foundation for custom UI controls
- **Maintainer**: .NET Foundation (Community Toolkit)
- **Status**: ✅ Active

### CommunityToolkit.WinUI.Controls.Segmented
- **Version**: 8.2.250402
- **License**: MIT
- **Purpose**: Segmented control for WinUI 3 (similar to iOS segmented control)
- **Usage**: UI component for tab-like selections
- **Maintainer**: .NET Foundation (Community Toolkit)
- **Status**: ✅ Active
- **Use Cases**: Settings tabs, view switchers

### CommunityToolkit.WinUI.Controls.SettingsControls
- **Version**: 8.2.250402
- **License**: MIT
- **Purpose**: Pre-built settings page controls following Windows 11 design
- **Usage**: Settings UI implementation
- **Maintainer**: .NET Foundation (Community Toolkit)
- **Status**: ✅ Active
- **Use Cases**: Application settings pages, preference panels

### CommunityToolkit.WinUI.Controls.Sizers
- **Version**: 8.2.250402
- **License**: MIT
- **Purpose**: Resizable controls and splitters
- **Usage**: Layout management with user-adjustable sizes
- **Maintainer**: .NET Foundation (Community Toolkit)
- **Status**: ✅ Active
- **Use Cases**: Split views, resizable panels

### CommunityToolkit.WinUI.Controls.TokenizingTextBox
- **Version**: 8.2.250402
- **License**: MIT
- **Purpose**: Text input with token/tag support (like email recipients)
- **Usage**: Input fields that require multiple discrete values
- **Maintainer**: .NET Foundation (Community Toolkit)
- **Status**: ✅ Active
- **Use Cases**: Tag input, multi-value fields

### CommunityToolkit.WinUI.Converters
- **Version**: 8.2.250402
- **License**: MIT
- **Purpose**: XAML value converters for data binding
- **Usage**: Convert data types in XAML bindings
- **Maintainer**: .NET Foundation (Community Toolkit)
- **Status**: ✅ Active
- **Use Cases**: Boolean to visibility, string formatting, type conversion

### CommunityToolkit.WinUI.Media
- **Version**: 8.2.250402
- **License**: MIT
- **Purpose**: Media and visual effects for WinUI 3
- **Usage**: Background effects, brushes, and visual enhancements
- **Maintainer**: .NET Foundation (Community Toolkit)
- **Status**: ✅ Active
- **Use Cases**: Acrylic effects, custom brushes, visual effects

### H.NotifyIcon.WinUI
- **Version**: 2.3.0
- **License**: MIT
- **Purpose**: System tray notification icon support for WinUI 3
- **Usage**: System tray integration, background notifications
- **Maintainer**: HavenDV
- **Documentation**: https://github.com/HavenDV/H.NotifyIcon
- **Status**: ✅ Active
- **Downloads**: 500K+
- **Use Cases**: System tray icon, notification display, background app presence
- **Security**: No known vulnerabilities

---

## Image Processing

### PhotoSauce.MagicScaler
- **Version**: 0.15.0
- **License**: Apache-2.0
- **Purpose**: High-performance image resizing and processing
- **Usage**: Package icon processing, image thumbnails
- **Maintainer**: PhotoSauce
- **Documentation**: https://github.com/saucecontrol/PhotoSauce
- **Status**: ✅ Active
- **Performance**: Optimized for performance and quality
- **Use Cases**: Image scaling, thumbnail generation, format conversion
- **Security**: No known vulnerabilities

### System.Drawing.Common
- **Version**: 9.0.8
- **License**: MIT
- **Purpose**: GDI+ graphics library for Windows
- **Usage**: Image manipulation, icon handling
- **Maintainer**: Microsoft (.NET Team)
- **Documentation**: https://docs.microsoft.com/en-us/dotnet/api/system.drawing
- **Status**: ✅ Active
- **Attributes**: Aliases=DrawingCommon
- **Notes**: Windows-only, primarily for legacy code compatibility
- **Security**: Regularly patched by Microsoft

---

## Authentication and Security

### IdentityModel.OidcClient
- **Version**: 6.0.0
- **License**: Apache-2.0
- **Purpose**: OpenID Connect and OAuth 2.0 client library
- **Usage**: User authentication, OAuth flows
- **Maintainer**: Duende Software
- **Documentation**: https://github.com/IdentityModel/IdentityModel.OidcClient
- **Status**: ✅ Active
- **Downloads**: 5M+
- **Use Cases**: User login, token management, OAuth integration
- **Security**: Industry-standard authentication, regular security updates
- **Compliance**: OIDC and OAuth 2.0 certified

---

## Data and Serialization

### YamlDotNet
- **Version**: 16.3.0
- **License**: MIT
- **Purpose**: YAML parser and serializer for .NET
- **Usage**: Configuration file parsing, YAML data handling
- **Maintainer**: Antoine Aubry and contributors
- **Documentation**: https://github.com/aaubry/YamlDotNet
- **Status**: ✅ Active
- **Downloads**: 100M+
- **Use Cases**: Reading package manager manifests, configuration files
- **Security**: No known vulnerabilities, actively maintained

### MessageFormat
- **Version**: 7.1.3
- **License**: MIT
- **Purpose**: ICU MessageFormat implementation for .NET
- **Usage**: Internationalization (i18n), message formatting with placeholders
- **Maintainer**: Jeffijoe
- **Documentation**: https://github.com/jeffijoe/messageformat.net
- **Status**: ✅ Active
- **Use Cases**: Localized message formatting, pluralization, variable substitution
- **Notes**: Supports ICU MessageFormat syntax used in translation files

### System.Text.RegularExpressions
- **Version**: 4.3.1
- **License**: MIT
- **Purpose**: Regular expression functionality
- **Usage**: Pattern matching, text parsing
- **Maintainer**: Microsoft (.NET Team)
- **Status**: ✅ Required for compatibility
- **Security**: CVE-2019-0820 (resolved in this version)
- **Notes**: Explicit version for security compliance

### System.Net.Http
- **Version**: 4.3.4
- **License**: MIT
- **Purpose**: HTTP client functionality
- **Usage**: Network requests, API calls
- **Maintainer**: Microsoft (.NET Team)
- **Status**: ✅ Required for compatibility
- **Security**: Multiple CVEs resolved in this version (CVE-2018-8292, CVE-2019-0657)
- **Notes**: Explicit version for security compliance

### System.Private.Uri
- **Version**: 4.3.2
- **License**: MIT
- **Purpose**: URI parsing and manipulation
- **Usage**: URL handling
- **Maintainer**: Microsoft (.NET Team)
- **Status**: ✅ Required for compatibility
- **Security**: Security fixes included
- **Notes**: Explicit version for security compliance

---

## GitHub Integration

### Octokit
- **Version**: 14.0.0
- **License**: MIT
- **Purpose**: GitHub API client library for .NET
- **Usage**: GitHub integration, repository data access
- **Maintainer**: GitHub
- **Documentation**: https://github.com/octokit/octokit.net
- **Status**: ✅ Active
- **Downloads**: 50M+
- **Use Cases**: Repository information, release data, issue tracking
- **Security**: Official GitHub library, regularly maintained
- **API Coverage**: GitHub REST API v3 and GraphQL API

---

## Windows Package Manager Integration

### Microsoft.WindowsPackageManager.ComInterop
- **Version**: 1.12.350
- **License**: MIT
- **Purpose**: COM interop for Windows Package Manager
- **Usage**: WinGet integration
- **Maintainer**: Microsoft
- **Status**: ✅ Active, Required
- **Attributes**: PrivateAssets=all, IncludeAssets=runtime;build;native;contentfiles;analyzers;buildtransitive

### Microsoft.WindowsPackageManager.InProcCom
- **Version**: 1.12.350
- **License**: MIT
- **Purpose**: In-process COM support for Windows Package Manager
- **Usage**: WinGet in-process integration
- **Maintainer**: Microsoft
- **Status**: ✅ Active, Required
- **Attributes**: PrivateAssets=all, IncludeAssets=runtime;build;native;contentfiles;analyzers;buildtransitive

---

## Testing Frameworks

### xUnit
- **Version**: 2.9.3
- **License**: Apache-2.0
- **Purpose**: Core xUnit testing framework
- **Usage**: Unit testing across all test projects
- **Maintainer**: .NET Foundation (xUnit project)
- **Documentation**: https://xunit.net/
- **Status**: ✅ Active, Required for testing
- **Downloads**: 100M+
- **Scope**: Test projects only

### xUnit.runner.visualstudio
- **Version**: 3.1.4
- **License**: Apache-2.0
- **Purpose**: Visual Studio test adapter for xUnit
- **Usage**: Run xUnit tests in Visual Studio and CLI
- **Maintainer**: .NET Foundation (xUnit project)
- **Status**: ✅ Active, Required for testing
- **Attributes**: PrivateAssets=all, IncludeAssets=runtime;build;native;contentfiles;analyzers;buildtransitive
- **Scope**: Test projects only

### xUnit.abstractions
- **Version**: 2.0.3
- **License**: Apache-2.0
- **Purpose**: Abstractions for xUnit extensibility
- **Usage**: Custom test framework extensions
- **Maintainer**: .NET Foundation (xUnit project)
- **Status**: ✅ Active
- **Scope**: Test projects only

### xUnit.analyzers
- **Version**: 1.24.0
- **License**: Apache-2.0
- **Purpose**: Code analyzers for xUnit best practices
- **Usage**: Compile-time analysis of test code
- **Maintainer**: .NET Foundation (xUnit project)
- **Status**: ✅ Active
- **Attributes**: PrivateAssets=all, IncludeAssets=runtime;build;native;contentfiles;analyzers;buildtransitive
- **Scope**: Test projects only

### Microsoft.NET.Test.Sdk
- **Version**: 17.14.1
- **License**: MIT
- **Purpose**: .NET Test SDK for executing tests
- **Usage**: Test execution infrastructure
- **Maintainer**: Microsoft
- **Documentation**: https://docs.microsoft.com/en-us/dotnet/core/testing/
- **Status**: ✅ Active, Required for testing
- **Scope**: Test projects only

### NSubstitute
- **Version**: 5.3.0
- **License**: BSD-3-Clause
- **Purpose**: Friendly mocking framework for .NET
- **Usage**: Create test doubles and mocks in unit tests
- **Maintainer**: NSubstitute Contributors
- **Documentation**: https://nsubstitute.github.io/
- **Status**: ✅ Active
- **Downloads**: 50M+
- **Use Cases**: Interface mocking, dependency injection testing
- **Scope**: Test projects only

### coverlet.collector
- **Version**: 6.0.4
- **License**: MIT
- **Purpose**: Code coverage data collector
- **Usage**: Collect code coverage during test execution
- **Maintainer**: Coverlet Contributors
- **Documentation**: https://github.com/coverlet-coverage/coverlet
- **Status**: ✅ Active
- **Attributes**: PrivateAssets=all, IncludeAssets=runtime;build;native;contentfiles;analyzers;buildtransitive
- **Scope**: Test projects only
- **Integration**: Works with Visual Studio, CLI, and CI/CD pipelines

---

## Build and Development Tools

Build and development packages are typically set with `PrivateAssets=all` to ensure they don't propagate to consuming projects.

### Code Analyzers and Source Generators

All analyzer and source generator packages should be configured with:
```xml
<PackageReference Include="PackageName" Version="X.Y.Z">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
```

This ensures they only affect build-time and don't become runtime dependencies.

---

## Package Summary

### Total Package Count
- **Production Packages**: ~25
- **Test Packages**: ~7
- **Build Tools**: ~4

### License Distribution
- **MIT**: ~25 packages (majority)
- **Apache-2.0**: ~6 packages
- **BSD-3-Clause**: ~1 package
- **Microsoft Software License**: ~2 packages

### Security Status
All packages are:
- ✅ Free of known critical vulnerabilities
- ✅ Actively maintained
- ✅ Regularly scanned by WhiteSource/Mend
- ✅ Monitored by Renovate Bot for updates

---

## Package Evaluation Criteria

Before a package can be added to this list, it must meet the following criteria:

### Technical Requirements
- [ ] Compatible with .NET 8.0 and Windows 10.0.19041.0+
- [ ] No unresolved critical or high-severity vulnerabilities
- [ ] Actively maintained (commits within last 6 months)
- [ ] Stable versioning (not pre-release for production use)
- [ ] Reasonable package size and dependency count

### Legal Requirements
- [ ] License is compatible with MIT (project license)
- [ ] License is documented and approved
- [ ] Attribution requirements are met (if applicable)
- [ ] No proprietary or commercial-only restrictions

### Quality Requirements
- [ ] High download count (typically > 1M for established packages)
- [ ] Good documentation and examples
- [ ] Active community support
- [ ] Clear changelog and release notes
- [ ] Follows semantic versioning

### Justification Requirements
- [ ] Clear use case documented
- [ ] Alternatives considered and documented
- [ ] Cost-benefit analysis completed
- [ ] Approved by team lead (for major dependencies)

---

## Package Update Policy

### Security Updates
- **Priority**: Immediate (within 1-2 days)
- **Process**: Automated via Renovate or manual emergency patch
- **Testing**: Basic smoke testing, CI/CD validation
- **Approval**: Fast-track approval for critical vulnerabilities

### Major Updates (X.0.0)
- **Priority**: Quarterly review
- **Process**: Manual review of breaking changes
- **Testing**: Full regression testing required
- **Approval**: Team lead approval required

### Minor Updates (0.X.0)
- **Priority**: Monthly review
- **Process**: Automated via Renovate PR
- **Testing**: Standard CI/CD testing
- **Approval**: Code review approval

### Patch Updates (0.0.X)
- **Priority**: Bi-weekly review
- **Process**: Automated via Renovate PR
- **Testing**: Standard CI/CD testing
- **Approval**: Automated approval if CI passes

---

## Deprecated Packages

### Removed Packages

No packages have been removed yet. When packages are removed, they will be documented here with:
- Package name and last used version
- Removal date
- Reason for removal
- Migration path/alternative

---

## Future Considerations

### Under Evaluation

No packages are currently under evaluation. Packages being considered for addition will be documented here.

### Planned Additions

No packages are currently planned for addition. Future planned packages will be documented here with:
- Package name
- Planned use case
- Target milestone
- Evaluation status

---

## Related Documentation

- [Package Guidelines](./package-guidelines.md) - Dependency management standards and guidelines
- [Vulnerability Scanning](./vulnerability-scanning.md) - Security scanning tools and processes

## Maintenance

This document should be updated:
- ✅ When adding a new package
- ✅ When removing a package
- ✅ When updating package versions (quarterly review)
- ✅ When security status changes
- ✅ When license information changes

**Document Owner**: Development Team  
**Review Cycle**: Quarterly  
**Last Review**: 2025-11-05
