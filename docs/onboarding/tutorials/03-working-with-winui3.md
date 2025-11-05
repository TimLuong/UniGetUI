# Tutorial 3: Working with WinUI 3

**Difficulty:** Intermediate  
**Time:** 2-3 hours  
**Prerequisites:** Basic XAML knowledge, completed Tutorial 1

## 📖 Overview

This tutorial teaches you how to work with WinUI 3, the modern Windows UI framework used in UniGetUI. You'll learn:
- WinUI 3 fundamentals and controls
- XAML data binding
- Event handling patterns
- Creating custom controls
- UI/Logic separation (MVVM-lite)

## 🎯 Learning Objectives

By the end of this tutorial, you'll be able to:
- Create and modify WinUI 3 XAML pages
- Use common WinUI 3 controls
- Implement data binding
- Handle user interactions
- Create reusable UI components
- Follow UniGetUI's UI patterns

## 📚 WinUI 3 Basics

### What is WinUI 3?

WinUI 3 is Microsoft's modern native UI framework for Windows apps:
- **Modern design:** Fluent Design System
- **Native performance:** Built on Windows App SDK
- **Rich controls:** Buttons, lists, navigation, etc.
- **Windows 10/11 only:** Not cross-platform

### XAML Structure

```xml
<Page
    x:Class="UniGetUI.Pages.MyPage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <!-- Your UI elements here -->
    <StackPanel>
        <TextBlock Text="Hello, World!" />
        <Button Content="Click Me" Click="Button_Click" />
    </StackPanel>
</Page>
```

**Key parts:**
- `x:Class`: Links XAML to code-behind class
- `xmlns`: XML namespaces for WinUI controls
- Elements: WinUI controls (Button, TextBlock, etc.)
- Attributes: Properties (Text, Content) and events (Click)

## 📋 Step 1: Common WinUI 3 Controls

### Layout Controls

**StackPanel:** Arranges children vertically or horizontally
```xml
<StackPanel Orientation="Vertical" Spacing="8">
    <TextBlock Text="Item 1" />
    <TextBlock Text="Item 2" />
    <TextBlock Text="Item 3" />
</StackPanel>
```

**Grid:** Flexible grid layout with rows and columns
```xml
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto" />
        <RowDefinition Height="*" />
    </Grid.RowDefinitions>
    
    <TextBlock Grid.Row="0" Text="Header" />
    <ListView Grid.Row="1" />
</Grid>
```

**ScrollViewer:** Makes content scrollable
```xml
<ScrollViewer>
    <StackPanel>
        <!-- Long content that needs scrolling -->
    </StackPanel>
</ScrollViewer>
```

### Input Controls

**TextBox:** Single or multi-line text input
```xml
<TextBox PlaceholderText="Enter package name..." />
```

**Button:** Click handler
```xml
<Button Content="Install" Click="InstallButton_Click" />
```

**ToggleSwitch:** On/off toggle
```xml
<ToggleSwitch Header="Enable Feature" Toggled="Toggle_Toggled" />
```

**ComboBox:** Dropdown selection
```xml
<ComboBox Header="Select Option">
    <ComboBoxItem Content="Option 1" />
    <ComboBoxItem Content="Option 2" />
</ComboBox>
```

### Display Controls

**TextBlock:** Display text
```xml
<TextBlock Text="Package Manager" FontSize="24" />
```

**ListView:** List of items
```xml
<ListView ItemsSource="{x:Bind Packages}">
    <ListView.ItemTemplate>
        <DataTemplate x:DataType="local:Package">
            <TextBlock Text="{x:Bind Name}" />
        </DataTemplate>
    </ListView.ItemTemplate>
</ListView>
```

**ProgressRing:** Loading indicator
```xml
<ProgressRing IsActive="True" />
```

## 📋 Step 2: Data Binding

### Why Data Binding?

Instead of manually updating UI:
```csharp
// Bad: Manual updates
packageNameTextBlock.Text = package.Name;
packageVersionTextBlock.Text = package.Version;
```

Use binding:
```xml
<!-- Good: Automatic updates -->
<TextBlock Text="{x:Bind Package.Name}" />
<TextBlock Text="{x:Bind Package.Version}" />
```

### x:Bind vs Binding

**x:Bind (preferred):**
- Compile-time checked
- Better performance
- Type-safe

```xml
<TextBlock Text="{x:Bind PackageName}" />
```

**Binding (legacy):**
- Runtime evaluated
- More flexible but slower

```xml
<TextBlock Text="{Binding PackageName}" />
```

### One-Way vs Two-Way Binding

**One-Way:** Data flows from source to UI
```xml
<TextBlock Text="{x:Bind PackageName, Mode=OneWay}" />
```

