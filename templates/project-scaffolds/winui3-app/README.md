# WinUI 3 Application Template

This template provides a complete scaffolding for a WinUI 3 application following MVVM pattern and clean architecture principles.

## Structure

```
winui3-app/
├── README.md (this file)
├── MyWinUI3App.sln
├── src/
│   ├── MyWinUI3App/
│   │   ├── MyWinUI3App.csproj
│   │   ├── App.xaml
│   │   ├── App.xaml.cs
│   │   ├── MainWindow.xaml
│   │   ├── MainWindow.xaml.cs
│   │   ├── Package.appxmanifest
│   │   ├── Views/
│   │   │   ├── HomePage.xaml
│   │   │   ├── SettingsPage.xaml
│   │   │   └── ShellPage.xaml
│   │   ├── ViewModels/
│   │   │   ├── ViewModelBase.cs
│   │   │   ├── ShellViewModel.cs
│   │   │   ├── HomeViewModel.cs
│   │   │   └── SettingsViewModel.cs
│   │   ├── Controls/
│   │   │   └── InfoCard.xaml
│   │   ├── Converters/
│   │   │   └── BoolToVisibilityConverter.cs
│   │   ├── Services/
│   │   │   ├── INavigationService.cs
│   │   │   ├── NavigationService.cs
│   │   │   ├── IActivationService.cs
│   │   │   └── ActivationService.cs
│   │   ├── Activation/
│   │   │   └── IActivationHandler.cs
│   │   ├── Helpers/
│   │   │   └── ResourceExtensions.cs
│   │   ├── Themes/
│   │   │   └── Generic.xaml
│   │   └── Assets/
│   │       ├── Icon.ico
│   │       └── Images/
│   ├── MyWinUI3App.Core/
│   │   └── [Domain layer]
│   ├── MyWinUI3App.Application/
│   │   └── [Application layer]
│   └── MyWinUI3App.Infrastructure/
│       └── [Infrastructure layer]
└── tests/
    └── [Test projects]
```

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Windows 11 SDK (10.0.22621.0 or later)
- Windows App SDK 1.4 or later
- Visual Studio 2022 17.8 or later (recommended)
- Windows 10 version 1809 (Build 17763) or later

### Setup Instructions

1. Install WinUI 3 project templates if not already installed:
   ```bash
   dotnet new install Microsoft.WindowsAppSDK.Templates
   ```

2. Create new WinUI 3 application:
   ```bash
   dotnet new winui -n MyWinUI3App
   ```

3. Create supporting projects and add references
4. Build and run the application

### Quick Start Commands

```bash
# Create WinUI 3 app with templates
dotnet new winui -n MyWinUI3App
cd MyWinUI3App

# Create class libraries
dotnet new classlib -n MyWinUI3App.Core -o src/MyWinUI3App.Core
dotnet new classlib -n MyWinUI3App.Application -o src/MyWinUI3App.Application
dotnet new classlib -n MyWinUI3App.Infrastructure -o src/MyWinUI3App.Infrastructure

# Add project references
dotnet add src/MyWinUI3App/MyWinUI3App.csproj reference src/MyWinUI3App.Application/MyWinUI3App.Application.csproj
dotnet add src/MyWinUI3App/MyWinUI3App.csproj reference src/MyWinUI3App.Infrastructure/MyWinUI3App.Infrastructure.csproj
```

## Project Files

### src/MyWinUI3App/MyWinUI3App.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows10.0.22621.0</TargetFramework>
    <TargetPlatformMinVersion>10.0.17763.0</TargetPlatformMinVersion>
    <RootNamespace>MyWinUI3App</RootNamespace>
    <ApplicationManifest>app.manifest</ApplicationManifest>
    <Platforms>x86;x64;ARM64</Platforms>
    <RuntimeIdentifiers>win-x86;win-x64;win-arm64</RuntimeIdentifiers>
    <PublishProfile>win-$(Platform).pubxml</PublishProfile>
    <UseWinUI>true</UseWinUI>
    <EnableMsixTooling>true</EnableMsixTooling>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
    <PackageReference Include="CommunityToolkit.WinUI.UI.Controls" Version="7.1.2" />
    <PackageReference Include="CommunityToolkit.WinUI.UI.Animations" Version="7.1.2" />
    <PackageReference Include="Microsoft.WindowsAppSDK" Version="1.4.231115000" />
    <PackageReference Include="Microsoft.Windows.SDK.BuildTools" Version="10.0.22621.756" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\MyWinUI3App.Application\MyWinUI3App.Application.csproj" />
    <ProjectReference Include="..\MyWinUI3App.Infrastructure\MyWinUI3App.Infrastructure.csproj" />
  </ItemGroup>

  <ItemGroup>
    <Manifest Include="$(ApplicationManifest)" />
  </ItemGroup>

  <ItemGroup>
    <Content Include="Assets\**\*">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>
