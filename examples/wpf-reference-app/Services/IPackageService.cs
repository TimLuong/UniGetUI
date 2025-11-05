using WpfReferenceApp.Models;

namespace WpfReferenceApp.Services;

/// <summary>
/// Interface for package service implementations
/// Demonstrates Strategy Pattern - defines a family of algorithms (package managers)
/// and makes them interchangeable
/// </summary>
public interface IPackageService
{
    /// <summary>
    /// Gets the name of the package manager
    /// </summary>
    string ServiceName { get; }

    /// <summary>
    /// Gets a list of available packages from this source
    /// This method is fail-safe and will return an empty list if an error occurs
    /// </summary>
    Task<List<Package>> GetAvailablePackagesAsync();

    /// <summary>
    /// Gets a list of installed packages
    /// This method is fail-safe and will return an empty list if an error occurs
    /// </summary>
    Task<List<Package>> GetInstalledPackagesAsync();

    /// <summary>
    /// Installs a package
    /// </summary>
    /// <param name="package">The package to install</param>
    /// <returns>True if installation succeeded, false otherwise</returns>
    Task<bool> InstallPackageAsync(Package package);

    /// <summary>
    /// Uninstalls a package
    /// </summary>
    /// <param name="package">The package to uninstall</param>
    /// <returns>True if uninstallation succeeded, false otherwise</returns>
    Task<bool> UninstallPackageAsync(Package package);

    /// <summary>
    /// Checks if the package manager is available on the system
    /// </summary>
    Task<bool> IsAvailableAsync();
}
