namespace WpfReferenceApp.Models;

/// <summary>
/// Represents a software package
/// Demonstrates domain model design
/// </summary>
public class Package
{
    /// <summary>
    /// Package unique identifier
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Package display name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Package version
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Package description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Package source (WinGet, Scoop, etc.)
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Publisher name
    /// </summary>
    public string Publisher { get; set; } = string.Empty;

    /// <summary>
    /// Download URL
    /// </summary>
    public string? DownloadUrl { get; set; }

    /// <summary>
    /// Package size in bytes
    /// </summary>
    public long? Size { get; set; }

    /// <summary>
    /// Installation date (if installed)
    /// </summary>
    public DateTime? InstalledDate { get; set; }

    /// <summary>
    /// Whether the package is currently installed
    /// </summary>
    public bool IsInstalled { get; set; }

    public override string ToString()
    {
        return $"{Name} {Version} ({Source})";
    }
}
