# UI Component Library

Reusable UI components for WPF and WinUI 3 applications.

## Components

### 1. PackageCard Control
A reusable card component for displaying package information.

```xml
<controls:PackageCard
    PackageName="{Binding Name}"
    PackageVersion="{Binding Version}"
    PackageDescription="{Binding Description}"
    IsInstalled="{Binding IsInstalled}"
    InstallCommand="{Binding InstallCommand}"/>
```

### 2. SearchBox Control
Enhanced search box with debouncing and suggestions.

```xml
<controls:SearchBox
    SearchText="{Binding SearchQuery, Mode=TwoWay}"
    Placeholder="Search packages..."
    SearchCommand="{Binding SearchCommand}"
    DebounceMs="300"/>
```

### 3. ProgressRing Control
Loading indicator with customizable styles.

```xml
<controls:ProgressRing 
    IsActive="{Binding IsLoading}"
    Message="Loading packages..."/>
```

## Attached Properties

### AutoCompleteBox
```csharp
public static class AutoCompleteBehavior
{
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.RegisterAttached(
            "ItemsSource",
            typeof(IEnumerable),
            typeof(AutoCompleteBehavior),
            new PropertyMetadata(null, OnItemsSourceChanged));
}
```

## Value Converters

### BoolToVisibilityConverter
```csharp
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, 
        object parameter, CultureInfo culture)
    {
        return (bool)value ? Visibility.Visible : Visibility.Collapsed;
    }
}
```

### FileSizeConverter
```csharp
public class FileSizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, 
        object parameter, CultureInfo culture)
    {
        if (value is long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
        return "0 B";
    }
}
```

## Behaviors

### ScrollIntoViewBehavior
```csharp
public class ScrollIntoViewBehavior : Behavior<ListBox>
{
    protected override void OnAttached()
    {
        AssociatedObject.SelectionChanged += OnSelectionChanged;
    }
    
    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0)
        {
            AssociatedObject.ScrollIntoView(e.AddedItems[0]);
        }
    }
}
```

## Styles and Templates

### Modern Button Style
```xml
<Style x:Key="AccentButton" TargetType="Button">
    <Setter Property="Background" Value="{StaticResource AccentBrush}"/>
    <Setter Property="Foreground" Value="White"/>
    <Setter Property="Padding" Value="16,8"/>
    <Setter Property="BorderThickness" Value="0"/>
    <Setter Property="CornerRadius" Value="4"/>
    <Setter Property="Cursor" Value="Hand"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="Button">
                <Border Background="{TemplateBinding Background}"
                        CornerRadius="4"
                        Padding="{TemplateBinding Padding}">
                    <ContentPresenter HorizontalAlignment="Center"
                                    VerticalAlignment="Center"/>
                </Border>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

## Theme Support

### Light and Dark Themes
```csharp
public class ThemeManager
{
    public static void SetTheme(Theme theme)
    {
        var resources = Application.Current.Resources;
        var themeDict = theme switch
        {
            Theme.Light => new ResourceDictionary 
            { 
                Source = new Uri("Themes/Light.xaml", UriKind.Relative) 
            },
            Theme.Dark => new ResourceDictionary 
            { 
                Source = new Uri("Themes/Dark.xaml", UriKind.Relative) 
            },
            _ => throw new ArgumentException("Invalid theme")
        };
        
        resources.MergedDictionaries.Clear();
        resources.MergedDictionaries.Add(themeDict);
    }
}
```

## Usage Examples

```xml
<Window xmlns:controls="clr-namespace:UIComponents.Controls">
    <Window.Resources>
        <converters:BoolToVisibilityConverter x:Key="BoolToVis"/>
        <converters:FileSizeConverter x:Key="FileSize"/>
    </Window.Resources>
    
    <Grid>
        <controls:SearchBox SearchText="{Binding Query}"/>
        
        <ListView ItemsSource="{Binding Packages}">
            <ListView.ItemTemplate>
                <DataTemplate>
                    <controls:PackageCard
                        PackageName="{Binding Name}"
                        PackageSize="{Binding Size, Converter={StaticResource FileSize}}"
                        Visibility="{Binding IsVisible, Converter={StaticResource BoolToVis}}"/>
                </DataTemplate>
            </ListView.ItemTemplate>
        </ListView>
    </Grid>
</Window>
```

## Building

```bash
cd ui-component-library
dotnet build
```

## License

Part of the UniGetUI project examples.
