# Localization and Internationalization Guide

## Table of Contents
- [Overview](#overview)
- [Resource File Organization](#resource-file-organization)
- [Language Switching Mechanisms](#language-switching-mechanisms)
- [Date, Time, and Number Formatting](#date-time-and-number-formatting)
- [Right-to-Left (RTL) Language Support](#right-to-left-rtl-language-support)
- [Currency and Regional Settings](#currency-and-regional-settings)
- [String Externalization Best Practices](#string-externalization-best-practices)
- [Testing Localization](#testing-localization)

## Overview

This guide provides comprehensive guidelines for implementing localization (L10n) and internationalization (i18n) in Windows applications built with the CodingKit framework. Proper localization ensures your application can adapt to different languages, regions, and cultures seamlessly.

### Key Concepts

- **Internationalization (i18n)**: Designing and developing applications to support multiple languages and regions without requiring engineering changes.
- **Localization (L10n)**: Adapting an internationalized application to a specific language or region by translating text and adjusting regional settings.
- **Culture**: A combination of language and region (e.g., `en-US` for English (United States), `fr-FR` for French (France)).
- **Neutral Culture**: A culture that specifies a language but not a region (e.g., `en` for English, `fr` for French).

## Resource File Organization

### .resx File Structure

Resource files (`.resx`) are XML-based files that store localizable content such as strings, images, and other data. Proper organization is crucial for maintainability.

#### Recommended Directory Structure

```
YourProject/
├── Properties/
│   └── Resources.resx              # Default (neutral) resources
├── Resources/
│   ├── Strings/
│   │   ├── Strings.resx            # Default (English) strings
│   │   ├── Strings.fr.resx         # French strings
│   │   ├── Strings.de.resx         # German strings
│   │   ├── Strings.ja.resx         # Japanese strings
│   │   └── Strings.ar.resx         # Arabic strings
│   ├── Images/
│   │   ├── Images.resx             # Default images
│   │   └── Images.ja.resx          # Japanese-specific images
│   └── Dialogs/
│       ├── ErrorMessages.resx      # Default error messages
│       └── ErrorMessages.fr.resx   # French error messages
```

#### Naming Conventions

1. **Base Resource File**: `ResourceName.resx` (e.g., `Strings.resx`)
   - Contains default language resources (typically English)
   - Acts as the fallback when specific culture resources are not found

2. **Neutral Culture Resources**: `ResourceName.{culture}.resx` (e.g., `Strings.fr.resx`)
   - Two-letter ISO 639-1 language code
   - Used when no specific regional variant is available

3. **Specific Culture Resources**: `ResourceName.{culture-region}.resx` (e.g., `Strings.en-GB.resx`)
   - Combination of language and region codes
   - Provides region-specific translations

### Creating Resource Files

#### Using Visual Studio

1. Right-click on your project or folder
2. Select **Add > New Item**
3. Choose **Resources File (.resx)**
4. Name it appropriately (e.g., `Strings.resx`)
5. For culture-specific files, add the culture code before `.resx` (e.g., `Strings.fr.resx`)

#### Setting Resource File Properties

For each `.resx` file:
- **Build Action**: Set to `Embedded Resource`
- **Custom Tool**: Set to `ResXFileCodeGenerator` (for default resources) or `PublicResXFileCodeGenerator` (for public access)
- **Access Modifier**: Set to `Public` or `Internal` based on your needs

### Resource File Content Structure

Each `.resx` file contains key-value pairs:

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <data name="WelcomeMessage" xml:space="preserve">
    <value>Welcome to the application!</value>
  </data>
  <data name="ErrorFileNotFound" xml:space="preserve">
    <value>File not found: {0}</value>
    <comment>Parameter {0} is the file path</comment>
  </data>
</root>
```

**Best Practices:**
- Use descriptive key names (e.g., `WelcomeMessage`, `ErrorFileNotFound`)
- Add comments to explain context, especially for parameterized strings
- Keep keys consistent across all culture-specific files
- Use XML `xml:space="preserve"` to maintain whitespace

### Resource Manager Usage

The `ResourceManager` class provides runtime access to resources:

```csharp
using System.Resources;
using System.Globalization;

// Create ResourceManager instance
ResourceManager rm = new ResourceManager("YourNamespace.Resources.Strings", 
                                         typeof(YourClass).Assembly);

// Get string for current UI culture
string message = rm.GetString("WelcomeMessage");

// Get string for specific culture
CultureInfo culture = new CultureInfo("fr-FR");
string frenchMessage = rm.GetString("WelcomeMessage", culture);

// Get string with parameters
string error = string.Format(rm.GetString("ErrorFileNotFound"), filePath);
```

### Strongly Typed Resource Classes

Visual Studio can generate strongly-typed resource classes automatically:

```csharp
// Auto-generated by Visual Studio
public class Strings {
    private static ResourceManager resourceMan;
    
    public static string WelcomeMessage {
        get {
            return ResourceManager.GetString("WelcomeMessage", resourceCulture);
        }
    }
}

// Usage
string message = Strings.WelcomeMessage;
```

## Language Switching Mechanisms

### Setting the Current Culture

The application's culture determines which resources are loaded:

```csharp
using System.Globalization;
using System.Threading;

public static class CultureHelper
{
    /// <summary>
    /// Sets the application's UI culture
    /// </summary>
    /// <param name="cultureName">Culture code (e.g., "fr-FR", "en-US")</param>
    public static void SetCulture(string cultureName)
    {
        try
        {
            CultureInfo culture = new CultureInfo(cultureName);
            
            // Set culture for the current thread
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            
            // Set default culture for new threads (optional)
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }
        catch (CultureNotFoundException ex)
        {
            // Log error and fall back to default culture
            Console.WriteLine($"Culture '{cultureName}' not found: {ex.Message}");
        }
    }
}
```

### Runtime Language Switching

For applications that allow users to change language at runtime:

```csharp
public class LanguageSwitcher
{
    public event EventHandler LanguageChanged;
    
    public void ChangeLanguage(string cultureName)
    {
        // Save preference
        Settings.Default.PreferredLanguage = cultureName;
        Settings.Default.Save();
        
        // Apply culture
        CultureHelper.SetCulture(cultureName);
        
        // Notify listeners to refresh UI
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }
}
```

### UI Refresh After Language Change

When language changes at runtime, UI elements need to be refreshed:

```csharp
public class MainWindow : Form
{
    private LanguageSwitcher languageSwitcher;
    
    public MainWindow()
    {
        InitializeComponent();
        
        languageSwitcher = new LanguageSwitcher();
        languageSwitcher.LanguageChanged += OnLanguageChanged;
    }
    
    private void OnLanguageChanged(object sender, EventArgs e)
    {
        // Refresh all UI text
        RefreshUIText();
        
        // Recreate dynamic controls if necessary
        RecreateControls();
    }
    
    private void RefreshUIText()
    {
        // Update labels, buttons, etc.
        welcomeLabel.Text = Strings.WelcomeMessage;
        okButton.Text = Strings.OkButton;
        cancelButton.Text = Strings.CancelButton;
        
        // Update window title
        this.Text = Strings.ApplicationTitle;
    }
}
```

### Persisting Language Preferences

Store user's language preference:

```csharp
public static class LanguagePreferences
{
    private const string PreferenceKey = "UserLanguage";
    
    public static void SaveLanguagePreference(string cultureName)
    {
        // Using app settings
        Properties.Settings.Default.PreferredLanguage = cultureName;
        Properties.Settings.Default.Save();
        
        // Alternative: Using registry
        // RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\YourApp");
        // key.SetValue(PreferenceKey, cultureName);
    }
    
    public static string GetLanguagePreference()
    {
        string savedLanguage = Properties.Settings.Default.PreferredLanguage;
        
        if (string.IsNullOrEmpty(savedLanguage))
        {
            // Use system default
            return CultureInfo.CurrentUICulture.Name;
        }
        
        return savedLanguage;
    }
}
```

### Application Startup

Set culture at application startup:

```csharp
static class Program
{
    [STAThread]
    static void Main()
    {
        // Load saved language preference
        string preferredLanguage = LanguagePreferences.GetLanguagePreference();
        CultureHelper.SetCulture(preferredLanguage);
        
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainWindow());
    }
}
```

## Date, Time, and Number Formatting

### Date and Time Formatting

Always use culture-aware formatting for dates and times:

```csharp
using System.Globalization;

public class DateTimeFormatter
{
    /// <summary>
    /// Formats a date according to the current culture
    /// </summary>
    public static string FormatDate(DateTime date)
    {
        // Short date pattern (e.g., "12/31/2023" in en-US, "31/12/2023" in fr-FR)
        return date.ToString("d", CultureInfo.CurrentCulture);
    }
    
    /// <summary>
    /// Formats a date and time according to the current culture
    /// </summary>
    public static string FormatDateTime(DateTime dateTime)
    {
        // Full date/time pattern
        return dateTime.ToString("f", CultureInfo.CurrentCulture);
    }
    
    /// <summary>
    /// Formats time according to the current culture
    /// </summary>
    public static string FormatTime(DateTime time)
    {
        // Short time pattern (e.g., "11:30 PM" in en-US, "23:30" in fr-FR)
        return time.ToString("t", CultureInfo.CurrentCulture);
    }
    
    /// <summary>
    /// Custom date format with culture awareness
    /// </summary>
    public static string FormatCustomDate(DateTime date, string format)
    {
        return date.ToString(format, CultureInfo.CurrentCulture);
    }
}

// Usage examples
DateTime now = DateTime.Now;
string shortDate = DateTimeFormatter.FormatDate(now);        // "11/5/2025"
string fullDateTime = DateTimeFormatter.FormatDateTime(now);  // "Wednesday, November 5, 2025 3:33 PM"
string time = DateTimeFormatter.FormatTime(now);             // "3:33 PM"
```

### Standard Date/Time Format Strings

| Specifier | Description | en-US Example | fr-FR Example |
|-----------|-------------|---------------|---------------|
| `d` | Short date | 11/5/2025 | 05/11/2025 |
| `D` | Long date | Wednesday, November 5, 2025 | mercredi 5 novembre 2025 |
| `t` | Short time | 3:33 PM | 15:33 |
| `T` | Long time | 3:33:02 PM | 15:33:02 |
| `f` | Full date/time (short) | Wednesday, November 5, 2025 3:33 PM | mercredi 5 novembre 2025 15:33 |
| `F` | Full date/time (long) | Wednesday, November 5, 2025 3:33:02 PM | mercredi 5 novembre 2025 15:33:02 |
| `g` | General (short) | 11/5/2025 3:33 PM | 05/11/2025 15:33 |
| `G` | General (long) | 11/5/2025 3:33:02 PM | 05/11/2025 15:33:02 |

### Number Formatting

Format numbers according to cultural conventions:

```csharp
public class NumberFormatter
{
    /// <summary>
    /// Formats a number with cultural conventions
    /// </summary>
    public static string FormatNumber(double number, int decimals = 2)
    {
        return number.ToString($"N{decimals}", CultureInfo.CurrentCulture);
    }
    
    /// <summary>
    /// Formats a percentage
    /// </summary>
    public static string FormatPercent(double value, int decimals = 1)
    {
        return value.ToString($"P{decimals}", CultureInfo.CurrentCulture);
    }
    
    /// <summary>
    /// Formats a currency value
    /// </summary>
    public static string FormatCurrency(decimal amount)
    {
        return amount.ToString("C", CultureInfo.CurrentCulture);
    }
    
    /// <summary>
    /// Formats file size with appropriate units
    /// </summary>
    public static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        
        return $"{len.ToString("0.##", CultureInfo.CurrentCulture)} {sizes[order]}";
    }
}

// Usage examples
double value = 1234567.89;
Console.WriteLine(NumberFormatter.FormatNumber(value));     // "1,234,567.89" (en-US)
                                                           // "1 234 567,89" (fr-FR)

double percent = 0.7525;
Console.WriteLine(NumberFormatter.FormatPercent(percent)); // "75.3%" (en-US)
                                                          // "75,3 %" (fr-FR)

decimal price = 49.99m;
Console.WriteLine(NumberFormatter.FormatCurrency(price));  // "$49.99" (en-US)
                                                          // "49,99 €" (fr-FR)
```

### Parsing Culture-Aware Input

When parsing user input, use culture-aware parsing:

```csharp
public class InputParser
{
    /// <summary>
    /// Parses a date string according to current culture
    /// </summary>
    public static bool TryParseDate(string input, out DateTime result)
    {
        return DateTime.TryParse(input, CultureInfo.CurrentCulture, 
                                DateTimeStyles.None, out result);
    }
    
    /// <summary>
    /// Parses a number string according to current culture
    /// </summary>
    public static bool TryParseNumber(string input, out double result)
    {
        return double.TryParse(input, NumberStyles.Number, 
                              CultureInfo.CurrentCulture, out result);
    }
    
    /// <summary>
    /// Parses a currency string according to current culture
    /// </summary>
    public static bool TryParseCurrency(string input, out decimal result)
    {
        return decimal.TryParse(input, NumberStyles.Currency, 
                               CultureInfo.CurrentCulture, out result);
    }
}
```

## Right-to-Left (RTL) Language Support

### Enabling RTL Support

For languages like Arabic, Hebrew, Persian, and Urdu that read right-to-left:

```csharp
public class RTLHelper
{
    /// <summary>
    /// Determines if the current culture uses RTL reading order
    /// </summary>
    public static bool IsRTLCulture()
    {
        return CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft;
    }
    
    /// <summary>
    /// Applies RTL layout to a form
    /// </summary>
    public static void ApplyRTLLayout(Form form)
    {
        if (IsRTLCulture())
        {
            form.RightToLeft = RightToLeft.Yes;
            form.RightToLeftLayout = true;
        }
        else
        {
            form.RightToLeft = RightToLeft.No;
            form.RightToLeftLayout = false;
        }
    }
    
    /// <summary>
    /// Applies RTL to all forms in the application
    /// </summary>
    public static void ApplyRTLToApplication()
    {
        if (IsRTLCulture())
        {
            Application.CurrentCulture = CultureInfo.CurrentUICulture;
        }
    }
}
```

### Form Design for RTL

In Visual Studio designer:

1. Set `Form.RightToLeft` property to `Yes` for RTL testing
2. Set `Form.RightToLeftLayout` property to `True`
3. Controls will automatically mirror their positions

**Important Considerations:**

- **Menu Items**: Automatically flip position
- **Toolbars**: Icons and buttons reverse order
- **Scrollbars**: Appear on the left side
- **Text Alignment**: Right-align for RTL languages
- **Icons**: Some icons may need mirroring (e.g., arrows)

### Runtime RTL Application

```csharp
public class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        
        // Apply RTL if needed
        RTLHelper.ApplyRTLLayout(this);
        
        // Subscribe to language changes
        LanguageManager.LanguageChanged += OnLanguageChanged;
    }
    
    private void OnLanguageChanged(object sender, EventArgs e)
    {
        // Reapply RTL settings when language changes
        RTLHelper.ApplyRTLLayout(this);
        
        // Refresh layout
        this.PerformLayout();
    }
}
```

### RTL-Specific Resources

Some resources may need RTL-specific versions:

```xml
<!-- Strings.resx (LTR) -->
<data name="NavigationHint">
  <value>Click the arrow to continue →</value>
</data>

<!-- Strings.ar.resx (RTL) -->
<data name="NavigationHint">
  <value>انقر على السهم للمتابعة ←</value>
</data>
```

### Mirroring Images

Some images need to be flipped for RTL:

```csharp
public class ImageHelper
{
    /// <summary>
    /// Flips an image horizontally for RTL cultures
    /// </summary>
    public static Image GetCultureAwareImage(Image originalImage)
    {
        if (RTLHelper.IsRTLCulture() && ShouldMirrorImage(originalImage))
        {
            Image flipped = (Image)originalImage.Clone();
            flipped.RotateFlip(RotateFlipType.RotateNoneFlipX);
            return flipped;
        }
        
        return originalImage;
    }
    
    private static bool ShouldMirrorImage(Image image)
    {
        // Logic to determine if image should be mirrored
        // e.g., arrows, pointing hands, etc.
        return true; // Simplified
    }
}
```

## Currency and Regional Settings

### Currency Formatting

Use `CultureInfo` for proper currency formatting:

```csharp
public class CurrencyHelper
{
    /// <summary>
    /// Formats currency according to the current culture
    /// </summary>
    public static string FormatCurrency(decimal amount)
    {
        return amount.ToString("C", CultureInfo.CurrentCulture);
    }
    
    /// <summary>
    /// Formats currency for a specific culture/region
    /// </summary>
    public static string FormatCurrency(decimal amount, string cultureName)
    {
        CultureInfo culture = new CultureInfo(cultureName);
        return amount.ToString("C", culture);
    }
    
    /// <summary>
    /// Gets the currency symbol for the current culture
    /// </summary>
    public static string GetCurrencySymbol()
    {
        return CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;
    }
    
    /// <summary>
    /// Gets currency information for a specific region
    /// </summary>
    public static RegionInfo GetRegionInfo()
    {
        return new RegionInfo(CultureInfo.CurrentCulture.Name);
    }
}

// Usage
decimal price = 1234.56m;
Console.WriteLine(CurrencyHelper.FormatCurrency(price));
// en-US: "$1,234.56"
// fr-FR: "1 234,56 €"
// ja-JP: "¥1,235" (note: yen doesn't use decimal places)
// de-DE: "1.234,56 €"

RegionInfo region = CurrencyHelper.GetRegionInfo();
Console.WriteLine($"Currency: {region.CurrencySymbol}");
Console.WriteLine($"Currency Name: {region.CurrencyEnglishName}");
Console.WriteLine($"ISO Currency Symbol: {region.ISOCurrencySymbol}");
```

### Regional Settings

Access regional information:

```csharp
public class RegionalSettingsHelper
{
    /// <summary>
    /// Gets comprehensive regional information
    /// </summary>
    public static RegionalSettings GetRegionalSettings()
    {
        CultureInfo culture = CultureInfo.CurrentCulture;
        RegionInfo region = new RegionInfo(culture.Name);
        
        return new RegionalSettings
        {
            CultureName = culture.Name,
            DisplayName = culture.DisplayName,
            EnglishName = culture.EnglishName,
            NativeName = culture.NativeName,
            
            // Regional info
            CountryName = region.EnglishName,
            CountryNativeName = region.NativeName,
            TwoLetterISORegionName = region.TwoLetterISORegionName,
            ThreeLetterISORegionName = region.ThreeLetterISORegionName,
            
            // Currency info
            CurrencySymbol = region.CurrencySymbol,
            ISOCurrencySymbol = region.ISOCurrencySymbol,
            CurrencyEnglishName = region.CurrencyEnglishName,
            CurrencyNativeName = region.CurrencyNativeName,
            
            // Number formatting
            NumberDecimalSeparator = culture.NumberFormat.NumberDecimalSeparator,
            NumberGroupSeparator = culture.NumberFormat.NumberGroupSeparator,
            
            // Date/time formatting
            ShortDatePattern = culture.DateTimeFormat.ShortDatePattern,
            LongDatePattern = culture.DateTimeFormat.LongDatePattern,
            ShortTimePattern = culture.DateTimeFormat.ShortTimePattern,
            LongTimePattern = culture.DateTimeFormat.LongTimePattern,
            FirstDayOfWeek = culture.DateTimeFormat.FirstDayOfWeek,
            
            // Text direction
            IsRightToLeft = culture.TextInfo.IsRightToLeft
        };
    }
}

public class RegionalSettings
{
    public string CultureName { get; set; }
    public string DisplayName { get; set; }
    public string EnglishName { get; set; }
    public string NativeName { get; set; }
    public string CountryName { get; set; }
    public string CountryNativeName { get; set; }
    public string TwoLetterISORegionName { get; set; }
    public string ThreeLetterISORegionName { get; set; }
    public string CurrencySymbol { get; set; }
    public string ISOCurrencySymbol { get; set; }
    public string CurrencyEnglishName { get; set; }
    public string CurrencyNativeName { get; set; }
    public string NumberDecimalSeparator { get; set; }
    public string NumberGroupSeparator { get; set; }
    public string ShortDatePattern { get; set; }
    public string LongDatePattern { get; set; }
    public string ShortTimePattern { get; set; }
    public string LongTimePattern { get; set; }
    public DayOfWeek FirstDayOfWeek { get; set; }
    public bool IsRightToLeft { get; set; }
}
```

### Measurement Units

Handle different measurement systems:

```csharp
public class MeasurementHelper
{
    /// <summary>
    /// Determines if the region uses metric system
    /// </summary>
    public static bool IsMetricRegion()
    {
        RegionInfo region = new RegionInfo(CultureInfo.CurrentCulture.Name);
        
        // US, Liberia, and Myanmar primarily use imperial
        return !region.TwoLetterISORegionName.Equals("US", StringComparison.OrdinalIgnoreCase) &&
               !region.TwoLetterISORegionName.Equals("LR", StringComparison.OrdinalIgnoreCase) &&
               !region.TwoLetterISORegionName.Equals("MM", StringComparison.OrdinalIgnoreCase);
    }
    
    /// <summary>
    /// Formats temperature according to regional preference
    /// </summary>
    public static string FormatTemperature(double celsius)
    {
        if (IsMetricRegion())
        {
            return $"{celsius:F1}°C";
        }
        else
        {
            double fahrenheit = (celsius * 9 / 5) + 32;
            return $"{fahrenheit:F1}°F";
        }
    }
    
    /// <summary>
    /// Formats distance according to regional preference
    /// </summary>
    public static string FormatDistance(double meters)
    {
        if (IsMetricRegion())
        {
            if (meters < 1000)
                return $"{meters:F0} m";
            else
                return $"{meters / 1000:F2} km";
        }
        else
        {
            double feet = meters * 3.28084;
            if (feet < 5280)
                return $"{feet:F0} ft";
            else
                return $"{feet / 5280:F2} mi";
        }
    }
}
```

## String Externalization Best Practices

### Key Naming Conventions

Use consistent, descriptive key names:

```
Format: [Context]_[Component]_[Action/State]_[Element]

Examples:
- MainWindow_Title
- LoginDialog_Button_Submit
- ErrorMessage_FileNotFound
- StatusBar_Label_Ready
- Menu_File_Open
- Tooltip_Button_Save
```

### Organizing Resource Keys

Group related keys together:

```xml
<!-- UI Elements -->
<data name="MainWindow_Title">
  <value>Application Name</value>
</data>
<data name="MainWindow_Menu_File">
  <value>File</value>
</data>
<data name="MainWindow_Menu_Edit">
  <value>Edit</value>
</data>

<!-- Buttons -->
<data name="Button_OK">
  <value>OK</value>
</data>
<data name="Button_Cancel">
  <value>Cancel</value>
</data>
<data name="Button_Apply">
  <value>Apply</value>
</data>

<!-- Error Messages -->
<data name="Error_FileNotFound">
  <value>The file '{0}' was not found.</value>
  <comment>Parameter {0}: file path</comment>
</data>
<data name="Error_AccessDenied">
  <value>Access denied to resource '{0}'.</value>
  <comment>Parameter {0}: resource name</comment>
</data>
```

### Handling Pluralization

Different languages have different pluralization rules. Use conditional logic:

```csharp
public class PluralizationHelper
{
    /// <summary>
    /// Gets the appropriate plural form for the current culture
    /// </summary>
    public static string GetPluralForm(int count, string singularKey, string pluralKey)
    {
        ResourceManager rm = new ResourceManager(
            "YourNamespace.Resources.Strings", 
            typeof(PluralizationHelper).Assembly);
        
        // English-like languages: singular for 1, plural for others
        if (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "en")
        {
            return count == 1 
                ? rm.GetString(singularKey) 
                : rm.GetString(pluralKey);
        }
        
        // Russian, Polish, etc.: complex plural rules
        // Implement using ICU rules or third-party libraries
        
        return rm.GetString(pluralKey);
    }
}

// Resource file entries
// Strings.resx
<data name="Items_Singular">
  <value>{0} item</value>
</data>
<data name="Items_Plural">
  <value>{0} items</value>
</data>

// Strings.ru.resx (Russian has 3 plural forms)
<data name="Items_Singular">
  <value>{0} элемент</value>
</data>
<data name="Items_Few">
  <value>{0} элемента</value>
</data>
<data name="Items_Plural">
  <value>{0} элементов</value>
</data>
```

### Context-Dependent Translations

Some words have different meanings based on context:

```xml
<!-- English -->
<data name="Common_Save_Verb">
  <value>Save</value>
  <comment>Action button: save the document</comment>
</data>
<data name="Common_Save_Noun">
  <value>Save</value>
  <comment>Noun: a saved game state</comment>
</data>

<!-- German - different words needed -->
<data name="Common_Save_Verb">
  <value>Speichern</value>
  <comment>Action button: save the document</comment>
</data>
<data name="Common_Save_Noun">
  <value>Speicherstand</value>
  <comment>Noun: a saved game state</comment>
</data>
```

### String Composition

Avoid string concatenation:

**❌ Bad Practice:**
```csharp
// Hard to translate; word order varies by language
string message = Strings.Welcome + " " + userName + "!";
```

**✅ Good Practice:**
```csharp
// Resource: "Welcome, {0}!"
string message = string.Format(Strings.WelcomeUser, userName);
```

### Handling Long Strings

For long strings, consider breaking them into paragraphs:

```xml
<data name="HelpText_Introduction">
  <value>Welcome to the application. This guide will help you get started.</value>
</data>
<data name="HelpText_Features">
  <value>The application includes the following features: package management, automatic updates, and customizable settings.</value>
</data>
```

### Variables in Strings

Use numbered placeholders for flexibility:

```csharp
// Resource: "File {0} was saved to {1} at {2}"
string message = string.Format(
    Strings.FileSaveSuccess,
    fileName,      // {0}
    folderPath,    // {1}
    DateTime.Now   // {2}
);
```

**Benefit**: Translators can reorder parameters as needed for their language:
- English: "File {0} was saved to {1} at {2}"
- Japanese: "{2}に{1}へ{0}ファイルが保存されました"

## Testing Localization

### Pseudo-Localization

Pseudo-localization helps identify localization issues without actual translations:

```csharp
public class PseudoLocalizer
{
    private static readonly Dictionary<char, char> CharacterMap = new()
    {
        {'A', 'Å'}, {'B', 'Β'}, {'C', 'Ç'}, {'D', 'Ð'}, {'E', 'É'},
        {'F', 'Ƒ'}, {'G', 'Ğ'}, {'H', 'Ĥ'}, {'I', 'Ï'}, {'J', 'Ĵ'},
        {'K', 'Ķ'}, {'L', 'Ļ'}, {'M', 'М'}, {'N', 'Ñ'}, {'O', 'Ö'},
        {'P', 'Þ'}, {'Q', 'Q'}, {'R', 'Ř'}, {'S', 'Š'}, {'T', 'Ţ'},
        {'U', 'Ü'}, {'V', 'V'}, {'W', 'Ŵ'}, {'X', 'Χ'}, {'Y', 'Ý'},
        {'Z', 'Ž'},
        {'a', 'á'}, {'b', 'ƀ'}, {'c', 'ç'}, {'d', 'đ'}, {'e', 'é'},
        {'f', 'ƒ'}, {'g', 'ğ'}, {'h', 'ĥ'}, {'i', 'ï'}, {'j', 'ĵ'},
        {'k', 'ķ'}, {'l', 'ļ'}, {'m', 'м'}, {'n', 'ñ'}, {'o', 'ö'},
        {'p', 'þ'}, {'q', 'q'}, {'r', 'ř'}, {'s', 'š'}, {'t', 'ţ'},
        {'u', 'ü'}, {'v', 'v'}, {'w', 'ŵ'}, {'x', 'χ'}, {'y', 'ý'},
        {'z', 'ž'}
    };
    
    /// <summary>
    /// Converts a string to pseudo-localized version
    /// </summary>
    public static string Pseudolocalize(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        
        StringBuilder result = new StringBuilder();
        result.Append('['); // Add brackets to show boundaries
        
        foreach (char c in input)
        {
            // Replace with accented character if available
            if (CharacterMap.TryGetValue(c, out char replacement))
            {
                result.Append(replacement);
            }
            else
            {
                result.Append(c);
            }
        }
        
        // Expand string by 30% (some languages are longer)
        int expandBy = (int)(result.Length * 0.3);
        result.Append(new string('~', expandBy));
        
        result.Append(']');
        return result.ToString();
    }
}

// Usage
// Original: "Save File"
// Pseudo: "[Šávé Fïļé~~~]"
```

**Benefits of Pseudo-Localization:**
1. **Identifies hard-coded strings**: Strings that don't change are not externalized
2. **Reveals UI truncation**: Expanded strings show if UI has enough space
3. **Tests encoding**: Accented characters reveal encoding issues
4. **Validates boundaries**: Brackets show if strings are properly separated

### Testing Checklist

- [ ] **All strings externalized**: No hard-coded UI text
- [ ] **UI accommodates expansion**: Text doesn't truncate (30-40% expansion)
- [ ] **RTL support works**: Test with Arabic/Hebrew pseudo-locale
- [ ] **Date/time formatting correct**: Uses culture-aware formatting
- [ ] **Number formatting correct**: Decimals and thousands separators proper
- [ ] **Currency displays correctly**: Symbol and format appropriate
- [ ] **Images mirror properly**: RTL-sensitive images flip correctly
- [ ] **No string concatenation**: All strings use proper composition
- [ ] **Pluralization works**: Handles singular/plural correctly
- [ ] **Context preserved**: Translators have sufficient context

### Testing with Different Cultures

```csharp
public class LocalizationTester
{
    /// <summary>
    /// Tests the application with multiple cultures
    /// </summary>
    public static void RunLocalizationTests()
    {
        string[] testCultures = {
            "en-US",  // English (United States)
            "fr-FR",  // French (France)
            "de-DE",  // German (Germany)
            "ja-JP",  // Japanese (Japan)
            "ar-SA",  // Arabic (Saudi Arabia) - RTL
            "zh-CN",  // Chinese (Simplified)
            "ru-RU",  // Russian
            "pt-BR",  // Portuguese (Brazil)
        };
        
        foreach (string culture in testCultures)
        {
            Console.WriteLine($"\n--- Testing Culture: {culture} ---");
            CultureInfo testCulture = new CultureInfo(culture);
            
            // Test date formatting
            DateTime now = DateTime.Now;
            Console.WriteLine($"Date: {now.ToString("d", testCulture)}");
            Console.WriteLine($"Time: {now.ToString("t", testCulture)}");
            
            // Test number formatting
            double number = 1234567.89;
            Console.WriteLine($"Number: {number.ToString("N2", testCulture)}");
            
            // Test currency
            decimal price = 99.99m;
            Console.WriteLine($"Currency: {price.ToString("C", testCulture)}");
            
            // Test RTL
            Console.WriteLine($"Is RTL: {testCulture.TextInfo.IsRightToLeft}");
        }
    }
}
```

### Localization Validation Tool

```csharp
public class LocalizationValidator
{
    /// <summary>
    /// Validates that all required translations exist
    /// </summary>
    public static List<string> ValidateTranslations(string[] requiredCultures)
    {
        List<string> missingKeys = new List<string>();
        ResourceManager rm = new ResourceManager(
            "YourNamespace.Resources.Strings",
            typeof(LocalizationValidator).Assembly);
        
        // Get all keys from default resource
        ResourceSet defaultSet = rm.GetResourceSet(
            CultureInfo.InvariantCulture, true, true);
        
        List<string> allKeys = new List<string>();
        foreach (DictionaryEntry entry in defaultSet)
        {
            allKeys.Add(entry.Key.ToString());
        }
        
        // Check each culture
        foreach (string cultureName in requiredCultures)
        {
            CultureInfo culture = new CultureInfo(cultureName);
            
            foreach (string key in allKeys)
            {
                string value = rm.GetString(key, culture);
                
                if (string.IsNullOrEmpty(value))
                {
                    missingKeys.Add($"{cultureName}: {key}");
                }
            }
        }
        
        return missingKeys;
    }
}
```

## Additional Resources

### Recommended Tools

- **ResX Resource Manager**: Visual Studio extension for managing translations
- **Multilingual App Toolkit**: Microsoft's localization toolset
- **Crowdin/Lokalise**: Translation management platforms
- **PseudoLocalizer**: Automated pseudo-localization testing

### Further Reading

- [.NET Globalization and Localization](https://docs.microsoft.com/en-us/dotnet/core/extensions/globalization-and-localization)
- [Best Practices for Developing World-Ready Applications](https://docs.microsoft.com/en-us/dotnet/standard/globalization-localization/best-practices-for-developing-world-ready-apps)
- [Working with Resource Files](https://docs.microsoft.com/en-us/dotnet/core/extensions/create-resource-files)
- [Right-to-Left Language Support in .NET](https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.righttoeft)

### Common Pitfalls to Avoid

1. ❌ Hard-coding UI strings
2. ❌ Concatenating translated strings
3. ❌ Assuming text length stays constant
4. ❌ Using culture-invariant formatting for user-facing data
5. ❌ Forgetting to test with RTL languages
6. ❌ Not providing context for translators
7. ❌ Storing dates/numbers as strings
8. ❌ Using images with embedded text
9. ❌ Not planning for text expansion
10. ❌ Ignoring pluralization rules

---

**Document Version**: 1.0  
**Last Updated**: November 2025  
**Maintainer**: CodingKit Framework Team
