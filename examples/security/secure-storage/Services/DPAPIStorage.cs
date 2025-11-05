using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SecureStorageExample.Services;

/// <summary>
/// Secure storage implementation using Data Protection API (DPAPI)
/// </summary>
public class DPAPIStorage
{
    private readonly string _storageDirectory;
    private readonly byte[]? _entropy;
    
    public DPAPIStorage(string applicationName, string? password = null)
    {
        if (string.IsNullOrWhiteSpace(applicationName))
            throw new ArgumentException("Application name cannot be empty", nameof(applicationName));
        
        _storageDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            applicationName,
            "SecureData"
        );
        
        _entropy = password != null ? Encoding.UTF8.GetBytes(password) : null;
        
        if (!Directory.Exists(_storageDirectory))
            Directory.CreateDirectory(_storageDirectory);
    }
    
    /// <summary>
    /// Saves a configuration object securely using DPAPI
    /// </summary>
    public void SaveConfiguration<T>(T configuration, string fileName = "config.dat") where T : class
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));
        
        try
        {
            // Serialize to JSON
            var json = JsonSerializer.Serialize(configuration, new JsonSerializerOptions
            {
                WriteIndented = false
            });
            
            var plainBytes = Encoding.UTF8.GetBytes(json);
            
            // Encrypt using DPAPI
            var encryptedBytes = ProtectedData.Protect(
                plainBytes,
                _entropy,
                DataProtectionScope.CurrentUser
            );
            
            var filePath = Path.Combine(_storageDirectory, fileName);
            File.WriteAllBytes(filePath, encryptedBytes);
            
            Console.WriteLine($"✓ Configuration saved securely to '{filePath}'");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to save configuration", ex);
        }
    }
    
    /// <summary>
    /// Loads a configuration object from encrypted storage
    /// </summary>
    public T? LoadConfiguration<T>(string fileName = "config.dat") where T : class
    {
        var filePath = Path.Combine(_storageDirectory, fileName);
        
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"! Configuration file not found: '{filePath}'");
            return null;
        }
        
        try
        {
            var encryptedBytes = File.ReadAllBytes(filePath);
            
            // Decrypt using DPAPI
            var plainBytes = ProtectedData.Unprotect(
                encryptedBytes,
                _entropy,
                DataProtectionScope.CurrentUser
            );
            
            var json = Encoding.UTF8.GetString(plainBytes);
            var configuration = JsonSerializer.Deserialize<T>(json);
            
            Console.WriteLine($"✓ Configuration loaded from '{filePath}'");
            return configuration;
        }
        catch (CryptographicException ex)
        {
            Console.WriteLine($"✗ Failed to decrypt configuration: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to load configuration", ex);
        }
    }
    
    /// <summary>
    /// Securely deletes a configuration file
    /// </summary>
    public void DeleteConfiguration(string fileName = "config.dat")
    {
        var filePath = Path.Combine(_storageDirectory, fileName);
        
        if (File.Exists(filePath))
        {
            try
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
                Console.WriteLine($"✓ Configuration securely deleted: '{filePath}'");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to delete configuration", ex);
            }
        }
    }
}