**Two-Way:** Data flows both directions (for input controls)
```xml
<TextBox Text="{x:Bind SearchQuery, Mode=TwoWay}" />
```

### Example: Binding to a Property

Code-behind:
```csharp
public sealed partial class MyPage : Page
{
    public string PackageName { get; set; } = "Example Package";
    
    public MyPage()
    {
        InitializeComponent();
    }
}
```

XAML:
```xml
<TextBlock Text="{x:Bind PackageName}" />
```

## 📋 Step 3: Event Handling

### Click Events

```xml
<Button Content="Search" Click="SearchButton_Click" />
```

Code-behind:
```csharp
private void SearchButton_Click(object sender, RoutedEventArgs e)
{
    // Handle button click
    string query = searchTextBox.Text;
    PerformSearch(query);
}
```

### Toggle Events

```xml
<ToggleSwitch Toggled="FeatureToggle_Toggled" />
```

Code-behind:
```csharp
private void FeatureToggle_Toggled(object sender, RoutedEventArgs e)
{
    var toggle = (ToggleSwitch)sender;
    bool isOn = toggle.IsOn;
    Settings.Set("Feature", isOn);
}
```

### Selection Changed

```xml
<ComboBox SelectionChanged="ComboBox_SelectionChanged">
    <ComboBoxItem Content="Option 1" Tag="opt1" />
    <ComboBoxItem Content="Option 2" Tag="opt2" />
</ComboBox>
```

Code-behind:
```csharp
private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    if (sender is ComboBox comboBox && 
        comboBox.SelectedItem is ComboBoxItem item)
    {
        string selectedValue = item.Tag.ToString();
        // Handle selection
    }
}
```

## 📋 Step 4: Creating a Custom Control

### When to Create Custom Controls

Create custom controls when you:
- Need to reuse UI components
- Have complex UI logic
- Want to encapsulate behavior

### Example: Custom Package Card

**PackageCard.xaml:**
```xml
<UserControl
    x:Class="UniGetUI.Controls.PackageCard"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <Grid Padding="16" BorderBrush="Gray" BorderThickness="1" CornerRadius="8">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="Auto" />
            <ColumnDefinition Width="*" />
            <ColumnDefinition Width="Auto" />
        </Grid.ColumnDefinitions>
        
        <!-- Icon -->
        <Image Grid.Column="0" Width="48" Height="48" 
               Source="{x:Bind IconSource, Mode=OneWay}" />
        
        <!-- Package Info -->
        <StackPanel Grid.Column="1" Margin="16,0,0,0">
            <TextBlock Text="{x:Bind PackageName, Mode=OneWay}" 
                       FontWeight="Bold" />
            <TextBlock Text="{x:Bind PackageVersion, Mode=OneWay}" 
                       Foreground="Gray" />
        </StackPanel>
        
        <!-- Install Button -->
        <Button Grid.Column="2" Content="Install" 
                Click="InstallButton_Click" />
    </Grid>
</UserControl>
```

**PackageCard.xaml.cs:**
```csharp
public sealed partial class PackageCard : UserControl
{
    public static readonly DependencyProperty PackageNameProperty =
        DependencyProperty.Register(
            nameof(PackageName),
            typeof(string),
            typeof(PackageCard),
            new PropertyMetadata(string.Empty));
    
    public string PackageName
    {
        get => (string)GetValue(PackageNameProperty);
        set => SetValue(PackageNameProperty, value);
    }
    
    public static readonly DependencyProperty PackageVersionProperty =
        DependencyProperty.Register(
            nameof(PackageVersion),
            typeof(string),
            typeof(PackageCard),
            new PropertyMetadata(string.Empty));
    
    public string PackageVersion
    {
        get => (string)GetValue(PackageVersionProperty);
        set => SetValue(PackageVersionProperty, value);
    }
    
    public event EventHandler<RoutedEventArgs> InstallClicked;
    
    public PackageCard()
    {
        InitializeComponent();
    }
    
    private void InstallButton_Click(object sender, RoutedEventArgs e)
    {
        InstallClicked?.Invoke(this, e);
    }
}
```

**Usage:**
```xml
<local:PackageCard 
    PackageName="Visual Studio Code"
    PackageVersion="1.85.0"
    InstallClicked="PackageCard_InstallClicked" />
```

## 📋 Step 5: CommunityToolkit.WinUI

### SettingsCard

UniGetUI uses CommunityToolkit for enhanced controls:

```xml
xmlns:controls="using:CommunityToolkit.WinUI.Controls"

<controls:SettingsCard 
    Header="Dark Mode"
    Description="Switch to dark theme">
    <ToggleSwitch IsOn="{x:Bind IsDarkMode, Mode=TwoWay}" />
</controls:SettingsCard>
```

