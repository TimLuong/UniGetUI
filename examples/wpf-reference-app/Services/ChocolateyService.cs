using WpfReferenceApp.Models;

namespace WpfReferenceApp.Services;

/// <summary>
/// Chocolatey package manager service implementation
/// Demonstrates Strategy Pattern implementation
/// </summary>
public class ChocolateyService : IPackageService
{
    public string ServiceName => "Chocolatey";

    public async Task<List<Package>> GetAvailablePackagesAsync()
    {
        try
        {
            Utilities.Logger.Info($"[{ServiceName}] Fetching available packages...");
            await Task.Delay(1100).ConfigureAwait(false);

            return new List<Package>
            {
                new Package
                {
                    Id = "googlechrome",
                    Name = "Google Chrome",
                    Version = "120.0.6099.109",
                    Description = "Fast, secure, and free web browser",
                    Source = ServiceName,
                    Publisher = "Google",
                    Size = 80000000,
                    IsInstalled = false
                },
                new Package
                {
                    Id = "firefox",
                    Name = "Mozilla Firefox",
                    Version = "121.0",
                    Description = "Free and open-source web browser",
                    Source = ServiceName,
                    Publisher = "Mozilla",
                    Size = 70000000,
                    IsInstalled = false
                }
            };
        }
        catch (Exception ex)
        {
            Utilities.Logger.Error($"[{ServiceName}] Error fetching available packages: {ex.Message}");
            Utilities.Logger.Error(ex);
            return new List<Package>();
        }
    }

    public async Task<List<Package>> GetInstalledPackagesAsync()
    {
        try
        {
            Utilities.Logger.Info($"[{ServiceName}] Fetching installed packages...");
            await Task.Delay(750).ConfigureAwait(false);
            return new List<Package>();
        }
        catch (Exception ex)
        {
            Utilities.Logger.Error($"[{ServiceName}] Error fetching installed packages: {ex.Message}");
            Utilities.Logger.Error(ex);
            return new List<Package>();
        }
    }

    public async Task<bool> InstallPackageAsync(Package package)
    {
        try
        {
            Utilities.Logger.Info($"[{ServiceName}] Installing package: {package.Name}");
            await Task.Delay(2200).ConfigureAwait(false);
            Utilities.Logger.Info($"[{ServiceName}] Successfully installed: {package.Name}");
            return true;
        }
        catch (Exception ex)
        {
            Utilities.Logger.Error($"[{ServiceName}] Failed to install {package.Name}: {ex.Message}");
            Utilities.Logger.Error(ex);
            return false;
        }
    }

    public async Task<bool> UninstallPackageAsync(Package package)
    {
        try
        {
            Utilities.Logger.Info($"[{ServiceName}] Uninstalling package: {package.Name}");
            await Task.Delay(1400).ConfigureAwait(false);
            Utilities.Logger.Info($"[{ServiceName}] Successfully uninstalled: {package.Name}");
            return true;
        }
        catch (Exception ex)
        {
            Utilities.Logger.Error($"[{ServiceName}] Failed to uninstall {package.Name}: {ex.Message}");
            Utilities.Logger.Error(ex);
            return false;
        }
    }

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            await Task.Delay(100).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            Utilities.Logger.Error($"[{ServiceName}] Error checking availability: {ex.Message}");
            return false;
        }
    }
}
