# Interactive Learning Exercises

This document contains hands-on exercises to help you learn UniGetUI development through practice.

## 🎯 How to Use This Guide

Each exercise:
- Has a clear objective
- Includes starter code or files to modify
- Provides hints when you're stuck
- Has a solution you can reference
- Builds on previous exercises

**Recommendation:** Try to solve exercises without looking at solutions first. Learning happens through struggle and discovery!

---

## 🎓 Beginner Exercises

### Exercise 1: Fix the Naming Violations

**Objective:** Practice proper C# naming conventions

**Starter Code:**
```csharp
public class package_manager
{
    private string PACKAGE_PATH = "C:\\packages";
    private ILogger logger;
    public const int max_retries = 3;
    
    public bool install_package(string package_name, bool bRunAsAdmin)
    {
        int retry_count = 0;
        string CMD = "install";
        
        // Implementation...
        return true;
    }
    
    public List<string> get_installed_packages()
    {
        // Implementation...
        return new List<string>();
    }
}
```

**Tasks:**
1. Fix all naming convention violations
2. Follow UniGetUI standards (see [Coding Standards](../../codebase-analysis/07-best-practices/patterns-standards.md))

<details>
<summary>💡 Hints</summary>

- Classes: PascalCase
- Private fields: _camelCase
- Methods: PascalCase
- Parameters: camelCase
- Constants: PascalCase (not SCREAMING_CASE in C#)
- Remove Hungarian notation (b prefix)
</details>

<details>
<summary>✅ Solution</summary>

```csharp
public class PackageManager
{
    private string _packagePath = "C:\\packages";
    private readonly ILogger _logger;
    private const int MaxRetries = 3;
    
    public bool InstallPackage(string packageName, bool runAsAdmin)
    {
        int retryCount = 0;
        string command = "install";
        
        // Implementation...
        return true;
    }
    
    public List<string> GetInstalledPackages()
    {
        // Implementation...
        return new List<string>();
    }
}
```
</details>

---

### Exercise 2: Add a Simple Setting

**Objective:** Learn to add a setting to the UI

**Task:** Add a toggle setting called "Show Notification on Update" to a settings page.

**Steps:**
1. Find an appropriate settings page XAML file
2. Add a `SettingsCard` with a `ToggleSwitch`
3. Implement the code-behind to save/load the setting
4. Build and test your changes

**Required Components:**
```xml
<!-- XAML -->
<controls:SettingsCard 
    Header="Your Header"
    Description="Your Description">
    <ToggleSwitch x:Name="YourToggleName" Toggled="YourToggle_Toggled" />
</controls:SettingsCard>
```

```csharp
// Code-behind
private void YourToggle_Toggled(object sender, RoutedEventArgs e)
{
    Settings.Set("SettingKey", YourToggleName.IsOn);
}
```

<details>
<summary>💡 Hints</summary>

- Use a descriptive setting key like "ShowNotificationOnUpdate"
- Don't forget to load the setting in the page constructor
- Test by closing and reopening the app
</details>

<details>
<summary>✅ Solution</summary>

**XAML:**
```xml
<controls:SettingsCard 
    Header="Show Notification on Update"
    Description="Display a notification when packages are updated">
    <ToggleSwitch 
        x:Name="ShowUpdateNotificationToggle" 
        Toggled="ShowUpdateNotificationToggle_Toggled" />
</controls:SettingsCard>
```

**Code-behind:**
```csharp
public SettingsPage()
{
    InitializeComponent();
    LoadSettings();
}

private void LoadSettings()
{
    ShowUpdateNotificationToggle.IsOn = Settings.Get("ShowNotificationOnUpdate");
}

private void ShowUpdateNotificationToggle_Toggled(object sender, RoutedEventArgs e)
{
    Settings.Set("ShowNotificationOnUpdate", ShowUpdateNotificationToggle.IsOn);
}
```
</details>

---

### Exercise 3: Write Unit Tests

**Objective:** Practice writing unit tests with xUnit

**Code to Test:**
```csharp
public class PackageValidator
{
    public bool IsValidPackageName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;
        
        if (name.Length < 2)
            return false;
        
        if (name.Length > 100)
            return false;
        
        return true;
    }
}
```

**Tasks:**
Write tests for:
1. Null input
2. Empty string
3. Whitespace only
4. Too short (1 character)
5. Too long (101 characters)
6. Valid name

<details>
<summary>💡 Hints</summary>

- Use `[Fact]` attribute for each test
- Follow AAA pattern: Arrange, Act, Assert
- Use descriptive test names: `TestMethodName_Scenario_ExpectedResult`
</details>

<details>
<summary>✅ Solution</summary>

```csharp
public class PackageValidatorTests
{
    [Fact]
    public void TestIsValidPackageName_NullInput_ReturnsFalse()
    {
        // Arrange
        var validator = new PackageValidator();
        
        // Act
        bool result = validator.IsValidPackageName(null);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void TestIsValidPackageName_EmptyString_ReturnsFalse()
    {
        var validator = new PackageValidator();
        bool result = validator.IsValidPackageName("");
        Assert.False(result);
    }
    
    [Fact]
    public void TestIsValidPackageName_WhitespaceOnly_ReturnsFalse()
    {
        var validator = new PackageValidator();
        bool result = validator.IsValidPackageName("   ");
        Assert.False(result);
    }
    
    [Fact]
    public void TestIsValidPackageName_TooShort_ReturnsFalse()
    {
        var validator = new PackageValidator();
        bool result = validator.IsValidPackageName("A");
        Assert.False(result);
    }
    
    [Fact]
    public void TestIsValidPackageName_TooLong_ReturnsFalse()
    {
        var validator = new PackageValidator();
        string longName = new string('A', 101);
        bool result = validator.IsValidPackageName(longName);
        Assert.False(result);
    }
    
    [Fact]
    public void TestIsValidPackageName_ValidName_ReturnsTrue()
    {
        var validator = new PackageValidator();
        bool result = validator.IsValidPackageName("ValidPackage");
        Assert.True(result);
    }
}
```
</details>

---

## 🎯 Intermediate Exercises

### Exercise 4: Refactor Using the Helper Pattern

**Objective:** Apply the Helper pattern to separate concerns

**Starter Code:**
```csharp
public class PackageManager
{
    public async Task<List<Package>> GetInstalledPackages()
    {
        // 50 lines of CLI execution and parsing...
    }
    
    public async Task<PackageDetails> GetPackageDetails(string id)
    {
        // 80 lines of CLI execution and parsing...
    }
    
    public async Task<bool> InstallPackage(Package package, InstallOptions options)
    {
        // 100 lines of argument building and CLI execution...
    }
    
    public async Task<bool> UninstallPackage(Package package)
    {
        // 60 lines of argument building and CLI execution...
    }
    
    public async Task<List<Source>> GetSources()
    {
        // 40 lines of CLI execution and parsing...
    }
}
```

**Task:** Refactor this into separate helper classes:
- `PackageListHelper` - for getting packages
- `PackageDetailsHelper` - for getting details
- `PackageOperationHelper` - for install/uninstall
- `SourceHelper` - for managing sources

<details>
<summary>💡 Hints</summary>

- Each helper should have a reference to the parent PackageManager
- Helpers should inherit from base classes if available
- Keep the PackageManager class as a coordinator
</details>

<details>
<summary>✅ Solution</summary>

```csharp
public class PackageManager
{
    public IPackageListHelper ListHelper { get; }
    public IPackageDetailsHelper DetailsHelper { get; }
    public IPackageOperationHelper OperationHelper { get; }
    public ISourceHelper SourceHelper { get; }
    
    public PackageManager()
    {
        ListHelper = new PackageListHelper(this);
        DetailsHelper = new PackageDetailsHelper(this);
        OperationHelper = new PackageOperationHelper(this);
        SourceHelper = new SourceHelper(this);
    }
    
    // Delegate methods
    public async Task<List<Package>> GetInstalledPackages()
        => await ListHelper.GetInstalledPackages();
    
    public async Task<PackageDetails> GetPackageDetails(string id)
        => await DetailsHelper.GetPackageDetails(id);
    
    public async Task<bool> InstallPackage(Package package, InstallOptions options)
        => await OperationHelper.InstallPackage(package, options);
    
    public async Task<bool> UninstallPackage(Package package)
        => await OperationHelper.UninstallPackage(package);
    
    public async Task<List<Source>> GetSources()
        => await SourceHelper.GetSources();
}

public class PackageListHelper
{
    private readonly PackageManager _manager;
    
    public PackageListHelper(PackageManager manager)
    {
        _manager = manager;
    }
    
    public async Task<List<Package>> GetInstalledPackages()
    {
        // Implementation moved here...
    }
}

// Similar for other helpers...
```
</details>

---

### Exercise 5: Implement a Simple Custom Control

**Objective:** Create a reusable WinUI 3 custom control

**Task:** Create a `PackageStatusBadge` control that displays:
- Package name
- Current version
- Status (Installed, Update Available, etc.)

**Requirements:**
- Accept properties via dependency properties
- Use appropriate styling
- Raise an event when clicked

<details>
<summary>💡 Hints</summary>

- Inherit from `UserControl`
- Use `DependencyProperty.Register` for bindable properties
- Design the XAML to be visually appealing
- Use `EventHandler` for custom events
</details>

<details>
<summary>✅ Solution</summary>

**PackageStatusBadge.xaml:**
```xml
<UserControl
    x:Class="UniGetUI.Controls.PackageStatusBadge"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <Border 
        BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
        BorderThickness="1"
        CornerRadius="4"
        Padding="8"
        Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
        PointerPressed="Border_PointerPressed">
        
        <StackPanel Spacing="4">
            <TextBlock 
                Text="{x:Bind PackageName, Mode=OneWay}"
                FontWeight="SemiBold" />
            
            <TextBlock 
                Text="{x:Bind Version, Mode=OneWay}"
                Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                FontSize="12" />
            
            <Border 
                Background="{x:Bind StatusColor, Mode=OneWay}"
                CornerRadius="2"
                Padding="4,2"
                HorizontalAlignment="Left">
                <TextBlock 
                    Text="{x:Bind Status, Mode=OneWay}"
                    FontSize="10"
                    Foreground="White" />
            </Border>
        </StackPanel>
    </Border>
</UserControl>
```

**PackageStatusBadge.xaml.cs:**
```csharp
public sealed partial class PackageStatusBadge : UserControl
{
    public static readonly DependencyProperty PackageNameProperty =
        DependencyProperty.Register(
            nameof(PackageName),
            typeof(string),
            typeof(PackageStatusBadge),
            new PropertyMetadata(string.Empty));
    
    public string PackageName
    {
        get => (string)GetValue(PackageNameProperty);
        set => SetValue(PackageNameProperty, value);
    }
    
    public static readonly DependencyProperty VersionProperty =
        DependencyProperty.Register(
            nameof(Version),
            typeof(string),
            typeof(PackageStatusBadge),
            new PropertyMetadata(string.Empty));
    
    public string Version
    {
        get => (string)GetValue(VersionProperty);
        set => SetValue(VersionProperty, value);
    }
    
    public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register(
            nameof(Status),
            typeof(string),
            typeof(PackageStatusBadge),
            new PropertyMetadata("Unknown"));
    
    public string Status
    {
        get => (string)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }
    
    public static readonly DependencyProperty StatusColorProperty =
        DependencyProperty.Register(
            nameof(StatusColor),
            typeof(Brush),
            typeof(PackageStatusBadge),
            new PropertyMetadata(new SolidColorBrush(Colors.Gray)));
    
    public Brush StatusColor
    {
        get => (Brush)GetValue(StatusColorProperty);
        set => SetValue(StatusColorProperty, value);
    }
    
    public event EventHandler<RoutedEventArgs> BadgeClicked;
    
    public PackageStatusBadge()
    {
        InitializeComponent();
    }
    
    private void Border_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        BadgeClicked?.Invoke(this, new RoutedEventArgs());
    }
}
```

**Usage:**
```xml
<local:PackageStatusBadge 
    PackageName="Visual Studio Code"
    Version="1.85.0"
    Status="Update Available"
    StatusColor="Orange"
    BadgeClicked="Badge_Clicked" />
```
</details>

---

### Exercise 6: Debug a Failing Test

**Objective:** Practice debugging skills

**Scenario:** This test is failing. Find and fix the bug.

```csharp
public class VersionComparer
{
    public bool IsNewerVersion(string currentVersion, string newVersion)
    {
        var current = Version.Parse(currentVersion);
        var newer = Version.Parse(newVersion);
        return newer > current;
    }
}

[Fact]
public void TestIsNewerVersion_NewerVersion_ReturnsTrue()
{
    var comparer = new VersionComparer();
    bool result = comparer.IsNewerVersion("1.0.0", "2.0.0");
    Assert.True(result); // This passes
}

[Fact]
public void TestIsNewerVersion_InvalidVersion_HandlesGracefully()
{
    var comparer = new VersionComparer();
    bool result = comparer.IsNewerVersion("invalid", "1.0.0");
    Assert.False(result); // This FAILS with exception!
}
```

**Task:** Fix the code to handle invalid versions gracefully.

<details>
<summary>💡 Hints</summary>

- What happens when `Version.Parse()` receives invalid input?
- How should the method behave with invalid input?
- Add try-catch? Return false? Throw a custom exception?
</details>

<details>
<summary>✅ Solution</summary>

```csharp
public class VersionComparer
{
    public bool IsNewerVersion(string currentVersion, string newVersion)
    {
        try
        {
            var current = Version.Parse(currentVersion);
            var newer = Version.Parse(newVersion);
            return newer > current;
        }
        catch (ArgumentException)
        {
            // Invalid version format
            return false;
        }
        catch (FormatException)
        {
            // Invalid version format
            return false;
        }
    }
}

// Add more comprehensive tests
[Fact]
public void TestIsNewerVersion_BothInvalid_ReturnsFalse()
{
    var comparer = new VersionComparer();
    bool result = comparer.IsNewerVersion("invalid", "also-invalid");
    Assert.False(result);
}

[Fact]
public void TestIsNewerVersion_EmptyStrings_ReturnsFalse()
{
    var comparer = new VersionComparer();
    bool result = comparer.IsNewerVersion("", "");
    Assert.False(result);
}
```
</details>

---

## 🚀 Advanced Exercises

### Exercise 7: Implement a Package Manager

**Objective:** Apply all learned concepts to create a basic package manager implementation

**Task:** Implement a simple package manager for a fictional CLI tool called "pkg".

**Requirements:**
1. Implement `IPackageManager` interface
2. Create at least one helper class
3. Parse CLI output to extract packages
4. Handle errors gracefully
5. Write unit tests

**CLI Commands:**
- `pkg list` - Lists installed packages
- `pkg search <query>` - Searches for packages
- `pkg install <name>` - Installs a package
- `pkg uninstall <name>` - Uninstalls a package

**Example Output:**
```
pkg list
Package1 1.0.0
Package2 2.3.1
Package3 0.9.5
```

<details>
<summary>💡 Hints</summary>

- Start with the main class inheriting from PackageManager
- Define capabilities and properties
- Create an OperationHelper to build command arguments
- Use `Process.Start()` to execute commands
- Parse stdout line by line
- Return empty lists on errors (fail-safe)
</details>

<details>
<summary>✅ Solution Outline</summary>

Due to complexity, here's a structure outline:

```csharp
public class PkgManager : PackageManager
{
    public PkgManager()
    {
        Capabilities = new ManagerCapabilities
        {
            CanRunAsAdmin = false,
            SupportsCustomVersions = false,
            // ...
        };
        
        Properties = new ManagerProperties
        {
            Name = "Pkg",
            DisplayName = "Pkg",
            // ...
        };
        
        OperationHelper = new PkgOperationHelper(this);
    }
    
    public override async Task<List<Package>> GetInstalledPackages()
    {
        try
        {
            string output = await ExecuteCommand("pkg", "list");
            return ParsePackageList(output);
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to get packages: {ex.Message}");
            return new List<Package>();
        }
    }
    
    private List<Package> ParsePackageList(string output)
    {
        var packages = new List<Package>();
        foreach (var line in output.Split('\n'))
        {
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                packages.Add(new Package
                {
                    Name = parts[0],
                    Version = parts[1],
                    Manager = this
                });
            }
        }
        return packages;
    }
    
    // More methods...
}
```

Complete implementation would include:
- Error handling
- Progress reporting
- Source management
- Package details fetching
- Comprehensive tests
</details>

---

### Exercise 8: Code Review Challenge

**Objective:** Practice code review skills

**Task:** Review this PR and provide constructive feedback.

```csharp
// PR: "Add feature to download packages"

public class PackageDownloader
{
    public void Download(string p)
    {
        var url = "http://packages.com/" + p;
        var wc = new WebClient();
        wc.DownloadFile(url, "C:\\Downloads\\" + p + ".zip");
        Console.WriteLine("Downloaded!");
    }
}
```

**Questions to consider:**
1. What naming issues exist?
2. Are there security concerns?
3. Is error handling adequate?
4. Is it testable?
5. Does it follow async best practices?
6. What improvements would you suggest?

<details>
<summary>✅ Sample Review</summary>

**Feedback:**

1. **Naming:** Parameter `p` should be `packageName` for clarity.

2. **Security Concerns:**
   - Hardcoded HTTP (not HTTPS) - security risk
   - No URL validation - potential path traversal
   - Hardcoded download path - won't work for all users
   
3. **Error Handling:**
   - No try-catch for network failures
   - No check if file already exists
   - No cleanup on failure
   
4. **Async:**
   - Should use async/await for I/O operations
   - Blocks the UI thread currently
   
5. **Logging:**
   - Uses Console.WriteLine instead of proper logging
   - No progress reporting
   
6. **Testing:**
   - Hard to test due to static dependencies
   - Consider dependency injection
   
**Suggested Improvements:**
```csharp
public class PackageDownloader
{
    private readonly ILogger _logger;
    private readonly string _downloadPath;
    
    public PackageDownloader(ILogger logger, string downloadPath)
    {
        _logger = logger;
        _downloadPath = downloadPath;
    }
    
    public async Task<bool> DownloadAsync(string packageName, string baseUrl)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(packageName))
                throw new ArgumentException("Package name cannot be empty", nameof(packageName));
            
            if (!baseUrl.StartsWith("https://"))
                throw new ArgumentException("Only HTTPS URLs are allowed", nameof(baseUrl));
            
            string safePackageName = Path.GetFileName(packageName);
            string url = $"{baseUrl.TrimEnd('/')}/{safePackageName}";
            string filePath = Path.Combine(_downloadPath, $"{safePackageName}.zip");
            
            using var client = new HttpClient();
            using var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            await using var fileStream = File.Create(filePath);
            await response.Content.CopyToAsync(fileStream);
            
            _logger.Info($"Successfully downloaded {packageName}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to download {packageName}: {ex.Message}");
            return false;
        }
    }
}
```
</details>

---

## 🎯 Challenge Projects

### Challenge 1: Add Dark Mode Toggle
Create a fully functional dark mode toggle that:
- Saves preference
- Switches theme immediately
- Persists across restarts
- Updates all pages

### Challenge 2: Package Search Enhancement
Add a debounced search feature that:
- Waits for user to stop typing
- Shows loading indicator
- Cancels previous searches
- Displays results efficiently

### Challenge 3: Export Package List
Implement feature to export installed packages to:
- CSV format
- JSON format
- Markdown table
- Include metadata (version, source, etc.)

---

## 📚 Learning Resources

After completing these exercises, explore:
- [Adding Features Guide](../../codebase-analysis/08-extension/adding-features.md)
- [Design Patterns](../../codebase-analysis/07-best-practices/patterns-standards.md)
- [Architecture Overview](../../codebase-analysis/01-overview/architecture.md)

---

## ✅ Progress Tracker

Track your progress:

**Beginner:**
- [ ] Exercise 1: Fix Naming Violations
- [ ] Exercise 2: Add a Simple Setting
- [ ] Exercise 3: Write Unit Tests

**Intermediate:**
- [ ] Exercise 4: Refactor Using Helper Pattern
- [ ] Exercise 5: Implement Custom Control
- [ ] Exercise 6: Debug Failing Test

**Advanced:**
- [ ] Exercise 7: Implement Package Manager
- [ ] Exercise 8: Code Review Challenge

**Challenges:**
- [ ] Challenge 1: Dark Mode Toggle
- [ ] Challenge 2: Package Search Enhancement
- [ ] Challenge 3: Export Package List

---

**Keep learning and practicing!** 🚀

The best way to learn is by doing. Don't be afraid to make mistakes - that's how we all learn!

**Questions?** Ask in [GitHub Discussions](https://github.com/marticliment/UniGetUI/discussions)
