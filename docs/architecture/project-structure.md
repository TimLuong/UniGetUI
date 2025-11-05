# Standard Project Structure for Windows Applications

This document defines the recommended folder structure and organization patterns for Windows applications built with .NET, WPF, and WinUI 3.

## Table of Contents

- [Overview](#overview)
- [Recommended Folder Structure](#recommended-folder-structure)
- [Directory Descriptions](#directory-descriptions)
- [Source Code Organization](#source-code-organization)
- [Configuration Files](#configuration-files)
- [Best Practices](#best-practices)

## Overview

A well-organized project structure provides:
- Clear separation of concerns
- Easy navigation and discoverability
- Simplified maintenance and scaling
- Consistent patterns across teams
- Better tooling support

This structure is based on industry best practices and real-world implementations from projects like UniGetUI.

## Recommended Folder Structure

```
/ProjectRoot
├── /src                          # Source code
│   ├── /ProjectName              # Main application project
│   ├── /ProjectName.Core         # Core business logic
│   ├── /ProjectName.Data         # Data access layer
│   ├── /ProjectName.Services     # Service implementations
│   └── /ProjectName.Tests        # Test projects
├── /docs                         # Documentation
│   ├── /architecture             # Architecture documentation
│   ├── /api                      # API documentation
│   └── /guides                   # User and developer guides
├── /tests                        # Integration/E2E tests
├── /scripts                      # Build and automation scripts
├── /config                       # Configuration files
├── /templates                    # Project templates
├── /tools                        # Development tools
├── /media                        # Images, icons, and assets
├── /build                        # Build output (gitignored)
├── /packages                     # NuGet packages (gitignored)
├── .editorconfig                 # Editor configuration
├── .gitignore                    # Git ignore rules
├── .gitattributes                # Git attributes
├── Directory.Build.props         # Shared MSBuild properties
├── Solution.props                # Solution-wide properties
├── README.md                     # Project readme
├── LICENSE                       # License file
└── ProjectName.sln               # Solution file
```

## Directory Descriptions

### /src - Source Code

Contains all source code projects organized by layer and responsibility.

**Main Application Project** (`/src/ProjectName`)
- Entry point (Program.cs, App.xaml)
- UI components (XAML files, views, controls)
- View models (if using MVVM)
- Application configuration
- Platform-specific code

**Core Layer** (`/src/ProjectName.Core`)
- Domain models and entities
- Business logic interfaces
- Core utilities and helpers
- Constants and enumerations
- Shared abstractions

**Business Logic Layer** (`/src/ProjectName.Business` or `/src/ProjectName.Services`)
- Service implementations
- Business rule enforcement
- Domain logic orchestration
- Application use cases

**Data Access Layer** (`/src/ProjectName.Data`)
- Repository implementations
- Database context
- Data models and DTOs
- Migrations
- Data access abstractions

**Infrastructure Layer** (`/src/ProjectName.Infrastructure`)
- External service integrations
- File system operations
- Network communications
- Logging implementations
- Configuration providers

**Shared Components** (`/src/ProjectName.Shared`)
- Cross-cutting concerns
- Shared utilities
- Common interfaces
- Extension methods

### /tests - Test Projects

Organize tests to mirror the source code structure:

```
/tests
├── /ProjectName.Tests            # Unit tests for main project
├── /ProjectName.Core.Tests       # Unit tests for core layer
├── /ProjectName.Integration.Tests # Integration tests
└── /ProjectName.E2E.Tests        # End-to-end tests
```

**Test Organization Best Practices:**
- One test project per source project
- Use `*.Tests` suffix for unit test projects
- Use `*.Integration.Tests` for integration tests
- Use `*.E2E.Tests` for end-to-end tests
- Mirror the folder structure of the code being tested

### /docs - Documentation

```
/docs
├── /architecture                 # Architecture decisions and patterns
│   ├── project-structure.md
│   ├── layered-architecture.md
│   └── adr-template.md
├── /api                          # API documentation
├── /guides                       # User and developer guides
│   ├── getting-started.md
│   ├── development-guide.md
│   └── deployment-guide.md
└── /diagrams                     # Architecture diagrams
```

### /scripts - Automation Scripts

Contains scripts for build automation, deployment, and maintenance:

```
/scripts
├── build.ps1                     # Build script
├── test.ps1                      # Test runner
├── deploy.ps1                    # Deployment script
├── clean.ps1                     # Clean build artifacts
└── version.ps1                   # Version management
```

### /config - Configuration Files

Application configuration files for different environments:

```
/config
├── appsettings.json              # Default settings
├── appsettings.Development.json  # Development settings
├── appsettings.Production.json   # Production settings
└── logging.json                  # Logging configuration
```

### /templates - Project Templates

Scaffolding templates for common patterns:

```
/templates
├── /project-scaffolds            # Complete project templates
├── /service-template             # Service implementation template
├── /repository-template          # Repository template
└── /controller-template          # Controller template
```

## Source Code Organization

### Project Naming Conventions

Follow these patterns for project names:

- **Main Application:** `CompanyName.ProjectName` or `ProjectName`
- **Core/Domain:** `ProjectName.Core` or `ProjectName.Domain`
- **Business Logic:** `ProjectName.Business` or `ProjectName.Services`
- **Data Access:** `ProjectName.Data` or `ProjectName.Infrastructure.Data`
- **Tests:** `ProjectName.ComponentName.Tests`

### Namespace Organization

Namespaces should mirror the folder structure:

```csharp
// Main application
namespace CompanyName.ProjectName;

// Core layer
namespace CompanyName.ProjectName.Core.Entities;
namespace CompanyName.ProjectName.Core.Interfaces;

// Business layer
namespace CompanyName.ProjectName.Services.PackageManagement;

// Data layer
namespace CompanyName.ProjectName.Data.Repositories;
```

### File Organization Within Projects

Organize files by feature or responsibility:

```
/ProjectName.Core
├── /Entities                     # Domain entities
├── /Interfaces                   # Abstractions
├── /Enums                        # Enumerations
├── /Constants                    # Constants
├── /Extensions                   # Extension methods
├── /Utilities                    # Helper classes
└── /Exceptions                   # Custom exceptions
```

### Feature-Based Organization (Alternative)

For larger applications, consider organizing by feature:

```
/ProjectName
├── /Features
│   ├── /PackageManagement
│   │   ├── PackageService.cs
│   │   ├── PackageRepository.cs
│   │   ├── PackageController.cs
│   │   └── PackageViewModel.cs
│   ├── /UserManagement
│   │   ├── UserService.cs
│   │   ├── UserRepository.cs
│   │   └── UserViewModel.cs
│   └── /Settings
│       ├── SettingsService.cs
│       └── SettingsViewModel.cs
```

## Configuration Files

### Solution-Level Configuration

**Directory.Build.props** - Shared MSBuild properties:
```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  </PropertyGroup>
</Project>
```

**Solution.props** - Solution-wide properties:
```xml
<Project>
  <PropertyGroup>
    <Authors>Your Name</Authors>
    <Company>Your Company</Company>
    <Copyright>Copyright © 2024</Copyright>
    <Version>1.0.0</Version>
  </PropertyGroup>
</Project>
```

**.editorconfig** - Code style enforcement:
```ini
root = true

[*.cs]
indent_style = space
indent_size = 4
charset = utf-8
trim_trailing_whitespace = true
insert_final_newline = true
```

### Project References

Order dependencies from most abstract to most concrete:

```
[UI Layer]
    ↓ depends on
[Business Layer]
    ↓ depends on
[Data Layer]
    ↓ depends on
[Core Layer]
```

**Example csproj references:**
```xml
<ItemGroup>
  <!-- UI depends on Business -->
  <ProjectReference Include="..\ProjectName.Business\ProjectName.Business.csproj" />
  <!-- Business depends on Data and Core -->
  <ProjectReference Include="..\ProjectName.Data\ProjectName.Data.csproj" />
  <ProjectReference Include="..\ProjectName.Core\ProjectName.Core.csproj" />
</ItemGroup>
```

## Best Practices

### 1. Separation of Concerns

- Keep UI, business logic, and data access in separate projects
- Each project should have a single, well-defined responsibility
- Avoid circular dependencies between projects

### 2. Dependency Direction

- Dependencies should flow inward toward the core
- Core layer should have no dependencies on outer layers
- UI and Infrastructure depend on Business/Core, never the reverse

### 3. Testability

- Structure code to enable easy unit testing
- Keep test projects parallel to source projects
- Use interfaces to enable mocking and substitution

### 4. Modularity

- Create separate projects for distinct functional areas
- Keep projects focused and cohesive
- Enable independent development and testing

### 5. Configuration Management

- Keep configuration separate from code
- Use environment-specific configuration files
- Never commit sensitive data (use user secrets or environment variables)

### 6. Documentation

- Maintain up-to-date architecture documentation
- Include README files in major project folders
- Document architectural decisions (see ADR template)

### 7. Version Control

- Use `.gitignore` to exclude build artifacts, packages, and IDE files
- Commit configuration files but not secrets
- Include `.editorconfig` for consistent code formatting

### 8. Build Automation

- Provide scripts for common tasks (build, test, deploy)
- Use CI/CD pipelines for automated building and testing
- Version your builds consistently

### 9. Naming Consistency

- Use consistent naming patterns across projects
- Follow .NET naming conventions
- Use descriptive names that reflect purpose

### 10. Keep It Simple

- Don't over-engineer the structure
- Add complexity only when needed
- Refactor as the project grows

## Platform-Specific Considerations

### WPF Applications

```
/ProjectName (WPF)
├── /Views                        # XAML views
├── /ViewModels                   # View models (MVVM)
├── /Models                       # Data models
├── /Controls                     # Custom controls
├── /Converters                   # Value converters
├── /Resources                    # Resource dictionaries
│   ├── Styles.xaml
│   └── Colors.xaml
├── App.xaml                      # Application definition
└── MainWindow.xaml               # Main window
```

### WinUI 3 Applications

```
/ProjectName (WinUI 3)
├── /Views                        # XAML pages and views
├── /ViewModels                   # View models
├── /Controls                     # Custom controls
├── /Services                     # UI services
├── /Themes                       # Theme resource dictionaries
├── /Assets                       # Images and resources
├── App.xaml                      # Application definition
├── MainWindow.xaml               # Main window
└── Package.appxmanifest          # App manifest
```

### Console Applications

```
/ProjectName (Console)
├── /Commands                     # Command implementations
├── /Services                     # Business services
├── /Models                       # Data models
├── /Configuration                # Configuration classes
├── Program.cs                    # Entry point
└── appsettings.json              # Configuration file
```

## Migration Path

When restructuring an existing project:

1. **Assessment**: Analyze current structure and identify issues
2. **Planning**: Design target structure aligned with this guide
3. **Incremental Refactoring**: Move components gradually
4. **Testing**: Verify functionality after each move
5. **Documentation**: Update documentation to reflect changes
6. **Team Communication**: Ensure team understands new structure

## Examples

See `/templates/project-scaffolds/` for complete working examples of:
- .NET Console Application
- WPF Application
- WinUI 3 Application

Each template demonstrates this structure in practice with a simple working application.

## References

- [.NET Project Structure Best Practices](https://docs.microsoft.com/en-us/dotnet/core/project-sdk/overview)
- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Microsoft Application Architecture Guide](https://docs.microsoft.com/en-us/dotnet/architecture/)

## Related Documentation

- [Layered Architecture](./layered-architecture.md)
- [ADR Template](./adr-template.md)
- [SOLID Principles](./solid-principles.md)
