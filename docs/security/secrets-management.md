# Secrets Management Best Practices

## Overview

This document provides comprehensive guidance on managing secrets, API keys, and sensitive configuration in Windows desktop applications.

## Table of Contents

1. [What Are Secrets?](#what-are-secrets)
2. [Secret Storage Options](#secret-storage-options)
3. [Azure Key Vault Integration](#azure-key-vault-integration)
4. [Environment Variables](#environment-variables)
5. [Configuration Management](#configuration-management)
6. [Development vs Production](#development-vs-production)
7. [Secret Rotation](#secret-rotation)
8. [Best Practices](#best-practices)

---

## What Are Secrets?

### Types of Secrets

- **API Keys** - Third-party service authentication keys
- **Connection Strings** - Database connection information
- **Certificates** - Code signing and TLS certificates
- **Tokens** - OAuth tokens, session tokens
- **Passwords** - Service account passwords
- **Private Keys** - Cryptographic keys
- **Client Secrets** - OAuth client secrets

### What NOT to Do

❌ **Never commit secrets to source control**
```csharp
// DON'T DO THIS
public class Config
{
    public const string ApiKey = "sk_live_123456789"; // NEVER!
    public const string DatabasePassword = "MyP@ssw0rd"; // NEVER!
}
```

❌ **Never log secrets**
```csharp
// DON'T DO THIS
Logger.Info($"Using API key: {apiKey}"); // NEVER!
Logger.Debug($"Database connection: {connectionString}"); // NEVER!
```

❌ **Never store secrets in plain text files**
```json
// config.json - DON'T DO THIS
{
  "ApiKey": "sk_live_123456789",
  "DatabasePassword": "MyP@ssw0rd"
}
```

---

## Secret Storage Options

### 1. Windows Credential Manager

**Best for:** User-specific credentials, OAuth tokens

```csharp
using Windows.Security.Credentials;

public class SecretStore
{
    private const string ResourceName = "MyApp/Secrets";
    
    public static void StoreSecret(string key, string value)
    {
        var vault = new PasswordVault();
        var credential = new PasswordCredential(
            $"{ResourceName}/{key}",
            Environment.UserName,
            value
        );
        vault.Add(credential);
    }
    
    public static string? GetSecret(string key)
    {
        try
        {
            var vault = new PasswordVault();
            var credential = vault.Retrieve(
                $"{ResourceName}/{key}",
                Environment.UserName
            );
            credential.RetrievePassword();
            return credential.Password;
        }
        catch
        {
            return null;
        }
    }
}
```

### 2. DPAPI (Data Protection API)

**Best for:** Application-wide secrets, configuration files

```csharp
using System.Security.Cryptography;
using System.Text;

public class DPAPISecrets
{
    public static string Protect(string plainText)
    {
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = ProtectedData.Protect(
            plainBytes,
            null,
            DataProtectionScope.CurrentUser
        );
        return Convert.ToBase64String(encryptedBytes);
    }
    
    public static string Unprotect(string encryptedText)
    {
        var encryptedBytes = Convert.FromBase64String(encryptedText);
        var plainBytes = ProtectedData.Unprotect(
            encryptedBytes,
            null,
            DataProtectionScope.CurrentUser
        );
        return Encoding.UTF8.GetString(plainBytes);
    }
}
```

### 3. Azure Key Vault (Recommended for Enterprise)

**Best for:** Centralized secret management, enterprise applications

See [Azure Key Vault Integration](#azure-key-vault-integration) section.

### 4. Environment Variables

**Best for:** Development, CI/CD pipelines

See [Environment Variables](#environment-variables) section.

---

## Azure Key Vault Integration

### Overview

Azure Key Vault provides:
- Centralized secret management
- Access control and auditing
- Automatic secret rotation
- HSM-backed key storage
- Integration with Azure AD

### Setup

**1. Install NuGet Package:**
```bash
dotnet add package Azure.Identity
dotnet add package Azure.Security.KeyVault.Secrets
```

**2. Create Key Vault:**
```bash
# Azure CLI
az keyvault create \
    --name myapp-keyvault \
    --resource-group myapp-rg \
    --location eastus

# Add secret
az keyvault secret set \
    --vault-name myapp-keyvault \
    --name "DatabasePassword" \
    --value "MySecurePassword123!"
```

### Implementation

```csharp
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

public class AzureKeyVaultService
{
    private readonly SecretClient _client;
    private readonly Dictionary<string, string> _cache;
    
    public AzureKeyVaultService(string keyVaultUrl)
    {
        var credential = new DefaultAzureCredential();
        _client = new SecretClient(new Uri(keyVaultUrl), credential);
        _cache = new Dictionary<string, string>();
    }
    
    public async Task<string> GetSecretAsync(string secretName)
    {
        // Check cache first
        if (_cache.TryGetValue(secretName, out var cachedValue))
            return cachedValue;
        
        try
        {
            var secret = await _client.GetSecretAsync(secretName);
            var value = secret.Value.Value;
            
            // Cache for performance (5 minutes)
            _cache[secretName] = value;
            Task.Delay(TimeSpan.FromMinutes(5)).ContinueWith(_ => 
                _cache.Remove(secretName)
            );
            
            return value;
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to retrieve secret '{secretName}': {ex.Message}");
            throw;
        }
    }
    
    public async Task SetSecretAsync(string secretName, string secretValue)
    {
        try
        {
            await _client.SetSecretAsync(secretName, secretValue);
            _cache.Remove(secretName); // Invalidate cache
            Logger.Info($"Secret '{secretName}' updated in Key Vault");
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to set secret '{secretName}': {ex.Message}");
            throw;
        }
    }
}

// Usage
public class Application
{
    private readonly AzureKeyVaultService _keyVault;
    
    public Application()
    {
        var keyVaultUrl = "https://myapp-keyvault.vault.azure.net/";
        _keyVault = new AzureKeyVaultService(keyVaultUrl);
    }
    
    public async Task InitializeAsync()
    {
        var apiKey = await _keyVault.GetSecretAsync("ApiKey");
        var dbPassword = await _keyVault.GetSecretAsync("DatabasePassword");
        
        // Use secrets
        ConfigureApi(apiKey);
        ConfigureDatabase(dbPassword);
    }
}
```

### Authentication Options

**1. Managed Identity (Recommended for Azure)**
```csharp
// Automatically uses managed identity when running on Azure
var credential = new DefaultAzureCredential();
```

**2. Service Principal**
```csharp
var credential = new ClientSecretCredential(
    tenantId: "your-tenant-id",
    clientId: "your-client-id",
    clientSecret: Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET")
);
```

**3. Interactive (Development)**
```csharp
var credential = new InteractiveBrowserCredential();
```

### Access Control

```bash
# Grant application access to Key Vault
az keyvault set-policy \
    --name myapp-keyvault \
    --object-id <application-object-id> \
    --secret-permissions get list
```

---

## Environment Variables

### Overview

Environment variables are suitable for:
- Development configuration
- CI/CD pipelines
- Container deployments
- Temporary secrets

### Usage

```csharp
public class EnvironmentSecrets
{
    public static string? GetSecret(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);
        
        if (string.IsNullOrEmpty(value))
        {
            Logger.Warn($"Environment variable '{key}' not found");
        }
        
        return value;
    }
    
    public static string GetRequiredSecret(string key)
    {
        var value = GetSecret(key);
        
        if (string.IsNullOrEmpty(value))
        {
            throw new InvalidOperationException(
                $"Required environment variable '{key}' is not set"
            );
        }
        
        return value;
    }
}

// Usage
var apiKey = EnvironmentSecrets.GetRequiredSecret("MYAPP_API_KEY");
var dbConnection = EnvironmentSecrets.GetRequiredSecret("MYAPP_DB_CONNECTION");
```

### Setting Environment Variables

**Windows:**
```powershell
# User-level
[Environment]::SetEnvironmentVariable("MYAPP_API_KEY", "secret-key", "User")

# System-level (requires admin)
[Environment]::SetEnvironmentVariable("MYAPP_API_KEY", "secret-key", "Machine")
```

**CI/CD (GitHub Actions):**
```yaml
jobs:
  build:
    runs-on: windows-latest
    env:
      MYAPP_API_KEY: ${{ secrets.API_KEY }}
      MYAPP_DB_CONNECTION: ${{ secrets.DB_CONNECTION }}
```

### .env Files (Development Only)

```csharp
// Install: dotnet add package DotNetEnv

public class DevelopmentSecrets
{
    public static void LoadFromFile(string filePath = ".env")
    {
        if (File.Exists(filePath))
        {
            DotNetEnv.Env.Load(filePath);
            Logger.Info("Loaded secrets from .env file");
        }
    }
}

// .env file (NEVER commit to git!)
MYAPP_API_KEY=sk_test_123456789
MYAPP_DB_CONNECTION=Server=localhost;...
```

**Add to .gitignore:**
```
.env
.env.local
.env.*.local
```

---

## Configuration Management

### Hierarchical Configuration

```csharp
using Microsoft.Extensions.Configuration;

public class SecureConfiguration
{
    private readonly IConfiguration _configuration;
    
    public SecureConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.local.json", optional: true) // Local overrides
            .AddEnvironmentVariables("MYAPP_") // Environment variables
            .AddUserSecrets<SecureConfiguration>(); // User secrets (dev only)
        
        _configuration = builder.Build();
    }
    
    public string GetSecret(string key)
    {
        var value = _configuration[key];
        
        if (string.IsNullOrEmpty(value))
        {
            throw new InvalidOperationException($"Configuration key '{key}' not found");
        }
        
        return value;
    }
    
    public T GetSection<T>(string sectionName) where T : new()
    {
        var section = _configuration.GetSection(sectionName);
        var instance = new T();
        section.Bind(instance);
        return instance;
    }
}

// appsettings.json (committed to git)
{
  "ApiSettings": {
    "BaseUrl": "https://api.example.com",
    "Timeout": 30
  }
}

// appsettings.local.json (NOT committed to git)
{
  "ApiSettings": {
    "ApiKey": "sk_test_123456789"
  },
  "Database": {
    "ConnectionString": "Server=localhost;..."
  }
}
```

### User Secrets (Development)

```bash
# Initialize user secrets
dotnet user-secrets init

# Add secret
dotnet user-secrets set "ApiKey" "sk_test_123456789"
dotnet user-secrets set "Database:Password" "MyDevPassword"

# List secrets
dotnet user-secrets list

# Remove secret
dotnet user-secrets remove "ApiKey"
```

**Location:** 
- Windows: `%APPDATA%\Microsoft\UserSecrets\<user_secrets_id>\secrets.json`
- Encrypted by DPAPI automatically

---

## Development vs Production

### Development Secrets

```csharp
public class DevelopmentSecretsProvider : ISecretsProvider
{
    public string GetSecret(string key)
    {
        // Try user secrets first
        var userSecret = UserSecrets.Get(key);
        if (userSecret != null)
            return userSecret;
        
        // Try environment variables
        var envVar = Environment.GetEnvironmentVariable($"MYAPP_{key}");
        if (envVar != null)
            return envVar;
        
        // Try .env file
        DotNetEnv.Env.Load();
        var envFile = Environment.GetEnvironmentVariable(key);
        if (envFile != null)
            return envFile;
        
        throw new InvalidOperationException($"Secret '{key}' not found");
    }
}
```

### Production Secrets

```csharp
public class ProductionSecretsProvider : ISecretsProvider
{
    private readonly AzureKeyVaultService _keyVault;
    
    public ProductionSecretsProvider()
    {
        var keyVaultUrl = Environment.GetEnvironmentVariable("KEY_VAULT_URL")
            ?? throw new InvalidOperationException("KEY_VAULT_URL not set");
        
        _keyVault = new AzureKeyVaultService(keyVaultUrl);
    }
    
    public string GetSecret(string key)
    {
        return _keyVault.GetSecretAsync(key).GetAwaiter().GetResult();
    }
}
```

### Factory Pattern

```csharp
public interface ISecretsProvider
{
    string GetSecret(string key);
}

public static class SecretsProviderFactory
{
    public static ISecretsProvider Create()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? "Production";
        
        return environment switch
        {
            "Development" => new DevelopmentSecretsProvider(),
            "Production" => new ProductionSecretsProvider(),
            _ => throw new InvalidOperationException($"Unknown environment: {environment}")
        };
    }
}

// Usage
var secretsProvider = SecretsProviderFactory.Create();
var apiKey = secretsProvider.GetSecret("ApiKey");
```

---

## Secret Rotation

### Automatic Rotation with Azure Key Vault

```csharp
public class RotatingSecretProvider
{
    private readonly AzureKeyVaultService _keyVault;
    private readonly Dictionary<string, (string Value, DateTime Expiry)> _cache;
    private readonly TimeSpan _cacheLifetime = TimeSpan.FromHours(1);
    
    public RotatingSecretProvider(AzureKeyVaultService keyVault)
    {
        _keyVault = keyVault;
        _cache = new Dictionary<string, (string, DateTime)>();
    }
    
    public async Task<string> GetSecretAsync(string secretName)
    {
        // Check if cached value is still valid
        if (_cache.TryGetValue(secretName, out var cached))
        {
            if (DateTime.UtcNow < cached.Expiry)
                return cached.Value;
        }
        
        // Fetch new value from Key Vault
        var value = await _keyVault.GetSecretAsync(secretName);
        
        // Cache with expiration
        _cache[secretName] = (value, DateTime.UtcNow + _cacheLifetime);
        
        return value;
    }
    
    public void InvalidateCache(string secretName)
    {
        _cache.Remove(secretName);
        Logger.Info($"Cache invalidated for secret '{secretName}'");
    }
}
```

### Manual Rotation Process

1. **Generate new secret**
2. **Add new secret to Key Vault** (version automatically incremented)
3. **Deploy applications** (will fetch new secret on next refresh)
4. **Verify all instances using new secret**
5. **Disable old secret version**
6. **Delete old secret version** (after grace period)

### API Key Rotation Example

```csharp
public class ApiKeyRotationManager
{
    private readonly AzureKeyVaultService _keyVault;
    private readonly string _primaryKeyName = "ApiKey-Primary";
    private readonly string _secondaryKeyName = "ApiKey-Secondary";
    
    public async Task RotateApiKeyAsync()
    {
        // Step 1: Generate new key
        var newKey = GenerateSecureApiKey();
        
        // Step 2: Get current primary key
        var currentPrimary = await _keyVault.GetSecretAsync(_primaryKeyName);
        
        // Step 3: Move primary to secondary
        await _keyVault.SetSecretAsync(_secondaryKeyName, currentPrimary);
        
        // Step 4: Set new primary
        await _keyVault.SetSecretAsync(_primaryKeyName, newKey);
        
        Logger.Info("API key rotation completed");
        
        // Step 5: Wait for propagation (allow time for all instances to refresh)
        await Task.Delay(TimeSpan.FromMinutes(10));
        
        // Step 6: Verify all services using new key
        // (implementation depends on your monitoring setup)
    }
    
    private string GenerateSecureApiKey()
    {
        var bytes = new byte[32];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return Convert.ToBase64String(bytes);
    }
}
```

---

## Best Practices

### 1. Never Commit Secrets

**Use .gitignore:**
```
# Secrets
.env
.env.*
secrets.json
appsettings.local.json
*.pfx
*.p12

# Azure
local.settings.json

# User-specific files
*.user
*.suo
```

**Pre-commit Hook:**
```bash
#!/bin/bash
# .git/hooks/pre-commit

# Check for potential secrets
if git diff --cached | grep -iE "(password|api[_-]?key|secret|token|private[_-]?key)" > /dev/null; then
    echo "⚠️  Warning: Potential secrets detected!"
    echo "Please review before committing."
    exit 1
fi
```

### 2. Principle of Least Privilege

```csharp
// DON'T: Grant broad permissions
az keyvault set-policy --secret-permissions all

// DO: Grant minimal required permissions
az keyvault set-policy --secret-permissions get list
```

### 3. Audit Access

```csharp
public class AuditedSecretsProvider : ISecretsProvider
{
    private readonly ISecretsProvider _inner;
    
    public AuditedSecretsProvider(ISecretsProvider inner)
    {
        _inner = inner;
    }
    
    public string GetSecret(string key)
    {
        Logger.Audit($"Secret access: {key} by {Environment.UserName}");
        return _inner.GetSecret(key);
    }
}
```

### 4. Encrypt at Rest

```csharp
// Always use DPAPI or Key Vault for local storage
var encrypted = ProtectedData.Protect(secretBytes, null, DataProtectionScope.CurrentUser);
```

### 5. Clear from Memory

```csharp
public class SecureString : IDisposable
{
    private byte[]? _data;
    
    public SecureString(string value)
    {
        _data = Encoding.UTF8.GetBytes(value);
    }
    
    public string GetValue()
    {
        if (_data == null)
            throw new ObjectDisposedException(nameof(SecureString));
        
        return Encoding.UTF8.GetString(_data);
    }
    
    public void Dispose()
    {
        if (_data != null)
        {
            Array.Clear(_data, 0, _data.Length);
            _data = null;
        }
    }
}
```

### 6. Rotate Regularly

- **API Keys:** Every 90 days
- **Passwords:** Every 60-90 days
- **Certificates:** Before expiration (automated)
- **Tokens:** Based on lifetime policy

### 7. Monitor Usage

```csharp
public class MonitoredSecretsProvider : ISecretsProvider
{
    private readonly ISecretsProvider _inner;
    private readonly Dictionary<string, int> _accessCount;
    
    public MonitoredSecretsProvider(ISecretsProvider inner)
    {
        _inner = inner;
        _accessCount = new Dictionary<string, int>();
    }
    
    public string GetSecret(string key)
    {
        _accessCount.TryGetValue(key, out var count);
        _accessCount[key] = count + 1;
        
        // Alert on unusual access patterns
        if (count > 1000)
        {
            Logger.Warn($"High access count for secret '{key}': {count}");
        }
        
        return _inner.GetSecret(key);
    }
}
```

---

## Security Checklist

- [ ] No secrets in source code
- [ ] No secrets in configuration files committed to git
- [ ] Secrets stored in Key Vault or Credential Manager
- [ ] Environment variables used for non-production
- [ ] User secrets used for development
- [ ] .gitignore configured to exclude secrets
- [ ] Pre-commit hooks to detect secrets
- [ ] Access to secrets audited
- [ ] Secrets rotated regularly
- [ ] Least privilege access control
- [ ] Secrets cleared from memory after use
- [ ] Backup and recovery plan for secrets

---

## Related Documentation

- [Authentication Patterns](./authentication-patterns.md) - Token management
- [Data Protection](./data-protection.md) - Encryption implementation
- [Code Signing and Deployment](./code-signing-deployment.md) - Certificate management

---

## References

- [Azure Key Vault Documentation](https://docs.microsoft.com/en-us/azure/key-vault/)
- [ASP.NET Core Secret Manager](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [OWASP Secrets Management](https://cheatsheetseries.owasp.org/cheatsheets/Secrets_Management_Cheat_Sheet.html)
- [NIST Key Management](https://csrc.nist.gov/projects/key-management)
