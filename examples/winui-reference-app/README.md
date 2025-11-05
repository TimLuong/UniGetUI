# WinUI 3 Reference Application

This is a comprehensive WinUI 3 reference application demonstrating all the design patterns and best practices documented in the UniGetUI codebase, using modern Windows App SDK.

## Overview

This reference application showcases the same patterns as the WPF example but using WinUI 3:
- Factory Pattern implementation
- Singleton Pattern (static class variant)
- Observer Pattern with custom events
- Strategy Pattern for pluggable behaviors
- Task Recycler pattern for CPU optimization
- MVVM architecture with WinUI 3
- Async/Await best practices
- Error handling patterns
- Modern C# features (C# 12)
- Windows App SDK features

## Key Differences from WPF

### 1. UI Framework
- **WinUI 3** instead of WPF
- Uses **Windows App SDK** (formerly Project Reunion)
- Modern Fluent Design System
- Better performance and native Windows 11 integration

### 2. XAML Namespace
```xml
<!-- WinUI 3 -->
xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"

<!-- Compared to WPF which uses the same namespaces but different implementations -->
```

### 3. Project Structure
```
winui-reference-app/
├── App.xaml                    # Application definition (WinUI 3)
├── App.xaml.cs                 # Application code-behind
├── MainWindow.xaml             # Main window XAML (WinUI 3)
├── MainWindow.xaml.cs          # Main window code-behind
├── Package.appxmanifest        # WinUI 3 app manifest
├── ViewModels/
│   └── MainViewModel.cs        # Identical to WPF version
├── Models/
│   └── Package.cs              # Identical to WPF version
├── Services/
│   ├── IPackageService.cs      # Identical to WPF version
│   ├── PackageServiceFactory.cs # Identical to WPF version
│   ├── TaskRecycler.cs         # Identical to WPF version
│   └── [Service implementations] # Identical to WPF version
└── Utilities/
    ├── ObservableQueue.cs      # Identical to WPF version
    └── Logger.cs               # Identical to WPF version
```

## Prerequisites

### Required Software
- **Windows 10 version 1809 (build 17763)** or later
- **.NET 8.0 SDK**
- **Windows App SDK 1.7 or later**
- **Visual Studio 2022** version 17.9 or later with:
  - .NET Desktop Development workload
  - Universal Windows Platform development workload (for Windows App SDK)

## Building the Application

### Using Visual Studio
1. Open `WinUIReferenceApp.sln` in Visual Studio 2022
2. Ensure Windows App SDK is installed (Visual Studio will prompt if needed)
3. Select **Release** or **Debug** configuration
4. Select **x64** or **ARM64** platform
5. Build > Build Solution (or press `Ctrl+Shift+B`)
6. Press F5 to run

### Using .NET CLI
```bash
# Restore dependencies
dotnet restore WinUIReferenceApp.sln

# Build the application
dotnet build WinUIReferenceApp.sln --configuration Release

# Run the application
dotnet run --project WinUIReferenceApp/WinUIReferenceApp.csproj
```

## Running the Application

After building:
1. Launch `WinUIReferenceApp.exe` from the build output
2. The application features the same functionality as the WPF version
3. Observe the modern Fluent Design System UI
4. All patterns work identically to the WPF version

## WinUI 3 Specific Features

### 1. Modern Controls
- **NavigationView** for app navigation
- **InfoBar** for status messages
- **TeachingTip** for user guidance
- **ProgressRing** for loading states

### 2. Fluent Design
- **Acrylic** materials for translucent surfaces
- **Reveal** highlight on hover
- **Shadow** effects for depth
- **Corner radius** for modern look

### 3. Adaptive Layout
- Responsive design that adapts to window size
- Better support for tablets and touch interfaces

### 4. Performance
- Hardware-accelerated rendering
- Better memory management
- Faster startup times

## Code Reusability

Most of the code is **identical** between WPF and WinUI 3:
- **ViewModels**: 100% shared
- **Models**: 100% shared
- **Services**: 100% shared
- **Utilities**: 100% shared

Only the View layer (XAML and code-behind) differs due to framework differences.

## Example: Converting WPF to WinUI 3

### WPF Button
```xml
<Button Content="Load Packages"
        Style="{StaticResource ModernButton}"
        Command="{Binding LoadPackagesCommand}"/>
```

### WinUI 3 Button (similar but with subtle differences)
```xml
<Button Content="Load Packages"
        Style="{StaticResource AccentButtonStyle}"
        Command="{Binding LoadPackagesCommand}"/>
```

### Key Differences
1. **Resource names**: WinUI 3 uses different built-in style names
2. **Namespaces**: Different xmlns declarations
3. **Controls**: Some controls have different names (e.g., `Frame` behavior)

## Best Practices for WinUI 3

### 1. Use Async/Await Throughout
```csharp
private async void OnLoaded(object sender, RoutedEventArgs e)
{
    await InitializeAsync();
}
```

### 2. Proper Threading
```csharp
// Use DispatcherQueue for UI thread access
DispatcherQueue.TryEnqueue(() =>
{
    StatusText.Text = "Updated";
});
```

### 3. Resource Management
```csharp
// Dispose of resources properly
public void Dispose()
{
    // Clean up
}
```

### 4. Window Customization
```csharp
// Customize title bar
AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
```

## Testing

The application demonstrates:
- Unit tests for ViewModels (shared with WPF)
- Integration tests for Services (shared with WPF)
- WinUI 3 specific UI tests using Windows Application Driver

## Deployment

### MSIX Package
WinUI 3 applications are typically packaged as MSIX:
```bash
# Build MSIX package
msbuild /t:Publish /p:Configuration=Release
```

### Microsoft Store
- WinUI 3 apps can be published to Microsoft Store
- Better update experience for users
- Automatic dependency management

## Performance Considerations

### WinUI 3 Advantages
- **Faster Rendering**: Hardware acceleration
- **Better Memory**: Improved garbage collection
- **Native Windows 11**: Full integration with OS features

### Optimization Tips
1. Use **virtualization** for long lists
2. Implement **incremental loading** for large datasets
3. Use **compiled bindings** for better performance
4. Leverage **Windows App SDK** features

## Migration from WPF

If you're migrating from WPF to WinUI 3:

### 1. Update Project File
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0-windows10.0.26100.0</TargetFramework>
    <UseWinUI>true</UseWinUI>
  </PropertyGroup>
</Project>
```

### 2. Update Namespaces
- WPF: `System.Windows.*`
- WinUI 3: `Microsoft.UI.Xaml.*`

### 3. Update Controls
- Some WPF controls have different WinUI 3 equivalents
- Check the [WinUI 3 documentation](https://learn.microsoft.com/en-us/windows/apps/winui/)

### 4. Keep Business Logic
- ViewModels, Models, Services remain unchanged
- Only UI layer needs updates

## Additional Resources

- [WinUI 3 Documentation](https://learn.microsoft.com/en-us/windows/apps/winui/winui3/)
- [Windows App SDK Documentation](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/)
- [WinUI 3 Gallery](https://github.com/microsoft/WinUI-Gallery) - Control samples
- [WinUI 3 Templates](https://learn.microsoft.com/en-us/windows/apps/winui/winui3/winui-project-templates-in-visual-studio)
- [Migrating from WPF](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/migrate-to-windows-app-sdk/guides/wpf)

## License

This example is provided as part of the UniGetUI project documentation and follows the same license as the main project.
