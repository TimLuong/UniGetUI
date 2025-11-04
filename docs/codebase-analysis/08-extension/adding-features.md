# Adding New Features

This guide provides a comprehensive approach to adding new features to the UniGetUI codebase. UniGetUI is a C# WinUI3 desktop application that provides a unified interface for multiple package managers on Windows.

## General Approach

### Planning Phase

1. **Define Feature Requirements**
   - Clearly document what the feature should accomplish
   - Identify user stories and use cases
   - Define acceptance criteria

2. **Identify Affected Components**
   - Determine which layers need changes (UI, business logic, data access)
   - Identify existing package managers or core components that will be impacted
   - Check for similar existing features to maintain consistency

3. **Plan Data Storage Changes**
   - Determine if new settings need to be persisted (using `UniGetUI.Core.Settings`)
   - Consider if secure storage is needed (using `UniGetUI.Core.SecureSettings`)
   - Plan any data structures needed for feature state management

4. **Design UI Components**
   - Sketch the user interface layout
   - Identify which XAML pages or controls need creation or modification
   - Ensure consistency with existing UI patterns and WinUI3 design guidelines

### Implementation Checklist

- [ ] Create feature branch from main
- [ ] Update data models (if needed)
- [ ] Implement core business logic
- [ ] Create or update UI components (XAML + code-behind)
- [ ] Add localization strings to language files
- [ ] Write unit tests (using xUnit)
- [ ] Update documentation
- [ ] Test manually across scenarios
- [ ] Code review
- [ ] Merge to main

## Architecture Overview

UniGetUI follows a modular architecture with clear separation of concerns:

```
src/
├── UniGetUI/                              # Main WinUI3 application
│   ├── App.xaml / App.xaml.cs            # Application entry point
│   ├── MainWindow.xaml / MainWindow.xaml.cs  # Main window
│   ├── Pages/                             # UI pages
│   ├── Controls/                          # Reusable UI controls
│   └── Services/                          # Application services
├── UniGetUI.Core.*/                       # Core libraries
│   ├── UniGetUI.Core.Data                # Data and constants
│   ├── UniGetUI.Core.Settings            # Settings management
│   ├── UniGetUI.Core.SecureSettings      # Secure storage
│   ├── UniGetUI.Core.Logger              # Logging infrastructure
│   ├── UniGetUI.Core.Tools               # Common utilities
│   └── UniGetUI.Core.LanguageEngine      # Localization
├── UniGetUI.PackageEngine.*/             # Package management
│   ├── PackageManagerClasses/            # Base classes
│   ├── Managers.*/                       # Specific manager implementations
│   ├── Operations/                       # Package operations
│   └── PackageLoader/                    # Package loading logic
└── UniGetUI.Interface.*/                 # Interface-related utilities
```

## Example: Adding a New Package Manager

This is one of the most common feature additions to UniGetUI.

### Feature: Adding Support for a New Package Manager (Example: "Cargo")

#### Files to Create

```
src/
└── UniGetUI.PackageEngine.Managers.Cargo/
    ├── Cargo.cs                          # Main package manager implementation
    ├── CargoSourceHelper.cs              # Source management (optional)
    ├── CargoPkgDetailsHelper.cs          # Package details retrieval (optional)
    ├── CargoPkgOperationHelper.cs        # Package operations (optional)
    └── UniGetUI.PackageEngine.Managers.Cargo.csproj  # Project file
```

#### Files to Modify

**src/UniGetUI.sln**
- Add new project to solution

**src/UniGetUI/UniGetUI.csproj**
- Add project reference to new package manager

**src/UniGetUI.PackageEngine.PackageEngine/PExt.cs** (or equivalent manager registry)
- Register the new package manager instance

### Implementation Steps

#### 1. Create the Project

```bash
# Create new class library project
dotnet new classlib -n UniGetUI.PackageEngine.Managers.Cargo -o src/UniGetUI.PackageEngine.Managers.Cargo
```

