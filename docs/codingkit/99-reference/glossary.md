# Glossary

## Project-Specific Terms

### Package Management

- **Package**: A software application or library that can be installed, updated, or removed
- **Package Manager**: A tool that automates the process of installing, updating, and removing packages (e.g., WinGet, Scoop, Chocolatey)
- **Package ID**: Unique identifier for a package within a package manager
- **Package Source**: Repository or registry where packages are hosted
- **Manifest**: Metadata file describing a package and its installation requirements

### UniGetUI Components

- **PEInterface**: Package Engine Interface - the facade providing access to all package managers
- **TaskRecycler**: Performance optimization pattern that deduplicates concurrent task executions
- **Core Services**: Application-wide utilities (Settings, Logging, Language, Icons)
- **Package Engine**: Unified package management abstraction layer
- **Package Loader**: Component that loads and caches package lists (Discoverable, Installed, Upgradable)

### Operations

- **Install Operation**: The process of installing a package on the system
- **Update Operation**: Upgrading an installed package to a newer version
- **Uninstall Operation**: Removing an installed package from the system
- **Bulk Operation**: Performing actions on multiple packages simultaneously

## Technical Terms

### Architecture Patterns

- **Layered Architecture**: Architectural style organizing code into distinct layers with specific responsibilities
- **Facade Pattern**: Design pattern providing simplified interface to complex subsystem
- **Strategy Pattern**: Design pattern enabling selection of algorithm at runtime
- **Observer Pattern**: Design pattern where objects notify subscribers of state changes
- **Factory Pattern**: Design pattern for creating objects without specifying exact class
- **Singleton Pattern**: Design pattern ensuring only one instance of a class exists

### .NET/C# Terms

- **async/await**: C# keywords for asynchronous programming
- **Task**: Represents an asynchronous operation in .NET
- **ConfigureAwait**: Method to configure how async methods resume after await
- **ConcurrentDictionary**: Thread-safe dictionary collection
- **IReadOnlyList**: Interface for read-only list access
- **Nullable Reference Types**: C# 8+ feature for null safety

### Windows Technologies

- **WinUI 3**: Microsoft's modern Windows UI framework
- **Windows App SDK**: Platform for building Windows desktop applications
- **XAML**: Markup language for defining Windows UI
- **MSIX**: Modern Windows app package format
- **COM Interop**: Technology for calling COM APIs from .NET

### Package Manager Specific

- **WinGet**: Windows Package Manager by Microsoft
- **Scoop**: Command-line installer for Windows
- **Chocolatey**: Package manager for Windows
- **Pip**: Python package installer
- **Npm**: Node.js package manager
- **Cargo**: Rust package manager
- **Vcpkg**: C++ library manager

## Acronyms

- **ADR**: Architecture Decision Record
- **API**: Application Programming Interface
- **CLI**: Command Line Interface
- **COM**: Component Object Model
- **CORS**: Cross-Origin Resource Sharing
- **GUI**: Graphical User Interface
- **I18N**: Internationalization (18 letters between I and N)
- **JSON**: JavaScript Object Notation
- **REST**: Representational State Transfer
- **SDK**: Software Development Kit
- **UI**: User Interface
- **UX**: User Experience
- **XAML**: eXtensible Application Markup Language
- **XML**: eXtensible Markup Language

## Related Documentation

- [System Architecture](../02-architecture/system-architecture.md)
- [Design Patterns](../03-development-standards/design-patterns.md)
- [Naming Conventions](../03-development-standards/naming-conventions.md)
