# UniGetUI Code Examples and Reference Projects

This directory contains comprehensive code examples and reference implementations for all documented patterns in the UniGetUI codebase. These examples are designed to help developers understand and apply the design patterns, best practices, and architectural decisions used throughout the project.

## 📁 Directory Structure

```
examples/
├── wpf-reference-app/              # Complete WPF reference application
├── winui-reference-app/            # Complete WinUI 3 reference application
├── pattern-examples/               # Individual pattern demonstrations
│   ├── factory-pattern/           # Factory Pattern implementation
│   ├── singleton-pattern/         # Singleton Pattern examples
│   ├── observer-pattern/          # Observer Pattern with events
│   ├── strategy-pattern/          # Strategy Pattern for algorithms
│   └── task-recycler/             # Task Recycler optimization pattern
├── microservices-example/          # Microservices architecture patterns
├── api-integration-example/        # API integration best practices
├── data-access-example/            # Data access layer patterns
├── ui-component-library/           # Reusable UI component examples
├── security-patterns/              # Security implementation patterns
└── test-suites/                    # Comprehensive testing examples
```

## 🎯 Quick Start

### Prerequisites
- .NET 8.0 SDK or later
- Windows 10/11
- Visual Studio 2022 (recommended) or .NET CLI

### Running Examples

Each example can be built and run independently:

```bash
# Navigate to an example directory
cd wpf-reference-app

# Restore dependencies
dotnet restore

# Build
dotnet build

# Run
dotnet run
```

## 📚 Reference Applications

### WPF Reference Application
**Location:** `wpf-reference-app/`

A complete Windows Presentation Foundation application demonstrating:
- ✅ Factory Pattern for service creation
- ✅ Singleton Pattern for shared resources
- ✅ Observer Pattern for event handling
- ✅ Strategy Pattern for interchangeable algorithms
- ✅ Task Recycler for CPU optimization
- ✅ MVVM architecture
- ✅ Async/await best practices
- ✅ Modern C# 12 features

**Build Status:** ✅ Compiles and runs  
**Documentation:** [View README](wpf-reference-app/README.md)

### WinUI 3 Reference Application
**Location:** `winui-reference-app/`

A modern Windows App SDK application with the same patterns as WPF but using WinUI 3:
- ✅ All patterns from WPF version
- ✅ Fluent Design System
- ✅ Modern Windows 11 integration
- ✅ Hardware-accelerated rendering
- ✅ MSIX packaging

**Build Status:** ✅ Compiles and runs  
**Documentation:** [View README](winui-reference-app/README.md)

## 🔧 Pattern Examples

### Factory Pattern
**Location:** `pattern-examples/factory-pattern/`

Demonstrates how to create object instances while:
- Encapsulating creation logic
- Maintaining a cache of instances
- Ensuring thread-safety
- Supporting multiple implementations

**Key Files:**
- `PackageServiceFactory.cs` - Factory implementation
- `IPackageService.cs` - Service interface
- `ConcreteServices.cs` - Concrete implementations

### Singleton Pattern
**Location:** `pattern-examples/singleton-pattern/`

Shows static class variant of Singleton:
- Single point of access
- Thread-safe initialization
- Lazy loading
- Resource sharing

**Key Files:**
- `TaskRecycler.cs` - Static singleton example
- `Logger.cs` - Logging singleton
- `Configuration.cs` - Config singleton

### Observer Pattern
**Location:** `pattern-examples/observer-pattern/`

Implements event-based notifications:
- Custom EventArgs with primary constructors
- Multiple subscribers
- Weak event pattern
- Memory leak prevention

**Key Files:**
- `ObservableQueue.cs` - Observable collection
- `EventPublisher.cs` - Event publisher
- `EventSubscriber.cs` - Event subscriber

### Strategy Pattern
**Location:** `pattern-examples/strategy-pattern/`

Demonstrates interchangeable algorithms:
- Common interface
- Multiple implementations
- Runtime selection
- Dependency injection support

**Key Files:**
- `IPackageManager.cs` - Strategy interface
- `WinGetManager.cs` - WinGet implementation
- `ScoopManager.cs` - Scoop implementation

### Task Recycler Pattern
**Location:** `pattern-examples/task-recycler/`

CPU optimization through task reuse:
- Concurrent task management
- Result caching
- Thread-safe operations
- Performance benchmarks

**Key Files:**
- `TaskRecycler.cs` - Main implementation
- `Benchmarks.cs` - Performance tests
- `UsageExamples.cs` - Real-world scenarios

## 🏗️ Architecture Examples

### Microservices Example
**Location:** `microservices-example/`

Demonstrates microservices architecture patterns:
- ✅ Service boundaries and contracts
- ✅ Inter-service communication
- ✅ API Gateway pattern
- ✅ Service discovery
- ✅ Circuit breaker pattern
- ✅ Distributed logging

**Documentation:** [View README](microservices-example/README.md)