#### 2. Package Manager Core Class

```csharp
// src/UniGetUI.PackageEngine.Managers.Cargo/Cargo.cs
using UniGetUI.Core.Data;
using UniGetUI.Core.Logging;
using UniGetUI.Core.Tools;
using UniGetUI.PackageEngine.Classes.Manager;
using UniGetUI.PackageEngine.Classes.Manager.ManagerHelpers;
using UniGetUI.PackageEngine.Enums;
using UniGetUI.PackageEngine.ManagerClasses.Classes;
using UniGetUI.PackageEngine.ManagerClasses.Manager;
using UniGetUI.PackageEngine.PackageClasses;

namespace UniGetUI.PackageEngine.Managers.CargoManager
{
    public class Cargo : PackageManager
    {
        public Cargo()
        {
            // Define capabilities
            Capabilities = new ManagerCapabilities
            {
                CanRunAsAdmin = true,
                CanSkipIntegrityChecks = false,
                CanRunInteractively = false,
                SupportsCustomVersions = true,
                SupportsCustomSources = true,
                SupportsCustomScopes = false,
                // ... other capabilities
            };

            // Define properties
            Properties = new ManagerProperties
            {
                Name = "Cargo",
                DisplayName = "Cargo",
                Description = CoreTools.Translate("Rust's package manager for Rust crates<br>Contains: <b>Rust libraries and tools</b>"),
                IconId = IconType.Cargo,
                ColorIconId = "cargo_color",
                ExecutableFriendlyName = "cargo.exe",
                InstallVerb = "install",
                UninstallVerb = "uninstall",
                UpdateVerb = "update",
                KnownSources = [
                    new ManagerSource(this, "crates.io", new Uri("https://crates.io"))
                ],
                DefaultSource = new ManagerSource(this, "crates.io", new Uri("https://crates.io"))
            };

            // Set up helpers
            SourcesHelper = new CargoSourceHelper(this);
            DetailsHelper = new CargoPkgDetailsHelper(this);
            OperationHelper = new CargoPkgOperationHelper(this);
        }

        protected override void _loadManagerExecutableFile(out bool found, out string path, out string callArguments)
        {
            // Locate the cargo executable
            path = "cargo"; // Or full path detection logic
            callArguments = "";
            found = File.Exists(path) || WhichCommand.IsAvailable("cargo");
        }

        protected override void _loadManagerVersion(out string version)
        {
            // Get version from 'cargo --version'
            version = "Unknown";
            // Implementation to parse version...
        }

        // Additional methods for package listing, searching, etc.
    }
}
```

#### 3. Helper Classes (Optional but Recommended)

```csharp
// src/UniGetUI.PackageEngine.Managers.Cargo/CargoPkgDetailsHelper.cs
public class CargoPkgDetailsHelper : BasePackageDetailsHelper
{
    public CargoPkgDetailsHelper(PackageManager manager) : base(manager) { }

    protected override async Task<PackageDetails> GetPackageDetails_UnSafe(Package package)
    {
        // Fetch and return package details
        var details = new PackageDetails
        {
            Name = package.Name,
            Id = package.Id,
            Description = "...",
            // ... other details
        };
        return details;
    }
}
```

```csharp
// src/UniGetUI.PackageEngine.Managers.Cargo/CargoPkgOperationHelper.cs
public class CargoPkgOperationHelper : BasePackageOperationHelper
{
    public CargoPkgOperationHelper(PackageManager manager) : base(manager) { }

    protected override string[] _getOperationParameters(
        Package package, 
        IInstallationOptions options, 
        OperationType operation)
    {
        // Build command-line arguments for install/uninstall/update
        List<string> parameters = new();
        
        if (operation == OperationType.Install)
        {
            parameters.Add("install");
            parameters.Add(package.Id);
        }
        // ... handle other operations
        
        return parameters.ToArray();
    }
}
```

