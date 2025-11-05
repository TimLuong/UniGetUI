# Getting Started with UniGetUI Development

Welcome to UniGetUI! This guide will help you get started as a new developer contributing to the project.

## 📋 New Developer Onboarding Checklist

### Week 1: Setup and Familiarization

- [ ] **Day 1-2: Environment Setup**
  - [ ] Complete the [Environment Setup Guide](environment-setup.md)
  - [ ] Successfully build the solution locally
  - [ ] Run the application and explore its features
  - [ ] Run all tests successfully

- [ ] **Day 3-4: Understanding the Codebase**
  - [ ] Read the [Project Overview](../codebase-analysis/01-overview/repository-overview.md)
  - [ ] Review the [Architecture](../codebase-analysis/01-overview/architecture.md)
  - [ ] Study the [Technology Stack](../codebase-analysis/01-overview/technology-stack.md)
  - [ ] Explore the [Project Structure](../codebase-analysis/01-overview/project-structure.md)

- [ ] **Day 5: Contribution Guidelines**
  - [ ] Read [CONTRIBUTING.md](../../CONTRIBUTING.md)
  - [ ] Review [Design Patterns & Coding Standards](../codebase-analysis/07-best-practices/patterns-standards.md)
  - [ ] Set up your IDE with the `.editorconfig` settings
  - [ ] Join the project discussions on GitHub

### Week 2: First Contributions

- [ ] **Day 1-2: Simple Bug Fix or Documentation Improvement**
  - [ ] Find a "good first issue" or documentation improvement
  - [ ] Create a feature branch
  - [ ] Make your changes following coding standards
  - [ ] Write tests if applicable
  - [ ] Submit your first pull request

- [ ] **Day 3-4: Code Review and Learning**
  - [ ] Review feedback on your PR
  - [ ] Study other recent pull requests
  - [ ] Explore how different package managers are implemented
  - [ ] Complete [Tutorial 1: Adding a Simple Feature](tutorials/01-adding-simple-feature.md)

- [ ] **Day 5: Advanced Topics**
  - [ ] Learn about the Package Engine architecture
  - [ ] Understand the WinUI3 UI patterns used in the project
  - [ ] Complete [Tutorial 2: Understanding Package Managers](tutorials/02-understanding-package-managers.md)

### Ongoing: Continuous Learning

- [ ] Participate in code reviews
- [ ] Contribute to documentation improvements
- [ ] Help other new developers in discussions
- [ ] Stay updated with project changes and announcements

## 🎯 Quick Orientation

### What is UniGetUI?

UniGetUI (formerly WingetUI) is a modern Windows desktop application that provides a unified graphical interface for managing packages from multiple package managers including:
- WinGet
- Scoop
- Chocolatey
- Pip
- Npm
- .NET Tool
- PowerShell Gallery
- And more!

### Technology Overview

**Core Technologies:**
- **Language:** C# 12
- **Framework:** .NET 8.0
- **UI Framework:** WinUI 3 (Windows App SDK)
- **Target OS:** Windows 10/11
- **Build System:** MSBuild / .NET CLI

**Key Libraries:**
- Windows App SDK for modern Windows UI
- CommunityToolkit.WinUI for enhanced controls
- YamlDotNet for configuration parsing
- xUnit for testing

### Project Structure at a Glance

```
src/
├── UniGetUI/                              # Main WinUI3 application
│   ├── Pages/                             # UI pages
│   ├── Controls/                          # Reusable UI controls
│   └── Assets/                            # Icons, images, resources
├── UniGetUI.Core.*/                       # Core functionality libraries
│   ├── UniGetUI.Core.Data                # Constants and data structures
│   ├── UniGetUI.Core.Settings            # Settings management
│   ├── UniGetUI.Core.Logger              # Logging infrastructure
│   ├── UniGetUI.Core.LanguageEngine      # Localization system
│   └── UniGetUI.Core.Tools               # Common utilities
├── UniGetUI.PackageEngine.*/             # Package management engine
│   ├── Managers.WinGet/                  # WinGet implementation
│   ├── Managers.Scoop/                   # Scoop implementation
│   ├── Managers.Chocolatey/              # Chocolatey implementation
│   └── ...                                # Other package managers
└── UniGetUI.Interface.*/                 # Interface utilities
```

## 🚀 Your First Day Tasks

### 1. Build and Run (30 minutes)

```bash
# Clone the repository
git clone https://github.com/marticliment/UniGetUI.git
cd UniGetUI

# Restore dependencies
cd src
dotnet restore

# Build the solution
dotnet build UniGetUI.sln --configuration Debug

# Run the application
cd UniGetUI/bin/x64/Debug/net8.0-windows10.0.26100.0/
./UniGetUI.exe
```

### 2. Explore the Application (30 minutes)

- Launch UniGetUI
- Browse available packages
- Try installing a test package (like 7zip)
- Explore the settings
- Check available updates
- Navigate through different tabs

### 3. Run the Tests (15 minutes)

```bash
cd src
dotnet test UniGetUI.sln --verbosity quiet --nologo
```

### 4. Read Core Documentation (1-2 hours)

Start with these essential documents:
1. [README.md](../../README.md) - Project overview
2. [CONTRIBUTING.md](../../CONTRIBUTING.md) - Contribution guidelines
3. [Local Setup Guide](../codebase-analysis/06-workflow/local-setup.md) - Detailed setup instructions

