# WPF Application Template

This template provides a complete scaffolding for a WPF application following MVVM pattern and clean architecture principles.

## Structure

```
wpf-app/
├── README.md (this file)
├── MyWpfApp.sln
├── src/
│   ├── MyWpfApp/
│   │   ├── MyWpfApp.csproj
│   │   ├── App.xaml
│   │   ├── App.xaml.cs
│   │   ├── MainWindow.xaml
│   │   ├── MainWindow.xaml.cs
│   │   ├── Views/
│   │   │   ├── HomeView.xaml
│   │   │   └── SettingsView.xaml
│   │   ├── ViewModels/
│   │   │   ├── ViewModelBase.cs
│   │   │   ├── MainViewModel.cs
│   │   │   ├── HomeViewModel.cs
│   │   │   └── SettingsViewModel.cs
│   │   ├── Controls/
│   │   │   └── CustomButton.xaml
│   │   ├── Converters/
│   │   │   └── BoolToVisibilityConverter.cs
│   │   ├── Services/
│   │   │   ├── INavigationService.cs
│   │   │   ├── NavigationService.cs
│   │   │   ├── IDialogService.cs
│   │   │   └── DialogService.cs
│   │   └── Resources/
│   │       ├── Styles.xaml
│   │       └── Colors.xaml
│   ├── MyWpfApp.Core/
│   │   └── [Same as console template]
│   ├── MyWpfApp.Application/
│   │   └── [Same as console template]
│   └── MyWpfApp.Infrastructure/
│       └── [Same as console template]
└── tests/
    └── [Test projects]
```

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 (recommended) or JetBrains Rider
- Windows 10 or later

### Setup Instructions

1. Copy this template to your new project directory
2. Rename all instances of "MyWpfApp" to your application name
3. Update namespace declarations
4. Restore NuGet packages
5. Build and run

### Quick Start Commands

```bash
# Create new WPF application
dotnet new wpf -n MyWpfApp
cd MyWpfApp

# Create class libraries
dotnet new classlib -n MyWpfApp.Core -o src/MyWpfApp.Core
dotnet new classlib -n MyWpfApp.Application -o src/MyWpfApp.Application
dotnet new classlib -n MyWpfApp.Infrastructure -o src/MyWpfApp.Infrastructure

# Add project references
dotnet add src/MyWpfApp/MyWpfApp.csproj reference src/MyWpfApp.Application/MyWpfApp.Application.csproj
dotnet add src/MyWpfApp/MyWpfApp.csproj reference src/MyWpfApp.Infrastructure/MyWpfApp.Infrastructure.csproj
```

## Project Files

### src/MyWpfApp/MyWpfApp.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <Nullable>enable</Nullable>
    <RootNamespace>MyWpfApp</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\MyWpfApp.Application\MyWpfApp.Application.csproj" />
    <ProjectReference Include="..\MyWpfApp.Infrastructure\MyWpfApp.Infrastructure.csproj" />
  </ItemGroup>
</Project>
```

### src/MyWpfApp/App.xaml

```xml
<Application x:Class="MyWpfApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             StartupUri="MainWindow.xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="Resources/Colors.xaml"/>
                <ResourceDictionary Source="Resources/Styles.xaml"/>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

### src/MyWpfApp/App.xaml.cs

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyWpfApp.Application.Services;
using MyWpfApp.Core.Interfaces;
using MyWpfApp.Core.Services;
using MyWpfApp.Infrastructure.Repositories;
using MyWpfApp.Services;
using MyWpfApp.ViewModels;
using System.Windows;

namespace MyWpfApp;

public partial class App : Application
{
    private readonly ServiceProvider _serviceProvider;

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
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

        // UI Services
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IDialogService, DialogService>();

        // ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<SettingsViewModel>();

