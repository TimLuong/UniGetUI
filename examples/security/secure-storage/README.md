# Secure Storage Example

This example demonstrates secure storage implementation for Windows desktop applications using Windows Credential Manager and Data Protection API (DPAPI).

## Overview

The secure storage implementation provides:
- Token storage using Windows Credential Manager
- File encryption using DPAPI
- Secure configuration management
- Temporary file handling with secure deletion

## Project Structure

```
SecureStorageExample/
├── SecureStorageExample.csproj
├── Program.cs
├── Services/
│   ├── ISecureStorage.cs
│   ├── WindowsCredentialStorage.cs
│   ├── DPAPIStorage.cs
│   └── SecureFileStorage.cs
├── Models/
│   └── AppConfiguration.cs
└── README.md
```

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Windows 10 or later (this example uses Windows-specific APIs)

### Build and Run

**Note:** This example must be built and run on Windows as it uses Windows-specific APIs like Windows Credential Manager.

```bash
cd examples/security/secure-storage
dotnet build
dotnet run
```

**For cross-platform builds from non-Windows systems:**
```bash
dotnet build /p:EnableWindowsTargeting=true
```

## Implementation Details

See the source files for complete implementation examples demonstrating:

1. **Credential Management** - Storing tokens and passwords securely
2. **File Encryption** - Encrypting configuration files
3. **Secure Deletion** - Overwriting sensitive data before deletion
4. **Memory Protection** - Clearing sensitive data from memory

## Security Features

- ✅ Windows Credential Manager integration
- ✅ DPAPI encryption with user-scope protection
- ✅ Secure random token generation
- ✅ File permission management
- ✅ Secure deletion with data overwriting
- ✅ Memory cleanup for sensitive data

## Usage Examples

### Storing and Retrieving Credentials

```csharp
var storage = new WindowsCredentialStorage("MyApp");

// Store token
storage.Store("AccessToken", "my-secret-token-12345");

// Retrieve token
var token = storage.Retrieve("AccessToken");

// Delete token
storage.Delete("AccessToken");
```

### Encrypting Configuration Files

```csharp
var config = new AppConfiguration
{
    ApiKey = "secret-api-key",
    DatabaseConnection = "Server=localhost;..."
};

var storage = new DPAPIStorage("MyApp");
storage.SaveConfiguration(config);

var loaded = storage.LoadConfiguration<AppConfiguration>();
```

### Secure Temporary Files

```csharp
using (var tempFile = new SecureTempFile())
{
    tempFile.WriteContent(Encoding.UTF8.GetBytes("sensitive data"));
    
    // Process file
    var content = tempFile.ReadContent();
    
    // File automatically and securely deleted when disposed
}
```

## Security Best Practices

1. **Never store credentials in plain text**
2. **Use Windows Credential Manager for tokens and passwords**
3. **Encrypt configuration files with DPAPI**
4. **Set restrictive file permissions**
5. **Implement secure deletion for sensitive files**
6. **Clear sensitive data from memory after use**

## Testing

Run the included tests to verify security implementations:

```bash
dotnet test
```

## Related Documentation

- [Authentication Patterns](../../../docs/security/authentication-patterns.md)
- [Data Protection](../../../docs/security/data-protection.md)
- [Vulnerability Prevention](../../../docs/security/vulnerability-prevention.md)
- [Security Testing](../../../docs/security/security-testing.md)
