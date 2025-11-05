# Local Development Setup

## Prerequisites

### Required Software
- [ ] **Windows 10 or Windows 11** - The application is built specifically for Windows
- [ ] **.NET SDK 8.0** - Required for building the application
  - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
  - Target framework: `net8.0-windows10.0.26100.0`
  - **Note:** .NET 8.0 is specifically required; newer versions may not be compatible
- [ ] **Python 3.x** - Required for build scripts (version management, integrity generation)
  - Download from: https://www.python.org/downloads/
- [ ] **Git** - For version control
  - Download from: https://git-scm.com/downloads

### Optional Tools
- [ ] **Visual Studio 2022** (recommended) or **Visual Studio Code**
  - Visual Studio 2022 provides the best development experience for Windows desktop apps
  - Required workload: ".NET Desktop Development"
  - Download from: https://visualstudio.microsoft.com/
- [ ] **7-Zip** - Required for creating release packages
  - Download from: https://www.7-zip.org/
- [ ] **Inno Setup 6** - Required for creating installers
  - Download from: https://jrsoftware.org/isdl.php
  - Default install location: `%localappdata%\Programs\Inno Setup 6\`
- [ ] **Windows SDK 10.0.26100.0** - Usually included with Visual Studio but can be installed separately

## Installation Steps

### 1. Clone the Repository
```bash
git clone https://github.com/marticliment/UniGetUI.git
cd UniGetUI
```

Or if you're working with a fork:
```bash
git clone https://github.com/YOUR_USERNAME/UniGetUI.git
cd UniGetUI
```

### 2. Install Dependencies
Restore all NuGet packages and project dependencies:

```bash
cd src
dotnet restore
```

This will restore dependencies for all projects in the solution, including:
- Main application (UniGetUI)
- Core libraries (UniGetUI.Core.*)
- Package engine managers (WinGet, Scoop, Chocolatey, etc.)
- Test projects

### 3. Environment Configuration

UniGetUI does not require environment configuration files for basic development. The application uses Windows-specific settings and registry entries that are managed at runtime.

However, for building releases, you may need to configure:

**Build Scripts Configuration:**
- The `build_release.cmd` script uses environment variables for code signing
- Set `%signcommand%` if you need to sign binaries (optional for local development)

**Version Management:**
- Version information is managed through `scripts/apply_versions.py`
- Build numbers are tracked in `scripts/BuildNumber`
- You typically don't need to modify these for local development

### 4. Build the Application

#### Option A: Build with Visual Studio
1. Open `src/UniGetUI.sln` in Visual Studio 2022
2. Select **Release** or **Debug** configuration
3. Select **x64** platform
4. Build > Build Solution (or press `Ctrl+Shift+B`)
5. The compiled application will be in: `src/UniGetUI/bin/x64/Release/net8.0-windows10.0.26100.0/`

#### Option B: Build with .NET CLI
```bash
cd src

# Clean previous builds (optional)
dotnet clean UniGetUI.sln

# Build in Release mode
dotnet build UniGetUI.sln --configuration Release --property:Platform=x64

# Or build in Debug mode
dotnet build UniGetUI.sln --configuration Debug --property:Platform=x64
```

#### Option C: Publish for Distribution
```bash
cd src
dotnet publish UniGetUI/UniGetUI.csproj --configuration Release --property:Platform=x64
```

The published output will be in: `src/UniGetUI/bin/x64/Release/net8.0-windows10.0.26100.0/win-x64/publish/`

### 5. Run the Application

#### From Visual Studio
1. Set **UniGetUI** as the startup project (right-click project > Set as Startup Project)
2. Press `F5` to run with debugging or `Ctrl+F5` to run without debugging

#### From Command Line
```bash
# After building, navigate to the output directory
cd src\UniGetUI\bin\x64\Release\net8.0-windows10.0.26100.0\

# Run the application
UniGetUI.exe
```

Or for published builds:
```bash
cd src\UniGetUI\bin\x64\Release\net8.0-windows10.0.26100.0\win-x64\publish\
UniGetUI.exe
```

### 6. Run Tests

Run all unit tests to verify your setup:

```bash
cd src

# Run all tests with minimal verbosity
dotnet test UniGetUI.sln --verbosity quiet --nologo

# Run tests with detailed output
dotnet test UniGetUI.sln --verbosity normal

# Run tests for a specific project
dotnet test UniGetUI.Core.Classes.Tests/
```

## Verification

### Check if Setup is Successful

1. **Build Verification:**
   ```bash
   cd src
   dotnet build UniGetUI.sln --configuration Debug
   ```
   - You should see: `Build succeeded.` with 0 errors

2. **Test Verification:**
   ```bash
   dotnet test UniGetUI.sln --verbosity quiet --nologo
   ```
   - You should see: Tests passing (e.g., `Passed! - Failed: 0, Passed: X, Skipped: 0`)

3. **Application Launch:**
   - Run the application as described in step 5
   - The UniGetUI window should open without errors
   - The application will automatically detect available package managers on your system

4. **Package Manager Detection:**
   - Open UniGetUI
   - Go to Settings to see which package managers were detected
   - At minimum, WinGet should be available on Windows 10/11

## Common Issues

### Issue 1: .NET SDK Not Found
```
error : The project was restored using Microsoft.NETCore.App version 8.0.x, 
but with current settings, version 8.0.y would be used instead.
```
**Solution:** Install the correct .NET 8.0 SDK version
```bash
# Check installed .NET versions
dotnet --list-sdks

