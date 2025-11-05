# Settings Management

## Overview

The **Settings Engine** (`UniGetUI.Core.Settings`) manages application configuration and user preferences using a file-based storage system.

## Storage Locations

- **Standard Settings**: `%LOCALAPPDATA%\UniGetUI\Configuration\`
- **Secure Settings**: `%LOCALAPPDATA%\UniGetUI\SecureConfiguration\` (encrypted)
- **Portable Mode**: `.\Settings\` (relative to executable)

## Usage

### Boolean Settings

```csharp
// Read boolean setting
bool autoUpdate = Settings.Get(Settings.K.AutomaticallyUpdatePackages);

// Write boolean setting
Settings.Set(Settings.K.AutomaticallyUpdatePackages, true);
```

### Value Settings

```csharp
// Read string value
string language = Settings.GetValue(Settings.K.PreferredLanguage);

// Write string value
Settings.SetValue(Settings.K.PreferredLanguage, "en");
```

### Dictionary Settings

```csharp
// Read dictionary
var ignoredUpdates = Settings.GetDictionary<string, string>(Settings.K.IgnoredPackageUpdates);

// Modify and write back
ignoredUpdates["package-id"] = "version";
Settings.SetDictionary(Settings.K.IgnoredPackageUpdates, ignoredUpdates);
```

## Secure Settings

For sensitive data (API keys, credentials):

```csharp
// Store securely (encrypted)
SecureSettings.SetValue("ApiKey", encryptedKey);

// Retrieve securely
string apiKey = SecureSettings.GetValue("ApiKey");
```

## Settings Keys

All settings keys defined in `Settings.K` enum. Common keys:
- `AutomaticallyUpdatePackages`
- `PreferredLanguage`
- `DisableAutoUpdateCheck`
- `IgnoredPackageUpdates`
- Many more...

## Related Documentation

- [Data Flow](./data-flow.md)
- [System Architecture](../02-architecture/system-architecture.md)
