using Windows.Security.Credentials;

namespace SecureStorageExample.Services;

/// <summary>
/// Secure storage implementation using Windows Credential Manager
/// </summary>
public class WindowsCredentialStorage : ISecureStorage
{
    private readonly string _resourcePrefix;
    private readonly string _userName;
    
    public WindowsCredentialStorage(string applicationName)
    {
        if (string.IsNullOrWhiteSpace(applicationName))
            throw new ArgumentException("Application name cannot be empty", nameof(applicationName));
        
        _resourcePrefix = applicationName;
        _userName = Environment.UserName;
    }
    
    /// <summary>
    /// Stores a value in Windows Credential Manager
    /// </summary>
    public void Store(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be empty", nameof(key));
        
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be empty", nameof(value));
        
        try
        {
            var vault = new PasswordVault();
            var resourceName = GetResourceName(key);
            
            // Remove existing credential if present
            if (Exists(key))
                Delete(key);
            
            var credential = new PasswordCredential(resourceName, _userName, value);
            vault.Add(credential);
            
            Console.WriteLine($"✓ Stored '{key}' securely in Credential Manager");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to store credential for key '{key}'", ex);
        }
    }
    
    /// <summary>
    /// Retrieves a value from Windows Credential Manager
    /// </summary>
    public string? Retrieve(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be empty", nameof(key));
        
        try
        {
            var vault = new PasswordVault();
            var resourceName = GetResourceName(key);
            var credential = vault.Retrieve(resourceName, _userName);
            
            // Explicitly retrieve the password
            credential.RetrievePassword();
            
            Console.WriteLine($"✓ Retrieved '{key}' from Credential Manager");
            return credential.Password;
        }
        catch (Exception)
        {
            return null;
        }
    }
    
    /// <summary>
    /// Deletes a value from Windows Credential Manager
    /// </summary>
    public void Delete(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be empty", nameof(key));
        
        try
        {
            var vault = new PasswordVault();
            var resourceName = GetResourceName(key);
            var credentials = vault.FindAllByResource(resourceName);
            
            foreach (var credential in credentials)
            {
                vault.Remove(credential);
                Console.WriteLine($"✓ Deleted '{key}' from Credential Manager");
            }
        }
        catch (Exception)
        {
            // Credential doesn't exist, nothing to delete
        }
    }
    
    /// <summary>
    /// Checks if a key exists in Windows Credential Manager
    /// </summary>
    public bool Exists(string key)
    {
        return Retrieve(key) != null;
    }
    
    private string GetResourceName(string key)
    {
        return $"{_resourcePrefix}/{key}";
    }
}