#### 4. Register the Package Manager

```csharp
// In the appropriate initialization file (e.g., PExt.cs or similar)
public static void InitializePackageManagers()
{
    Managers.Add(new WinGet());
    Managers.Add(new Scoop());
    Managers.Add(new Chocolatey());
    Managers.Add(new Cargo());  // Add new manager
    // ... other managers
}
```

#### 5. Add Icon Resources

**src/UniGetUI/Assets/**
- Add `cargo.png` for light theme
- Add `cargo_dark.png` for dark theme
- Add `cargo_color.png` for color icon

**src/UniGetUI.Interface.Enums/IconType.cs**
```csharp
public enum IconType
{
    // ... existing icons
    Cargo,
    // ...
}
```

#### 6. Tests

```csharp
// src/UniGetUI.PackageEngine.Managers.Cargo.Tests/CargoTests.cs
namespace UniGetUI.PackageEngine.Managers.Cargo.Tests
{
    public class CargoTests
    {
        [Fact]
        public void TestCargoInitialization()
        {
            // Arrange
            var cargo = new Cargo();
            
            // Act
            cargo.Initialize();
            
            // Assert
            Assert.NotNull(cargo.Properties);
            Assert.Equal("Cargo", cargo.Name);
            Assert.Equal("Cargo", cargo.DisplayName);
        }

        [Fact]
        public void TestCargoCapabilities()
        {
            // Arrange
            var cargo = new Cargo();
            
            // Assert
            Assert.True(cargo.Capabilities.CanRunAsAdmin);
            Assert.True(cargo.Capabilities.SupportsCustomVersions);
        }
    }
}
```

## Example: Adding a New UI Feature

### Feature: Adding a New Settings Page

#### Files to Create

```
src/UniGetUI/Pages/SettingsPages/GeneralPages/
├── MyNewFeature.xaml              # UI layout
└── MyNewFeature.xaml.cs           # Code-behind
```

#### Files to Modify

**src/UniGetUI/Pages/SettingsPage.xaml.cs**
- Add navigation item for the new settings page

**src/UniGetUI.Core.Settings/SettingsEnums.cs** (if adding new settings)
- Define new setting keys

**src/UniGetUI.Core.LanguageEngine/lang/en.json** (and other languages)
- Add localization strings

### Implementation Steps

#### 1. Create XAML UI

```xml
<!-- src/UniGetUI/Pages/SettingsPages/GeneralPages/MyNewFeature.xaml -->
<Page
    x:Class="UniGetUI.Pages.SettingsPages.GeneralPages.MyNewFeature"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:controls="using:UniGetUI.Controls"
    mc:Ignorable="d">

    <StackPanel Spacing="4" Padding="20">
        <TextBlock 
            Text="My New Feature Settings" 
            Style="{StaticResource SubtitleTextBlockStyle}"
            Margin="0,0,0,16"/>
        
        <controls:SettingsCard 
            Header="Enable My Feature"
            Description="This enables the new feature functionality">
            <ToggleSwitch 
                x:Name="EnableFeatureToggle"
                Toggled="EnableFeatureToggle_Toggled"/>
        </controls:SettingsCard>
        
        <controls:SettingsCard 
            Header="Feature Option"
            Description="Configure how the feature behaves">
            <ComboBox 
                x:Name="FeatureOptionComboBox"
                MinWidth="200"
                SelectionChanged="FeatureOptionComboBox_SelectionChanged">
                <ComboBoxItem Content="Option 1" Tag="option1"/>
                <ComboBoxItem Content="Option 2" Tag="option2"/>
            </ComboBox>
        </controls:SettingsCard>
    </StackPanel>
</Page>
```

#### 2. Implement Code-Behind

```csharp
// src/UniGetUI/Pages/SettingsPages/GeneralPages/MyNewFeature.xaml.cs
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using UniGetUI.Core.SettingsEngine;
using UniGetUI.Core.Tools;

namespace UniGetUI.Pages.SettingsPages.GeneralPages
{
    public sealed partial class MyNewFeature : Page
    {
        public MyNewFeature()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            // Load saved settings
            EnableFeatureToggle.IsOn = Settings.Get("EnableMyFeature");
            
            string savedOption = Settings.GetValue("MyFeatureOption");
            foreach (ComboBoxItem item in FeatureOptionComboBox.Items)
            {
                if (item.Tag.ToString() == savedOption)
                {
                    FeatureOptionComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void EnableFeatureToggle_Toggled(object sender, RoutedEventArgs e)
        {
            Settings.Set("EnableMyFeature", EnableFeatureToggle.IsOn);
        }

        private void FeatureOptionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FeatureOptionComboBox.SelectedItem is ComboBoxItem item)
            {
                Settings.SetValue("MyFeatureOption", item.Tag.ToString());
            }
        }
    }
}
```

#### 3. Add Settings Keys

```csharp
// In appropriate settings enum or constants file
public static class SettingsKeys
{
    public const string EnableMyFeature = "EnableMyFeature";
    public const string MyFeatureOption = "MyFeatureOption";
}
```

#### 4. Add Localization Strings

```json
// src/UniGetUI.Core.LanguageEngine/lang/en.json
{
    "My New Feature Settings": "My New Feature Settings",
    "Enable My Feature": "Enable My Feature",
    "This enables the new feature functionality": "This enables the new feature functionality",
    "Feature Option": "Feature Option",
    "Configure how the feature behaves": "Configure how the feature behaves"
}
```

#### 5. Register Navigation

```csharp
// In the settings navigation setup (e.g., SettingsPage.xaml.cs)
private void SetupNavigation()
{
    // ... existing navigation items
    
    var myFeatureItem = new NavigationViewItem
    {
        Content = CoreTools.Translate("My New Feature"),
        Icon = new FontIcon { Glyph = "\uE8B7" },
        Tag = "MyNewFeature"
    };
    SettingsNavigationView.MenuItems.Add(myFeatureItem);
}
```

#### 6. Tests

```csharp
// src/UniGetUI.Core.Settings.Tests/MyFeatureSettingsTests.cs
namespace UniGetUI.Core.Settings.Tests
{
    public class MyFeatureSettingsTests
    {
        [Fact]
        public void TestFeatureEnabledByDefault()
        {
            // Arrange & Act
            bool isEnabled = Settings.Get("EnableMyFeature");
            
            // Assert
            Assert.False(isEnabled); // Or True, depending on default
        }

        [Fact]
        public void TestFeatureOptionPersistence()
        {
            // Arrange
            string testValue = "option2";
            
            // Act
            Settings.SetValue("MyFeatureOption", testValue);
            string retrievedValue = Settings.GetValue("MyFeatureOption");
            
            // Assert
            Assert.Equal(testValue, retrievedValue);
        }
    }
}
```

## Common Patterns

### Adding a Reusable UI Control

1. **Create the control**
   - Add XAML file in `src/UniGetUI/Controls/`
   - Implement code-behind
   - Follow WinUI3 custom control patterns

2. **Make it reusable**
   - Use dependency properties for customizable properties
   - Raise events for user interactions
   - Document usage in XML comments

Example:
```csharp
public sealed partial class CustomButton : UserControl
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(CustomButton),
            new PropertyMetadata(string.Empty));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public event EventHandler<RoutedEventArgs> ButtonClicked;

    public CustomButton()
    {
        InitializeComponent();
    }

    private void OnButtonClick(object sender, RoutedEventArgs e)
    {
        ButtonClicked?.Invoke(this, e);
    }
}
```

### Adding a New Service

Services in UniGetUI are typically singleton or static classes that provide functionality across the application.

1. **Create service class** in `src/UniGetUI/Services/`
2. **Define interface** (optional but recommended)
3. **Implement service logic**
4. **Register in dependency injection** (if applicable)

Example:
```csharp
// src/UniGetUI/Services/IMyFeatureService.cs
public interface IMyFeatureService
{
    Task<bool> PerformOperationAsync(string parameter);
    void Initialize();
}

// src/UniGetUI/Services/MyFeatureService.cs
public class MyFeatureService : IMyFeatureService
{
    private static MyFeatureService? _instance;
    public static MyFeatureService Instance => _instance ??= new MyFeatureService();

    private MyFeatureService() { }

    public void Initialize()
    {
        // Initialization logic
    }

    public async Task<bool> PerformOperationAsync(string parameter)
    {
        // Service logic
        await Task.Delay(100); // Example async operation
        return true;
    }
}
```

### Adding Settings

UniGetUI uses a centralized settings system:

1. **Define setting key** as a constant or enum value
2. **Use `Settings.Get(key)` and `Settings.Set(key, value)`** for boolean settings
3. **Use `Settings.GetValue(key)` and `Settings.SetValue(key, value)`** for string settings
4. **Use `SecureSettings`** for sensitive data like passwords or tokens

Example:
```csharp
// Reading settings
bool isFeatureEnabled = Settings.Get("MyFeatureEnabled");
string option = Settings.GetValue("MyFeatureOption");

// Writing settings
Settings.Set("MyFeatureEnabled", true);
Settings.SetValue("MyFeatureOption", "value");

// Secure settings (e.g., API tokens)
SecureSettings.SetValue("MyAPIToken", "secret_token");
string token = SecureSettings.GetValue("MyAPIToken");
```

### Localization

All user-facing strings should be localized:

1. **Add strings to language files** in `src/UniGetUI.Core.LanguageEngine/lang/`
2. **Use `CoreTools.Translate()`** to get localized strings
3. **Support placeholders** with `.Replace("{placeholder}", value)`

Example:
```csharp
// Simple translation
string text = CoreTools.Translate("Hello, World!");

// With placeholder
string message = CoreTools.Translate("Found {0} packages")
    .Replace("{0}", packageCount.ToString());
```

### Logging

Use the built-in logging system:

```csharp
using UniGetUI.Core.Logging;

// Different log levels
Logger.Debug("Debug information");
Logger.Info("Informational message");
Logger.Warn("Warning message");
Logger.Error("Error occurred");
Logger.ImportantInfo("Important information");

// Log exceptions
try
{
    // Code that might throw
}
catch (Exception ex)
{
    Logger.Error($"Operation failed: {ex.Message}");
    Logger.Error(ex); // Log full exception details
}
```

## Testing New Features

### Unit Testing Guidelines

UniGetUI uses xUnit for unit testing. Tests should:

1. **Follow the Arrange-Act-Assert pattern**
2. **Test one thing per test method**
3. **Use descriptive test names** (e.g., `TestFeatureEnabledByDefault`)
4. **Mock external dependencies** when appropriate

### Test Project Structure

Each core library has a corresponding test project:
- `UniGetUI.Core.Classes` → `UniGetUI.Core.Classes.Tests`
- `UniGetUI.Core.Settings` → `UniGetUI.Core.Settings.Tests`
- etc.

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests for a specific project
dotnet test src/UniGetUI.Core.Classes.Tests

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"
```

### Manual Testing

1. **Build the solution**
   ```bash
   dotnet build
   ```

2. **Run the application**
   - Launch from Visual Studio (F5)
   - Or run the built executable

3. **Test scenarios**
   - Test happy path
   - Test edge cases
   - Test error handling
   - Test with different package managers enabled/disabled
   - Test with different settings configurations

4. **Cross-platform considerations**
   - UniGetUI is Windows-only (WinUI3)
   - Test on both Windows 10 and Windows 11
   - Test with different display scales (100%, 125%, 150%, etc.)

## Documentation Requirements

When adding a new feature, update the following documentation:

### Code Documentation

1. **XML comments on public APIs**
   ```csharp
   /// <summary>
   /// Performs the specified operation on the package.
   /// </summary>
   /// <param name="package">The package to operate on.</param>
   /// <param name="operation">The operation to perform.</param>
   /// <returns>True if successful, false otherwise.</returns>
   public async Task<bool> PerformOperation(Package package, OperationType operation)
   {
       // Implementation
   }
   ```

2. **README files** for new projects or modules

### User Documentation

1. **Update repository README** if the feature is user-facing
2. **Add to CLI arguments documentation** if adding command-line options
3. **Update configuration.md** if adding configuration options

### Developer Documentation

1. **Update codebase analysis docs** if architectural changes are made
2. **Add feature documentation** in `docs/` for significant features
3. **Document any new patterns or conventions** introduced

## Best Practices

### Code Quality

1. **Follow existing code style** and conventions
   - Use camelCase for variables and methods
   - Use PascalCase for classes and public properties
   - Use CAPITAL_CASE for constants

2. **Keep methods focused** and single-purpose
3. **Avoid code duplication** - extract common logic
4. **Handle errors gracefully** - don't let exceptions crash the app
5. **Dispose resources properly** - use `using` statements

### Performance

1. **Async/await for I/O operations** (file access, network calls)
2. **Avoid blocking the UI thread** - use `Task.Run()` for CPU-intensive work
3. **Cache expensive operations** when appropriate
4. **Use efficient data structures**

### Security

1. **Validate all user input**
2. **Use `SecureSettings` for sensitive data**
3. **Don't log sensitive information**
4. **Be careful with process execution** - validate and sanitize arguments
5. **Follow principle of least privilege**

### Maintainability

1. **Write self-documenting code** with clear names
2. **Add comments for complex logic**, not obvious code
3. **Keep classes and methods small**
4. **Use dependency injection** for testability
5. **Avoid tight coupling** between components

## Getting Help

- **GitHub Discussions**: For questions about implementation
- **GitHub Issues**: For bug reports and feature requests
- **Code Review**: Request reviews from maintainers on PRs
- **CONTRIBUTING.md**: General contribution guidelines

## Example Checklist for a New Feature

Let's say you're adding "Export to CSV" functionality:

- [x] Create feature branch: `feature/export-to-csv`
- [ ] Plan data format and structure
- [ ] Implement export service:
  - [ ] Create `IExportService` interface
  - [ ] Implement `CsvExportService`
  - [ ] Add error handling for file operations
- [ ] Add UI:
  - [ ] Add "Export" button to package list
  - [ ] Add file picker dialog
  - [ ] Show progress indicator during export
  - [ ] Show success/error notification
- [ ] Localization:
  - [ ] Add strings to `en.json`
  - [ ] Mark for translation in other languages
- [ ] Tests:
  - [ ] Unit tests for CSV generation logic
  - [ ] Test with various package lists (empty, single, many)
  - [ ] Test error cases (invalid path, permission denied)
- [ ] Documentation:
  - [ ] Add XML comments to public methods
  - [ ] Update user guide with export feature
  - [ ] Document CSV format specification
- [ ] Manual testing:
  - [ ] Test with different package lists
  - [ ] Test with different file locations
  - [ ] Test cancellation
  - [ ] Test with special characters in package names
- [ ] Code review and merge

## Conclusion

This guide provides a structured approach to adding features to UniGetUI. The key principles are:

1. **Understand the architecture** before making changes
2. **Follow existing patterns** and conventions
3. **Test thoroughly** at multiple levels
4. **Document your changes** for users and developers
5. **Ask for help** when needed

Remember that UniGetUI is a community-driven project. Your contributions help make package management on Windows more accessible and user-friendly for everyone.