# Download and install .NET 8.0 SDK from:
# https://dotnet.microsoft.com/download/dotnet/8.0
```

### Issue 2: Windows SDK Missing
```
error NETSDK1005: Assets file 'project.assets.json' doesn't have a target for 
'net8.0-windows10.0.26100.0'
```
**Solution:** Install Windows 10 SDK version 10.0.26100.0 or later
- Through Visual Studio Installer: Modify your installation and add "Windows 10 SDK (10.0.26100.0)"
- Or download standalone: https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/

### Issue 3: Python Script Errors
```
Python is not recognized as an internal or external command
```
**Solution:** Install Python and add it to PATH
```bash
# Verify Python installation
python --version

# Or use py launcher on Windows (included with Python 3.3+)
py --version
```
- Download Python from: https://www.python.org/downloads/
- During installation, check "Add Python to PATH"

### Issue 4: Build Fails with Platform Configuration
```
error : The Platform property is not set
```
**Solution:** Ensure you specify the platform explicitly
```bash
# Correct .NET CLI syntax
dotnet build --configuration Release --property:Platform=x64

# Or set platform in Visual Studio through Configuration Manager
```

### Issue 5: NuGet Restore Failed
```
error NU1301: Unable to load the service index for source
```
**Solution:** Check your internet connection and NuGet sources
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore with verbose output to see what's failing
dotnet restore --verbosity detailed
```

### Issue 6: Application Won't Start - Missing DLLs
**Solution:** Use `dotnet publish` instead of `dotnet build` or run from Visual Studio
```bash
dotnet publish src/UniGetUI/UniGetUI.csproj --configuration Release --property:Platform=x64
```
The publish command ensures all dependencies are copied to the output directory.

## Development Tools

### Recommended Visual Studio Extensions
- **ReSharper** or **Rider** - Enhanced C# development experience
- **CodeMaid** - Code cleanup and formatting
- **.editorconfig** support - The project includes an `.editorconfig` file for consistent formatting
- **GitHub Extension for Visual Studio** - Integrated GitHub workflow

### Recommended VS Code Extensions
- **C# Dev Kit** - C# language support
- **C#** - Base C# support
- **.NET Core Test Explorer** - Run and debug tests
- **GitLens** - Enhanced Git integration
- **EditorConfig for VS Code** - Respect .editorconfig settings

### Code Style Guidelines
The project follows these conventions (from CONTRIBUTING.md):
- **Naming:** Use camelCase for functions and variables
- **Constants:** Use CAPITAL_LETTERS_WITH_UNDERSCORES
- **Type hints:** Specify variable data types and function return types when possible
- **Spacing:** Add spaces and newlines for readability
- **Single responsibility:** Each commit should focus on one feature or fix

### Useful Commands

```bash
# View solution structure
dotnet sln src/UniGetUI.sln list

# Clean all build outputs
dotnet clean src/UniGetUI.sln

# Build specific configuration
dotnet build src/UniGetUI.sln --configuration Debug
dotnet build src/UniGetUI.sln --configuration Release

# Run specific test project (examples)
dotnet test src/UniGetUI.Core.Classes.Tests/
dotnet test src/UniGetUI.Core.Data.Tests/

# Watch for changes and rebuild automatically (requires dotnet watch)
dotnet watch --project src/UniGetUI/UniGetUI.csproj run
```

### Debugging Tips

1. **Enable Diagnostic Logging:**
   - UniGetUI logs are typically stored in the application data directory
   - Check logs when troubleshooting runtime issues

2. **Debug Specific Package Managers:**
   - Each package manager has its own project under `src/UniGetUI.PackageEngine.Managers.*`
   - Set breakpoints in the relevant manager to debug package operations

3. **Test Individual Components:**
   - The solution includes multiple test projects for different components
   - Run specific tests to verify your changes don't break existing functionality

4. **Use Conditional Breakpoints:**
   - When debugging package operations, use conditional breakpoints based on package name or manager type

### Building the Installer (Advanced)

To create a full installer package:

1. Ensure you have Inno Setup 6 installed at: `%localappdata%\Programs\Inno Setup 6\`
2. Run the release build script:
   ```cmd
   build_release.cmd
   ```
3. Follow the prompts (you can skip code signing for local builds)
4. The installer will be created in the `output/` directory

**Note:** This is only needed for creating distribution packages. For local development, just use `dotnet build` or Visual Studio.

## Additional Resources

- **Official Documentation:** https://www.marticliment.com/unigetui/
- **Contributing Guidelines:** https://github.com/marticliment/UniGetUI/blob/main/CONTRIBUTING.md
- **Issue Tracker:** https://github.com/marticliment/UniGetUI/issues
- **Wiki:** https://github.com/marticliment/UniGetUI/wiki
- **Command-line Arguments:** https://github.com/marticliment/UniGetUI/blob/main/cli-arguments.md

## Getting Help

If you encounter issues not covered here:

1. Check the [GitHub Issues](https://github.com/marticliment/UniGetUI/issues) for similar problems
2. Review the [Discussions](https://github.com/marticliment/UniGetUI/discussions) section
3. Read the [Contributing Guidelines](https://github.com/marticliment/UniGetUI/blob/main/CONTRIBUTING.md) for development standards
4. Create a new issue with:
   - Your Windows version
   - .NET SDK version (`dotnet --version`)
   - Complete error messages
   - Steps to reproduce the problem
