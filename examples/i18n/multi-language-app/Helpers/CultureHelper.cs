using System;
using System.Globalization;
using System.Threading;

namespace MultiLanguageApp.Helpers
{
    /// <summary>
    /// Helper class for managing application culture and localization
    /// </summary>
    public static class CultureHelper
    {
        /// <summary>
        /// Event raised when the application culture changes
        /// </summary>
        public static event EventHandler? CultureChanged;
        
        /// <summary>
        /// Sets the application's UI culture
        /// </summary>
        /// <param name="cultureName">Culture code (e.g., "fr-FR", "en-US", "ja-JP")</param>
        public static void SetCulture(string cultureName)
        {
            try
            {
                CultureInfo culture = new CultureInfo(cultureName);
                
                // Set culture for the current thread
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
                
                // Set default culture for new threads
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
                
                // Raise culture changed event
                CultureChanged?.Invoke(null, EventArgs.Empty);
            }
            catch (CultureNotFoundException ex)
            {
                // Log error and fall back to default culture
                System.Diagnostics.Debug.WriteLine($"Culture '{cultureName}' not found: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Gets the current UI culture name
        /// </summary>
        public static string GetCurrentCultureName()
        {
            return CultureInfo.CurrentUICulture.Name;
        }
        
        /// <summary>
        /// Gets the current UI culture display name
        /// </summary>
        public static string GetCurrentCultureDisplayName()
        {
            return CultureInfo.CurrentUICulture.DisplayName;
        }
        
        /// <summary>
        /// Checks if the current culture uses right-to-left reading order
        /// </summary>
        public static bool IsCurrentCultureRTL()
        {
            return CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft;
        }
    }
}
