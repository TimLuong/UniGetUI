# Security Patterns

Security implementation examples for Windows applications.

## Patterns Covered

- **Secure credential storage**
- **Encryption/decryption**
- **Token management**
- **OAuth 2.0 / OIDC authentication**
- **Input validation**
- **SQL injection prevention**
- **Secrets management**

## 1. Secure Credential Storage

### Using Windows Credential Manager (UniGetUI approach)

```csharp
public class SecureCredentialStore
{
    private const string TargetNamePrefix = "UniGetUI";
    
    public void SaveCredential(string key, string value)
    {
        var credential = new Credential
        {
            Target = $"{TargetNamePrefix}_{key}",
            Username = Environment.UserName,
            Password = value,
            Type = CredentialType.Generic,
            PersistenceType = PersistenceType.LocalMachine
        };
        
        credential.Save();
    }
    
    public string? GetCredential(string key)
    {
        var credential = new Credential
        {
            Target = $"{TargetNamePrefix}_{key}"
        };
        
        return credential.Load() ? credential.Password : null;
    }
    
    public void DeleteCredential(string key)
    {
        var credential = new Credential
        {
            Target = $"{TargetNamePrefix}_{key}"
        };
        
        credential.Delete();
    }
}
```

### Using Data Protection API (DPAPI)

```csharp
public class DpapiSecureStorage
{
    public string EncryptString(string plainText)
    {
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] encryptedBytes = ProtectedData.Protect(
            plainBytes,
            entropy: null,
            scope: DataProtectionScope.CurrentUser);
        
        return Convert.ToBase64String(encryptedBytes);
    }
    
    public string DecryptString(string encryptedText)
    {
        byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
        byte[] plainBytes = ProtectedData.Unprotect(
            encryptedBytes,
            entropy: null,
            scope: DataProtectionScope.CurrentUser);
        
        return Encoding.UTF8.GetString(plainBytes);
    }
}
```

## 2. Encryption/Decryption

### AES Encryption

```csharp
public class AesEncryption
{
    private readonly byte[] _key;
    private readonly byte[] _iv;
    
    public AesEncryption(string password)
    {
        using var deriveBytes = new Rfc2898DeriveBytes(
            password,
            Encoding.UTF8.GetBytes("UniGetUISalt"),
            10000,
            HashAlgorithmName.SHA256);
        
        _key = deriveBytes.GetBytes(32); // 256-bit key
        _iv = deriveBytes.GetBytes(16);  // 128-bit IV
    }
    
    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        
        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        using var sw = new StreamWriter(cs);
        
        sw.Write(plainText);
        sw.Close();
        
        return Convert.ToBase64String(ms.ToArray());
    }
    
    public string Decrypt(string cipherText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        
        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        
        return sr.ReadToEnd();
    }
}
```

## 3. Token Management

### Secure Token Manager (as in UniGetUI)

```csharp
public class SecureTokenManager
{
    private readonly DpapiSecureStorage _storage;
    private readonly ILogger<SecureTokenManager> _logger;
    
    public async Task<string?> GetGitHubTokenAsync()
    {
        try
        {
            var encryptedToken = GetStoredEncryptedToken();
            if (string.IsNullOrEmpty(encryptedToken))
            {
                return null;
            }
            
            return _storage.DecryptString(encryptedToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve GitHub token");
            return null;
        }
    }
    
    public async Task SaveGitHubTokenAsync(string token)
    {
        try
        {
            var encryptedToken = _storage.EncryptString(token);
            SaveEncryptedToken(encryptedToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save GitHub token");
            throw;
        }
    }
    
    public async Task DeleteGitHubTokenAsync()
    {
        try
        {
            DeleteStoredToken();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete GitHub token");
            throw;
        }
    }
    
    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
            
            var response = await client.GetAsync("https://api.github.com/user");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
```

## 4. OAuth 2.0 / OIDC Authentication

### GitHub OAuth Implementation

```csharp
public class GitHubOAuthService
{
    private readonly OidcClient _oidcClient;
    private readonly string _clientId;
    private readonly string _redirectUri;
    
    public GitHubOAuthService(string clientId, string redirectUri)
    {
        _clientId = clientId;
        _redirectUri = redirectUri;
        
        var options = new OidcClientOptions
        {
            Authority = "https://github.com/login/oauth",
            ClientId = clientId,
            RedirectUri = redirectUri,
            Scope = "user:email repo",
            Flow = OidcClientOptions.AuthenticationFlow.AuthorizationCode
        };
        
        _oidcClient = new OidcClient(options);
    }
    
    public async Task<LoginResult> LoginAsync()
    {
        try
        {
            // Start the authentication flow
            var state = Guid.NewGuid().ToString("N");
            var authUrl = $"https://github.com/login/oauth/authorize?" +
                         $"client_id={_clientId}&" +
                         $"redirect_uri={Uri.EscapeDataString(_redirectUri)}&" +
                         $"scope=user:email%20repo&" +
                         $"state={state}";
            
            // Open browser for user authentication
            Process.Start(new ProcessStartInfo
            {
                FileName = authUrl,
                UseShellExecute = true
            });
            
            // Wait for callback...
            var result = await WaitForCallbackAsync(state);
            return result;
        }
        catch (Exception ex)
        {
            return new LoginResult
            {
                IsError = true,
                Error = ex.Message
            };
        }
    }
}
```

## 5. Input Validation

