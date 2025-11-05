namespace SecureStorageExample.Models;

/// <summary>
/// Example configuration class with sensitive data
/// </summary>
public class AppConfiguration
{
    public string ApiKey { get; set; } = "";
    public string DatabaseConnection { get; set; } = "";
    public List<string> TrustedServers { get; set; } = new();
    public int MaxConnections { get; set; } = 10;
    public bool EnableLogging { get; set; } = true;
}
