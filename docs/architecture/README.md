# CodingKit Architecture Documentation

This directory contains comprehensive documentation for standard project structure and architectural patterns for Windows applications.

## 📚 Documentation Index

### Core Architecture Documentation

1. **[Project Structure](./project-structure.md)**
   - Recommended folder structure (src, tests, docs, config, etc.)
   - Source code organization patterns
   - Platform-specific considerations for .NET, WPF, and WinUI 3
   - Configuration files and best practices
   - Migration paths for existing projects

2. **[Layered Architecture](./layered-architecture.md)**
   - Five-layer architecture approach (Presentation, Application, Business Logic, Data Access, Infrastructure)
   - Dependency injection patterns and containers
   - Clean architecture / hexagonal architecture guidelines
   - SOLID principles with implementation examples
   - Design patterns (Repository, Factory, Strategy, Observer, etc.)
   - Testing strategies for each layer

3. **[ADR Template](./adr-template.md)**
   - Architecture Decision Record template
   - Complete example ADR (WinUI 3 selection decision)
   - Best practices for writing and maintaining ADRs
   - ADR lifecycle management
   - When to create ADRs

## 🎯 Project Templates

Ready-to-use project scaffolds demonstrating best practices:

### [.NET Console Application](../../templates/project-scaffolds/dotnet-console/)
Complete console application template with:
- Layered architecture implementation
- Dependency injection setup
- Command-line argument parsing
- Repository pattern
- Unit tests with xUnit
- Full project structure

### [WPF Application](../../templates/project-scaffolds/wpf-app/)
Modern WPF application template with:
- MVVM pattern using CommunityToolkit.Mvvm
- Navigation service
- Dialog service
- Resource dictionaries and styling
- Data binding examples
- Observable collections

### [WinUI 3 Application](../../templates/project-scaffolds/winui3-app/)
Windows App SDK / WinUI 3 template with:
- Modern Windows 11 design
- NavigationView implementation
- Frame-based navigation
- Fluent Design System
- Activation service
- MSIX packaging configuration

## 🎓 Learning Path

For developers new to the architecture:

1. **Start with Basics**
   - Read [Project Structure](./project-structure.md) to understand folder organization
   - Explore one of the project templates that matches your needs

2. **Understand Layers**
   - Study [Layered Architecture](./layered-architecture.md)
   - Focus on dependency flow and separation of concerns
   - Review the SOLID principles section

3. **Apply Patterns**
   - Examine design patterns in the layered architecture document
   - Study dependency injection examples
   - Implement patterns in your projects

4. **Document Decisions**
   - Use the [ADR Template](./adr-template.md) for significant architectural decisions
   - Review the example ADR to understand the format
   - Create ADRs as your architecture evolves

## 🚀 Quick Start

### Creating a New Project

1. **Choose a Template**
   - Console app: Copy from `/templates/project-scaffolds/dotnet-console/`
   - WPF app: Copy from `/templates/project-scaffolds/wpf-app/`
   - WinUI 3 app: Copy from `/templates/project-scaffolds/winui3-app/`

2. **Follow Template Instructions**
   - Each template includes a detailed README
   - Setup instructions and quick start commands provided
   - Project file examples included

3. **Customize for Your Needs**
   - Rename projects and namespaces
   - Add your business logic
   - Extend with additional features

## 📖 Key Concepts

### Layered Architecture Benefits
- **Separation of Concerns**: Each layer has distinct responsibilities
- **Maintainability**: Changes in one layer minimize impact on others
- **Testability**: Layers can be tested independently
- **Scalability**: Layers evolve independently
- **Team Collaboration**: Different teams work on different layers

### SOLID Principles Covered
- **S**ingle Responsibility Principle
- **O**pen/Closed Principle
- **L**iskov Substitution Principle
- **I**nterface Segregation Principle
- **D**ependency Inversion Principle

### Design Patterns Included
- Repository Pattern
- Factory Pattern
- Strategy Pattern
- Observer Pattern
- Unit of Work Pattern
- Dependency Injection

## 🔗 Related Resources

### Microsoft Documentation
- [.NET Application Architecture](https://docs.microsoft.com/en-us/dotnet/architecture/)
- [WPF Documentation](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
- [WinUI 3 Documentation](https://docs.microsoft.com/en-us/windows/apps/winui/)
- [Windows App SDK](https://docs.microsoft.com/en-us/windows/apps/windows-app-sdk/)

### External Resources
- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Domain-Driven Design](https://www.domainlanguage.com/ddd/)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
- [Architecture Decision Records](https://adr.github.io/)

## 📝 Contributing

When contributing to this documentation:

1. **Follow Existing Patterns**
   - Maintain consistency with existing documentation
   - Use the same formatting and structure

2. **Update Index**
   - Update this README when adding new documents
   - Ensure all links work correctly

3. **Provide Examples**
   - Include code examples where appropriate
   - Use real-world scenarios

4. **Keep Current**
   - Review and update documentation regularly
   - Remove outdated information
   - Add new patterns and practices as they emerge

## 🎯 Acceptance Criteria Met

This documentation suite fulfills the following requirements:

- ✅ Documented recommended folder structure (src, tests, docs, config, etc.)
- ✅ Defined layered architecture approach (Presentation, Business Logic, Data Access)
- ✅ Specified dependency injection patterns and containers
- ✅ Documented clean architecture / hexagonal architecture guidelines
- ✅ Included SOLID principles implementation examples
- ✅ Created architectural decision records (ADR) template
- ✅ Provided project scaffolding templates for .NET, WPF, WinUI 3

## 📊 Document Statistics

- **Total Pages**: 3 main documents + 3 templates = 6 comprehensive guides
- **Code Examples**: 100+ code snippets across all documents
- **Patterns Covered**: 10+ design patterns with implementations
- **Template Projects**: 3 complete, ready-to-use project scaffolds

## 🔄 Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2024-11 | Initial release with all core documentation and templates |

## 📧 Support

For questions or feedback about this documentation:
- Create an issue in the repository
- Review existing documentation for answers
- Consult the external resources listed above

---

**Part of**: CodingKit Framework for Windows Applications (Epic #37)  
**Phase**: Phase 1 - Foundation (Weeks 1-2)
