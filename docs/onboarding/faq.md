# Frequently Asked Questions (FAQ)

This document answers common questions new developers have when contributing to UniGetUI.

## 📚 Table of Contents

- [General Questions](#general-questions)
- [Getting Started](#getting-started)
- [Development Environment](#development-environment)
- [Building and Testing](#building-and-testing)
- [Contributing](#contributing)
- [Architecture and Design](#architecture-and-design)
- [Common Issues](#common-issues)
- [Best Practices](#best-practices)

## General Questions

### What is UniGetUI?

UniGetUI (formerly WingetUI) is a graphical user interface for multiple Windows package managers. It provides a unified interface to discover, install, update, and manage packages from WinGet, Scoop, Chocolatey, Pip, Npm, and other package managers.

### What programming languages do I need to know?

**Required:**
- **C#** - The primary language for the application (C# 12)
- **.NET** - Understanding of .NET 8.0 framework

**Helpful but not required:**
- **XAML** - For UI development (WinUI 3)
- **PowerShell** - For understanding build scripts
- **Python** - For understanding build automation scripts

### What is the tech stack?

- **Language:** C# 12
- **Framework:** .NET 8.0
- **UI Framework:** WinUI 3 (Windows App SDK)
- **Platform:** Windows 10/11 only
- **Testing:** xUnit
- **Build:** MSBuild / .NET CLI

See the full [Technology Stack](../codebase-analysis/01-overview/technology-stack.md) documentation.

### Is UniGetUI cross-platform?

No, UniGetUI is Windows-only because it uses WinUI 3, which is a Windows-specific UI framework.

### Can I use macOS or Linux for development?

No, you need Windows 10 (version 2004 or later) or Windows 11 to develop UniGetUI because:
- WinUI 3 only works on Windows
- The application targets Windows-specific APIs
- Package managers being integrated are Windows-focused

### How much time should I commit?

It depends on your goals:
- **Casual contributor:** A few hours per week (documentation, simple fixes)
- **Regular contributor:** 5-10 hours per week (features, bug fixes)
- **Core contributor:** 20+ hours per week (major features, architecture)

All levels of contribution are welcome!

## Getting Started

### Where do I start?

1. Follow the [Getting Started Guide](getting-started.md)
2. Complete the [Environment Setup](environment-setup.md)
3. Read [CONTRIBUTING.md](../../CONTRIBUTING.md)
4. Pick a "good first issue" from GitHub Issues
5. Join GitHub Discussions

### What is a "good first issue"?

Good first issues are tasks labeled as beginner-friendly:
- Simple bug fixes
- Documentation improvements
- UI text updates
- Translation additions
- Minor feature additions

Search for issues with the "good first issue" label on GitHub.

### How do I claim an issue?

1. Find an issue you want to work on
2. Comment on the issue: "I'd like to work on this"
3. Wait for maintainer acknowledgment
4. Create a feature branch and start working
5. Submit a PR when ready

### Do I need permission to contribute?

No! UniGetUI is open source. You can:
- Fork the repository
- Make changes in your fork
- Submit pull requests

However, commenting on issues before working helps avoid duplicate efforts.

### What if I don't know how to fix an issue?

That's okay! You can:
- Ask questions in the issue comments
- Request guidance in GitHub Discussions
- Study similar code in the project
- Start with simpler issues first
- Pair with experienced contributors

## Development Environment

### What IDE should I use?

**Recommended:** Visual Studio 2022 (Community Edition is free)
- Best WinUI 3 support
- Integrated debugging
- Better IntelliSense for XAML
- Built-in Git support

**Alternative:** Visual Studio Code
- Lighter weight
- Requires extensions (C# Dev Kit)
- Less integrated for WinUI 3 development

### Why does the build fail with .NET version errors?

UniGetUI specifically requires **.NET 8.0** SDK. Newer or older versions may not work.

**Solution:**
```bash
# Check your version
dotnet --version

# Install .NET 8.0 if needed
# Download from: https://dotnet.microsoft.com/download/dotnet/8.0
```

### Do I need Windows 11?

No, Windows 10 (version 2004 or later) works fine for development. The application supports both Windows 10 and 11.

### How much disk space do I need?

- **Minimum:** 10 GB free space
- **Recommended:** 20 GB (includes Visual Studio and dependencies)

### Can I develop on a virtual machine?

Yes, but ensure:
- The VM has Windows 10/11 (not Windows Server)
- Sufficient RAM (8+ GB allocated)
- Nested virtualization enabled (if using Hyper-V)
- GPU acceleration for better UI performance

## Building and Testing

### How do I build the project?

**Visual Studio:**
1. Open `src/UniGetUI.sln`
2. Select **Debug** or **Release** configuration
3. Select **x64** platform
4. Press `Ctrl+Shift+B` to build

**Command Line:**
```bash
cd src
dotnet build UniGetUI.sln --configuration Debug --property:Platform=x64
```

See [Environment Setup](environment-setup.md) for details.

### Why does the build take so long the first time?

The first build:
- Downloads NuGet packages
- Compiles 40+ projects in the solution
- Generates intermediate files
- May take 3-10 minutes

Subsequent builds are much faster (typically 30 seconds to 2 minutes).

### How do I run tests?

```bash
cd src
dotnet test UniGetUI.sln --verbosity quiet --nologo
```

Or in Visual Studio:
- **Test > Run All Tests**
- Or use Test Explorer window

### What if tests fail?

1. Check if the failures are pre-existing (run tests before making changes)
2. Only fix test failures related to your changes
3. Ask for help in your PR if you're unsure

### Do I need to write tests for my changes?

**Yes, when:**
- Adding new functionality
- Modifying business logic
- Fixing bugs (add regression tests)

**No, when:**
- Changing documentation only
- Updating UI text/translations
- Making cosmetic UI changes

### How do I run the application?

**From Visual Studio:**
- Press `F5` (with debugging)
- Or `Ctrl+F5` (without debugging)

**From Command Line:**
```bash
cd src\UniGetUI\bin\x64\Debug\net8.0-windows10.0.26100.0\
UniGetUI.exe
```

### Why does the application crash on startup?

Common causes:
1. Missing dependencies - Use `dotnet publish` instead of `dotnet build`
2. Incorrect platform - Ensure you're building for x64
3. Missing Windows SDK - Install Windows 10 SDK (10.0.26100.0+)
4. Debug vs Release mismatch - Check your configuration

See [Troubleshooting](environment-setup.md#troubleshooting) for solutions.

## Contributing

### How do I create a pull request?

1. Fork the repository on GitHub
2. Clone your fork locally
3. Create a feature branch: `git checkout -b feature/my-feature`
4. Make your changes
5. Commit: `git commit -m "Description"`
6. Push: `git push origin feature/my-feature`
7. Open a PR on GitHub from your branch to `main`

### What should I include in my PR?

- Clear description of changes
- Reference to related issue (e.g., "Fixes #123")
- List of changes if multiple
- Screenshots for UI changes
- Tests for new functionality

### How long does it take to get a PR reviewed?

Typically 3-7 days, but it varies:
- Simple changes may be reviewed in 1-2 days
- Complex changes may take longer
- Reviews are done by volunteers in their free time

Be patient and responsive to feedback!

### What if my PR is rejected?

Don't be discouraged! Common reasons:
- Code style doesn't match guidelines
- Missing tests
- Changes are too broad (should be split)
- Feature doesn't align with project goals

Address feedback and resubmit, or discuss in the PR comments.

### Can I work on multiple issues at once?

Yes, but use separate branches:
```bash
git checkout -b feature/issue-123
# Work on issue 123
git push origin feature/issue-123

git checkout main
git checkout -b feature/issue-124
# Work on issue 124
```

### How do I sync my fork with the main repository?

```bash
# Add upstream remote (once)
git remote add upstream https://github.com/marticliment/UniGetUI.git

# Fetch and merge updates
git fetch upstream
git checkout main
git merge upstream/main
git push origin main
```

## Architecture and Design

### What is the project structure?

```
src/
├── UniGetUI/                    # Main WinUI3 application
├── UniGetUI.Core.*/             # Core libraries (Settings, Logging, etc.)
├── UniGetUI.PackageEngine.*/    # Package management engine
└── UniGetUI.Interface.*/        # Interface utilities
```

See [Project Structure](../codebase-analysis/01-overview/project-structure.md) for details.

### What is the PackageEngine?

The PackageEngine is the abstraction layer for package managers. It provides a unified interface (`IPackageManager`) that each package manager (WinGet, Scoop, etc.) implements.

### How do I add a new package manager?

See [Adding Features - Package Manager](../codebase-analysis/08-extension/adding-features.md#example-adding-a-new-package-manager) for a complete guide.

High-level steps:
1. Create a new project: `UniGetUI.PackageEngine.Managers.YourManager`
2. Implement `IPackageManager` interface
3. Add helper classes for operations, details, and sources
4. Register in the manager initialization
5. Add icons and localization

### What design patterns are used?

- **Factory Pattern:** For creating sources and objects
- **Strategy Pattern:** For different package manager implementations
- **Observer Pattern:** For event-driven updates
- **Singleton Pattern:** For shared services

See [Design Patterns](../codebase-analysis/07-best-practices/patterns-standards.md) for details.

### How does localization work?

1. All user-facing strings go in language files: `src/UniGetUI.Core.LanguageEngine/lang/`
2. Use `CoreTools.Translate("String to translate")` in code
3. Add translations to JSON files for each language
4. English (`en.json`) is the base language

### How do settings work?

**Boolean settings:**
```csharp
bool value = Settings.Get("SettingKey");
Settings.Set("SettingKey", true);
```

**String settings:**
```csharp
string value = Settings.GetValue("SettingKey");
Settings.SetValue("SettingKey", "value");
```

**Secure settings (passwords, tokens):**
```csharp
string token = SecureSettings.GetValue("APIToken");
SecureSettings.SetValue("APIToken", "secret");
```

### What is TaskRecycler?

`TaskRecycler<T>` is a utility that reduces CPU usage by caching and reusing results of expensive operations when multiple callers request the same data concurrently.

Example: If three components request installed packages simultaneously, only one actual query runs, and all three receive the result.

## Common Issues

### "Platform is not set" error when building

**Solution:** Always specify platform when using .NET CLI:
```bash
dotnet build --configuration Debug --property:Platform=x64
```

In Visual Studio, ensure x64 is selected in the configuration dropdown.

### NuGet restore fails

**Solution:**
```bash
# Clear cache
dotnet nuget locals all --clear

# Restore
dotnet restore --verbosity detailed
```

### Missing Windows SDK error

**Solution:**
1. Open Visual Studio Installer
2. Modify your installation
3. Select "Individual components"
4. Install "Windows 10 SDK (10.0.26100.0)" or later

### Python not found

**Solution:**
1. Verify: `python --version` or `py --version`
2. Reinstall Python with "Add to PATH" checked
3. Restart terminal/IDE

### Git push fails with authentication error

**Solution:**
```bash
# Use GitHub CLI for authentication
gh auth login

# Or configure personal access token
git remote set-url origin https://YOUR_TOKEN@github.com/YOUR_USERNAME/UniGetUI.git
```

### Application doesn't detect package managers

This is expected! Package managers are detected dynamically:
- **WinGet:** Pre-installed on Windows 11, downloadable on Windows 10
- **Scoop:** Requires manual installation
- **Chocolatey:** Requires manual installation

The application works even if no package managers are installed, but you'll need at least one for full functionality.

## Best Practices

### What coding style should I follow?

Follow the [Coding Standards](../codebase-analysis/07-best-practices/patterns-standards.md):
- **camelCase** for variables and parameters
- **PascalCase** for methods, classes, properties
- **CAPITAL_CASE** for constants
- Use `.editorconfig` settings (automatic in Visual Studio)

### Should I add comments?

Add comments for:
- Complex logic that isn't immediately obvious
- Public APIs (XML documentation)
- "Why" not "what" (explain intent, not the code)

Don't add comments for:
- Obvious operations (e.g., `i++; // Increment i`)
- Self-explanatory code

### How should I name my branches?

Use descriptive names:
- `feature/add-cargo-support`
- `fix/settings-crash`
- `docs/update-faq`
- `refactor/simplify-package-loader`

### How do I write good commit messages?

```
Brief summary in present tense (50 chars or less)

Longer description if needed, explaining what and why,
not how (the code shows how).

Related issue: #123
```

Example:
```
Add support for Cargo package manager

Implements IPackageManager interface for Rust's Cargo,
enabling installation and management of Rust crates.

Fixes #456
```

### Should I make one big PR or multiple small ones?

**Multiple small PRs are preferred!**
- Easier to review
- Faster to merge
- Less risk of conflicts
- Easier to revert if needed

Break large features into logical, reviewable chunks.

### How do I handle merge conflicts?

```bash
# Update your branch with latest main
git fetch upstream
git merge upstream/main

# Resolve conflicts in your editor
# (Visual Studio has good merge tools)

# Mark as resolved
git add .
git commit -m "Resolve merge conflicts"
git push origin your-branch
```

### What if I'm stuck?

1. **Read the documentation** - Check relevant docs
2. **Search existing issues** - Someone may have had the same problem
3. **Ask in the issue** - Comment on the issue you're working on
4. **GitHub Discussions** - Ask general questions
5. **PR comments** - Ask for help in your draft PR

The community is here to help! Don't hesitate to ask.

## Additional Resources

### Essential Documentation
- [Getting Started Guide](getting-started.md)
- [Environment Setup](environment-setup.md)
- [CONTRIBUTING.md](../../CONTRIBUTING.md)
- [Adding Features Guide](../codebase-analysis/08-extension/adding-features.md)

### Tutorials
- [Tutorial 1: Adding a Simple Feature](tutorials/01-adding-simple-feature.md)
- [Tutorial 2: Understanding Package Managers](tutorials/02-understanding-package-managers.md)
- [Tutorial 3: Working with WinUI 3](tutorials/03-working-with-winui3.md)
- [Best Practices Workshop](tutorials/best-practices-workshop.md)

### External Resources
- [WinUI 3 Documentation](https://learn.microsoft.com/en-us/windows/apps/winui/)
- [.NET 8.0 Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [C# Programming Guide](https://learn.microsoft.com/en-us/dotnet/csharp/)
- [xUnit Testing Framework](https://xunit.net/)

### Community
- [GitHub Repository](https://github.com/marticliment/UniGetUI)
- [GitHub Discussions](https://github.com/marticliment/UniGetUI/discussions)
- [GitHub Issues](https://github.com/marticliment/UniGetUI/issues)

## Still Have Questions?

If your question isn't answered here:

1. **Search GitHub Issues and Discussions** - Your question may already be answered
2. **Ask in GitHub Discussions** - For general questions
3. **Open an issue** - For specific bugs or problems
4. **Comment on relevant PRs** - For questions about specific changes

We're here to help you succeed in contributing to UniGetUI! 🎉

---

**Last Updated:** This FAQ is continuously updated based on common questions from contributors. If you have suggestions for additional Q&A, please submit a PR or open an issue.
