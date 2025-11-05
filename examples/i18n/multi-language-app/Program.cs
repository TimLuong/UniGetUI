using System;
using System.Windows.Forms;
using System.Globalization;

namespace MultiLanguageApp
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Load saved language preference
            string preferredLanguage = Properties.Settings.Default.PreferredLanguage;
            
            if (!string.IsNullOrEmpty(preferredLanguage))
            {
                try
                {
                    Helpers.CultureHelper.SetCulture(preferredLanguage);
                }
                catch (CultureNotFoundException)
                {
                    // If saved culture is invalid, use system default
                    preferredLanguage = CultureInfo.CurrentUICulture.Name;
                }
            }
            else
            {
                // First run - use system default
                preferredLanguage = CultureInfo.CurrentUICulture.Name;
            }
            
            // Initialize Windows Forms application
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Run main form
            Application.Run(new MainForm());
        }
    }
}
