# XAML Best Practices and Naming Conventions

This document outlines XAML coding standards and best practices for the UniGetUI project.

## Table of Contents

1. [General Principles](#general-principles)
2. [Naming Conventions](#naming-conventions)
3. [Formatting and Structure](#formatting-and-structure)
4. [Resource Management](#resource-management)
5. [Data Binding](#data-binding)
6. [Styling and Templates](#styling-and-templates)
7. [Performance Best Practices](#performance-best-practices)
8. [Anti-Patterns to Avoid](#anti-patterns-to-avoid)
9. [Common Pitfalls](#common-pitfalls)

## General Principles

### File Organization

- One control or page per XAML file
- Keep XAML files focused and maintainable
- Separate concerns: XAML for UI structure, code-behind for event handling, ViewModel for business logic

### Readability

- Use proper indentation (4 spaces)
- Group related properties together
- Add comments for complex layouts
- Use meaningful names for resources and controls

## Naming Conventions

### Elements with x:Name

Use PascalCase for named elements with meaningful, descriptive names:

```xaml
<!-- ✅ Good -->
<Button x:Name="InstallButton" Content="Install" />
<TextBlock x:Name="PackageNameText" Text="Package Name" />
<ListView x:Name="PackageListView" />

<!-- ❌ Bad -->
<Button x:Name="btn1" Content="Install" />
<TextBlock x:Name="txtPackageName" Text="Package Name" />
<ListView x:Name="lv" />
```

### Naming Patterns

- **Buttons**: `[Action]Button` (e.g., `InstallButton`, `CancelButton`)
- **TextBlocks**: `[Purpose]Text` or `[Purpose]TextBlock` (e.g., `StatusText`, `ErrorMessageText`)
- **TextBox**: `[Purpose]TextBox` (e.g., `SearchTextBox`, `PackageNameTextBox`)
- **ListView/GridView**: `[Content]ListView` or `[Content]GridView` (e.g., `PackageListView`, `UpdatesGridView`)
- **Panels**: `[Content]Panel` or `[Content]Container` (e.g., `HeaderPanel`, `ContentContainer`)

### Resources

Use PascalCase with descriptive names:

```xaml
<!-- ✅ Good -->
<SolidColorBrush x:Key="PrimaryAccentBrush" Color="#0078D4" />
<Style x:Key="LargeButtonStyle" TargetType="Button">
    <!-- Style definition -->
</Style>
<DataTemplate x:Key="PackageItemTemplate">
    <!-- Template definition -->
</DataTemplate>

<!-- ❌ Bad -->
<SolidColorBrush x:Key="brush1" Color="#0078D4" />
<Style x:Key="btnStyle" TargetType="Button" />
<DataTemplate x:Key="template1" />
```

## Formatting and Structure

### Indentation and Layout

Use consistent indentation (4 spaces) to reflect the visual hierarchy:

```xaml
<!-- ✅ Good -->
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto" />
        <RowDefinition Height="*" />
    </Grid.RowDefinitions>

    <StackPanel Grid.Row="0" Orientation="Horizontal" Spacing="8">
        <TextBlock Text="Package Manager" Style="{StaticResource TitleTextBlockStyle}" />
        <Button Content="Refresh" Click="OnRefreshClick" />
    </StackPanel>

    <ListView Grid.Row="1" ItemsSource="{x:Bind Packages}">
        <ListView.ItemTemplate>
            <DataTemplate x:DataType="local:Package">
                <TextBlock Text="{x:Bind Name}" />
            </DataTemplate>
        </ListView.ItemTemplate>
    </ListView>
</Grid>

<!-- ❌ Bad -->
<Grid>
<Grid.RowDefinitions>
<RowDefinition Height="Auto" />
<RowDefinition Height="*" />
</Grid.RowDefinitions>
<StackPanel Grid.Row="0" Orientation="Horizontal" Spacing="8"><TextBlock Text="Package Manager" Style="{StaticResource TitleTextBlockStyle}" /><Button Content="Refresh" Click="OnRefreshClick" /></StackPanel>
<ListView Grid.Row="1" ItemsSource="{x:Bind Packages}">
<ListView.ItemTemplate>
<DataTemplate x:DataType="local:Package"><TextBlock Text="{x:Bind Name}" /></DataTemplate>
</ListView.ItemTemplate>
</ListView>
</Grid>
```

### Attribute Ordering

Order attributes consistently for better readability:

1. `x:Name`
2. `x:Key`
3. Layout properties (Grid.Row, Grid.Column, etc.)
4. Size properties (Width, Height, MinWidth, etc.)
5. Alignment properties (HorizontalAlignment, VerticalAlignment)
6. Margin and Padding
7. Visual properties (Background, Foreground, etc.)
8. Content properties (Content, Text, ItemsSource)
9. Behavior properties (IsEnabled, Visibility)
10. Event handlers (Click, SelectionChanged, etc.)
11. Style and Template
12. Other properties

```xaml
<!-- ✅ Good -->
<Button
    x:Name="InstallButton"
    Grid.Row="1"
    Grid.Column="0"
    Width="120"
    Height="40"
    HorizontalAlignment="Right"
    Margin="0,8,8,0"
    Content="Install"
    IsEnabled="{x:Bind CanInstall, Mode=OneWay}"
    Click="OnInstallClick"
    Style="{StaticResource AccentButtonStyle}" />

<!-- ❌ Bad -->
<Button
    Click="OnInstallClick"
    Content="Install"
    x:Name="InstallButton"
    Style="{StaticResource AccentButtonStyle}"
    Height="40"
    Grid.Row="1"
    Width="120"
    IsEnabled="{x:Bind CanInstall, Mode=OneWay}"
    Margin="0,8,8,0"
    Grid.Column="0"
    HorizontalAlignment="Right" />
```

### Self-Closing Tags

Use self-closing tags for elements without children:

```xaml
<!-- ✅ Good -->
<TextBlock Text="Hello World" />
<Rectangle Fill="Red" Width="100" Height="100" />

<!-- ❌ Bad -->
<TextBlock Text="Hello World"></TextBlock>
<Rectangle Fill="Red" Width="100" Height="100"></Rectangle>
```

### Line Breaks for Multiple Attributes

Break long attribute lists across multiple lines:

```xaml
<!-- ✅ Good -->
<Button
    Content="Install Package"
    IsEnabled="{x:Bind ViewModel.CanInstall, Mode=OneWay}"
    Command="{x:Bind ViewModel.InstallCommand}"
    Style="{StaticResource AccentButtonStyle}" />

<!-- ❌ Bad -->
<Button Content="Install Package" IsEnabled="{x:Bind ViewModel.CanInstall, Mode=OneWay}" Command="{x:Bind ViewModel.InstallCommand}" Style="{StaticResource AccentButtonStyle}" />
```

## Resource Management

### Resource Dictionaries

Organize resources in logical groups:

```xaml
<!-- ✅ Good -->
<ResourceDictionary>
    <!-- Color Resources -->
    <SolidColorBrush x:Key="PrimaryBrush" Color="#0078D4" />
    <SolidColorBrush x:Key="SecondaryBrush" Color="#106EBE" />

    <!-- Text Styles -->
    <Style x:Key="TitleTextStyle" TargetType="TextBlock">
        <Setter Property="FontSize" Value="24" />
        <Setter Property="FontWeight" Value="SemiBold" />
    </Style>

    <!-- Button Styles -->
    <Style x:Key="PrimaryButtonStyle" TargetType="Button">
        <Setter Property="Background" Value="{StaticResource PrimaryBrush}" />
        <Setter Property="Foreground" Value="White" />
    </Style>
</ResourceDictionary>
```

### Merged Dictionaries

Use merged dictionaries for better organization:

```xaml
<!-- ✅ Good -->
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="Themes/Colors.xaml" />
            <ResourceDictionary Source="Themes/Styles.xaml" />
            <ResourceDictionary Source="Themes/Templates.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

### StaticResource vs ThemeResource

- Use `StaticResource` for resources that don't change
- Use `ThemeResource` for theme-aware resources that can change at runtime

```xaml
<!-- ✅ Good -->
<Button Background="{ThemeResource ButtonBackgroundBrush}" />
<TextBlock Foreground="{ThemeResource TextFillColorPrimaryBrush}" />

<!-- Use StaticResource for app-specific resources -->
<Border Background="{StaticResource AppPrimaryBrush}" />
```

## Data Binding

### x:Bind vs Binding

Prefer `x:Bind` over `Binding` for better performance and compile-time validation:

```xaml
<!-- ✅ Good - x:Bind -->
<TextBlock Text="{x:Bind ViewModel.PackageName, Mode=OneWay}" />
<ListView ItemsSource="{x:Bind ViewModel.Packages}" />

<!-- ✅ Acceptable - Binding (when x:Bind is not possible) -->
<TextBlock Text="{Binding PackageName, Mode=OneWay}" />
```

### Binding Modes

Be explicit about binding modes:

```xaml
<!-- ✅ Good -->
<TextBlock Text="{x:Bind ViewModel.Status, Mode=OneWay}" />
<TextBox Text="{x:Bind ViewModel.SearchQuery, Mode=TwoWay}" />
<CheckBox IsChecked="{x:Bind ViewModel.IsEnabled, Mode=TwoWay}" />

<!-- ❌ Bad - Relying on default mode -->
<TextBlock Text="{x:Bind ViewModel.Status}" />
```

### DataTemplate with x:DataType

Always specify `x:DataType` in DataTemplates for type safety:

```xaml
<!-- ✅ Good -->
<ListView ItemsSource="{x:Bind Packages}">
    <ListView.ItemTemplate>
        <DataTemplate x:DataType="local:Package">
            <StackPanel>
                <TextBlock Text="{x:Bind Name}" FontWeight="SemiBold" />
                <TextBlock Text="{x:Bind Version}" Foreground="Gray" />
            </StackPanel>
        </DataTemplate>
    </ListView.ItemTemplate>
</ListView>

<!-- ❌ Bad - No x:DataType -->
<ListView ItemsSource="{x:Bind Packages}">
    <ListView.ItemTemplate>
        <DataTemplate>
            <StackPanel>
                <TextBlock Text="{Binding Name}" />
                <TextBlock Text="{Binding Version}" />
            </StackPanel>
        </DataTemplate>
    </ListView.ItemTemplate>
</ListView>
```

### Null Coalescing in Bindings

Use fallback values for null or missing data:

```xaml
<!-- ✅ Good -->
<TextBlock Text="{x:Bind Package.Description, Mode=OneWay, FallbackValue='No description available'}" />
<Image Source="{x:Bind Package.IconUrl, Mode=OneWay, TargetNullValue='/Assets/DefaultIcon.png'}" />
```

## Styling and Templates

### Style Inheritance

Use BasedOn for style inheritance:

```xaml
<!-- ✅ Good -->
<Style x:Key="BaseButtonStyle" TargetType="Button">
    <Setter Property="Padding" Value="12,8" />
    <Setter Property="CornerRadius" Value="4" />
</Style>

<Style x:Key="PrimaryButtonStyle" TargetType="Button" BasedOn="{StaticResource BaseButtonStyle}">
    <Setter Property="Background" Value="{ThemeResource AccentFillColorDefaultBrush}" />
    <Setter Property="Foreground" Value="White" />
</Style>
```

### ControlTemplates

Keep templates focused and maintainable:

```xaml
<!-- ✅ Good -->
<ControlTemplate x:Key="CustomButtonTemplate" TargetType="Button">
    <Grid>
        <VisualStateManager.VisualStateGroups>
            <VisualStateGroup x:Name="CommonStates">
                <VisualState x:Name="Normal" />
                <VisualState x:Name="PointerOver">
                    <VisualState.Setters>
                        <Setter Target="RootBorder.Background" Value="{ThemeResource ButtonBackgroundPointerOver}" />
                    </VisualState.Setters>
                </VisualState>
                <VisualState x:Name="Pressed">
                    <VisualState.Setters>
                        <Setter Target="RootBorder.Background" Value="{ThemeResource ButtonBackgroundPressed}" />
                    </VisualState.Setters>
                </VisualState>
            </VisualStateGroup>
        </VisualStateManager.VisualStateGroups>

        <Border x:Name="RootBorder" Background="{TemplateBinding Background}">
            <ContentPresenter
                HorizontalAlignment="{TemplateBinding HorizontalContentAlignment}"
                VerticalAlignment="{TemplateBinding VerticalContentAlignment}"
                Content="{TemplateBinding Content}" />
        </Border>
    </Grid>
</ControlTemplate>
```

### Implicit vs Explicit Styles

- Use implicit styles for consistent theming
- Use explicit styles for specific variations

```xaml
<!-- ✅ Good - Implicit style (applies to all buttons without a style) -->
<Style TargetType="Button">
    <Setter Property="Padding" Value="12,8" />
</Style>

<!-- ✅ Good - Explicit style (requires x:Key) -->
<Style x:Key="AccentButtonStyle" TargetType="Button">
    <Setter Property="Background" Value="{ThemeResource AccentFillColorDefaultBrush}" />
</Style>
```

## Performance Best Practices

### Virtualization

Use virtualized controls for large collections:

```xaml
<!-- ✅ Good -->
<ListView ItemsSource="{x:Bind LargePackageList}">
    <!-- ListView virtualizes by default -->
</ListView>

<ItemsRepeater ItemsSource="{x:Bind LargePackageList}">
    <!-- ItemsRepeater virtualizes by default -->
</ItemsRepeater>

<!-- ❌ Bad for large lists -->
<StackPanel>
    <ItemsControl ItemsSource="{x:Bind LargePackageList}">
        <!-- ItemsControl doesn't virtualize -->
    </ItemsControl>
</StackPanel>
```

### Image Loading

Use appropriate image loading strategies:

```xaml
<!-- ✅ Good - Async loading with thumbnail -->
<Image Source="{x:Bind Package.IconUrl}" Stretch="Uniform">
    <Image.DecodePixelWidth>200</Image.DecodePixelWidth>
    <Image.DecodePixelHeight>200</Image.DecodePixelHeight>
</Image>

<!-- ✅ Good - Using BitmapImage for more control -->
<Image>
    <Image.Source>
        <BitmapImage
            UriSource="{x:Bind Package.IconUrl}"
            DecodePixelWidth="200"
            DecodePixelHeight="200" />
    </Image.Source>
</Image>
```

### Reduce Visual Tree Depth

Keep visual tree shallow for better performance:

```xaml
<!-- ✅ Good -->
<Grid>
    <TextBlock Text="{x:Bind Title}" />
</Grid>

<!-- ❌ Bad - Unnecessary nesting -->
<Grid>
    <Border>
        <Grid>
            <StackPanel>
                <TextBlock Text="{x:Bind Title}" />
            </StackPanel>
        </Grid>
    </Border>
</Grid>
```

### x:Load for Conditional UI

Use `x:Load` instead of `Visibility` for performance:

```xaml
<!-- ✅ Good - Element not created if false -->
<ProgressRing x:Load="{x:Bind ViewModel.IsLoading, Mode=OneWay}" />

<!-- ❌ Bad - Element always created, just hidden -->
<ProgressRing Visibility="{x:Bind ViewModel.IsLoading, Mode=OneWay, Converter={StaticResource BoolToVisibilityConverter}}" />
```

### Defer Loading

Use x:DeferLoadStrategy for initially hidden content:

```xaml
<!-- ✅ Good -->
<Grid x:Name="DetailPanel" x:DeferLoadStrategy="Lazy" Visibility="Collapsed">
    <!-- Complex content loaded only when needed -->
</Grid>
```

## Anti-Patterns to Avoid

### 1. Code-Behind Logic

```xaml
<!-- ❌ Bad - Business logic in code-behind -->
<Button Content="Install" Click="OnInstallClick" />

<!-- Code-behind -->
private async void OnInstallClick(object sender, RoutedEventArgs e)
{
    // Database calls, business logic, etc.
}

<!-- ✅ Good - Use commands and MVVM -->
<Button Content="Install" Command="{x:Bind ViewModel.InstallCommand}" />
```

### 2. Hard-Coded Values

```xaml
<!-- ❌ Bad -->
<TextBlock Text="Install Package" Foreground="#0078D4" FontSize="16" />

<!-- ✅ Good -->
<TextBlock
    Text="{x:Bind LocalizationService.GetString('InstallPackage')}"
    Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"
    Style="{StaticResource BodyTextBlockStyle}" />
```

### 3. Excessive Nesting

```xaml
<!-- ❌ Bad -->
<Grid>
    <Border>
        <Grid>
            <StackPanel>
                <Border>
                    <Grid>
                        <TextBlock Text="Hello" />
                    </Grid>
                </Border>
            </StackPanel>
        </Grid>
    </Border>
</Grid>

<!-- ✅ Good -->
<Border>
    <TextBlock Text="Hello" />
</Border>
```

### 4. Mixing Data and UI

```xaml
<!-- ❌ Bad - Data mixed with UI -->
<ListView>
    <ListViewItem Content="Package 1" />
    <ListViewItem Content="Package 2" />
    <ListViewItem Content="Package 3" />
</ListView>

<!-- ✅ Good - Data binding -->
<ListView ItemsSource="{x:Bind Packages}" />
```

### 5. Not Using Attached Properties

```xaml
<!-- ❌ Bad - Repeating properties -->
<StackPanel>
    <Button Margin="4" />
    <Button Margin="4" />
    <Button Margin="4" />
</StackPanel>

<!-- ✅ Good - Using Spacing -->
<StackPanel Spacing="4">
    <Button />
    <Button />
    <Button />
</StackPanel>
```

## Common Pitfalls

### 1. Not Setting DataContext

```xaml
<!-- ❌ Bad - Bindings won't work -->
<UserControl>
    <TextBlock Text="{Binding Title}" />
</UserControl>

<!-- ✅ Good - DataContext set in code-behind or XAML -->
<UserControl>
    <UserControl.DataContext>
        <local:MyViewModel />
    </UserControl.DataContext>
    <TextBlock Text="{Binding Title}" />
</UserControl>
```

### 2. Incorrect Grid Definitions

```xaml
<!-- ❌ Bad - Missing row/column definitions -->
<Grid>
    <Button Grid.Row="0" />
    <Button Grid.Row="1" />
</Grid>

<!-- ✅ Good -->
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto" />
        <RowDefinition Height="*" />
    </Grid.RowDefinitions>
    <Button Grid.Row="0" />
    <Button Grid.Row="1" />
</Grid>
```

### 3. Not Handling Null in Bindings

```xaml
<!-- ❌ Bad - Crash if Package is null -->
<TextBlock Text="{x:Bind Package.Name}" />

<!-- ✅ Good - Fallback value -->
<TextBlock Text="{x:Bind Package.Name, FallbackValue='N/A'}" />

<!-- ✅ Also good - Null conditional -->
<TextBlock Text="{x:Bind Package.Name, Mode=OneWay, TargetNullValue='N/A'}" />
```

### 4. Overusing UpdateSourceTrigger

```xaml
<!-- ❌ Bad - Unnecessary performance hit -->
<TextBox Text="{Binding SearchQuery, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />

<!-- ✅ Good - Let it update on LostFocus (default) -->
<TextBox Text="{Binding SearchQuery, Mode=TwoWay}" />

<!-- ✅ Use PropertyChanged only when needed for real-time search -->
<TextBox Text="{x:Bind ViewModel.SearchQuery, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />
```

### 5. Not Cleaning Up Event Handlers

```csharp
// ❌ Bad - Memory leak
public MyControl()
{
    InitializeComponent();
    SomeService.DataChanged += OnDataChanged;
}

// ✅ Good - Unsubscribe
public MyControl()
{
    InitializeComponent();
    SomeService.DataChanged += OnDataChanged;
    Unloaded += (s, e) => SomeService.DataChanged -= OnDataChanged;
}
```

## Layout Guidelines

### Use Appropriate Panels

- **Grid**: For complex layouts with rows and columns
- **StackPanel**: For simple vertical or horizontal stacks
- **RelativePanel**: For responsive layouts with relative positioning
- **Canvas**: Only for absolute positioning (rare cases)

```xaml
<!-- ✅ Good - Grid for form layout -->
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto" />
        <RowDefinition Height="Auto" />
    </Grid.RowDefinitions>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto" />
        <ColumnDefinition Width="*" />
    </Grid.ColumnDefinitions>

    <TextBlock Grid.Row="0" Grid.Column="0" Text="Name:" />
    <TextBox Grid.Row="0" Grid.Column="1" />

    <TextBlock Grid.Row="1" Grid.Column="0" Text="Version:" />
    <TextBox Grid.Row="1" Grid.Column="1" />
</Grid>
```

## Accessibility

### Always Include Automation Properties

```xaml
<!-- ✅ Good -->
<Button
    Content="Install"
    AutomationProperties.Name="Install Package"
    AutomationProperties.HelpText="Installs the selected package" />

<Image
    Source="icon.png"
    AutomationProperties.Name="Package Icon"
    AutomationProperties.Role="Image" />
```

## Additional Resources

- [XAML Overview (Microsoft)](https://docs.microsoft.com/en-us/windows/uwp/xaml-platform/xaml-overview)
- [Data Binding in Depth](https://docs.microsoft.com/en-us/windows/uwp/data-binding/data-binding-in-depth)
- [Performance Best Practices](https://docs.microsoft.com/en-us/windows/uwp/debug-test-perf/performance-and-xaml-ui)