</Project>
```

### src/MyWinUI3App/App.xaml

```xml
<Application
    x:Class="MyWinUI3App.App"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <XamlControlsResources xmlns="using:Microsoft.UI.Xaml.Controls" />
                <ResourceDictionary Source="Themes/Generic.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

### src/MyWinUI3App/App.xaml.cs

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using MyWinUI3App.Activation;
using MyWinUI3App.Application.Services;
using MyWinUI3App.Core.Interfaces;
using MyWinUI3App.Core.Services;
using MyWinUI3App.Infrastructure.Repositories;
using MyWinUI3App.Services;
using MyWinUI3App.ViewModels;
using MyWinUI3App.Views;

namespace MyWinUI3App;

public partial class App : Application
{
    private static IServiceProvider? s_serviceProvider;
    private Window? m_window;

    public static IServiceProvider Services => s_serviceProvider!;

    public App()
    {
        InitializeComponent();
        
        var services = new ServiceCollection();
        ConfigureServices(services);
        s_serviceProvider = services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Logging
        services.AddLogging(builder =>
        {
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Core services
        services.AddSingleton<IItemService, ItemService>();

        // Infrastructure
        services.AddSingleton<IItemRepository, ItemRepository>();

        // App services
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IActivationService, ActivationService>();

        // ViewModels
        services.AddTransient<ShellViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<SettingsViewModel>();

        // Views
        services.AddTransient<ShellPage>();
        services.AddTransient<HomePage>();
        services.AddTransient<SettingsPage>();

        // Main Window
        services.AddSingleton<MainWindow>();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        var activationService = Services.GetRequiredService<IActivationService>();
        await activationService.ActivateAsync(args);

        m_window = Services.GetRequiredService<MainWindow>();
        m_window.Activate();
    }
}
```

### src/MyWinUI3App/MainWindow.xaml

```xml
<Window
    x:Class="MyWinUI3App.MainWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    mc:Ignorable="d">

    <Grid>
        <Frame x:Name="RootFrame"/>
    </Grid>
</Window>
```

### src/MyWinUI3App/MainWindow.xaml.cs

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using MyWinUI3App.Views;

namespace MyWinUI3App;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        Title = "My WinUI 3 Application";
        
        // Navigate to shell page
        var shellPage = App.Services.GetRequiredService<ShellPage>();
        RootFrame.Content = shellPage;
    }
}
```

### src/MyWinUI3App/Views/ShellPage.xaml

```xml
<Page
    x:Class="MyWinUI3App.Views.ShellPage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    mc:Ignorable="d">

    <Grid>
        <NavigationView
            x:Name="NavigationViewControl"
            IsBackButtonVisible="Auto"
            IsBackEnabled="{x:Bind ViewModel.IsBackEnabled, Mode=OneWay}"
            IsSettingsVisible="True"
            BackRequested="NavigationViewControl_BackRequested"
            ItemInvoked="NavigationViewControl_ItemInvoked"
            PaneDisplayMode="Auto">
            
            <NavigationView.MenuItems>
                <NavigationViewItem Content="Home" Tag="Home">
                    <NavigationViewItem.Icon>
                        <FontIcon Glyph="&#xE80F;"/>
                    </NavigationViewItem.Icon>
                </NavigationViewItem>
            </NavigationView.MenuItems>

            <Frame x:Name="ContentFrame" />
        </NavigationView>
    </Grid>
</Page>
```

### src/MyWinUI3App/Views/ShellPage.xaml.cs

```csharp
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyWinUI3App.Services;
using MyWinUI3App.ViewModels;

namespace MyWinUI3App.Views;

public sealed partial class ShellPage : Page
{
    public ShellViewModel ViewModel { get; }

    public ShellPage(ShellViewModel viewModel, INavigationService navigationService)
    {
        ViewModel = viewModel;
        InitializeComponent();
        
        navigationService.Frame = ContentFrame;
        
        // Navigate to home page by default
        navigationService.NavigateTo(typeof(HomePage));
    }

    private void NavigationViewControl_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.IsSettingsInvoked)
        {
            ViewModel.NavigateToSettings();
        }
        else if (args.InvokedItemContainer != null)
        {
            var tag = args.InvokedItemContainer.Tag?.ToString();
            
            if (tag == "Home")
            {
                ViewModel.NavigateToHome();
            }
        }
    }

    private void NavigationViewControl_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        ViewModel.GoBack();
    }
}
```

### src/MyWinUI3App/ViewModels/ShellViewModel.cs

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyWinUI3App.Services;
using MyWinUI3App.Views;

namespace MyWinUI3App.ViewModels;

public partial class ShellViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private bool _isBackEnabled;

    public ShellViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        _navigationService.Navigated += OnNavigated;
    }

