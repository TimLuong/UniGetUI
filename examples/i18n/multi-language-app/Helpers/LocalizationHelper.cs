using System;
using System.Globalization;

namespace MultiLanguageApp.Helpers
{
    /// <summary>
    /// Helper class for localization-related formatting and display
    /// </summary>
    public static class LocalizationHelper
    {
        /// <summary>
        /// Formats a date according to the current culture
        /// </summary>
        public static string FormatDate(DateTime date)
        {
            return date.ToString("d", CultureInfo.CurrentCulture);
        }
        
        /// <summary>
        /// Formats a date and time according to the current culture
        /// </summary>
        public static string FormatDateTime(DateTime dateTime)
        {
            return dateTime.ToString("f", CultureInfo.CurrentCulture);
        }
        
        /// <summary>
        /// Formats time according to the current culture
        /// </summary>
        public static string FormatTime(DateTime time)
        {
            return time.ToString("t", CultureInfo.CurrentCulture);
        }
        
        /// <summary>
        /// Formats a number with cultural conventions
        /// </summary>
        public static string FormatNumber(double number, int decimals = 2)
        {
            return number.ToString($"N{decimals}", CultureInfo.CurrentCulture);
        }
        
        /// <summary>
        /// Formats a currency value
        /// </summary>
        public static string FormatCurrency(decimal amount)
        {
            return amount.ToString("C", CultureInfo.CurrentCulture);
        }
        
        /// <summary>
        /// Formats a percentage
        /// </summary>
        public static string FormatPercent(double value, int decimals = 1)
        {
            return value.ToString($"P{decimals}", CultureInfo.CurrentCulture);
        }
        
        /// <summary>
        /// Gets the currency symbol for the current culture
        /// </summary>
        public static string GetCurrencySymbol()
        {
            return CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;
        }
        
        /// <summary>
        /// Gets comprehensive regional information for the current culture
        /// </summary>
        public static RegionalInfo GetRegionalInfo()
        {
            CultureInfo culture = CultureInfo.CurrentCulture;
            RegionInfo region = new RegionInfo(culture.Name);
            
            return new RegionalInfo
            {
                CultureName = culture.Name,
                DisplayName = culture.DisplayName,
                EnglishName = culture.EnglishName,
                NativeName = culture.NativeName,
                CountryName = region.EnglishName,
                CurrencySymbol = region.CurrencySymbol,
                ISOCurrencySymbol = region.ISOCurrencySymbol,
                NumberDecimalSeparator = culture.NumberFormat.NumberDecimalSeparator,
                NumberGroupSeparator = culture.NumberFormat.NumberGroupSeparator,
                ShortDatePattern = culture.DateTimeFormat.ShortDatePattern,
                LongDatePattern = culture.DateTimeFormat.LongDatePattern,
                ShortTimePattern = culture.DateTimeFormat.ShortTimePattern,
                LongTimePattern = culture.DateTimeFormat.LongTimePattern,
                FirstDayOfWeek = culture.DateTimeFormat.FirstDayOfWeek,
                IsRightToLeft = culture.TextInfo.IsRightToLeft
            };
        }
    }
    
    /// <summary>
    /// Class to hold regional information
    /// </summary>
    public class RegionalInfo
    {
        public string CultureName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string EnglishName { get; set; } = string.Empty;
        public string NativeName { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public string CurrencySymbol { get; set; } = string.Empty;
        public string ISOCurrencySymbol { get; set; } = string.Empty;
        public string NumberDecimalSeparator { get; set; } = string.Empty;
        public string NumberGroupSeparator { get; set; } = string.Empty;
        public string ShortDatePattern { get; set; } = string.Empty;
        public string LongDatePattern { get; set; } = string.Empty;
        public string ShortTimePattern { get; set; } = string.Empty;
        public string LongTimePattern { get; set; } = string.Empty;
        public DayOfWeek FirstDayOfWeek { get; set; }
        public bool IsRightToLeft { get; set; }
    }
}
