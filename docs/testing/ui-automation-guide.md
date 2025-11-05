# UI Automation Testing Guide

## Overview

UI automation testing validates the user interface and user experience of Windows applications. This guide covers UI testing strategies for WinUI 3 desktop applications using modern automation frameworks.

UI tests are at the top of the testing pyramid and should be used strategically to test critical user workflows and interactions that cannot be adequately covered by unit or integration tests.

## UI Testing Pyramid

```
        /\
       /  \  UI Tests (Critical paths only)
      /____\
     /      \
    /        \ Integration Tests
   /__________\
  /            \
 /              \ Unit Tests
/________________\
```

### When to Write UI Tests

✅ **DO** write UI tests for:
- Critical user workflows (installation, updates, uninstallation)
- Complex UI interactions (drag-and-drop, multi-selection)
- Navigation flows
- UI state management
- Accessibility requirements
- Visual regression testing

❌ **DON'T** write UI tests for:
- Business logic (use unit tests)
- Data validation (use unit/integration tests)
- API interactions (use integration tests)
- Simple CRUD operations (covered by integration tests)

## UI Automation Frameworks

### WinAppDriver (Recommended for WinUI 3)

WinAppDriver is Microsoft's UI automation framework for Windows applications, providing WebDriver protocol support for Windows apps.

#### Installation

1. **Download WinAppDriver**
   ```powershell
   # Install via Chocolatey
   choco install winappdriver
   
   # Or download from GitHub
   # https://github.com/microsoft/WinAppDriver/releases
   ```

2. **Install Appium WebDriver Client**
   ```xml
   <PackageReference Include="Appium.WebDriver" Version="5.0.0" />
   <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
   <PackageReference Include="xunit" Version="2.9.3" />
   ```

#### Basic Setup

```csharp
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace UniGetUI.UITests;

public class BasicUITests : IDisposable
{
    private WindowsDriver _driver;
    private const string AppPath = @"C:\Program Files\UniGetUI\UniGetUI.exe";
    private const string WinAppDriverUrl = "http://127.0.0.1:4723";

    public BasicUITests()
    {
        // Ensure WinAppDriver is running
        StartWinAppDriver();
        
        // Configure app capabilities
        var appCapabilities = new AppiumOptions();
        appCapabilities.AddAdditionalCapability("app", AppPath);
        appCapabilities.AddAdditionalCapability("deviceName", "WindowsPC");
        appCapabilities.AddAdditionalCapability("platformName", "Windows");
        
        // Launch app
        _driver = new WindowsDriver(new Uri(WinAppDriverUrl), appCapabilities);
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
    }

    [Fact]
    public void AppLaunches_Successfully()
    {
        // Assert app window is present
        Assert.NotNull(_driver.Title);
        Assert.Contains("UniGetUI", _driver.Title);
    }

    public void Dispose()
    {
        _driver?.Quit();
    }

    private void StartWinAppDriver()
    {
        // Check if WinAppDriver is already running
        var processes = Process.GetProcessesByName("WinAppDriver");
        if (processes.Length == 0)
        {
            Process.Start(@"C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe");
            Thread.Sleep(2000); // Wait for WinAppDriver to start
        }
    }
}
```

### Appium Desktop

Appium provides cross-platform UI automation capabilities with inspector tools for element identification.

#### Installation

```bash
# Install Appium
npm install -g appium

# Install Windows Driver
appium driver install windows

# Start Appium
appium
```

#### Appium Configuration

```csharp
public class AppiumUITests
{
    private WindowsDriver _driver;

    [SetUp]
    public void Setup()
    {
        var options = new AppiumOptions();
        options.AddAdditionalCapability("app", "Root");
        options.AddAdditionalCapability("deviceName", "WindowsPC");
        options.AddAdditionalCapability("platformName", "Windows");
        options.AddAdditionalCapability("ms:waitForAppLaunch", 25);
        
        _driver = new WindowsDriver(
            new Uri("http://127.0.0.1:4723"),
            options,
            TimeSpan.FromMinutes(2)
        );
    }

    [Fact]
    public void CanFindMainWindow()
    {
        var mainWindow = _driver.FindElement(
            By.Name("UniGetUI")
        );
        
        Assert.NotNull(mainWindow);
    }
}
```