    [RelayCommand]
    public void NavigateToHome()
    {
        _navigationService.NavigateTo(typeof(HomePage));
    }

    [RelayCommand]
    public void NavigateToSettings()
    {
        _navigationService.NavigateTo(typeof(SettingsPage));
    }

    [RelayCommand]
    public void GoBack()
    {
        if (_navigationService.CanGoBack)
        {
            _navigationService.GoBack();
        }
    }

    private void OnNavigated(object? sender, EventArgs e)
    {
        IsBackEnabled = _navigationService.CanGoBack;
    }
}
```

### src/MyWinUI3App/ViewModels/HomeViewModel.cs

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using MyWinUI3App.Application.DTOs;
using MyWinUI3App.Core.Services;
using System.Collections.ObjectModel;

namespace MyWinUI3App.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly IItemService _itemService;
    private readonly ILogger<HomeViewModel> _logger;

    [ObservableProperty]
    private ObservableCollection<ItemDto> _items = new();

    [ObservableProperty]
    private ItemDto? _selectedItem;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    public HomeViewModel(
        IItemService itemService,
        ILogger<HomeViewModel> logger)
    {
        _itemService = itemService;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        await LoadItemsAsync();
    }

    [RelayCommand]
    private async Task LoadItemsAsync()
    {
        try
        {
            IsLoading = true;
            var items = await _itemService.GetAllItemsAsync();
            Items = new ObservableCollection<ItemDto>(items);
            _logger.LogInformation("Loaded {Count} items", Items.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load items");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AddItemAsync()
    {
        try
        {
            var newItem = await _itemService.CreateItemAsync(
                "New Item",
                "Description");
            Items.Add(newItem);
            _logger.LogInformation("Added new item: {Name}", newItem.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add item");
        }
    }

    [RelayCommand]
    private async Task DeleteItemAsync(ItemDto? item)
    {
        if (item == null) return;

        try
        {
            await _itemService.DeleteItemAsync(item.Id);
            Items.Remove(item);
            _logger.LogInformation("Deleted item: {Name}", item.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete item");
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        // Implement search logic
        await LoadItemsAsync();
    }
}
```

### src/MyWinUI3App/Views/HomePage.xaml

