# Multi-Language Application Example

This example demonstrates best practices for implementing localization and internationalization in a Windows application using the CodingKit framework.

## Overview

This sample application showcases:
- Resource file organization (.resx files)
- Language switching at runtime
- Culture-aware date, time, and number formatting
- Right-to-Left (RTL) language support
- Currency and regional settings
- String externalization best practices

## Project Structure

```
multi-language-app/
├── README.md                           # This file
├── MultiLanguageApp.csproj             # Project file
├── Program.cs                          # Application entry point
├── MainForm.cs                         # Main application window
├── MainForm.Designer.cs                # Form designer code
├── Resources/                          # Resource files directory
│   ├── Strings.resx                   # Default (English) strings
│   ├── Strings.fr.resx                # French strings
│   ├── Strings.de.resx                # German strings
│   ├── Strings.ja.resx                # Japanese strings
│   ├── Strings.ar.resx                # Arabic strings (RTL example)
│   └── Strings.Designer.cs            # Auto-generated resource accessor
├── Helpers/                            # Helper classes
│   ├── CultureHelper.cs               # Culture management
│   ├── LocalizationHelper.cs          # Localization utilities
│   └── RTLHelper.cs                   # RTL support utilities
└── Properties/
    └── Settings.settings               # Application settings
```

## Features Demonstrated

### 1. Resource File Organization

The application uses separate `.resx` files for each supported language:
- `Strings.resx` - Default/fallback (English)
- `Strings.fr.resx` - French
- `Strings.de.resx` - German
- `Strings.ja.resx` - Japanese
- `Strings.ar.resx` - Arabic (demonstrates RTL)

### 2. Runtime Language Switching

Users can change the application language without restarting:
```csharp
// Change language
CultureHelper.SetCulture("fr-FR");

// Refresh UI
mainForm.RefreshUIText();
```

### 3. Culture-Aware Formatting

The application demonstrates proper formatting for:
- **Dates**: Short and long date formats
- **Times**: 12-hour vs 24-hour formats
- **Numbers**: Decimal separators and digit grouping
- **Currency**: Currency symbols and formatting

### 4. RTL Language Support

When Arabic or other RTL languages are selected:
- Form layout automatically mirrors
- Text alignment changes to right-to-left
- Scrollbars appear on the left side

### 5. Regional Settings Display

Shows comprehensive regional information:
- Culture name and display name
- Country/region information
- Currency symbols and codes
- Number and date/time format patterns

## Getting Started

### Prerequisites