### SettingsExpander

For grouped settings:

```xml
<controls:SettingsExpander Header="Advanced Options">
    <controls:SettingsExpander.Items>
        <controls:SettingsCard Header="Option 1">
            <ToggleSwitch />
        </controls:SettingsCard>
        <controls:SettingsCard Header="Option 2">
            <ComboBox />
        </controls:SettingsCard>
    </controls:SettingsExpander.Items>
</controls:SettingsExpander>
```

## 📋 Step 6: Navigation

### NavigationView

UniGetUI uses NavigationView for app navigation:

```xml
<NavigationView>
    <NavigationView.MenuItems>
        <NavigationViewItem Content="Home" Tag="home" />
        <NavigationViewItem Content="Settings" Tag="settings" />
    </NavigationView.MenuItems>
    
    <Frame x:Name="contentFrame" />
</NavigationView>
```

Code-behind:
```csharp
private void NavigationView_SelectionChanged(
    NavigationView sender, 
    NavigationViewSelectionChangedEventArgs args)
{
    if (args.SelectedItem is NavigationViewItem item)
    {
        string tag = item.Tag.ToString();
        
        switch (tag)
        {
            case "home":
                contentFrame.Navigate(typeof(HomePage));
                break;
            case "settings":
                contentFrame.Navigate(typeof(SettingsPage));
                break;
        }
    }
}
```

## 📋 Step 7: Styling and Theming

### Resources

Define reusable styles:

```xml
<Page.Resources>
    <Style x:Key="HeaderTextStyle" TargetType="TextBlock">
        <Setter Property="FontSize" Value="24" />
        <Setter Property="FontWeight" Value="Bold" />
        <Setter Property="Margin" Value="0,0,0,16" />
    </Style>
</Page.Resources>

<TextBlock Text="My Header" Style="{StaticResource HeaderTextStyle}" />
```

### Theme-Aware Colors

```xml
<TextBlock Foreground="{ThemeResource TextFillColorPrimaryBrush}" />
```

## 📋 Step 8: Hands-On Exercise

### Exercise: Create a Package Search Page

Create a new page with:
1. TextBox for search input
2. Button to trigger search
3. ListView to display results
4. ProgressRing while searching

**Solution Template:**

```xml
<Page>
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
        </Grid.RowDefinitions>
        
        <!-- Search Bar -->
        <StackPanel Grid.Row="0" Orientation="Horizontal" Spacing="8">
            <TextBox x:Name="searchBox" PlaceholderText="Search packages..." 
                     Width="300" />
            <Button Content="Search" Click="SearchButton_Click" />
        </StackPanel>
        
        <!-- Results -->
        <Grid Grid.Row="1">
            <ListView x:Name="resultsListView" Visibility="Visible">
                <!-- Items will be added programmatically -->
            </ListView>
            
            <ProgressRing x:Name="loadingRing" 
                          IsActive="False" 
                          HorizontalAlignment="Center"
                          VerticalAlignment="Center" />
        </Grid>
    </Grid>
</Page>
```

Implement the code-behind to:
1. Show loading indicator when searching
2. Hide loading and show results when done
3. Handle empty results

## 🎓 What You Learned

Congratulations! You now understand:
- ✅ WinUI 3 fundamentals and common controls
- ✅ Data binding with x:Bind
- ✅ Event handling patterns
- ✅ Creating custom controls
- ✅ Using CommunityToolkit controls
- ✅ Navigation between pages
- ✅ Styling and theming

## 🚀 Next Steps

### Practice
1. Modify an existing page in UniGetUI
2. Create a new settings page
3. Add a custom control for a specific purpose

### Continue Learning
- [Microsoft WinUI 3 Docs](https://learn.microsoft.com/en-us/windows/apps/winui/)
- [CommunityToolkit Documentation](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/)
- [Tutorial 4: Adding a Package Manager](04-adding-package-manager.md)

## 📚 Resources

- [WinUI 3 Gallery](https://apps.microsoft.com/detail/9P3JFPWWDZRC) - Browse all controls
- [Fluent Design System](https://fluent2.microsoft.design/)
- [XAML Controls Guide](https://learn.microsoft.com/en-us/windows/apps/design/controls/)

## ✅ Self-Assessment

- [ ] Can you create a basic WinUI 3 page?
- [ ] Do you understand data binding?
- [ ] Can you handle user interactions?
- [ ] Can you create a custom control?
- [ ] Do you know when to use which control?

If yes, you're ready for more advanced topics! 🎉

Happy UI development! 🎨