```xml
<Page
    x:Class="MyWinUI3App.Views.HomePage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    mc:Ignorable="d">

    <Grid Padding="24">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <TextBlock 
            Grid.Row="0"
            Text="Home" 
            Style="{StaticResource TitleTextBlockStyle}"
            Margin="0,0,0,24"/>

        <!-- Toolbar -->
        <CommandBar Grid.Row="1" DefaultLabelPosition="Right" Margin="0,0,0,12">
            <AppBarButton Icon="Add" Label="Add Item" Command="{x:Bind ViewModel.AddItemCommand}"/>
            <AppBarButton Icon="Refresh" Label="Refresh" Command="{x:Bind ViewModel.LoadItemsCommand}"/>
            <AppBarSeparator/>
            <AppBarElementContainer>
                <AutoSuggestBox 
                    PlaceholderText="Search items..."
                    QueryIcon="Find"
                    Text="{x:Bind ViewModel.SearchQuery, Mode=TwoWay}"
                    Width="300"
                    Margin="8,0"/>
            </AppBarElementContainer>
        </CommandBar>

        <!-- Items List -->
        <ListView 
            Grid.Row="2"
            ItemsSource="{x:Bind ViewModel.Items, Mode=OneWay}"
            SelectedItem="{x:Bind ViewModel.SelectedItem, Mode=TwoWay}"
            SelectionMode="Single">
            <ListView.ItemTemplate>
                <DataTemplate x:DataType="local:ItemDto">
                    <Grid Padding="12">
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="*"/>
                            <ColumnDefinition Width="Auto"/>
                        </Grid.ColumnDefinitions>
                        
                        <StackPanel Grid.Column="0">
                            <TextBlock Text="{x:Bind Name}" 
                                     Style="{StaticResource SubtitleTextBlockStyle}"/>
                            <TextBlock Text="{x:Bind Description}" 
                                     Style="{StaticResource CaptionTextBlockStyle}"
                                     Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                                     Margin="0,4,0,0"/>
                            <TextBlock 
                                Text="{x:Bind CreatedAt}"
                                Style="{StaticResource CaptionTextBlockStyle}"
                                Foreground="{ThemeResource TextFillColorTertiaryBrush}"
                                Margin="0,4,0,0"/>
                        </StackPanel>
                        
                        <Button 
                            Grid.Column="1"
                            Content="Delete"
                            Style="{StaticResource AccentButtonStyle}"
                            Command="{Binding ViewModel.DeleteItemCommand, ElementName=PageRoot}"
                            CommandParameter="{x:Bind}"/>
                    </Grid>
                </DataTemplate>
            </ListView.ItemTemplate>
        </ListView>

        <!-- Loading Indicator -->
        <ProgressRing 
            Grid.Row="2"
            IsActive="{x:Bind ViewModel.IsLoading, Mode=OneWay}"
            Width="60"
            Height="60"
            HorizontalAlignment="Center"
            VerticalAlignment="Center"/>
    </Grid>
</Page>
```

### src/MyWinUI3App/Views/HomePage.xaml.cs

```csharp
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyWinUI3App.ViewModels;

namespace MyWinUI3App.Views;

public sealed partial class HomePage : Page
{
    public HomeViewModel ViewModel { get; }

    public HomePage(HomeViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.InitializeAsync();
    }
}
```

### src/MyWinUI3App/Services/INavigationService.cs

```csharp
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace MyWinUI3App.Services;

public interface INavigationService
{
    event EventHandler? Navigated;
    
    Frame? Frame { get; set; }
    
    bool CanGoBack { get; }
    
    void NavigateTo(Type pageType, object? parameter = null);
    void GoBack();
}
```

### src/MyWinUI3App/Services/NavigationService.cs

```csharp
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace MyWinUI3App.Services;

public class NavigationService : INavigationService
{
    private Frame? _frame;

    public event EventHandler? Navigated;

    public Frame? Frame
    {
        get => _frame;
        set
        {
            if (_frame != null)
            {
                _frame.Navigated -= OnNavigated;
            }

            _frame = value;

            if (_frame != null)
            {
                _frame.Navigated += OnNavigated;
            }
        }
    }

    public bool CanGoBack => Frame?.CanGoBack ?? false;

    public void NavigateTo(Type pageType, object? parameter = null)
    {
        if (Frame?.Content?.GetType() != pageType)
        {
            Frame?.Navigate(pageType, parameter);
        }
    }

    public void GoBack()
    {
        if (CanGoBack)
        {
            Frame?.GoBack();
        }
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        Navigated?.Invoke(this, EventArgs.Empty);
    }
}
```

### src/MyWinUI3App/Services/IActivationService.cs

```csharp
namespace MyWinUI3App.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}
```

### src/MyWinUI3App/Services/ActivationService.cs

```csharp
using Microsoft.UI.Xaml;

namespace MyWinUI3App.Services;

public class ActivationService : IActivationService
{
    private readonly IEnumerable<IActivationHandler> _activationHandlers;

    public ActivationService(IEnumerable<IActivationHandler> activationHandlers)
    {
        _activationHandlers = activationHandlers;
    }

    public async Task ActivateAsync(object activationArgs)
    {
        // Execute registered activation handlers
        await Task.WhenAll(_activationHandlers.Select(h => h.HandleAsync(activationArgs)));
    }
}

public interface IActivationHandler
{
    Task HandleAsync(object args);
}
```

### src/MyWinUI3App/Themes/Generic.xaml