## Element Identification Strategies

### Finding Elements

WinAppDriver supports multiple strategies for locating UI elements:

#### By AccessibilityId (Recommended)

```csharp
// Set in XAML
<Button x:Name="InstallButton" AutomationProperties.AutomationId="InstallButton"/>

// Find in test
var installButton = _driver.FindElement(By.Id("InstallButton"));
installButton.Click();
```

#### By Name

```csharp
// Find by element name
var searchBox = _driver.FindElement(By.Name("Search packages"));
searchBox.SendKeys("Visual Studio Code");
```

#### By ClassName

```csharp
// Find by WinUI control class
var buttons = _driver.FindElements(By.ClassName("Button"));
Assert.NotEmpty(buttons);
```

#### By XPath

```csharp
// Find using XPath
var packageList = _driver.FindElement(
    By.XPath("//List[@AutomationId='PackageListView']")
);
```

#### By Tag Name

```csharp
// Find by control type
var textBoxes = _driver.FindElements(By.TagName("Edit"));
```

### Element Inspection

Use **Accessibility Insights** or **Inspect.exe** to identify element properties:

```powershell
# Launch Inspect.exe (included with Windows SDK)
"C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\inspect.exe"
```

## Writing UI Tests

### Page Object Model (Recommended)

Organize UI tests using the Page Object Model pattern:

```csharp
// Page Objects/MainWindow.cs
public class MainWindowPage
{
    private readonly WindowsDriver _driver;

    public MainWindowPage(WindowsDriver driver)
    {
        _driver = driver;
    }

    // Elements
    private WindowsElement SearchBox => 
        _driver.FindElement(By.Id("SearchBox"));
    
    private WindowsElement PackageList => 
        _driver.FindElement(By.Id("PackageListView"));
    
    private WindowsElement InstallButton => 
        _driver.FindElement(By.Id("InstallButton"));

    // Actions
    public void SearchPackage(string packageName)
    {
        SearchBox.Clear();
        SearchBox.SendKeys(packageName);
    }

    public void SelectFirstPackage()
    {
        var packages = PackageList.FindElements(By.ClassName("ListViewItem"));
        packages.First().Click();
    }

    public void ClickInstall()
    {
        InstallButton.Click();
    }

    // Verifications
    public bool IsPackageDisplayed(string packageName)
    {
        var items = PackageList.FindElements(By.ClassName("ListViewItem"));
        return items.Any(item => item.Text.Contains(packageName));
    }

    public bool IsInstallButtonEnabled()
    {
        return InstallButton.Enabled;
    }
}

// Tests/MainWindowTests.cs
public class MainWindowTests : IDisposable
{
    private WindowsDriver _driver;
    private MainWindowPage _mainWindow;

    public MainWindowTests()
    {
        InitializeDriver();
        _mainWindow = new MainWindowPage(_driver);
    }

    [Fact]
    public void SearchPackage_DisplaysResults()
    {
        // Arrange & Act
        _mainWindow.SearchPackage("Visual Studio Code");
        
        // Wait for search results
        Thread.Sleep(2000); // Better: use explicit wait
        
        // Assert
        Assert.True(_mainWindow.IsPackageDisplayed("Visual Studio Code"));
    }

    [Fact]
    public void SelectPackage_EnablesInstallButton()
    {
        // Arrange
        _mainWindow.SearchPackage("Git");
        Thread.Sleep(1000);
        
        // Act
        _mainWindow.SelectFirstPackage();
        
        // Assert
        Assert.True(_mainWindow.IsInstallButtonEnabled());
    }

    public void Dispose()
    {
        _driver?.Quit();
    }
}
```

### Explicit Waits

Use explicit waits instead of Thread.Sleep:

