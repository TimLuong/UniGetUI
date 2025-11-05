# Troubleshooting Guide

This comprehensive guide helps you resolve common issues when developing UniGetUI.

## 📚 Quick Navigation

- [Build Issues](#build-issues)
- [Environment Setup Issues](#environment-setup-issues)
- [Runtime Issues](#runtime-issues)
- [Git and Version Control Issues](#git-and-version-control-issues)
- [Testing Issues](#testing-issues)
- [IDE Issues](#ide-issues)
- [Package Manager Issues](#package-manager-issues)
- [Getting More Help](#getting-more-help)

---

## Build Issues

### Issue: .NET SDK Version Mismatch

**Error:**
```
error : The project was restored using Microsoft.NETCore.App version 8.0.x, 
but with current settings, version 8.0.y would be used instead.
```

**Cause:** Wrong .NET SDK version installed

**Solution:**
```bash
# 1. Check installed SDKs
dotnet --list-sdks

# 2. Install .NET 8.0 SDK if not present
# Download from: https://dotnet.microsoft.com/download/dotnet/8.0

# 3. Verify installation
dotnet --version  # Should show 8.0.xxx

# 4. Restart terminal/IDE and retry
```

---

### Issue: Windows SDK Not Found

**Error:**
```
error NETSDK1005: Assets file 'project.assets.json' doesn't have a target for 
'net8.0-windows10.0.26100.0'
```

**Cause:** Missing Windows 10 SDK

**Solution via Visual Studio:**
1. Open Visual Studio Installer
2. Click "Modify" on your Visual Studio installation
3. Go to "Individual components" tab
4. Search for "Windows 10 SDK"
5. Select "Windows 10 SDK (10.0.26100.0)" or later
6. Click "Modify" to install
7. Restart Visual Studio

**Solution via Standalone Installer:**
1. Visit [Windows SDK Downloads](https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/)
2. Download and install Windows 10 SDK 10.0.26100.0 or later
3. Restart IDE

---

### Issue: Platform Not Set

**Error:**
```
error : The Platform property is not set
```

**Cause:** Build command missing platform specification

**Solution:**

**Command Line:**
```bash
# Always specify platform
dotnet build --configuration Debug --property:Platform=x64

# NOT just:
dotnet build  # This will fail
```

**Visual Studio:**
1. Open Configuration Manager (Build > Configuration Manager)
2. Ensure "x64" is selected in platform dropdown
3. Rebuild solution

---

### Issue: NuGet Restore Fails

**Error:**
```
error NU1301: Unable to load the service index for source
error NU1101: Unable to find package
```

**Cause:** Network issues, cache corruption, or proxy problems

**Solution:**
```bash
# 1. Clear NuGet cache
dotnet nuget locals all --clear

# 2. Check NuGet sources
dotnet nuget list source

# 3. Restore with verbose output to see what's failing
dotnet restore --verbosity detailed

# 4. If behind corporate proxy, configure NuGet
# Edit %AppData%\NuGet\NuGet.Config
# Add proxy settings:
<configuration>
  <config>
    <add key="http_proxy" value="http://proxy.company.com:8080" />
  </config>
</configuration>

# 5. Retry restore
dotnet restore
```

---

### Issue: Build Succeeds But Thousands of Warnings

**Cause:** Code analysis warnings (normal, not errors)

**What to do:**
- Warnings are informational and don't prevent building
- Focus on errors first
- New code should minimize warnings
- Existing warnings will be addressed over time

**Filter warnings in Visual Studio:**
1. Error List window
2. Click dropdown next to "Errors"
3. Select "Errors" only (hide warnings)

---

### Issue: Build Takes Forever

**Cause:** First build downloads packages and compiles 40+ projects

**Expected Times:**
- First build: 3-10 minutes
- Subsequent builds: 30 seconds - 2 minutes
- Clean rebuild: 2-5 minutes

**Speed up builds:**
```bash
# Build only what changed (incremental)
dotnet build

# Build specific project
dotnet build src/UniGetUI/UniGetUI.csproj

# Use multiple processors
dotnet build -m
```

---

## Environment Setup Issues

### Issue: Python Not Found

**Error:**
```
'python' is not recognized as an internal or external command
```

**Solution:**

**Option 1: Reinstall Python**
1. Download from [python.org](https://www.python.org/downloads/)
2. Run installer
3. ⚠️ **Check "Add Python to PATH"** during installation
4. Restart terminal

**Option 2: Manual PATH Addition**
```bash
# Windows: Add to PATH environment variable
# 1. Open "Environment Variables" (search in Start)
# 2. Edit "Path" in User Variables
# 3. Add Python install directory (e.g., C:\Python311\)
# 4. Add Python Scripts directory (e.g., C:\Python311\Scripts\)
# 5. Restart terminal

# Verify
python --version  # or: py --version
```

---

### Issue: Git Clone Fails

**Error:**
```
fatal: unable to access 'https://github.com/...': SSL certificate problem
```

**Solution:**

**Temporary (not recommended):**
```bash
git config --global http.sslVerify false
```

**Better Solutions:**
1. Update Git for Windows to latest version
2. Configure corporate certificate if behind proxy
3. Use SSH instead of HTTPS:
   ```bash
   git clone git@github.com:marticliment/UniGetUI.git
   ```

---

### Issue: Visual Studio Won't Open Solution

**Error:** "Unsupported" or "incompatible project" error

**Solution:**
1. Ensure you have Visual Studio **2022** (not 2019 or earlier)
2. Install ".NET Desktop Development" workload
3. Update Visual Studio to latest version
4. Open `src/UniGetUI.sln` (not other .sln files)

---

## Runtime Issues

### Issue: Application Crashes on Startup

**Symptoms:**
- App opens briefly then closes
- Error about missing DLLs
- Exception dialog

**Solutions:**

**Solution 1: Use Publish Instead of Build**
```bash
# Instead of:
dotnet build

# Use:
dotnet publish src/UniGetUI/UniGetUI.csproj --configuration Release --property:Platform=x64

# Then run from:
src/UniGetUI/bin/x64/Release/net8.0-windows10.0.26100.0/win-x64/publish/UniGetUI.exe
```

**Solution 2: Run from Visual Studio**
- Press F5 (or Ctrl+F5) instead of running the .exe directly
- Visual Studio handles dependencies automatically

**Solution 3: Clean and Rebuild**
```bash
cd src
dotnet clean
dotnet build --configuration Debug --property:Platform=x64
```

---

### Issue: Package Managers Not Detected

**Symptoms:**
- Application runs but shows no package managers
- Empty package list

**Expected Behavior:**
- This is actually NORMAL if you don't have package managers installed
- UniGetUI detects what's available on your system

**Solution:**
Install at least one package manager:

**WinGet (recommended):**
- Pre-installed on Windows 11
- On Windows 10: Install from Microsoft Store (App Installer)
- Or: `winget --version` to check if already installed

**Scoop:**
```powershell
# In PowerShell
Set-ExecutionPolicy RemoteSigned -Scope CurrentUser
irm get.scoop.sh | iex
```

**Chocolatey:**
```powershell
# In PowerShell (admin)
Set-ExecutionPolicy Bypass -Scope Process -Force
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072
iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
```

---

### Issue: Settings Don't Persist

**Symptoms:**
- Changes to settings are lost when app restarts
- Toggle switches reset

**Solution:**

**Check Settings Storage:**
Settings are stored in: `%LOCALAPPDATA%\UniGetUI\`

**Troubleshooting:**
1. Verify the folder exists and is writable
2. Check if running as different user
3. Look for permission issues
4. Delete settings file to reset (backup first!)

**In Code:**
```csharp
// Verify setting is being saved
Settings.Set("YourSetting", value);

// And loaded
bool value = Settings.Get("YourSetting");
```

---

## Git and Version Control Issues

### Issue: Can't Push to Repository

**Error:**
```
remote: Permission to marticliment/UniGetUI.git denied
fatal: unable to access 'https://github.com/...': The requested URL returned error: 403
```

**Cause:** Pushing to main repository without write access

**Solution:**
You need to fork and push to your fork:

```bash
# 1. Fork the repository on GitHub (click Fork button)

# 2. Add your fork as a remote
git remote add myfork https://github.com/YOUR_USERNAME/UniGetUI.git

# 3. Push to your fork
git push myfork your-branch

# 4. Create PR from your fork to main repository
```

---

### Issue: Merge Conflicts

**Error:**
```
CONFLICT (content): Merge conflict in file.cs
Automatic merge failed; fix conflicts and then commit the result.
```

**Solution:**

**Visual Studio:**
1. Open file with conflicts
2. Look for conflict markers:
   ```
   <<<<<<< HEAD
   Your changes
   =======
   Their changes
   >>>>>>> branch-name
   ```
3. Use Visual Studio's merge tool (appears automatically)
4. Choose: Take Current, Take Incoming, or manually merge
5. Mark as resolved
6. Commit

**Command Line:**
```bash
# 1. See which files have conflicts
git status

# 2. Edit files manually, remove conflict markers

# 3. Stage resolved files
git add file.cs

# 4. Complete merge
git commit -m "Resolve merge conflicts"
```

---

### Issue: Accidentally Committed to Wrong Branch

**Solution:**

```bash
# If you haven't pushed yet:

# 1. Create a new branch with your changes
git branch feature/my-feature

# 2. Reset current branch to before your commits
git reset --hard origin/main

# 3. Switch to your new branch
git checkout feature/my-feature

# Now your changes are on the correct branch!
```

---

### Issue: Want to Undo Last Commit

**Solution:**

```bash
# Keep changes, undo commit
git reset --soft HEAD~1

# Discard changes, undo commit (careful!)
git reset --hard HEAD~1

# Undo commit, keep files staged
git reset HEAD~1
```

---

## Testing Issues

### Issue: Tests Fail After Fresh Clone

**What to do:**
1. Run tests BEFORE making changes to establish baseline
2. If tests were already failing, it's not your fault
3. Only fix test failures related to your changes
4. Report pre-existing failures in the issue

**Check Test Status:**
```bash
dotnet test src/UniGetUI.sln --verbosity quiet
```

---

### Issue: Specific Test Keeps Failing

**Debugging Steps:**

**Run single test:**
```bash
# Run specific test project
dotnet test src/UniGetUI.Core.Classes.Tests/

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"
```

**Debug in Visual Studio:**
1. Open Test Explorer (Test > Test Explorer)
2. Right-click failing test
3. Select "Debug"
4. Set breakpoints and inspect values

---

### Issue: Tests Pass Locally But Fail in CI

**Common Causes:**
- Timing issues (tests too fast/slow in CI)
- File path differences (Windows vs. Linux separators)
- Environment-specific behavior

**What to do:**
1. Check CI logs for specific error
2. Try to reproduce locally
3. Ask for help in PR comments
4. Consider if test needs to be more robust

---

## IDE Issues

### Issue: IntelliSense Not Working

**Symptoms:**
- No autocomplete
- Red squiggles everywhere
- Can't navigate to definitions

**Solution:**

**Visual Studio:**
```
1. Close Visual Studio
2. Delete .vs folder in solution directory
3. Delete bin and obj folders in all project directories
4. Reopen solution
5. Let IntelliSense rebuild (wait 2-3 minutes)
6. If still broken: Tools > Options > Text Editor > C# > Advanced > Restart IntelliSense
```

**Visual Studio Code:**
```
1. Reload window: Ctrl+Shift+P > "Reload Window"
2. Restart OmniSharp: Ctrl+Shift+P > "OmniSharp: Restart OmniSharp"
3. Clean workspace: Delete .vs and .vscode folders
```

---

### Issue: Debugger Won't Attach

**Solution:**

1. Ensure "Debug" configuration selected (not "Release")
2. Build succeeds before debugging
3. Set UniGetUI as startup project
4. Disable "Just My Code": Tools > Options > Debugging > Enable Just My Code (uncheck)
5. Clean and rebuild

---

### Issue: XAML Designer Won't Load

**Error:** "The designer could not be loaded"

**Solution:**

**This is common and not critical!**
- XAML designer for WinUI 3 is less stable than WPF
- You can still edit XAML as text
- Preview changes by running the app (F5)

**Try:**
1. Close and reopen file
2. Restart Visual Studio
3. Update Visual Studio to latest version
4. Install latest Windows App SDK

**Workaround:**
- Edit XAML as text
- Run app to see changes (hot reload works!)
- Use XAML Hot Reload: make changes while debugging

---

## Package Manager Issues

### Issue: Can't Add New Package Manager

**Checklist:**
- [ ] Created project in correct location
- [ ] Implements IPackageManager interface
- [ ] Registered in manager initialization code
- [ ] Added to solution file
- [ ] Added project reference from main project
- [ ] Built successfully

**Common mistake:** Forgetting to register the manager

**Where to register:**
Look for manager initialization code (usually in PackageEngine initialization)

---

### Issue: Package Manager CLI Not Found

**Error:** "Could not find package manager executable"

**Cause:** Package manager not installed or not in PATH

**Solution:**
1. Verify package manager is installed:
   ```bash
   winget --version
   scoop --version
   choco --version
   ```
2. If not found, install it
3. Ensure it's in PATH environment variable
4. Restart IDE after installing

---

## Getting More Help

### When to Ask for Help

Ask for help when:
- You've tried solutions in this guide
- Spent >30 minutes on the same issue
- Error message is unclear
- Issue is unique to your setup

### Where to Ask

**GitHub Discussions (General Questions):**
- [UniGetUI Discussions](https://github.com/marticliment/UniGetUI/discussions)
- Best for: How-to questions, architecture questions, clarifications

**GitHub Issues (Bugs):**
- [UniGetUI Issues](https://github.com/marticliment/UniGetUI/issues)
- Best for: Actual bugs, build failures, feature requests
- Use templates provided

**Pull Request Comments:**
- Best for: Questions about your specific PR
- Tag maintainers for help

### What to Include When Asking

**Always include:**
1. **What you're trying to do**
2. **What you tried**
3. **Full error message** (paste as code block)
4. **Your environment:**
   ```
   - OS: Windows 10/11 (version)
   - Visual Studio version
   - .NET SDK version: (from `dotnet --version`)
   - What you've tried from this guide
   ```
5. **Relevant code/configuration** if applicable

**Example Good Question:**
```markdown
## Issue: Build fails with Windows SDK error

**Environment:**
- Windows 11 23H2
- Visual Studio 2022 Community (17.8.3)
- .NET SDK 8.0.100

**Error:**
```
error NETSDK1005: Assets file 'project.assets.json' doesn't have a target for 
'net8.0-windows10.0.26100.0'
```

**What I tried:**
1. Checked Visual Studio Installer - Windows SDK 10.0.22621.0 is installed
2. Tried reinstalling Visual Studio
3. Cleared NuGet cache: `dotnet nuget locals all --clear`

**Question:** The guide says I need SDK 10.0.26100.0, but I only see 
10.0.22621.0 in the installer. Where can I get the correct version?
```

---

## 📝 Troubleshooting Checklist

Before asking for help, try:
- [ ] Read error message carefully
- [ ] Search this guide for the error/issue
- [ ] Search GitHub issues for similar problems
- [ ] Try cleaning and rebuilding
- [ ] Restart IDE
- [ ] Check you're on the right branch
- [ ] Verify all prerequisites installed
- [ ] Read relevant documentation

---

## 🔄 Common Error Messages Quick Reference

| Error | Likely Cause | Quick Fix |
|-------|--------------|-----------|
| NETSDK1005 | Missing Windows SDK | Install Windows 10 SDK via VS Installer |
| NU1301 | NuGet source issue | `dotnet nuget locals all --clear` |
| Platform not set | Missing build parameter | Add `--property:Platform=x64` |
| Python not recognized | Python not in PATH | Reinstall Python with "Add to PATH" checked |
| SSL certificate problem | Git SSL issue | Update Git or use SSH |
| Permission denied (git) | Pushing to main repo | Push to your fork instead |
| Test failed | Pre-existing or your change | Check if failure is new |
| IntelliSense broken | Corrupted cache | Delete .vs folder, rebuild |

---

**Still stuck?** Don't hesitate to ask! The community is here to help. 

Remember: There are no stupid questions, and we were all beginners once. 🙂

**Return to:** [Getting Started](getting-started.md) | [Environment Setup](environment-setup.md) | [FAQ](faq.md)
