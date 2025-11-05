# Tutorial 1: Adding a Simple Feature

**Difficulty:** Beginner  
**Time:** 1-2 hours  
**Prerequisites:** Completed environment setup

## 📖 Overview

In this tutorial, you'll learn how to add a simple feature to UniGetUI by creating a new setting in the Settings page. This hands-on exercise will teach you:
- How to modify XAML UI
- How to use the Settings system
- How to implement event handlers
- How to test your changes
- How to follow coding standards

## 🎯 What We'll Build

We'll add a new setting called "Show Package Count" that displays the total number of installed packages in the system tray tooltip.

**Skills you'll learn:**
- XAML UI design with WinUI 3
- Settings management
- Event handling
- UI/Logic separation
- Testing changes

## 📋 Step 1: Understand the Settings System

### How Settings Work

UniGetUI uses a centralized settings system:

```csharp
// Boolean settings
Settings.Get("SettingKey");              // Read
Settings.Set("SettingKey", true);        // Write

// String settings
Settings.GetValue("SettingKey");         // Read
Settings.SetValue("SettingKey", "value"); // Write
```

Settings are automatically persisted to disk.

### Where Settings Are Defined

Settings UI is in: `src/UniGetUI/Pages/SettingsPages/`

The settings are organized by category:
- General settings
- Package manager-specific settings
- UI preferences
- Advanced settings

## 📋 Step 2: Plan Your Feature

### Feature Specification

**Setting Name:** "Show Package Count in System Tray"  
**Location:** General Settings page  
**Type:** Boolean toggle (on/off)  
**Default Value:** Off  
**Behavior:** When enabled, appends package count to the system tray tooltip

### Files We'll Modify

1. A settings page XAML file (for the UI)
2. A settings page code-behind file (for the logic)
3. Potentially the system tray handler (for displaying the count)

For this tutorial, we'll focus on adding the UI and setting persistence. The actual system tray integration would be a follow-up task.

## 📋 Step 3: Find the Right Place

### Locate Settings Pages

```bash
cd /path/to/UniGetUI/src/UniGetUI
ls Pages/SettingsPages/
```

You'll see various settings page files. For a general setting, we'd typically add it to a general settings page.

### Explore Existing Settings

Open an existing settings page to understand the pattern:

```bash
# Example: Look at a settings page
code Pages/SettingsPages/GeneralSettingsPage.xaml
code Pages/SettingsPages/GeneralSettingsPage.xaml.cs
```

Study the structure:
- How settings are laid out in XAML
- How toggle switches are used
- How event handlers are connected

## 📋 Step 4: Create the UI (XAML)

### Example: Adding a Setting Toggle

Let's add our setting to a settings page. Here's the XAML pattern:

```xml
<!-- Inside a StackPanel or similar container -->
<controls:SettingsCard 
    Header="Show Package Count in System Tray"
    Description="Display the total number of installed packages in the system tray icon tooltip">
    <ToggleSwitch 
        x:Name="ShowPackageCountToggle"
        Toggled="ShowPackageCountToggle_Toggled"/>
</controls:SettingsCard>
```

### Understanding the Code

- `SettingsCard`: A custom control from CommunityToolkit that provides consistent settings UI
- `Header`: The main label for the setting
- `Description`: Explanatory text shown below the header
- `ToggleSwitch`: The on/off control
- `x:Name`: Gives us a way to reference this control in code-behind
- `Toggled`: Event handler that fires when the toggle changes

### Add to XAML File

1. Open the appropriate settings page XAML file
2. Find a good location (usually near similar settings)
3. Add the `SettingsCard` with your toggle
4. Save the file

## 📋 Step 5: Implement the Logic (Code-Behind)

### Add Event Handler

Open the corresponding `.xaml.cs` file and add:

```csharp
private void ShowPackageCountToggle_Toggled(object sender, RoutedEventArgs e)
{
    // Save the setting when the toggle changes
    Settings.Set("ShowPackageCountInTray", ShowPackageCountToggle.IsOn);
}
```

### Load Setting on Page Load

In the page constructor or `OnNavigatedTo` method, add:

```csharp
private void LoadSettings()
{
    // Load the saved setting value
    ShowPackageCountToggle.IsOn = Settings.Get("ShowPackageCountInTray");
}
```

Call this method when the page loads:

```csharp
public SettingsPageName()
{
    InitializeComponent();
    LoadSettings();
}
```

## 📋 Step 6: Add Localization

### Update Language Files

Settings text should be localized. Add to language files:

1. Open: `src/UniGetUI.Core.LanguageEngine/lang/en.json`
2. Add entries:

```json
{
    "Show Package Count in System Tray": "Show Package Count in System Tray",
    "Display the total number of installed packages in the system tray icon tooltip": "Display the total number of installed packages in the system tray icon tooltip"
}
```

### Use Localized Strings in XAML

Instead of hardcoded text, you can use localized strings:

```xml
<controls:SettingsCard 
    Header="{x:Bind CoreTools.Translate('Show Package Count in System Tray')}"
    Description="{x:Bind CoreTools.Translate('Display the total number of installed packages in the system tray icon tooltip')}">
```