```xml
<ResourceDictionary
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- Custom brushes -->
    <SolidColorBrush x:Key="CustomAccentBrush" Color="#0078D4"/>
    
    <!-- Custom button style -->
    <Style x:Key="AccentButtonStyle" TargetType="Button" BasedOn="{StaticResource DefaultButtonStyle}">
        <Setter Property="Background" Value="{StaticResource CustomAccentBrush}"/>
        <Setter Property="Foreground" Value="White"/>
    </Style>

</ResourceDictionary>
```

### src/MyWinUI3App/Package.appxmanifest

```xml
<?xml version="1.0" encoding="utf-8"?>
<Package
  xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10"
  xmlns:uap="http://schemas.microsoft.com/appx/manifest/uap/windows10"
  xmlns:rescap="http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities"
  IgnorableNamespaces="uap rescap">

  <Identity Name="MyWinUI3App" Publisher="CN=Publisher" Version="1.0.0.0" />

  <Properties>
    <DisplayName>My WinUI 3 Application</DisplayName>
    <PublisherDisplayName>Publisher</PublisherDisplayName>
    <Logo>Assets\StoreLogo.png</Logo>
  </Properties>

  <Dependencies>
    <TargetDeviceFamily Name="Windows.Universal" MinVersion="10.0.17763.0" MaxVersionTested="10.0.22621.0" />
    <TargetDeviceFamily Name="Windows.Desktop" MinVersion="10.0.17763.0" MaxVersionTested="10.0.22621.0" />
  </Dependencies>

  <Resources>
    <Resource Language="x-generate"/>
  </Resources>

  <Applications>
    <Application Id="App" Executable="$targetnametoken$.exe" EntryPoint="$targetentrypoint$">
      <uap:VisualElements
        DisplayName="My WinUI 3 Application"
        Description="My WinUI 3 Application"
        BackgroundColor="transparent"
        Square150x150Logo="Assets\Square150x150Logo.png"
        Square44x44Logo="Assets\Square44x44Logo.png">
        <uap:DefaultTile Wide310x150Logo="Assets\Wide310x150Logo.png" />
        <uap:SplashScreen Image="Assets\SplashScreen.png" />
      </uap:VisualElements>
    </Application>
  </Applications>

  <Capabilities>
    <rescap:Capability Name="runFullTrust" />
  </Capabilities>
</Package>
```

## Features

This WinUI 3 template includes:

- ✅ Windows App SDK and WinUI 3
- ✅ MVVM pattern with CommunityToolkit.Mvvm
- ✅ NavigationView with modern Windows 11 design
- ✅ Dependency injection
- ✅ Frame-based navigation service
- ✅ Activation service for app lifecycle
- ✅ Layered architecture (Core, Application, Infrastructure)
- ✅ Async/await patterns throughout
- ✅ Observable collections and properties
- ✅ Command bar with modern controls
- ✅ AutoSuggestBox for search
- ✅ ListView with custom item templates
- ✅ Progress indicators
- ✅ Custom themes and styles
- ✅ Fluent Design System styling

## Differences from WPF

### Key Changes

1. **Namespace**: `Microsoft.UI.Xaml` instead of `System.Windows`
2. **Controls**: Some controls have different names or behavior
3. **Navigation**: Frame-based navigation is standard
4. **App Lifecycle**: Different activation patterns
5. **Packaging**: MSIX packaging with Package.appxmanifest
6. **Styling**: Fluent Design System by default

### Migration Tips

- Use `NavigationView` instead of custom navigation
- Use `Frame` for page navigation
- Leverage Windows App SDK features (notifications, widgets, etc.)
- Take advantage of WinUI 3 performance improvements
- Use modern Windows 11 design patterns

## Next Steps

1. Add more pages and functionality
2. Implement data persistence
3. Add Windows 11 context menus
4. Integrate notifications
5. Add app settings
6. Implement theme switching (Light/Dark)
7. Add unit tests
8. Configure MSIX packaging for distribution

## Resources

- [WinUI 3 Documentation](https://docs.microsoft.com/en-us/windows/apps/winui/)
- [Windows App SDK](https://docs.microsoft.com/en-us/windows/apps/windows-app-sdk/)
- [CommunityToolkit.WinUI](https://github.com/CommunityToolkit/Windows)
- [Windows Design](https://docs.microsoft.com/en-us/windows/apps/design/)

## License

[Your License Here]