```csharp
using OpenQA.Selenium.Support.UI;

public class WaitHelpers
{
    private readonly WindowsDriver _driver;
    private readonly WebDriverWait _wait;

    public WaitHelpers(WindowsDriver driver, TimeSpan timeout)
    {
        _driver = driver;
        _wait = new WebDriverWait(_driver, timeout);
    }

    public WindowsElement WaitForElement(By locator)
    {
        return _wait.Until(driver => 
        {
            try
            {
                var element = driver.FindElement(locator);
                return element.Displayed ? element : null;
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        });
    }

    public bool WaitForElementToBeClickable(By locator)
    {
        return _wait.Until(driver =>
        {
            try
            {
                var element = driver.FindElement(locator);
                return element.Enabled && element.Displayed;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        });
    }

    public bool WaitForTextToAppear(By locator, string expectedText)
    {
        return _wait.Until(driver =>
        {
            try
            {
                var element = driver.FindElement(locator);
                return element.Text.Contains(expectedText);
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        });
    }
}

// Usage
[Fact]
public void TestWithExplicitWait()
{
    var waitHelper = new WaitHelpers(_driver, TimeSpan.FromSeconds(10));
    
    // Wait for search box to be available
    var searchBox = waitHelper.WaitForElement(By.Id("SearchBox"));
    searchBox.SendKeys("test");
    
    // Wait for results to load
    Assert.True(waitHelper.WaitForTextToAppear(
        By.Id("ResultsCount"), 
        "packages found"
    ));
}
```

### Handling Dialogs

```csharp
[Fact]
public void InstallPackage_ShowsConfirmationDialog()
{
    // Arrange
    var mainWindow = new MainWindowPage(_driver);
    mainWindow.SearchPackage("VSCode");
    mainWindow.SelectFirstPackage();
    
    // Act
    mainWindow.ClickInstall();
    
    // Handle dialog
    var dialog = _driver.FindElement(By.Id("ConfirmationDialog"));
    Assert.NotNull(dialog);
    
    var confirmButton = dialog.FindElement(By.Name("Yes"));
    confirmButton.Click();
    
    // Verify installation started
    var statusLabel = _driver.FindElement(By.Id("StatusLabel"));
    Assert.Contains("Installing", statusLabel.Text);
}
```

### Testing Navigation

```csharp
[Fact]
public void NavigateToSettings_DisplaysSettingsPage()
{
    // Arrange
    var mainWindow = new MainWindowPage(_driver);
    
    // Act
    mainWindow.ClickSettingsButton();
    
    // Wait for navigation
    var waitHelper = new WaitHelpers(_driver, TimeSpan.FromSeconds(5));
    var settingsPage = waitHelper.WaitForElement(By.Id("SettingsPage"));
    
    // Assert
    Assert.NotNull(settingsPage);
    Assert.True(settingsPage.Displayed);
}
```

## Testing Common UI Patterns

### ListView Testing

```csharp
[Fact]
public void PackageList_DisplaysMultiplePackages()
{
    // Arrange
    var listView = _driver.FindElement(By.Id("PackageListView"));
    
    // Act
    var items = listView.FindElements(By.ClassName("ListViewItem"));
    
    // Assert
    Assert.True(items.Count > 0);
    Assert.All(items, item => Assert.NotEmpty(item.Text));
}

[Fact]
public void PackageList_SupportsMultiSelection()
{
    var listView = _driver.FindElement(By.Id("PackageListView"));
    var items = listView.FindElements(By.ClassName("ListViewItem"));
    
    // Select multiple items using Ctrl+Click
    var actions = new Actions(_driver);
    actions.KeyDown(Keys.Control);
    items[0].Click();
    items[1].Click();
    items[2].Click();
    actions.KeyUp(Keys.Control);
    actions.Perform();
    
    // Verify selection
    var selectedItems = items.Where(i => i.Selected).ToList();
    Assert.Equal(3, selectedItems.Count);
}
```

### ComboBox/Dropdown Testing

```csharp
[Fact]
public void SourceFilter_ChangesPackageList()
{
    // Find dropdown
    var sourceComboBox = _driver.FindElement(By.Id("SourceComboBox"));
    sourceComboBox.Click();
    
    // Select option
    var wingetOption = _driver.FindElement(By.Name("WinGet"));
    wingetOption.Click();
    
    // Verify filter applied
    Thread.Sleep(1000); // Wait for filtering
    var packageList = _driver.FindElement(By.Id("PackageListView"));
    var packages = packageList.FindElements(By.ClassName("ListViewItem"));
    
    Assert.All(packages, pkg => Assert.Contains("WinGet", pkg.Text));
}
```

