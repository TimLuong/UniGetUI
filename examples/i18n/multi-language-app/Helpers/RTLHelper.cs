using System;
using System.Globalization;
using System.Windows.Forms;

namespace MultiLanguageApp.Helpers
{
    /// <summary>
    /// Helper class for Right-to-Left (RTL) language support
    /// </summary>
    public static class RTLHelper
    {
        /// <summary>
        /// Determines if the current culture uses RTL reading order
        /// </summary>
        public static bool IsRTLCulture()
        {
            return CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft;
        }
        
        /// <summary>
        /// Applies RTL layout to a form based on current culture
        /// </summary>
        public static void ApplyRTLLayout(Form form)
        {
            if (IsRTLCulture())
            {
                form.RightToLeft = RightToLeft.Yes;
                form.RightToLeftLayout = true;
            }
            else
            {
                form.RightToLeft = RightToLeft.No;
                form.RightToLeftLayout = false;
            }
        }
        
        /// <summary>
        /// Gets the appropriate text alignment for the current culture
        /// </summary>
        public static HorizontalAlignment GetCultureAwareAlignment()
        {
            return IsRTLCulture() ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        }
    }
}