        // Windows
        services.AddTransient<MainWindow>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider.Dispose();
        base.OnExit(e);
    }
}
```

### src/MyWpfApp/MainWindow.xaml

```xml
<Window x:Class="MyWpfApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:viewModels="clr-namespace:MyWpfApp.ViewModels"
        mc:Ignorable="d"
        Title="My WPF Application" 
        Height="600" 
        Width="900"
        WindowStartupLocation="CenterScreen">
    
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        
        <!-- Header -->
        <Border Grid.Row="0" 
                Background="{StaticResource PrimaryBrush}"
                Padding="20,10">
            <TextBlock Text="My WPF Application" 
                       FontSize="24" 
                       FontWeight="Bold"
                       Foreground="White"/>
        </Border>
        
        <!-- Main Content Area -->
        <Grid Grid.Row="1">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="200"/>
                <ColumnDefinition Width="*"/>
            </Grid.ColumnDefinitions>
            
            <!-- Navigation Panel -->
            <Border Grid.Column="0" 
                    Background="{StaticResource SecondaryBrush}"
                    BorderBrush="{StaticResource BorderBrush}"
                    BorderThickness="0,0,1,0">
                <StackPanel Margin="10">
                    <Button Content="Home" 
                            Command="{Binding NavigateToHomeCommand}"
                            Style="{StaticResource NavigationButtonStyle}"
                            Margin="0,5"/>
                    <Button Content="Settings" 
                            Command="{Binding NavigateToSettingsCommand}"
                            Style="{StaticResource NavigationButtonStyle}"
                            Margin="0,5"/>
                </StackPanel>
            </Border>
            
            <!-- Content Area -->
            <ContentControl Grid.Column="1" 
                          Content="{Binding CurrentView}"
                          Margin="20"/>
        </Grid>
        
        <!-- Status Bar -->
        <Border Grid.Row="2" 
                Background="{StaticResource SecondaryBrush}"
                BorderBrush="{StaticResource BorderBrush}"
                BorderThickness="0,1,0,0"
                Padding="10,5">
            <TextBlock Text="{Binding StatusMessage}" 
                       FontSize="12"/>
        </Border>
    </Grid>
</Window>
```

### src/MyWpfApp/MainWindow.xaml.cs

```csharp
using MyWpfApp.ViewModels;
using System.Windows;

namespace MyWpfApp;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
```

### src/MyWpfApp/ViewModels/ViewModelBase.cs

```csharp
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyWpfApp.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    // Base functionality for all ViewModels
}
```

### src/MyWpfApp/ViewModels/MainViewModel.cs

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyWpfApp.Services;
using System.Windows.Input;

namespace MyWpfApp.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    public MainViewModel(
        INavigationService navigationService,
        HomeViewModel homeViewModel)
    {
        _navigationService = navigationService;
        
        // Set initial view
        CurrentView = homeViewModel;
        
        // Subscribe to navigation events
        _navigationService.CurrentViewChanged += OnCurrentViewChanged;
    }

    [RelayCommand]
    private void NavigateToHome()
    {
        _navigationService.NavigateTo<HomeViewModel>();
        StatusMessage = "Navigated to Home";
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        _navigationService.NavigateTo<SettingsViewModel>();
        StatusMessage = "Navigated to Settings";
    }

    private void OnCurrentViewChanged(object? sender, object? newView)
    {
        CurrentView = newView;
    }
}
```

### src/MyWpfApp/ViewModels/HomeViewModel.cs

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using MyWpfApp.Application.DTOs;
using MyWpfApp.Core.Services;
using System.Collections.ObjectModel;

namespace MyWpfApp.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly IItemService _itemService;
    private readonly ILogger<HomeViewModel> _logger;

    [ObservableProperty]
    private ObservableCollection<ItemDto> _items = new();

    [ObservableProperty]
    private ItemDto? _selectedItem;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    public HomeViewModel(
        IItemService itemService,
        ILogger<HomeViewModel> logger)
    {
        _itemService = itemService;
        _logger = logger;
        
        LoadItemsAsync().ConfigureAwait(false);
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
        await LoadItemsAsync();
        // Implement search filtering logic here
    }
}
```

### src/MyWpfApp/ViewModels/SettingsViewModel.cs

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace MyWpfApp.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly ILogger<SettingsViewModel> _logger;

    [ObservableProperty]
    private string _applicationName = "My WPF Application";

    [ObservableProperty]
    private string _version = "1.0.0";

    [ObservableProperty]
    private bool _darkModeEnabled;

    [ObservableProperty]
    private int _fontSize = 14;

    public SettingsViewModel(ILogger<SettingsViewModel> logger)
    {
        _logger = logger;
    }

    [RelayCommand]
    private void ApplySettings()
    {
        _logger.LogInformation("Applying settings: DarkMode={DarkMode}, FontSize={FontSize}", 
            DarkModeEnabled, FontSize);
        // Apply settings logic here
    }

    [RelayCommand]
    private void ResetSettings()
    {
        DarkModeEnabled = false;
        FontSize = 14;
        _logger.LogInformation("Settings reset to defaults");
    }
}
```