### TabControl Testing

```csharp
[Fact]
public void TabNavigation_SwitchesBetweenViews()
{
    // Click Discover tab
    var discoverTab = _driver.FindElement(By.Name("Discover"));
    discoverTab.Click();
    
    var discoverContent = _driver.FindElement(By.Id("DiscoverPanel"));
    Assert.True(discoverContent.Displayed);
    
    // Click Updates tab
    var updatesTab = _driver.FindElement(By.Name("Updates"));
    updatesTab.Click();
    
    var updatesContent = _driver.FindElement(By.Id("UpdatesPanel"));
    Assert.True(updatesContent.Displayed);
}
```

### TreeView Testing

```csharp
[Fact]
public void CategoryTree_ExpandsAndCollapses()
{
    var treeView = _driver.FindElement(By.Id("CategoryTreeView"));
    var rootNode = treeView.FindElement(By.Name("Development"));
    
    // Expand node
    rootNode.Click();
    Thread.Sleep(500);
    
    var childNodes = rootNode.FindElements(By.ClassName("TreeViewItem"));
    Assert.NotEmpty(childNodes);
    
    // Collapse node
    rootNode.Click();
    Thread.Sleep(500);
}
```

## Accessibility Testing

### Keyboard Navigation

```csharp
[Fact]
public void KeyboardNavigation_WorksThroughApplication()
{
    var actions = new Actions(_driver);
    
    // Tab through focusable elements
    actions.SendKeys(Keys.Tab);
    actions.SendKeys(Keys.Tab);
    actions.SendKeys(Keys.Tab);
    actions.Perform();
    
    // Verify focus moved
    var focusedElement = _driver.SwitchTo().ActiveElement();
    Assert.NotNull(focusedElement);
}

[Fact]
public void SearchBox_AccessibleViaKeyboard()
{
    var actions = new Actions(_driver);
    
    // Focus search box with keyboard shortcut
    actions.KeyDown(Keys.Control);
    actions.SendKeys("f");
    actions.KeyUp(Keys.Control);
    actions.Perform();
    
    var activeElement = _driver.SwitchTo().ActiveElement();
    Assert.Equal("SearchBox", activeElement.GetAttribute("AutomationId"));
}
```

### Screen Reader Compatibility

```csharp
[Fact]
public void Elements_HaveAccessibilityLabels()
{
    var installButton = _driver.FindElement(By.Id("InstallButton"));
    
    // Verify AutomationProperties are set
    var automationName = installButton.GetAttribute("Name");
    var helpText = installButton.GetAttribute("HelpText");
    
    Assert.NotEmpty(automationName);
    Assert.Contains("Install", automationName);
}

[Fact]
public void Images_HaveAlternativeText()
{
    var packageIcon = _driver.FindElement(By.Id("PackageIcon"));
    var altText = packageIcon.GetAttribute("Name");
    
    Assert.NotEmpty(altText);
    Assert.NotEqual("Image", altText); // Should be descriptive
}
```

### High Contrast Mode Testing

```csharp
[Fact]
public void HighContrastMode_ElementsAreVisible()
{
    // Enable high contrast mode (requires system configuration)
    // This test verifies elements remain visible
    
    var elements = _driver.FindElements(By.ClassName("Button"));
    
    Assert.All(elements, element =>
    {
        Assert.True(element.Displayed);
        // Verify contrast ratio (requires additional tools)
    });
}
```

## Visual Regression Testing

### Screenshot Comparison