### Preventing Code Injection

```csharp
public class InputValidator
{
    private static readonly Regex PackageIdPattern = 
        new(@"^[a-zA-Z0-9\.\-_]+$", RegexOptions.Compiled);
    
    public static bool IsValidPackageId(string packageId)
    {
        if (string.IsNullOrWhiteSpace(packageId))
        {
            return false;
        }
        
        // Check length
        if (packageId.Length > 255)
        {
            return false;
        }
        
        // Check pattern
        if (!PackageIdPattern.IsMatch(packageId))
        {
            return false;
        }
        
        // Check for path traversal
        if (packageId.Contains("..") || packageId.Contains("/") || packageId.Contains("\\"))
        {
            return false;
        }
        
        return true;
    }
    
    public static string SanitizeInput(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }
        
        // Remove dangerous characters
        var sanitized = input
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#x27;")
            .Replace("/", "&#x2F;");
        
        return sanitized;
    }
    
    public static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp 
                || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
```

### Command Injection Prevention

```csharp
public class SecureCommandExecutor
{
    public async Task<string> ExecuteCommandAsync(string command, params string[] args)
    {
        // Validate command path
        if (!IsValidExecutablePath(command))
        {
            throw new SecurityException("Invalid executable path");
        }
        
        // Escape arguments
        var escapedArgs = args.Select(EscapeArgument).ToArray();
        
        var processInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = string.Join(" ", escapedArgs),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        
        using var process = Process.Start(processInfo);
        if (process == null)
        {
            throw new InvalidOperationException("Failed to start process");
        }
        
        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        
        return output;
    }
    
    private static string EscapeArgument(string arg)
    {
        // Escape special characters
        if (arg.Contains(" ") || arg.Contains("\""))
        {
            return $"\"{arg.Replace("\"", "\\\"")}\"";
        }
        return arg;
    }
    
    private static bool IsValidExecutablePath(string path)
    {
        // Check if file exists
        if (!File.Exists(path))
        {
            return false;
        }
        
        // Check for path traversal
        var fullPath = Path.GetFullPath(path);
        return fullPath == path;
    }
}
```

## 6. SQL Injection Prevention

### Using Parameterized Queries

```csharp
public class SecureDataAccess
{
    private readonly string _connectionString;
    
    // WRONG - Vulnerable to SQL injection
    public async Task<Package?> GetPackageWrong(string packageId)
    {
        using var connection = new SqlConnection(_connectionString);
        var query = $"SELECT * FROM Packages WHERE Id = '{packageId}'";
        // Don't do this!
    }
    
    // CORRECT - Using parameters
    public async Task<Package?> GetPackageCorrect(string packageId)
    {
        using var connection = new SqlConnection(_connectionString);
        var query = "SELECT * FROM Packages WHERE Id = @PackageId";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@PackageId", packageId);
        
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            return new Package
            {
                Id = reader.GetString(0),
                Name = reader.GetString(1),
                // ...
            };
        }
        
        return null;
    }
}
```

## 7. Secrets Management

### Configuration with User Secrets

```csharp
public class SecretsManager
{
    private readonly IConfiguration _configuration;
    
    public SecretsManager(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public string GetGitHubClientId()
    {
        return _configuration["GitHub:ClientId"] 
            ?? throw new InvalidOperationException("GitHub ClientId not configured");
    }
    
    public string GetGitHubClientSecret()
    {
        return _configuration["GitHub:ClientSecret"] 
            ?? throw new InvalidOperationException("GitHub ClientSecret not configured");
    }
}
```

### appsettings.json (public)
```json
{
  "GitHub": {
    "RedirectUri": "http://localhost:5000/callback"
  }
}
```

### secrets.json (not in source control)
```json
{
  "GitHub": {
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret"
  }
}
```

## Best Practices Summary

1. ✅ **Never hardcode secrets** - Use secure storage
2. ✅ **Encrypt sensitive data** - Use DPAPI or AES
3. ✅ **Validate all input** - Prevent injection attacks
4. ✅ **Use parameterized queries** - Prevent SQL injection
5. ✅ **Escape command arguments** - Prevent command injection
6. ✅ **Use HTTPS** - Encrypt data in transit
7. ✅ **Implement proper authentication** - OAuth 2.0, OIDC
8. ✅ **Log security events** - Monitor for suspicious activity
9. ✅ **Keep dependencies updated** - Patch vulnerabilities
10. ✅ **Follow principle of least privilege** - Minimize permissions

## Testing Security

```csharp
[Fact]
public void InputValidator_RejectsPathTraversal()
{
    var result = InputValidator.IsValidPackageId("../../etc/passwd");
    Assert.False(result);
}

[Fact]
public void InputValidator_RejectsScriptInjection()
{
    var result = InputValidator.IsValidPackageId("<script>alert('xss')</script>");
    Assert.False(result);
}

[Fact]
public void SecureCommandExecutor_EscapesArguments()
{
    var executor = new SecureCommandExecutor();
    var arg = "test;rm -rf /";
    var escaped = EscapeArgument(arg);
    
    Assert.DoesNotContain(";", escaped);
}
```

## Running the Example

```bash
cd security-patterns
dotnet build
dotnet test
```

## Further Reading

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [.NET Security Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/security/)
- [Secure Coding Practices](https://learn.microsoft.com/en-us/dotnet/standard/security/secure-coding-guidelines)

## License

Part of the UniGetUI project examples.
