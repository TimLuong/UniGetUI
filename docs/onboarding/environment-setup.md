# Development Environment Setup Guide

This guide provides step-by-step instructions for setting up your development environment for UniGetUI development.

## 📋 Prerequisites Checklist

Before you begin, ensure you have:
- [ ] A Windows 10 (version 2004 or later) or Windows 11 machine
- [ ] Administrator access to install software
- [ ] At least 10 GB of free disk space
- [ ] Stable internet connection for downloading dependencies
- [ ] GitHub account for contributing

## 🛠️ Required Software

### 1. .NET SDK 8.0

**Required Version:** .NET 8.0 (specifically required for this project)

**Installation:**
1. Visit [https://dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Download the .NET 8.0 SDK installer for Windows
3. Run the installer and follow the prompts
4. Verify installation:
   ```cmd
   dotnet --version
   ```
   Expected output: `8.0.xxx`

**Why .NET 8.0?**
- Target framework: `net8.0-windows10.0.26100.0`
- Newer versions may not be compatible with the project configuration

### 2. Visual Studio 2022 (Recommended)

**Edition:** Community (free), Professional, or Enterprise

**Installation:**
1. Download from [https://visualstudio.microsoft.com/](https://visualstudio.microsoft.com/)
2. Run the installer
3. Select the following workloads:
   - ✅ **.NET Desktop Development** (required)
   - ✅ **Windows application development** (recommended)
4. Under Individual Components, ensure these are selected:
   - ✅ Windows 10 SDK (10.0.26100.0 or later)
   - ✅ .NET 8.0 Runtime
   - ✅ C# and Visual Basic Roslyn compilers
5. Complete the installation (may take 30-60 minutes)

**Alternative: Visual Studio Code**
- Lighter weight but less integrated experience
- Install extensions: C# Dev Kit, C#, .NET Core Test Explorer
- [Download VS Code](https://code.visualstudio.com/)

### 3. Git for Windows

**Installation:**
1. Download from [https://git-scm.com/downloads](https://git-scm.com/downloads)
2. Run the installer
3. Recommended settings:
   - Use Visual Studio Code as Git's default editor (or your preference)
   - Override the default branch name: `main`
   - Git from the command line and also from 3rd-party software
   - Use bundled OpenSSH
   - Use the OpenSSL library
   - Checkout Windows-style, commit Unix-style line endings
   - Use Windows' default console window
4. Verify installation:
   ```cmd
   git --version
   ```

**Configure Git:**
```bash
git config --global user.name "Your Name"
git config --global user.email "your.email@example.com"
```

### 4. Python 3.x

**Required for:** Build scripts, version management, integrity generation

**Installation:**
1. Download from [https://www.python.org/downloads/](https://www.python.org/downloads/)
2. Run the installer
3. ⚠️ **Important:** Check "Add Python to PATH" during installation
4. Verify installation:
   ```cmd
   python --version
   ```
   Or on some systems:
   ```cmd
   py --version
   ```

## 🔧 Optional but Recommended Tools

### 7-Zip
- **Purpose:** Creating release packages
- **Download:** [https://www.7-zip.org/](https://www.7-zip.org/)
- **Note:** Only needed for creating distribution packages

### Inno Setup 6
- **Purpose:** Creating Windows installers
- **Download:** [https://jrsoftware.org/isdl.php](https://jrsoftware.org/isdl.php)
- **Default Location:** `%localappdata%\Programs\Inno Setup 6\`
- **Note:** Only needed for building installer packages

### Windows Terminal
- **Purpose:** Better command-line experience
- **Installation:** Available in Microsoft Store
- **Benefits:** Multiple tabs, better fonts, color schemes

### PowerShell 7+
- **Purpose:** Enhanced scripting capabilities
- **Download:** [https://github.com/PowerShell/PowerShell/releases](https://github.com/PowerShell/PowerShell/releases)
- **Note:** Windows PowerShell 5.1 (built-in) also works

## 📥 Clone the Repository

### Using Command Line

**Option 1: Clone the main repository (read-only)**
```bash
git clone https://github.com/marticliment/UniGetUI.git
cd UniGetUI
```

**Option 2: Clone your fork (for contributing)**
```bash
# First, fork the repository on GitHub
# Then clone your fork:
git clone https://github.com/YOUR_USERNAME/UniGetUI.git
cd UniGetUI

# Add the original repository as upstream
git remote add upstream https://github.com/marticliment/UniGetUI.git

# Verify remotes
git remote -v
```

### Using Visual Studio

1. Open Visual Studio 2022
2. Click "Clone a repository"
3. Enter repository URL: `https://github.com/marticliment/UniGetUI.git`
4. Choose local path
5. Click "Clone"

## 🔨 Build the Project

### Method 1: Visual Studio (Easiest)

1. Open `src/UniGetUI.sln` in Visual Studio 2022
2. Select configuration: **Debug** or **Release**
3. Select platform: **x64**
4. Click **Build > Build Solution** or press `Ctrl+Shift+B`
5. Wait for build to complete (first build takes 2-5 minutes)
6. Check Output window for any errors

**Success indicators:**
- Output window shows: `Build: X succeeded, 0 failed`
- No error messages in Error List

### Method 2: .NET CLI (Command Line)

```bash
# Navigate to src directory
cd src

# Restore NuGet packages (first time only)
dotnet restore UniGetUI.sln

# Build in Debug mode
dotnet build UniGetUI.sln --configuration Debug --property:Platform=x64

# Or build in Release mode
dotnet build UniGetUI.sln --configuration Release --property:Platform=x64
```

**Expected output:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### Method 3: Publish for Testing

Creates a self-contained deployment:

```bash
cd src
dotnet publish UniGetUI/UniGetUI.csproj --configuration Release --property:Platform=x64
```

Output location: `src/UniGetUI/bin/x64/Release/net8.0-windows10.0.26100.0/win-x64/publish/`

## ▶️ Run the Application

### From Visual Studio

1. In Solution Explorer, right-click **UniGetUI** project
2. Select **Set as Startup Project** (if not already)
3. Press **F5** (run with debugging) or **Ctrl+F5** (run without debugging)
4. The application should launch

### From Command Line

**After building:**
```cmd
cd src\UniGetUI\bin\x64\Debug\net8.0-windows10.0.26100.0\
UniGetUI.exe
```

**After publishing:**
```cmd
cd src\UniGetUI\bin\x64\Release\net8.0-windows10.0.26100.0\win-x64\publish\
UniGetUI.exe
```

## ✅ Verify Your Setup

### Step 1: Build Verification

```bash
cd src
dotnet build UniGetUI.sln --configuration Debug --property:Platform=x64
```

**Expected result:** `Build succeeded` with 0 errors

### Step 2: Test Verification

```bash
cd src
dotnet test UniGetUI.sln --verbosity quiet --nologo
```

**Expected result:** All tests passing (e.g., `Passed! - Failed: 0, Passed: XX, Skipped: 0`)

### Step 3: Application Launch

1. Run the application (using any method above)
2. The UniGetUI window should open
3. The main interface should display without errors
4. Package managers should be detected automatically

### Step 4: Feature Check

In the running application:
- [ ] Main window opens successfully
- [ ] Navigation between tabs works (Discover, Software, Updates, etc.)
- [ ] Settings page loads
- [ ] At least one package manager is detected (WinGet should be available on Windows 10/11)
- [ ] Search functionality works
- [ ] No error popups or crashes

## 🔍 IDE Configuration

### Visual Studio Setup

#### 1. Editor Config
The project includes an `.editorconfig` file that Visual Studio will automatically respect.

#### 2. Recommended Extensions
- **GitHub Extension for Visual Studio** - Integrated GitHub workflow
- **CodeMaid** (optional) - Code cleanup and formatting
- **Resharper** (optional, paid) - Advanced C# development tools

#### 3. Settings to Configure

**Tools > Options:**

**Text Editor > C# > Advanced:**
- ✅ Enable full solution analysis
- ✅ Place 'System' directives first when sorting usings

**Text Editor > C# > Code Style > Formatting:**
- Should automatically use `.editorconfig` settings

**Debugging:**
- ✅ Enable Just My Code
- ✅ Enable .NET Framework source stepping

### Visual Studio Code Setup

#### Required Extensions
```
code --install-extension ms-dotnettools.csdevkit
code --install-extension ms-dotnettools.csharp
code --install-extension formulahendry.dotnet-test-explorer
code --install-extension editorconfig.editorconfig
```

#### Recommended Extensions
```
code --install-extension eamodio.gitlens
code --install-extension ms-vscode.powershell
```

#### Settings (.vscode/settings.json)
```json
{
  "omnisharp.enableRoslynAnalyzers": true,
  "omnisharp.enableEditorConfigSupport": true,
  "dotnet-test-explorer.testProjectPath": "src/**/*Tests.csproj",
  "files.encoding": "utf8",
  "files.eol": "\n",
  "editor.formatOnSave": true
}
```

## 🐛 Troubleshooting

### Issue 1: .NET SDK Version Mismatch

**Error:**
```
error : The project was restored using Microsoft.NETCore.App version 8.0.x, 
but with current settings, version 8.0.y would be used instead.
```

**Solution:**
1. Check installed SDKs: `dotnet --list-sdks`
2. Install .NET 8.0 SDK from [https://dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
3. Restart your terminal/IDE
4. Retry the build

### Issue 2: Windows SDK Not Found

**Error:**
```
error NETSDK1005: Assets file 'project.assets.json' doesn't have a target for 
'net8.0-windows10.0.26100.0'
```

**Solution:**
1. Open Visual Studio Installer
2. Click "Modify" on your Visual Studio installation
3. Go to "Individual components"
4. Search for "Windows 10 SDK"
5. Select "Windows 10 SDK (10.0.26100.0)" or later
6. Click "Modify" to install

**Alternative:** Download standalone SDK from [https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/](https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/)

### Issue 3: Python Not Found

**Error:**
```
'python' is not recognized as an internal or external command
```

**Solution:**
1. Verify Python installation: `python --version` or `py --version`
2. If not installed, reinstall Python and **check "Add Python to PATH"**
3. Manual PATH addition:
   - Open "Environment Variables" in Windows
   - Add Python installation directory to PATH
   - Typical locations: `C:\Python3X\` or `C:\Users\USERNAME\AppData\Local\Programs\Python\Python3X\`
4. Restart terminal

### Issue 4: NuGet Restore Fails

**Error:**
```
error NU1301: Unable to load the service index for source
```

**Solution:**
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore with verbose output
dotnet restore --verbosity detailed

# If corporate proxy, configure NuGet:
# Edit %AppData%\NuGet\NuGet.Config
```

### Issue 5: Build Fails - Missing Platform

**Error:**
```
error : The Platform property is not set
```

**Solution:**
Always specify platform when building:
```bash
dotnet build --configuration Debug --property:Platform=x64
```

In Visual Studio: Use Configuration Manager to set platform to x64

### Issue 6: Application Won't Start - DLL Missing

**Symptoms:**
- Application crashes on launch
- Error about missing DLLs

**Solution:**
1. Use `dotnet publish` instead of `dotnet build`:
   ```bash
   dotnet publish src/UniGetUI/UniGetUI.csproj --configuration Release --property:Platform=x64
   ```
2. Run from the publish directory
3. Or run from Visual Studio (F5) which handles dependencies

### Issue 7: Git Clone Fails

**Error:**
```
fatal: unable to access 'https://github.com/...': SSL certificate problem
```

**Solution:**
```bash
# Temporary (not recommended for production):
git config --global http.sslVerify false

# Better: Update Git for Windows
# Or: Configure corporate certificate
```

### Issue 8: Visual Studio Solution Won't Open

**Symptoms:**
- "Unsupported" or "incompatible" project error

**Solution:**
1. Ensure Visual Studio 2022 (not 2019 or earlier)
2. Install ".NET Desktop Development" workload
3. Update Visual Studio to latest version
4. Open solution file: `src/UniGetUI.sln`

## 📊 System Requirements

### Minimum Requirements
- **OS:** Windows 10 (version 2004) or Windows 11
- **RAM:** 8 GB
- **Disk Space:** 10 GB free space
- **Processor:** x64 processor
- **.NET:** .NET 8.0 SDK

### Recommended Requirements
- **OS:** Windows 11 latest version
- **RAM:** 16 GB or more
- **Disk Space:** 20 GB free space (includes Visual Studio)
- **Processor:** Modern multi-core x64 processor
- **Display:** 1920x1080 or higher resolution

## 🎓 Next Steps

After successfully setting up your environment:

1. **Read the coding standards:** [Design Patterns & Coding Standards](../codebase-analysis/07-best-practices/patterns-standards.md)
2. **Explore the architecture:** [Project Architecture](../codebase-analysis/01-overview/architecture.md)
3. **Start with a tutorial:** [Tutorial 1: Adding a Simple Feature](tutorials/01-adding-simple-feature.md)
4. **Find your first issue:** Look for "good first issue" labels on GitHub
5. **Join discussions:** Participate in GitHub Discussions

## 🆘 Getting Additional Help

If you're still experiencing issues:

1. **Search existing issues:** [GitHub Issues](https://github.com/marticliment/UniGetUI/issues)
2. **Ask in discussions:** [GitHub Discussions](https://github.com/marticliment/UniGetUI/discussions)
3. **Review FAQ:** [FAQ Document](faq.md)
4. **Check detailed setup guide:** [Local Setup](../codebase-analysis/06-workflow/local-setup.md)

## ✅ Setup Complete!

If you've successfully:
- ✅ Installed all required software
- ✅ Cloned the repository
- ✅ Built the solution without errors
- ✅ Run the application
- ✅ All tests pass

**Congratulations!** Your development environment is ready. Continue to [Getting Started](getting-started.md) for your onboarding checklist.

## 📝 Quick Reference

### Common Commands
```bash
# Build
dotnet build src/UniGetUI.sln --configuration Debug --property:Platform=x64

# Run tests
dotnet test src/UniGetUI.sln --verbosity quiet

# Clean build artifacts
dotnet clean src/UniGetUI.sln

# Restore packages
dotnet restore src/UniGetUI.sln

# Publish
dotnet publish src/UniGetUI/UniGetUI.csproj --configuration Release --property:Platform=x64
```

### Important Paths
- **Solution File:** `src/UniGetUI.sln`
- **Main Project:** `src/UniGetUI/UniGetUI.csproj`
- **Build Output:** `src/UniGetUI/bin/x64/Debug/net8.0-windows10.0.26100.0/`
- **Test Projects:** `src/*Tests/`
- **Documentation:** `docs/`

### Useful Links
- **Main Repository:** [https://github.com/marticliment/UniGetUI](https://github.com/marticliment/UniGetUI)
- **.NET 8.0 Download:** [https://dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Visual Studio 2022:** [https://visualstudio.microsoft.com/](https://visualstudio.microsoft.com/)
- **WinUI 3 Docs:** [https://learn.microsoft.com/en-us/windows/apps/winui/](https://learn.microsoft.com/en-us/windows/apps/winui/)

Happy coding! 🚀