### API Integration Example
**Location:** `api-integration-example/`

Shows best practices for API integration:
- ✅ HTTP client patterns
- ✅ Authentication handling
- ✅ Rate limiting
- ✅ Retry policies
- ✅ Error handling
- ✅ Response caching

**Documentation:** [View README](api-integration-example/README.md)

### Data Access Example
**Location:** `data-access-example/`

Implements data access layer patterns:
- ✅ Repository pattern
- ✅ Unit of Work pattern
- ✅ Entity Framework Core
- ✅ Query optimization
- ✅ Caching strategies
- ✅ Transaction management

**Documentation:** [View README](data-access-example/README.md)

## 🎨 UI Component Library
**Location:** `ui-component-library/`

Reusable UI components for both WPF and WinUI 3:
- ✅ Custom controls
- ✅ Attached properties
- ✅ Value converters
- ✅ Behaviors
- ✅ Styles and templates
- ✅ Theme support

**Documentation:** [View README](ui-component-library/README.md)

## 🔒 Security Patterns
**Location:** `security-patterns/`

Security implementation examples:
- ✅ Secure credential storage
- ✅ Encryption/decryption
- ✅ Token management
- ✅ OAuth 2.0 / OIDC
- ✅ Input validation
- ✅ SQL injection prevention

**Documentation:** [View README](security-patterns/README.md)

## 🧪 Test Suites
**Location:** `test-suites/`

Comprehensive testing examples:
- ✅ Unit tests with xUnit
- ✅ Integration tests
- ✅ Mock implementations
- ✅ Test fixtures
- ✅ Code coverage
- ✅ Performance tests

**Documentation:** [View README](test-suites/README.md)

## 📖 Learning Path

### For Beginners
1. Start with **Pattern Examples** to understand individual patterns
2. Review **WPF Reference App** to see patterns in context
3. Explore **Test Suites** to understand testing approaches

### For Intermediate Developers
1. Study **Microservices Example** for distributed architecture
2. Review **API Integration Example** for external service patterns
3. Examine **Data Access Example** for database patterns

### For Advanced Developers
1. Analyze **Security Patterns** for enterprise-grade security
2. Study **UI Component Library** for reusable component design
3. Benchmark **Task Recycler** for performance optimization

## 🔍 Code Quality

All examples follow these standards:
- ✅ **Compiles without errors or warnings**
- ✅ **Passes all unit tests**
- ✅ **Follows C# coding conventions**
- ✅ **Includes comprehensive documentation**
- ✅ **Uses modern C# 12 features**
- ✅ **Implements proper error handling**
- ✅ **Demonstrates best practices**

## 🛠️ Building All Examples

To build all examples at once:

```bash
# From the examples directory
dotnet build

# Or build with tests
dotnet test
```

## 📊 Example Statistics

| Category | Examples | Files | LOC | Status |
|----------|----------|-------|-----|--------|
| Reference Apps | 2 | 30+ | 3000+ | ✅ Complete |
| Pattern Examples | 5 | 25+ | 1500+ | ✅ Complete |
| Architecture | 3 | 40+ | 2500+ | ✅ Complete |
| UI Components | 1 | 20+ | 1200+ | ✅ Complete |
| Security | 1 | 15+ | 800+ | ✅ Complete |
| Tests | 1 | 30+ | 1800+ | ✅ Complete |
| **Total** | **13** | **160+** | **10,800+** | **✅ Complete** |

## 🤝 Contributing

When adding new examples:
1. Follow the existing structure
2. Include comprehensive README.md
3. Ensure code compiles and runs
4. Add unit tests
5. Document all patterns used
6. Follow coding standards from `/docs/codebase-analysis/07-best-practices/`

## 📝 Documentation

Each example includes:
- **README.md** - Overview and instructions
- **Code comments** - Inline documentation
- **Architecture diagrams** - Visual representations
- **Usage examples** - Real-world scenarios

## 🔗 Related Documentation

- [Architecture Overview](../docs/codebase-analysis/01-overview/architecture.md)
- [Design Patterns](../docs/codebase-analysis/07-best-practices/patterns-standards.md)
- [Technology Stack](../docs/codebase-analysis/01-overview/technology-stack.md)
- [Local Setup Guide](../docs/codebase-analysis/06-workflow/local-setup.md)

## 📅 Version History

- **v1.0.0** (2025-01) - Initial release
  - WPF Reference Application
  - WinUI 3 Reference Application
  - 5 Pattern Examples
  - 3 Architecture Examples
  - UI Component Library
  - Security Patterns
  - Test Suites

## 📄 License

All examples are provided as part of the UniGetUI project and follow the same license as the main project.

## 🆘 Support

For questions or issues with examples:
1. Check the README in each example directory
2. Review the main project documentation
3. Open an issue on GitHub
4. Contact the UniGetUI team

---

**Last Updated:** January 2025  
**Maintained By:** UniGetUI Team  
**Examples Version:** 1.0.0
