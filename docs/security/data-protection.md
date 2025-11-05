# Data Protection and Secure Storage

## Overview

This document covers data protection strategies for Windows desktop applications, including encryption at rest, secure data storage, and protection of sensitive information.

## Table of Contents

1. [Encryption at Rest](#encryption-at-rest)
2. [Windows Credential Manager](#windows-credential-manager)
3. [Data Protection API (DPAPI)](#data-protection-api-dpapi)
4. [Configuration and Settings Security](#configuration-and-settings-security)
5. [Secure File Storage](#secure-file-storage)
6. [Memory Protection](#memory-protection)
7. [Best Practices](#best-practices)

---

## Encryption at Rest

### Overview

Encryption at rest protects data stored on disk from unauthorized access. Windows provides several APIs for implementing encryption at rest.

### Use Cases

- Protecting user credentials
- Encrypting sensitive configuration files
- Securing cached data
- Protecting API tokens and keys

---

## Windows Credential Manager

### Overview

Windows Credential Manager (PasswordVault) provides system-level encrypted storage for credentials, automatically handling encryption and decryption.

### Implementation

UniGetUI demonstrates secure token storage:

**Location:** `src/UniGetUI.Core.SecureSettings/SecureGHTokenManager.cs`

```csharp
using Windows.Security.Credentials;

public static class SecureTokenStorage
{
    private const string ResourceName = "MyApp/Token";
    private static readonly string UserName = Environment.UserName;
    
    public static void StoreToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            throw new ArgumentException("Token cannot be empty", nameof(token));
        
        var vault = new PasswordVault();
        
        // Remove existing token
        try
        {
            var existingCred = vault.Retrieve(ResourceName, UserName);
            vault.Remove(existingCred);
        }
        catch (Exception)
        {
            // Token doesn't exist, that's fine
        }
        
        // Store new token
        var credential = new PasswordCredential(ResourceName, UserName, token);
        vault.Add(credential);
    }
    
    public static string? RetrieveToken()
    {
        try
        {
            var vault = new PasswordVault();
            var credential = vault.Retrieve(ResourceName, UserName);
            credential.RetrievePassword();
            return credential.Password;
        }
        catch (Exception)
        {
            return null;
        }
    }
    
    public static void DeleteToken()
    {
        try
        {
            var vault = new PasswordVault();
            var credentials = vault.FindAllByResource(ResourceName);
            
            foreach (var cred in credentials)
            {
                vault.Remove(cred);
            }
        }
        catch (Exception)
        {
            // No credentials to delete
        }
    }
}
```

### Advanced Pattern: Multi-Field Credentials

```csharp
public class SecureCredentialStore
{
    private readonly string _resourcePrefix;
    
    public SecureCredentialStore(string appName)
    {
        _resourcePrefix = $"{appName}/";
    }
    
    public void StoreCredentials(string identifier, Dictionary<string, string> fields)
    {
        var vault = new PasswordVault();
        
        foreach (var field in fields)
        {
            var resourceName = $"{_resourcePrefix}{identifier}/{field.Key}";
            
            try
            {
                var existing = vault.Retrieve(resourceName, Environment.UserName);
                vault.Remove(existing);
            }
            catch { }
            
            var credential = new PasswordCredential(
                resourceName,
                Environment.UserName,
                field.Value
            );
            vault.Add(credential);
        }
    }
    
    public Dictionary<string, string> RetrieveCredentials(string identifier, string[] fieldNames)
    {
        var vault = new PasswordVault();
        var result = new Dictionary<string, string>();
        
        foreach (var fieldName in fieldNames)
        {
            var resourceName = $"{_resourcePrefix}{identifier}/{fieldName}";
            
            try
            {
                var credential = vault.Retrieve(resourceName, Environment.UserName);
                credential.RetrievePassword();
                result[fieldName] = credential.Password;
            }
            catch
            {
                result[fieldName] = string.Empty;
            }
        }
        
        return result;
    }
}

// Usage
var store = new SecureCredentialStore("MyApp");
store.StoreCredentials("DatabaseConnection", new Dictionary<string, string>
{
    ["Host"] = "localhost",
    ["Username"] = "admin",
    ["Password"] = "secretpassword",
    ["Database"] = "mydb"
});

var creds = store.RetrieveCredentials(
    "DatabaseConnection", 
    new[] { "Host", "Username", "Password", "Database" }
);
```

### Security Considerations

**✅ Advantages:**
- Automatic encryption by Windows
- Per-user credential isolation
- No additional encryption code required
- Integrated with Windows security model

**⚠️ Limitations:**
- Windows-only solution
- Accessible by all apps running under same user
- No built-in expiration mechanism
- Limited storage capacity

---

## Data Protection API (DPAPI)

### Overview

DPAPI provides cryptographic protection for data using user or machine-specific keys managed by Windows.

### Scopes

- **CurrentUser:** Data encrypted for current Windows user
- **LocalMachine:** Data encrypted for all users on machine (requires admin)

### Implementation

```csharp
using System.Security.Cryptography;
using System.Text;

public static class DataProtection
{
    /// <summary>
    /// Encrypts data using DPAPI for the current user
    /// </summary>
    public static byte[] Protect(byte[] data, byte[]? entropy = null)
    {
        if (data == null || data.Length == 0)
            throw new ArgumentException("Data cannot be null or empty", nameof(data));
        
        return ProtectedData.Protect(
            data,
            entropy,
            DataProtectionScope.CurrentUser
        );
    }
    
    /// <summary>
    /// Decrypts data protected with DPAPI
    /// </summary>
    public static byte[] Unprotect(byte[] encryptedData, byte[]? entropy = null)
    {
        if (encryptedData == null || encryptedData.Length == 0)
            throw new ArgumentException("Data cannot be null or empty", nameof(encryptedData));
        
        return ProtectedData.Unprotect(
            encryptedData,
            entropy,
            DataProtectionScope.CurrentUser
        );
    }
    
    /// <summary>
    /// Encrypts a string and returns base64 encoded result
    /// </summary>
    public static string ProtectString(string plainText, string? password = null)
    {
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var entropy = password != null 
            ? Encoding.UTF8.GetBytes(password) 
            : null;
        
        var encryptedBytes = Protect(plainBytes, entropy);
        return Convert.ToBase64String(encryptedBytes);
    }
    
    /// <summary>
    /// Decrypts a base64 encoded encrypted string
    /// </summary>
    public static string UnprotectString(string encryptedText, string? password = null)
    {
        var encryptedBytes = Convert.FromBase64String(encryptedText);
        var entropy = password != null 
            ? Encoding.UTF8.GetBytes(password) 
            : null;
        
        var plainBytes = Unprotect(encryptedBytes, entropy);
        return Encoding.UTF8.GetString(plainBytes);
    }
}

// Usage
var sensitiveData = "my-secret-api-key";
var encrypted = DataProtection.ProtectString(sensitiveData);
File.WriteAllText("secure-config.txt", encrypted);

var decrypted = DataProtection.UnprotectString(
    File.ReadAllText("secure-config.txt")
);
```

### Advanced: Secure File Storage with DPAPI

```csharp
public class SecureFileStorage
{
    private readonly string _storageDirectory;
    private readonly byte[]? _entropy;
    
    public SecureFileStorage(string directory, string? password = null)
    {
        _storageDirectory = directory;
        _entropy = password != null 
            ? Encoding.UTF8.GetBytes(password) 
            : null;
        
        if (!Directory.Exists(_storageDirectory))
            Directory.CreateDirectory(_storageDirectory);
    }
    
    public void WriteSecure(string fileName, string content)
    {
        var plainBytes = Encoding.UTF8.GetBytes(content);
        var encryptedBytes = ProtectedData.Protect(
            plainBytes,
            _entropy,
            DataProtectionScope.CurrentUser
        );
        
        var filePath = Path.Combine(_storageDirectory, fileName);
        
        // Write with integrity check
        using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        using (var writer = new BinaryWriter(fs))
        {
            // Write magic number for validation
            writer.Write(0x53454355); // "SECU" in hex
            
            // Write version
            writer.Write((byte)1);
            
            // Write data length
            writer.Write(encryptedBytes.Length);
            
            // Write encrypted data
            writer.Write(encryptedBytes);
            
            // Write checksum
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(encryptedBytes);
                writer.Write(hash);
            }
        }
    }
    
    public string? ReadSecure(string fileName)
    {
        var filePath = Path.Combine(_storageDirectory, fileName);
        
        if (!File.Exists(filePath))
            return null;
        
        try
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(fs))
            {
                // Validate magic number
                var magic = reader.ReadUInt32();
                if (magic != 0x53454355)
                    throw new InvalidDataException("Invalid file format");
                
                // Read version
                var version = reader.ReadByte();
                if (version != 1)
                    throw new InvalidDataException($"Unsupported version: {version}");
                
                // Read data length
                var length = reader.ReadInt32();
                
                // Read encrypted data
                var encryptedBytes = reader.ReadBytes(length);
                
                // Read and verify checksum
                var storedHash = reader.ReadBytes(32); // SHA256 hash size
                using (var sha256 = SHA256.Create())
                {
                    var computedHash = sha256.ComputeHash(encryptedBytes);
                    if (!storedHash.SequenceEqual(computedHash))
                        throw new InvalidDataException("Data integrity check failed");
                }
                
                // Decrypt data
                var plainBytes = ProtectedData.Unprotect(
                    encryptedBytes,
                    _entropy,
                    DataProtectionScope.CurrentUser
                );
                
                return Encoding.UTF8.GetString(plainBytes);
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to read secure file: {ex.Message}");
            return null;
        }
    }
    
    public void DeleteSecure(string fileName)
    {
        var filePath = Path.Combine(_storageDirectory, fileName);
        
        if (File.Exists(filePath))
        {
            // Overwrite file with random data before deletion
            var fileInfo = new FileInfo(filePath);
            var length = fileInfo.Length;
            
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Write))
            {
                var randomData = new byte[length];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(randomData);
                }
                fs.Write(randomData, 0, randomData.Length);
            }
            
            File.Delete(filePath);
        }
    }
}

// Usage
var storage = new SecureFileStorage(
    Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MyApp",
        "Secure"
    )
);

storage.WriteSecure("api-keys.dat", "secret-api-key-12345");
var apiKey = storage.ReadSecure("api-keys.dat");
```

---

## Configuration and Settings Security

### Overview

Application settings often contain sensitive information that requires protection. UniGetUI implements a secure settings system.

### Implementation Pattern

**Location:** `src/UniGetUI.Core.SecureSettings/SecureSettings.cs`

```csharp
public static class SecureSettings
{
    public enum K
    {
        AllowCLIArguments,
        AllowImportingCLIArguments,
        AllowPrePostOpCommand,
        ForceUserGSudo,
        AllowCustomManagerPaths
    }
    
    private static readonly Dictionary<string, bool> _cache = new();
    
    public static bool Get(K key)
    {
        string purifiedSetting = CoreTools.MakeValidFileName(ResolveKey(key));
        
        if (_cache.TryGetValue(purifiedSetting, out var value))
            return value;
        
        string purifiedUser = CoreTools.MakeValidFileName(Environment.UserName);
        
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var settingsLocation = Path.Join(appData, "UniGetUI\\SecureSettings", purifiedUser);
        var settingFile = Path.Join(settingsLocation, purifiedSetting);
        
        if (!Directory.Exists(settingsLocation))
        {
            _cache[purifiedSetting] = false;
            return false;
        }
        
        bool exists = File.Exists(settingFile);
        _cache[purifiedSetting] = exists;
        return exists;
    }
    
    public static async Task<bool> TrySet(K key, bool enabled)
    {
        // Requires elevation to modify secure settings
        using Process p = new Process();
        p.StartInfo = new()
        {
            UseShellExecute = true,
            CreateNoWindow = true,
            FileName = CoreData.UniGetUIExecutableFile,
            Verb = "runas", // Request elevation
            ArgumentList = { /* ... */ }
        };
        
        p.Start();
        await p.WaitForExitAsync();
        return p.ExitCode is 0;
    }
}
```

### Enhanced Configuration Security

```csharp
using System.Security.Cryptography;
using System.Text.Json;

public class SecureConfiguration
{
    private readonly string _configFile;
    private readonly byte[]? _entropy;
    
    public SecureConfiguration(string configFile, string? password = null)
    {
        _configFile = configFile;
        _entropy = password != null 
            ? Encoding.UTF8.GetBytes(password) 
            : null;
    }
    
    public void SaveSettings<T>(T settings) where T : class
    {
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
        {
            WriteIndented = false
        });
        
        var plainBytes = Encoding.UTF8.GetBytes(json);
        var encryptedBytes = ProtectedData.Protect(
            plainBytes,
            _entropy,
            DataProtectionScope.CurrentUser
        );
        
        File.WriteAllBytes(_configFile, encryptedBytes);
    }
    
    public T? LoadSettings<T>() where T : class
    {
        if (!File.Exists(_configFile))
            return null;
        
        try
        {
            var encryptedBytes = File.ReadAllBytes(_configFile);
            var plainBytes = ProtectedData.Unprotect(
                encryptedBytes,
                _entropy,
                DataProtectionScope.CurrentUser
            );
            
            var json = Encoding.UTF8.GetString(plainBytes);
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to load settings: {ex.Message}");
            return null;
        }
    }
}

// Usage
public class AppSettings
{
    public string ApiKey { get; set; } = "";
    public string DatabaseConnection { get; set; } = "";
    public List<string> TrustedServers { get; set; } = new();
}

var config = new SecureConfiguration(
    Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MyApp",
        "config.secure"
    )
);

var settings = new AppSettings
{
    ApiKey = "secret-key",
    DatabaseConnection = "Server=localhost;..."
};

config.SaveSettings(settings);
var loaded = config.LoadSettings<AppSettings>();
```

### Settings Export Security

Exclude sensitive settings from exports:

```csharp
public class SettingsExporter
{
    private static readonly HashSet<string> SensitiveKeys = new()
    {
        "CurrentSessionToken",
        "ApiKey",
        "Password",
        "ConnectionString",
        "PrivateKey"
    };
    
    public void Export(string filePath, Dictionary<string, object> settings)
    {
        var exportableSettings = settings
            .Where(kvp => !SensitiveKeys.Contains(kvp.Key))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        
        var json = JsonSerializer.Serialize(exportableSettings, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        
        File.WriteAllText(filePath, json);
        
        Logger.Info($"Exported {exportableSettings.Count} settings (excluded {settings.Count - exportableSettings.Count} sensitive items)");
    }
}
```

---

## Secure File Storage

### File Permissions

Set restrictive file permissions for sensitive data:

```csharp
using System.Security.AccessControl;
using System.Security.Principal;

public static class SecureFilePermissions
{
    public static void SetUserOnlyPermissions(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        var fileSecurity = fileInfo.GetAccessControl();
        
        // Disable inheritance
        fileSecurity.SetAccessRuleProtection(true, false);
        
        // Remove all existing rules
        foreach (FileSystemAccessRule rule in fileSecurity.GetAccessRules(true, false, typeof(NTAccount)))
        {
            fileSecurity.RemoveAccessRule(rule);
        }
        
        // Add rule for current user only
        var currentUser = WindowsIdentity.GetCurrent().User;
        var accessRule = new FileSystemAccessRule(
            currentUser,
            FileSystemRights.FullControl,
            AccessControlType.Allow
        );
        
        fileSecurity.AddAccessRule(accessRule);
        fileInfo.SetAccessControl(fileSecurity);
    }
    
    public static void CreateSecureDirectory(string directoryPath)
    {
        var dirInfo = new DirectoryInfo(directoryPath);
        
        if (!dirInfo.Exists)
            dirInfo.Create();
        
        var dirSecurity = dirInfo.GetAccessControl();
        
        // Disable inheritance
        dirSecurity.SetAccessRuleProtection(true, false);
        
        // Remove all existing rules
        foreach (FileSystemAccessRule rule in dirSecurity.GetAccessRules(true, false, typeof(NTAccount)))
        {
            dirSecurity.RemoveAccessRule(rule);
        }
        
        // Add rule for current user only
        var currentUser = WindowsIdentity.GetCurrent().User;
        var accessRule = new FileSystemAccessRule(
            currentUser,
            FileSystemRights.FullControl,
            InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
            PropagationFlags.None,
            AccessControlType.Allow
        );
        
        dirSecurity.AddAccessRule(accessRule);
        dirInfo.SetAccessControl(dirSecurity);
    }
}

// Usage
var secureDir = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "MyApp",
    "Secure"
);

SecureFilePermissions.CreateSecureDirectory(secureDir);

var secretFile = Path.Combine(secureDir, "secrets.txt");
File.WriteAllText(secretFile, "sensitive data");
SecureFilePermissions.SetUserOnlyPermissions(secretFile);
```

### Secure Temporary Files

Handle temporary files securely:

```csharp
public class SecureTempFile : IDisposable
{
    private readonly string _filePath;
    private bool _disposed;
    
    public string FilePath => _filePath;
    
    public SecureTempFile()
    {
        // Create in secure location
        var tempDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MyApp",
            "Temp"
        );
        
        if (!Directory.Exists(tempDir))
            SecureFilePermissions.CreateSecureDirectory(tempDir);
        
        _filePath = Path.Combine(tempDir, $"{Guid.NewGuid()}.tmp");
        
        // Create empty file with secure permissions
        File.WriteAllBytes(_filePath, Array.Empty<byte>());
        SecureFilePermissions.SetUserOnlyPermissions(_filePath);
    }
    
    public void WriteContent(byte[] content)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SecureTempFile));
        
        File.WriteAllBytes(_filePath, content);
    }
    
    public byte[] ReadContent()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SecureTempFile));
        
        return File.ReadAllBytes(_filePath);
    }
    
    public void Dispose()
    {
        if (_disposed)
            return;
        
        try
        {
            if (File.Exists(_filePath))
            {
                // Overwrite with random data before deletion
                var fileInfo = new FileInfo(_filePath);
                var length = fileInfo.Length;
                
                using (var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Write))
                {
                    var randomData = new byte[length];
                    using (var rng = RandomNumberGenerator.Create())
                    {
                        rng.GetBytes(randomData);
                    }
                    fs.Write(randomData, 0, randomData.Length);
                }
                
                File.Delete(_filePath);
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to securely delete temp file: {ex.Message}");
        }
        
        _disposed = true;
    }
}

// Usage
using (var tempFile = new SecureTempFile())
{
    tempFile.WriteContent(Encoding.UTF8.GetBytes("sensitive data"));
    
    // Process file
    var content = tempFile.ReadContent();
    
    // File automatically and securely deleted when disposed
}
```

---

## Memory Protection

### Secure Strings

Use `SecureString` for sensitive data in memory:

```csharp
public static class SecureStringHelper
{
    public static SecureString CreateSecureString(string plainText)
    {
        var secureString = new SecureString();
        
        foreach (char c in plainText)
        {
            secureString.AppendChar(c);
        }
        
        secureString.MakeReadOnly();
        return secureString;
    }
    
    public static string ConvertToPlainText(SecureString secureString)
    {
        IntPtr ptr = IntPtr.Zero;
        
        try
        {
            ptr = Marshal.SecureStringToGlobalAllocUnicode(secureString);
            return Marshal.PtrToStringUni(ptr) ?? string.Empty;
        }
        finally
        {
            if (ptr != IntPtr.Zero)
            {
                Marshal.ZeroFreeGlobalAllocUnicode(ptr);
            }
        }
    }
    
    public static void ClearString(ref string? sensitiveData)
    {
        if (sensitiveData != null)
        {
            unsafe
            {
                fixed (char* ptr = sensitiveData)
                {
                    for (int i = 0; i < sensitiveData.Length; i++)
                    {
                        ptr[i] = '\0';
                    }
                }
            }
            
            sensitiveData = null;
        }
    }
}

// Usage
var password = GetPasswordFromUser();
var securePassword = SecureStringHelper.CreateSecureString(password);

// Clear original password from memory
SecureStringHelper.ClearString(ref password);

// Use secure password when needed
var plainPassword = SecureStringHelper.ConvertToPlainText(securePassword);
// Use password
SecureStringHelper.ClearString(ref plainPassword);
```

### Memory Cleanup

```csharp
public class SensitiveDataContainer : IDisposable
{
    private byte[]? _data;
    private bool _disposed;
    
    public SensitiveDataContainer(byte[] data)
    {
        _data = new byte[data.Length];
        Array.Copy(data, _data, data.Length);
    }
    
    public byte[] GetData()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SensitiveDataContainer));
        
        if (_data == null)
            throw new InvalidOperationException("Data is null");
        
        var copy = new byte[_data.Length];
        Array.Copy(_data, copy, _data.Length);
        return copy;
    }
    
    public void Dispose()
    {
        if (_disposed)
            return;
        
        if (_data != null)
        {
            // Overwrite data with zeros
            Array.Clear(_data, 0, _data.Length);
            _data = null;
        }
        
        _disposed = true;
        GC.SuppressFinalize(this);
    }
    
    ~SensitiveDataContainer()
    {
        Dispose();
    }
}
```

---

## Best Practices

### 1. Storage Selection

**Use Windows Credential Manager for:**
- OAuth tokens
- API keys
- User passwords
- Short credentials (< 2KB)

**Use DPAPI for:**
- Configuration files
- Cached data
- Large secrets
- Application-specific data

**Use Encrypted Files for:**
- Bulk sensitive data
- Data requiring portability
- Custom encryption requirements

### 2. Key Management

**DO:**
- ✅ Use Windows-managed keys when possible
- ✅ Derive keys from user passwords using PBKDF2
- ✅ Store keys separately from encrypted data
- ✅ Rotate keys periodically

**DON'T:**
- ❌ Hard-code encryption keys
- ❌ Store keys in source control
- ❌ Reuse keys across applications
- ❌ Share keys between users

### 3. Data Lifecycle

**DO:**
- ✅ Encrypt data immediately after collection
- ✅ Clear sensitive data from memory after use
- ✅ Implement secure deletion (overwrite before delete)
- ✅ Set appropriate file permissions

**DON'T:**
- ❌ Store unencrypted sensitive data on disk
- ❌ Leave sensitive data in memory longer than needed
- ❌ Log sensitive data
- ❌ Use world-readable file permissions

### 4. Compliance

**DO:**
- ✅ Follow data protection regulations (GDPR, CCPA)
- ✅ Implement data retention policies
- ✅ Provide data export/deletion functionality
- ✅ Document encryption methods used

### 5. Testing

**DO:**
- ✅ Test encryption/decryption round-trips
- ✅ Verify file permissions are set correctly
- ✅ Test data recovery scenarios
- ✅ Validate secure deletion

---

## Testing Examples

```csharp
[Fact]
public void DataProtection_ShouldEncryptAndDecrypt()
{
    var original = "sensitive data";
    var encrypted = DataProtection.ProtectString(original);
    var decrypted = DataProtection.UnprotectString(encrypted);
    
    Assert.NotEqual(original, encrypted);
    Assert.Equal(original, decrypted);
}

[Fact]
public void SecureFileStorage_ShouldStoreAndRetrieve()
{
    var storage = new SecureFileStorage(Path.GetTempPath());
    var testData = "secret information";
    
    storage.WriteSecure("test.dat", testData);
    var retrieved = storage.ReadSecure("test.dat");
    
    Assert.Equal(testData, retrieved);
    
    storage.DeleteSecure("test.dat");
    Assert.False(File.Exists(Path.Combine(Path.GetTempPath(), "test.dat")));
}

[Fact]
public void CredentialManager_ShouldStoreToken()
{
    var token = "test-token-12345";
    SecureTokenStorage.StoreToken(token);
    
    var retrieved = SecureTokenStorage.RetrieveToken();
    Assert.Equal(token, retrieved);
    
    SecureTokenStorage.DeleteToken();
    Assert.Null(SecureTokenStorage.RetrieveToken());
}
```

---

## Related Documentation

- [Authentication Patterns](./authentication-patterns.md) - Token and credential management
- [Vulnerability Prevention](./vulnerability-prevention.md) - Protecting against attacks
- [Security Testing](./security-testing.md) - Testing security implementations

---

## References

- [Windows Data Protection API](https://docs.microsoft.com/en-us/windows/win32/api/dpapi/)
- [PasswordVault Class](https://docs.microsoft.com/en-us/uwp/api/windows.security.credentials.passwordvault)
- [OWASP Cryptographic Storage Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Cryptographic_Storage_Cheat_Sheet.html)
- [.NET Security Best Practices](https://docs.microsoft.com/en-us/dotnet/standard/security/)