## 💡 Key Concepts to Understand

### 1. Package Manager Abstraction

UniGetUI provides a unified interface for different package managers. Each manager implements the `IPackageManager` interface:

```csharp
public interface IPackageManager
{
    public IPackageDetailsHelper DetailsHelper { get; }
    public IPackageOperationHelper OperationHelper { get; }
    public IMultiSourceHelper SourcesHelper { get; }
    
    public IReadOnlyList<IPackage> FindPackages(string query);
    public IReadOnlyList<IPackage> GetAvailableUpdates();
    public IReadOnlyList<IPackage> GetInstalledPackages();
}
```

### 2. UI Layer (WinUI3)

The UI is built with WinUI3, Microsoft's modern Windows UI framework:
- **XAML** for declarative UI
- **Code-behind** (.cs files) for UI logic
- **MVVM patterns** for separation of concerns
- **CommunityToolkit.WinUI** for enhanced controls

### 3. Settings Management

Settings are managed through a centralized system:
- `Settings.Get(key)` / `Settings.Set(key, value)` for booleans
- `Settings.GetValue(key)` / `Settings.SetValue(key, value)` for strings
- `SecureSettings` for sensitive data like API tokens

### 4. Localization

All user-facing text should be localized:
```csharp
string text = CoreTools.Translate("Hello, World!");
```

Language files are in `src/UniGetUI.Core.LanguageEngine/lang/`

## 🎓 Learning Path

### Beginner Level
1. Complete environment setup
2. Make a simple documentation improvement
3. Fix a simple bug (typo, minor UI issue)
4. Add a translation for your language

### Intermediate Level
1. Add a new setting to the settings page
2. Create a new UI control
3. Improve error handling in existing code
4. Add unit tests for existing functionality

### Advanced Level
1. Add support for a new package manager
2. Implement a new major feature
3. Optimize performance of package operations
4. Refactor complex components

## 📚 Essential Reading

### Must Read (Priority 1)
- [Getting Started](getting-started.md) (this document)
- [Environment Setup](environment-setup.md)
- [CONTRIBUTING.md](../../CONTRIBUTING.md)
- [Design Patterns & Coding Standards](../codebase-analysis/07-best-practices/patterns-standards.md)

### Should Read (Priority 2)
- [Project Architecture](../codebase-analysis/01-overview/architecture.md)
- [Technology Stack](../codebase-analysis/01-overview/technology-stack.md)
- [Adding Features Guide](../codebase-analysis/08-extension/adding-features.md)
- [Build and Deployment](../codebase-analysis/06-workflow/build-deployment.md)

### Nice to Read (Priority 3)
- [Core Functionality](../codebase-analysis/02-core-functionality/features-mapping.md)
- [External APIs Integration](../codebase-analysis/05-integration/external-apis.md)
- All tutorial documents in [tutorials/](tutorials/)

## 🤝 Getting Help

### Resources
- **GitHub Discussions:** Ask questions and discuss features
- **GitHub Issues:** Report bugs and request features
- **Pull Requests:** Review code and learn from others
- **Documentation:** Comprehensive docs in `/docs/` directory

### Who to Ask
- **General Questions:** GitHub Discussions
- **Bug Reports:** GitHub Issues (use BUG/ISSUE template)
- **Feature Requests:** GitHub Issues (use FEATURE REQUEST template)
- **Code Review Help:** Tag maintainers in your PR

### Best Practices for Asking Questions
1. Search existing issues and discussions first
2. Provide context and what you've already tried
3. Include error messages and logs
4. Share relevant code snippets
5. Be respectful and patient

## 🎯 Setting Expectations

### Time Commitment
- **Minimum:** A few hours per week for simple contributions
- **Typical:** 5-10 hours per week for regular contributors
- **Active:** 20+ hours per week for core contributors

### Skill Level
- **Beginner:** C# basics, willing to learn WinUI3
- **Intermediate:** C# and .NET experience, familiar with desktop development
- **Advanced:** Deep C#/.NET knowledge, Windows desktop app expertise

### Response Times
- **Issue Response:** Usually within 1-3 days
- **PR Review:** Typically within 3-7 days
- **Question Answers:** Often within 24-48 hours

## ✅ Verification Checklist

Before moving forward, ensure you can:
- [ ] Build the solution without errors
- [ ] Run the application successfully
- [ ] Run all tests successfully
- [ ] Navigate the codebase confidently
- [ ] Understand the basic architecture
- [ ] Follow the coding standards
- [ ] Create a branch and commit changes
- [ ] Find relevant documentation when needed

## 🎉 Ready to Contribute?

Once you've completed this checklist, you're ready to:
1. Pick an issue labeled "good first issue"
2. Comment on the issue to claim it
3. Create a feature branch
4. Make your changes
5. Submit a pull request

Welcome to the UniGetUI community! We're excited to have you here. 🚀

## 📖 Next Steps

Continue your onboarding journey:
1. [Environment Setup Guide](environment-setup.md) - Detailed setup instructions
2. [Tutorial 1: Adding a Simple Feature](tutorials/01-adding-simple-feature.md) - Hands-on learning
3. [FAQ](faq.md) - Common questions and answers
4. [Best Practices Workshop](tutorials/best-practices-workshop.md) - In-depth patterns

Happy coding! 🎊
