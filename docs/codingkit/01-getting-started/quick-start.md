# Quick Start Guide

Get up and running with UniGetUI development in minutes!

## Prerequisites

Before you begin, ensure you have:

### Required Software

- **Windows 10 (version 2004) or Windows 11** - UniGetUI is a Windows-specific application
- **Visual Studio 2022** (version 17.9 or later) with:
  - .NET desktop development workload
  - Windows application development workload
  - C# language support
- **.NET 8.0 SDK** (version 8.0.407 or later)
- **Git** for version control

### Optional but Recommended

- **Windows Terminal** - Better command-line experience
- **Visual Studio Code** - For markdown editing and quick file edits
- **PowerShell 7+** - Required for some build scripts
- **Python 3.x** - For translation and build automation scripts

### At Least One Package Manager

UniGetUI requires at least one package manager installed for testing:
- **WinGet** (Windows Package Manager) - Included in Windows 11 and Windows 10 (21H2+)
- **Scoop** - Install via: `iwr -useb get.scoop.sh | iex`
- **Chocolatey** - Install via: [chocolatey.org/install](https://chocolatey.org/install)
- **Others**: Pip, Npm, .NET Tool, PowerShell Gallery, Cargo, Vcpkg

## Installation Steps

### 1. Clone the Repository

```bash
# Clone the repository
git clone https://github.com/marticliment/UniGetUI.git
cd UniGetUI

# Or if you've forked it
git clone https://github.com/YOUR_USERNAME/UniGetUI.git
cd UniGetUI
```

### 2. Open the Solution

```bash
# Open the solution in Visual Studio
start UniGetUI.sln

# Or use the Developer Command Prompt
devenv UniGetUI.sln
```

Alternatively, you can:
1. Open Visual Studio 2022
2. Click "Open a project or solution"
3. Navigate to the cloned repository
4. Select `UniGetUI.sln`

### 3. Restore NuGet Packages

Visual Studio should automatically restore NuGet packages when you open the solution. If not:

**In Visual Studio:**
- Right-click the solution in Solution Explorer
- Select "Restore NuGet Packages"

**Via Command Line:**
```bash
dotnet restore
```

### 4. Build the Solution

**In Visual Studio:**
- Press `Ctrl+Shift+B` or
- Select `Build → Build Solution` from the menu

**Via Command Line:**
```bash
dotnet build
```

Expected output:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### 5. Run UniGetUI

**In Visual Studio:**
- Press `F5` to run with debugging
- Or `Ctrl+F5` to run without debugging

**Via Command Line:**
```bash
# Navigate to the output directory
cd src/UniGetUI/bin/Debug/net8.0-windows10.0.26100.0/win-x64

# Run the application
./UniGetUI.exe
```

### 6. Verify the Setup

Once UniGetUI launches:

1. **Check Package Manager Detection**
   - Navigate to Settings → Package Managers
   - Verify that your installed package managers are detected

2. **Test Package Discovery**
   - Go to "Discover Packages" tab
   - Try searching for a package (e.g., "7zip")
   - Verify that results appear

3. **View Installed Packages**
   - Go to "Installed Packages" tab
   - Confirm that your installed packages are listed

If everything works, your development environment is ready! 🎉

## Development Workflow

### Making Changes

1. **Create a Feature Branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make Your Changes**
   - Edit source files in `src/`
   - Follow coding standards in [Coding Standards](../03-development-standards/coding-standards.md)

3. **Build and Test**
   ```bash
   # Build the solution
   dotnet build
   
   # Run unit tests
   dotnet test
   ```

4. **Commit Your Changes**
   ```bash
   git add .
   git commit -m "Brief description of changes"
   ```

### Running Tests

**All Tests:**
```bash
dotnet test
```

**Specific Test Project:**
```bash
dotnet test src/UniGetUI.Core.Classes.Tests/
```

**With Code Coverage:**
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Debugging

**Visual Studio Debugging:**
1. Set breakpoints by clicking in the left margin of code files (or press F9)
2. Press F5 to start debugging
3. Use F10 to step over, F11 to step into, Shift+F11 to step out

**Debug Output:**
- View debug output in the "Output" window (View → Output)
- Check the "Debug" dropdown for detailed logs

**Log Files:**
UniGetUI creates log files in:
- `%LOCALAPPDATA%\UniGetUI\Logs\`

### Hot Reload

WinUI 3 applications support XAML Hot Reload:
1. Run the application with debugging (F5)
2. Modify XAML files
3. Changes apply automatically without restarting

**Note:** C# code changes require a rebuild and restart.

## Common Setup Issues

### Issue: "Windows SDK not found"

**Solution:**
1. Install the Windows SDK via Visual Studio Installer
2. Open Visual Studio Installer → Modify
3. Go to "Individual components"
4. Install "Windows 10 SDK (10.0.26100.0)" or later

### Issue: ".NET 8.0 SDK not found"

**Solution:**
1. Download from [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Install the SDK (not just the runtime)
3. Restart Visual Studio

### Issue: "Build failed with 'Access denied' errors"

**Solution:**
1. Close all UniGetUI instances
2. Clean the solution: `Build → Clean Solution`
3. Rebuild: `Build → Rebuild Solution`

### Issue: "Package manager not detected at runtime"

**Solution:**
1. Ensure the package manager is installed and in PATH
2. Restart UniGetUI
3. Check Settings → Package Managers to manually enable/disable

### Issue: "App crashes on startup"

**Solution:**
1. Check for errors in `%LOCALAPPDATA%\UniGetUI\Logs\`
2. Ensure all runtime dependencies are installed
3. Try running as administrator (some operations require elevation)

## Next Steps

Now that you have UniGetUI running, explore the following:

### Learn the Architecture
- [System Architecture](../02-architecture/system-architecture.md) - Understand how UniGetUI is structured
- [Project Structure](../02-architecture/project-structure.md) - Navigate the codebase
- [Data Flow](../04-core-systems/data-flow.md) - Learn how data moves through the app

### Understand the Standards
- [Coding Standards](../03-development-standards/coding-standards.md) - Write consistent code
- [Design Patterns](../03-development-standards/design-patterns.md) - Use the right patterns
- [Naming Conventions](../03-development-standards/naming-conventions.md) - Name things correctly

### Start Contributing
- [Adding Features](../12-extending/adding-features.md) - Add new functionality
- [Contribution Guide](../12-extending/contribution-guide.md) - Submit your changes
- [Testing Strategy](../06-testing/testing-strategy.md) - Write good tests

## Additional Resources

### Official Documentation
- [Local Setup Guide](../11-devops/local-setup.md) - Detailed environment setup
- [Build & Deployment](../11-devops/build-deployment.md) - Build process details
- [Critical Files](../99-reference/critical-files.md) - Important files reference

### Community Resources
- [GitHub Discussions](https://github.com/marticliment/UniGetUI/discussions) - Ask questions
- [Issue Tracker](https://github.com/marticliment/UniGetUI/issues) - Report bugs
- [Contributing Guide](../../CONTRIBUTING.md) - Contribution guidelines

### External Resources
- [WinUI 3 Documentation](https://learn.microsoft.com/en-us/windows/apps/winui/) - WinUI 3 reference
- [.NET 8 Documentation](https://learn.microsoft.com/en-us/dotnet/) - .NET reference
- [C# Programming Guide](https://learn.microsoft.com/en-us/dotnet/csharp/) - C# language guide

## Tips for Success

1. **Read the documentation** - Familiarize yourself with the architecture before making changes
2. **Follow the standards** - Consistent code is maintainable code
3. **Test your changes** - Run tests and manually verify functionality
4. **Ask questions** - Use GitHub Discussions if you're unsure
5. **Start small** - Begin with minor fixes or improvements
6. **Review existing code** - Learn from the patterns already in use

## Welcome to the Team! 🚀

You're now ready to contribute to UniGetUI. The project welcomes contributions of all sizes—whether it's fixing a typo, improving documentation, or adding major features.

Check the [issue tracker](https://github.com/marticliment/UniGetUI/issues) for "good first issue" labels to find beginner-friendly tasks.

Happy coding!