For this tutorial, hardcoded strings are acceptable for learning purposes.

## 📋 Step 7: Build and Test

### Build the Project

```bash
cd src
dotnet build UniGetUI.sln --configuration Debug --property:Platform=x64
```

Expected output: `Build succeeded`

### Run the Application

```bash
cd UniGetUI/bin/x64/Debug/net8.0-windows10.0.26100.0/
./UniGetUI.exe
```

Or press `F5` in Visual Studio.

### Test Your Changes

1. Navigate to Settings in the application
2. Find your new setting
3. Toggle it on and off
4. Close and reopen the application
5. Verify the setting persists

### Verification Checklist

- [ ] Setting appears in the correct location
- [ ] Toggle works (switches on/off)
- [ ] Setting persists after restart
- [ ] No errors in console/logs
- [ ] UI looks consistent with other settings

## 📋 Step 8: Follow Coding Standards

### Review Your Code

Check against coding standards:

- [ ] Variable names use camelCase: `showPackageCount`
- [ ] Method names use PascalCase: `ShowPackageCountToggle_Toggled`
- [ ] Event handlers follow naming: `ControlName_EventName`
- [ ] Proper spacing and indentation (4 spaces)
- [ ] No trailing whitespace

### Clean Up

Run code formatter (if using Visual Studio):
- **Edit > Advanced > Format Document** (Ctrl+K, Ctrl+D)

Or let `.editorconfig` handle it automatically.

## 📋 Step 9: Commit Your Changes

### Check Status

```bash
git status
```

### Create Feature Branch

```bash
git checkout -b feature/add-package-count-setting
```

### Stage Changes

```bash
git add src/UniGetUI/Pages/SettingsPages/YourSettingsFile.xaml
git add src/UniGetUI/Pages/SettingsPages/YourSettingsFile.xaml.cs
```

### Commit

```bash
git commit -m "Add setting to show package count in system tray

Added a new toggle setting in the General Settings page that allows
users to enable/disable displaying the package count in the system
tray tooltip. Setting persists across application restarts.

Related to issue #XXX"
```

## 📋 Step 10: Extend Your Feature (Optional)

### Actually Use the Setting

To make this feature complete, you'd need to:

1. **Find the system tray code** (likely in `src/UniGetUI/` somewhere)
2. **Read the setting** when updating the tray tooltip:
   ```csharp
   if (Settings.Get("ShowPackageCountInTray"))
   {
       string count = GetInstalledPackageCount().ToString();
       tooltipText += $" ({count} packages)";
   }
   ```
3. **Test the integration**

This is left as an exercise! It teaches you how to:
- Navigate the codebase
- Find where features are implemented
- Integrate settings into functionality

## 🎓 What You Learned

Congratulations! You've learned:
- ✅ How to add UI elements in WinUI 3 XAML
- ✅ How to use the Settings system
- ✅ How to implement event handlers
- ✅ How to persist data
- ✅ How to follow coding standards
- ✅ How to test changes locally
- ✅ How to commit properly formatted code

## 🚀 Next Steps

### Immediate Next Steps
1. **Submit a PR** if you made actual changes
2. **Try modifying** different settings
3. **Explore** other settings pages to understand patterns

### Continue Learning
- [Tutorial 2: Understanding Package Managers](02-understanding-package-managers.md) - Dive deeper into the core architecture
- [Tutorial 3: Working with WinUI 3](03-working-with-winui3.md) - Advanced UI patterns
- [Adding Features Guide](../../codebase-analysis/08-extension/adding-features.md) - Comprehensive guide

### Challenge Yourself
Try adding:
- A string setting (using ComboBox or TextBox)
- A numeric setting (using NumberBox)
- A setting with validation
- A setting that changes UI behavior immediately

## 📚 Additional Resources

- [WinUI 3 Controls](https://learn.microsoft.com/en-us/windows/apps/design/controls/)
- [CommunityToolkit.WinUI](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/windows/)
- [Settings Pattern in UniGetUI](../../codebase-analysis/07-best-practices/patterns-standards.md)

## 🆘 Troubleshooting

### Build Errors

**"Cannot find SettingsCard"**
- Ensure proper namespace: `xmlns:controls="using:UniGetUI.Controls"`
- Check the control name is correct

**"ShowPackageCountToggle does not exist"**
- Ensure `x:Name` is set in XAML
- Build the project to generate references

### Runtime Errors

**Setting doesn't persist**
- Ensure you're calling `Settings.Set()` in the event handler
- Check the setting key is consistent (typos?)

**Toggle doesn't reflect saved value**
- Ensure `LoadSettings()` is called when page loads
- Verify you're reading the correct setting key

## ✅ Tutorial Complete!

You've successfully learned how to add a simple feature to UniGetUI! This foundational knowledge will help you tackle more complex contributions.

**Next:** Try [Tutorial 2: Understanding Package Managers](02-understanding-package-managers.md) to learn about the core architecture.

Happy coding! 🎉