### src/MyWpfApp/Views/HomeView.xaml

```xml
<UserControl x:Class="MyWpfApp.Views.HomeView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006" 
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008" 
             mc:Ignorable="d" 
             d:DesignHeight="450" d:DesignWidth="800">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>
        
        <!-- Title -->
        <TextBlock Grid.Row="0" 
                   Text="Home" 
                   FontSize="28" 
                   FontWeight="Bold"
                   Margin="0,0,0,20"/>
        
        <!-- Toolbar -->
        <StackPanel Grid.Row="1" 
                    Orientation="Horizontal" 
                    Margin="0,0,0,10">
            <TextBox Text="{Binding SearchText, UpdateSourceTrigger=PropertyChanged}"
                     Width="200"
                     Margin="0,0,10,0"
                     VerticalContentAlignment="Center"/>
            <Button Content="Search" 
                    Command="{Binding SearchCommand}"
                    Margin="0,0,10,0"/>
            <Button Content="Add Item" 
                    Command="{Binding AddItemCommand}"/>
            <Button Content="Refresh" 
                    Command="{Binding LoadItemsCommand}"
                    Margin="10,0,0,0"/>
        </StackPanel>
        
        <!-- Items List -->
        <ListView Grid.Row="2"
                  ItemsSource="{Binding Items}"
                  SelectedItem="{Binding SelectedItem}">
            <ListView.View>
                <GridView>
                    <GridViewColumn Header="ID" 
                                    Width="50"
                                    DisplayMemberBinding="{Binding Id}"/>
                    <GridViewColumn Header="Name" 
                                    Width="200"
                                    DisplayMemberBinding="{Binding Name}"/>
                    <GridViewColumn Header="Description" 
                                    Width="300"
                                    DisplayMemberBinding="{Binding Description}"/>
                    <GridViewColumn Header="Created" 
                                    Width="150"
                                    DisplayMemberBinding="{Binding CreatedAt, StringFormat='{}{0:yyyy-MM-dd HH:mm}'}"/>
                    <GridViewColumn Header="Actions" 
                                    Width="100">
                        <GridViewColumn.CellTemplate>
                            <DataTemplate>
                                <Button Content="Delete" 
                                        Command="{Binding DataContext.DeleteItemCommand, RelativeSource={RelativeSource AncestorType=ListView}}"
                                        CommandParameter="{Binding}"/>
                            </DataTemplate>
                        </GridViewColumn.CellTemplate>
                    </GridViewColumn>
                </GridView>
            </ListView.View>
        </ListView>
        
        <!-- Loading Indicator -->
        <Grid Grid.Row="2" 
              Visibility="{Binding IsLoading, Converter={StaticResource BoolToVisibilityConverter}}"
              Background="#80FFFFFF">
            <StackPanel HorizontalAlignment="Center" 
                       VerticalAlignment="Center">
                <ProgressBar IsIndeterminate="True" 
                            Width="200" 
                            Height="20"/>
                <TextBlock Text="Loading..." 
                          HorizontalAlignment="Center" 
                          Margin="0,10,0,0"/>
            </StackPanel>
        </Grid>
    </Grid>
</UserControl>
```

### src/MyWpfApp/Services/INavigationService.cs

```csharp
namespace MyWpfApp.Services;

public interface INavigationService
{
    event EventHandler<object?>? CurrentViewChanged;
    void NavigateTo<TViewModel>() where TViewModel : class;
    void GoBack();
    bool CanGoBack { get; }
}
```

### src/MyWpfApp/Services/NavigationService.cs

```csharp
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace MyWpfApp.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Stack<object> _navigationStack = new();
    
    public event EventHandler<object?>? CurrentViewChanged;
    
    public bool CanGoBack => _navigationStack.Count > 1;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void NavigateTo<TViewModel>() where TViewModel : class
    {
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
        _navigationStack.Push(viewModel);
        CurrentViewChanged?.Invoke(this, viewModel);
    }

    public void GoBack()
    {
        if (_navigationStack.Count > 1)
        {
            _navigationStack.Pop();
            var previousView = _navigationStack.Peek();
            CurrentViewChanged?.Invoke(this, previousView);
        }
    }
}
```