```csharp
using System.Drawing;
using System.Drawing.Imaging;

public class VisualRegressionTests
{
    private readonly WindowsDriver _driver;
    private readonly string _baselineDir = @".\Baselines";
    private readonly string _resultsDir = @".\Results";

    [Fact]
    public void MainWindow_MatchesBaseline()
    {
        // Take screenshot
        var screenshot = _driver.GetScreenshot();
        var screenshotPath = Path.Combine(_resultsDir, "main-window.png");
        screenshot.SaveAsFile(screenshotPath);
        
        // Compare with baseline
        var baselinePath = Path.Combine(_baselineDir, "main-window.png");
        if (File.Exists(baselinePath))
        {
            var similarity = CompareImages(baselinePath, screenshotPath);
            Assert.True(similarity > 0.99, 
                $"Visual regression detected. Similarity: {similarity:P2}");
        }
        else
        {
            // Save as new baseline
            File.Copy(screenshotPath, baselinePath);
        }
    }

    private double CompareImages(string image1Path, string image2Path)
    {
        using var img1 = new Bitmap(image1Path);
        using var img2 = new Bitmap(image2Path);
        
        if (img1.Width != img2.Width || img1.Height != img2.Height)
            return 0;
        
        int differences = 0;
        int totalPixels = img1.Width * img1.Height;
        
        for (int x = 0; x < img1.Width; x++)
        {
            for (int y = 0; y < img1.Height; y++)
            {
                if (img1.GetPixel(x, y) != img2.GetPixel(x, y))
                    differences++;
            }
        }
        
        return 1.0 - (differences / (double)totalPixels);
    }
}
```

### Element Screenshot

```csharp
[Fact]
public void PackageCard_VisualTest()
{
    var packageCard = _driver.FindElement(By.Id("PackageCard"));
    
    var screenshot = packageCard.GetScreenshot();
    var path = @".\Results\package-card.png";
    screenshot.SaveAsFile(path);
    
    // Compare with baseline...
}
```

## Performance Testing

### Page Load Performance

```csharp
[Fact]
public void MainWindow_LoadsQuickly()
{
    var stopwatch = Stopwatch.StartNew();
    
    // App already launched in constructor
    var mainWindow = _driver.FindElement(By.Id("MainWindow"));
    
    stopwatch.Stop();
    
    Assert.True(stopwatch.ElapsedMilliseconds < 3000,
        $"Main window loaded in {stopwatch.ElapsedMilliseconds}ms, expected < 3000ms");
}

[Fact]
public void PackageSearch_RespondsQuickly()
{
    var searchBox = _driver.FindElement(By.Id("SearchBox"));
    searchBox.SendKeys("test");
    
    var stopwatch = Stopwatch.StartNew();
    
    // Wait for results
    var waitHelper = new WaitHelpers(_driver, TimeSpan.FromSeconds(10));
    waitHelper.WaitForElement(By.Id("SearchResults"));
    
    stopwatch.Stop();
    
    Assert.True(stopwatch.ElapsedMilliseconds < 2000,
        $"Search completed in {stopwatch.ElapsedMilliseconds}ms");
}
```

## Test Organization

### Test Structure

```
UniGetUI.UITests/
├── PageObjects/
│   ├── MainWindowPage.cs
│   ├── SettingsPage.cs
│   ├── PackageDetailsPage.cs
│   └── UpdatesPage.cs
├── Tests/
│   ├── MainWindowTests.cs
│   ├── PackageInstallationTests.cs
│   ├── PackageUpdateTests.cs
│   ├── SearchTests.cs
│   └── SettingsTests.cs
├── Helpers/
│   ├── WaitHelpers.cs
│   ├── DriverFactory.cs
│   └── TestDataHelper.cs
├── Fixtures/
│   └── AppFixture.cs
└── Resources/
    ├── Baselines/
    └── TestData/
```

### Base Test Class

```csharp
public abstract class UITestBase : IDisposable
{
    protected WindowsDriver Driver { get; private set; }
    protected WaitHelpers WaitHelper { get; private set; }

    protected UITestBase()
    {
        InitializeDriver();
        WaitHelper = new WaitHelpers(Driver, TimeSpan.FromSeconds(10));
    }

    private void InitializeDriver()
    {
        var options = new AppiumOptions();
        options.AddAdditionalCapability("app", GetAppPath());
        options.AddAdditionalCapability("deviceName", "WindowsPC");
        options.AddAdditionalCapability("platformName", "Windows");
        
        Driver = new WindowsDriver(new Uri("http://127.0.0.1:4723"), options);
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
    }

    protected string GetAppPath()
    {
        // Get app path from config or environment
        return Environment.GetEnvironmentVariable("UNIGETUI_APP_PATH")
            ?? @"C:\Program Files\UniGetUI\UniGetUI.exe";
    }

    protected void TakeScreenshot(string name)
    {
        var screenshot = Driver.GetScreenshot();
        var path = Path.Combine(@".\Screenshots", $"{name}.png");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        screenshot.SaveAsFile(path);
    }

    public virtual void Dispose()
    {
        Driver?.Quit();
    }
}

// Usage
public class PackageTests : UITestBase
{
    [Fact]
    public void Test()
    {
        // Use Driver and WaitHelper
    }
}
```