- .NET 6.0 or later
- Visual Studio 2022 or later (or VS Code with C# extension)
- Windows 10 or later

### Building the Project

```bash
# Clone the repository
git clone [repository-url]

# Navigate to the example directory
cd examples/i18n/multi-language-app

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run
```

### Using Visual Studio

1. Open `MultiLanguageApp.csproj` in Visual Studio
2. Press F5 to build and run
3. Use the language dropdown to test different languages

## How to Use the Example

1. **Start the application** - It will load in your system's default language or English
2. **Select a language** from the dropdown menu in the toolbar
3. **Observe the changes**:
   - All UI text updates immediately
   - Date and time formats change
   - Number formatting changes
   - Currency symbols update
   - For Arabic: layout mirrors to RTL

## Code Examples

### Setting Culture

```csharp
public static class CultureHelper
{
    public static void SetCulture(string cultureName)
    {
        CultureInfo culture = new CultureInfo(cultureName);
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
}
```

### Accessing Localized Strings

```csharp
// Simple string access
string welcomeMessage = Strings.WelcomeMessage;

// String with parameters
string greeting = string.Format(Strings.GreetingWithName, userName);

// Accessing at runtime
ResourceManager rm = new ResourceManager("MultiLanguageApp.Resources.Strings", 
                                         typeof(Program).Assembly);
string message = rm.GetString("WelcomeMessage");
```

### Formatting Dates and Numbers

```csharp
// Date formatting
DateTime now = DateTime.Now;
string shortDate = now.ToString("d", CultureInfo.CurrentCulture);
string longDate = now.ToString("D", CultureInfo.CurrentCulture);

// Number formatting
double value = 1234567.89;
string formattedNumber = value.ToString("N2", CultureInfo.CurrentCulture);

// Currency formatting
decimal price = 49.99m;
string formattedPrice = price.ToString("C", CultureInfo.CurrentCulture);
```

### Handling RTL Languages

```csharp
public static class RTLHelper
{
    public static bool IsRTLCulture()
    {
        return CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft;
    }
    
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
}
```

## Supported Languages

| Language | Culture Code | Notes |
|----------|--------------|-------|
| English | en-US | Default/fallback |
| French | fr-FR | Demonstrates European formatting |
| German | de-DE | Demonstrates different date/number formats |
| Japanese | ja-JP | Demonstrates Asian character set |
| Arabic | ar-SA | Demonstrates RTL layout |

## Adding New Languages

To add support for a new language:

1. **Create a new resource file**:
   - Copy `Strings.resx` to `Strings.[culture].resx`
   - Example: `Strings.es.resx` for Spanish

2. **Translate the strings**:
   - Open the new file in Visual Studio or a text editor
   - Translate each `<value>` element
   - Keep all `<data name="">` keys unchanged
   - Preserve placeholders like `{0}`, `{1}`, etc.

3. **Add to language selector**:
   ```csharp
   languageComboBox.Items.Add(new LanguageOption("Español", "es-ES"));
   ```

4. **Build and test**:
   ```bash
   dotnet build
   dotnet run
   ```

## Best Practices Demonstrated

✅ **String Externalization**: All UI text stored in resource files  
✅ **Meaningful Key Names**: Descriptive resource keys (e.g., `WelcomeMessage`, `Button_Save`)  
✅ **Parameter Substitution**: Using `{0}`, `{1}` instead of concatenation  
✅ **Culture-Aware Formatting**: Proper date/time/number/currency formatting  
✅ **RTL Support**: Layout mirroring for right-to-left languages  
✅ **Runtime Switching**: No restart required to change language  
✅ **Persistent Preferences**: Language choice saved across sessions  
✅ **Fallback Handling**: Graceful fallback to default language  

## Common Pitfalls Avoided

❌ **Hard-coded strings**: All text externalized  
❌ **String concatenation**: Using format strings instead  
❌ **Fixed UI layouts**: Allows for text expansion  
❌ **Culture-invariant formatting**: Using culture-aware methods  
❌ **Missing RTL support**: Implemented RTL layout handling  

## Testing

### Manual Testing

1. Test each supported language
2. Verify all UI text translates correctly
3. Check date, time, number formats
4. Test RTL layout with Arabic
5. Verify language persists after restart

### Pseudo-Localization Testing

To test for localization issues without real translations:

```bash
# Generate pseudo-localized resources
dotnet run --project PseudoLocalizer

# Run app with pseudo-locale
# Set LANG environment variable or use in-app setting
```

## Troubleshooting

### Problem: Language doesn't change

**Solution**: Ensure you're refreshing the UI after setting culture:
```csharp
CultureHelper.SetCulture(newLanguage);
this.RefreshUIText(); // Must refresh UI controls
```

### Problem: Special characters appear garbled

**Solution**: Ensure resource files are saved as UTF-8:
1. Open .resx file in text editor
2. Save as UTF-8 with BOM
3. Rebuild project

### Problem: RTL layout not working

**Solution**: Verify both properties are set:
```csharp
form.RightToLeft = RightToLeft.Yes;
form.RightToLeftLayout = true; // Often forgotten!
```

### Problem: Numbers/dates format incorrectly

**Solution**: Use `CultureInfo.CurrentCulture`, not `InvariantCulture`:
```csharp
// ❌ Wrong
date.ToString("d", CultureInfo.InvariantCulture);

// ✅ Correct
date.ToString("d", CultureInfo.CurrentCulture);
```

## Further Reading

- [Localization Guide](../../docs/i18n/localization-guide.md)
- [Translation Workflow](../../docs/i18n/translation-workflow.md)
- [.NET Globalization Documentation](https://docs.microsoft.com/en-us/dotnet/core/extensions/globalization)

## License

This example is part of the CodingKit Framework and is provided as educational material.

---

**Version**: 1.0  
**Last Updated**: November 2025  
**Author**: CodingKit Framework Team
