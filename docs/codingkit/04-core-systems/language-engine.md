# Language Engine

## Overview

The **Language Engine** (`UniGetUI.Core.LanguageEngine`) provides internationalization (i18n) support for UniGetUI, enabling the application to display in 50+ languages.

## Key Features

- Support for 50+ languages
- Dynamic language switching at runtime
- Fallback to English for missing translations
- Community translation contributions via Tolgee platform
- Efficient in-memory caching of translations

## Usage

### Getting Translated Strings

```csharp
// Simple translation
string text = CoreTools.Translate("SettingsKey");

// Translation with parameters
string message = CoreTools.Translate("PackageInstalled", packageName);
```

### Changing Language

```csharp
// Set language via settings
Settings.SetValue(Settings.K.PreferredLanguage, "es");

// Language engine automatically reloads
```

## Translation Management

- Translations managed via Tolgee platform
- Python scripts in `/scripts/` handle translation sync
- Community contributors can add new languages
- Automated workflows keep translations updated

## Related Documentation

- [System Architecture](../02-architecture/system-architecture.md#layer-2-business-logic-layer-core-services)
- Translation scripts in `/scripts/`