## CI/CD Integration

### GitHub Actions Workflow

```yaml
name: UI Tests

on:
  pull_request:
    branches: [ main ]
  schedule:
    - cron: '0 2 * * *'  # Run nightly

jobs:
  ui-tests:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v5
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v5
      with:
        dotnet-version: 8.0.x
    
    - name: Install WinAppDriver
      run: |
        choco install winappdriver -y
    
    - name: Build Application
      working-directory: src
      run: dotnet build --configuration Release
    
    - name: Start WinAppDriver
      run: |
        Start-Process "C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe"
        Start-Sleep -Seconds 5
      shell: pwsh
    
    - name: Run UI Tests
      working-directory: tests/UI
      env:
        UNIGETUI_APP_PATH: "${{ github.workspace }}\src\UniGetUI\bin\Release\net8.0-windows10.0.22621.0\UniGetUI.exe"
      run: dotnet test --verbosity normal
    
    - name: Upload Screenshots
      if: failure()
      uses: actions/upload-artifact@v4
      with:
        name: test-screenshots
        path: tests/UI/Screenshots/
    
    - name: Stop WinAppDriver
      if: always()
      run: Stop-Process -Name "WinAppDriver" -Force -ErrorAction SilentlyContinue
      shell: pwsh
```

## Best Practices

### 1. Use Page Object Model

Separate page structure from test logic for maintainability.

### 2. Explicit Waits Over Implicit

Use explicit waits for better control and reliability.

### 3. Keep Tests Independent

Each test should run standalone without depending on others.

### 4. Test Critical Paths Only

Focus UI tests on critical user workflows.

### 5. Handle Test Data Carefully

Clean up test data after each test run.

### 6. Take Screenshots on Failure

Capture evidence when tests fail for debugging.

### 7. Use Appropriate Locators

Prefer AutomationId over XPath for stability.

### 8. Avoid Hard-Coded Waits

Use explicit waits with conditions instead of Thread.Sleep.

## Common Pitfalls

### 1. Brittle XPath Selectors

```csharp
// ❌ Bad - Fragile
var element = _driver.FindElement(
    By.XPath("//Window[1]/Grid[1]/StackPanel[2]/Button[3]")
);

// ✅ Good - Stable
var element = _driver.FindElement(By.Id("InstallButton"));
```

### 2. No Wait Strategy

```csharp
// ❌ Bad
searchBox.SendKeys("test");
var results = _driver.FindElement(By.Id("Results")); // Might not be ready

// ✅ Good
searchBox.SendKeys("test");
var results = waitHelper.WaitForElement(By.Id("Results"));
```

### 3. Testing Too Much UI

Focus on critical paths; don't try to test every UI interaction.

## Resources

### Tools
- [WinAppDriver](https://github.com/microsoft/WinAppDriver)
- [Appium](https://appium.io/)
- [Accessibility Insights](https://accessibilityinsights.io/)
- [Inspect.exe](https://docs.microsoft.com/en-us/windows/win32/winauto/inspect-objects)

### Documentation
- [WinAppDriver Documentation](https://github.com/microsoft/WinAppDriver/tree/master/Docs)
- [UI Automation Overview](https://docs.microsoft.com/en-us/dotnet/framework/ui-automation/ui-automation-overview)
- [WinUI 3 Testing](https://docs.microsoft.com/en-us/windows/apps/winui/winui3/)

## Examples

See `/examples/testing/ui-automation-examples/` for:
- Complete WinAppDriver test suite
- Page Object implementations
- Accessibility test examples
- Visual regression test examples

## Next Steps

- Review [Unit Testing Guide](./unit-testing-guide.md)
- Review [Integration Testing Guide](./integration-testing-guide.md)
- Set up WinAppDriver environment
- Create page objects for your application
- Write UI tests for critical workflows