### src/MyWpfApp/Services/IDialogService.cs

```csharp
namespace MyWpfApp.Services;

public interface IDialogService
{
    void ShowMessage(string title, string message);
    bool ShowConfirmation(string title, string message);
    Task ShowMessageAsync(string title, string message);
    Task<bool> ShowConfirmationAsync(string title, string message);
}
```

### src/MyWpfApp/Services/DialogService.cs

```csharp
using System.Threading.Tasks;
using System.Windows;

namespace MyWpfApp.Services;

public class DialogService : IDialogService
{
    public void ShowMessage(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public bool ShowConfirmation(string title, string message)
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return result == MessageBoxResult.Yes;
    }

    public Task ShowMessageAsync(string title, string message)
    {
        return Task.Run(() => ShowMessage(title, message));
    }

    public Task<bool> ShowConfirmationAsync(string title, string message)
    {
        return Task.Run(() => ShowConfirmation(title, message));
    }
}
```

### src/MyWpfApp/Converters/BoolToVisibilityConverter.cs

```csharp
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MyWpfApp.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible;
        }
        return false;
    }
}
```

### src/MyWpfApp/Resources/Colors.xaml

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <!-- Primary Colors -->
    <Color x:Key="PrimaryColor">#0078D4</Color>
    <Color x:Key="SecondaryColor">#F3F3F3</Color>
    <Color x:Key="AccentColor">#FFB900</Color>
    
    <!-- Brushes -->
    <SolidColorBrush x:Key="PrimaryBrush" Color="{StaticResource PrimaryColor}"/>
    <SolidColorBrush x:Key="SecondaryBrush" Color="{StaticResource SecondaryColor}"/>
    <SolidColorBrush x:Key="AccentBrush" Color="{StaticResource AccentColor}"/>
    <SolidColorBrush x:Key="BorderBrush" Color="#E0E0E0"/>
    
</ResourceDictionary>
```

### src/MyWpfApp/Resources/Styles.xaml

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <!-- Button Styles -->
    <Style x:Key="NavigationButtonStyle" TargetType="Button">
        <Setter Property="Background" Value="Transparent"/>
        <Setter Property="Foreground" Value="Black"/>
        <Setter Property="BorderThickness" Value="0"/>
        <Setter Property="Padding" Value="10,8"/>
        <Setter Property="HorizontalContentAlignment" Value="Left"/>
        <Setter Property="Cursor" Value="Hand"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="Button">
                    <Border Background="{TemplateBinding Background}"
                            BorderThickness="{TemplateBinding BorderThickness}"
                            BorderBrush="{TemplateBinding BorderBrush}"
                            Padding="{TemplateBinding Padding}">
                        <ContentPresenter HorizontalAlignment="{TemplateBinding HorizontalContentAlignment}"
                                        VerticalAlignment="{TemplateBinding VerticalContentAlignment}"/>
                    </Border>
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsMouseOver" Value="True">
                            <Setter Property="Background" Value="#E0E0E0"/>
                        </Trigger>
                        <Trigger Property="IsPressed" Value="True">
                            <Setter Property="Background" Value="#D0D0D0"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>
    
    <!-- Default Button Style -->
    <Style TargetType="Button">
        <Setter Property="Background" Value="{StaticResource PrimaryBrush}"/>
        <Setter Property="Foreground" Value="White"/>
        <Setter Property="Padding" Value="15,8"/>
        <Setter Property="Cursor" Value="Hand"/>
    </Style>
    
</ResourceDictionary>
```

## Features

This WPF template includes:

- ✅ MVVM pattern with CommunityToolkit.Mvvm
- ✅ Dependency injection
- ✅ Navigation service for view management
- ✅ Dialog service for user interactions
- ✅ Observable properties and relay commands
- ✅ Layered architecture
- ✅ Resource dictionaries for styling
- ✅ Value converters
- ✅ ListView with data binding
- ✅ Loading indicators
- ✅ Search functionality
- ✅ Clean separation of concerns

## Next Steps

1. Add more views and ViewModels
2. Implement data persistence with Entity Framework Core
3. Add custom themes/styles
4. Implement user authentication
5. Add localization support
6. Create custom controls
7. Add unit tests for ViewModels

## License

[Your License Here]
