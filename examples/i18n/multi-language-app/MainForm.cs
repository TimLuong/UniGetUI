using System;
using System.Windows.Forms;
using System.Globalization;
using MultiLanguageApp.Helpers;
using MultiLanguageApp.Resources;

namespace MultiLanguageApp
{
    /// <summary>
    /// Main application form demonstrating localization features
    /// </summary>
    public partial class MainForm : Form
    {
        private ComboBox languageComboBox = new ComboBox();
        private Label welcomeLabel = new Label();
        private GroupBox dateTimeGroupBox = new GroupBox();
        private Label shortDateLabel = new Label();
        private Label longDateLabel = new Label();
        private Label timeLabel = new Label();
        private GroupBox numberGroupBox = new GroupBox();
        private Label numberLabel = new Label();
        private Label currencyLabel = new Label();
        private Label percentLabel = new Label();
        private GroupBox regionalGroupBox = new GroupBox();
        private TextBox regionalInfoTextBox = new TextBox();
        private Button refreshButton = new Button();
        
        public MainForm()
        {
            InitializeComponent();
            InitializeCustomComponents();
            
            // Subscribe to culture changed event
            CultureHelper.CultureChanged += OnCultureChanged;
            
            // Apply RTL layout if needed
            RTLHelper.ApplyRTLLayout(this);
            
            // Initial UI refresh
            RefreshUIText();
            RefreshFormattedContent();
        }
        
        private void InitializeCustomComponents()
        {
            // Form properties
            this.Text = "Multi-Language Application Example";
            this.Size = new System.Drawing.Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            
            // Language selector
            languageComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            languageComboBox.Location = new System.Drawing.Point(20, 20);
            languageComboBox.Size = new System.Drawing.Size(200, 25);
            languageComboBox.SelectedIndexChanged += LanguageComboBox_SelectedIndexChanged;
            
            // Add supported languages
            languageComboBox.Items.Add(new LanguageItem("English (United States)", "en-US"));
            languageComboBox.Items.Add(new LanguageItem("Français (France)", "fr-FR"));
            languageComboBox.Items.Add(new LanguageItem("Deutsch (Deutschland)", "de-DE"));
            languageComboBox.Items.Add(new LanguageItem("日本語 (日本)", "ja-JP"));
            languageComboBox.Items.Add(new LanguageItem("العربية (السعودية)", "ar-SA"));
            
            // Set current language
            string currentCulture = CultureInfo.CurrentUICulture.Name;
            for (int i = 0; i < languageComboBox.Items.Count; i++)
            {
                if (((LanguageItem)languageComboBox.Items[i]).CultureCode == currentCulture)
                {
                    languageComboBox.SelectedIndex = i;
                    break;
                }
            }
            if (languageComboBox.SelectedIndex == -1)
                languageComboBox.SelectedIndex = 0;
            
            // Welcome label
            welcomeLabel.Location = new System.Drawing.Point(20, 60);
            welcomeLabel.Size = new System.Drawing.Size(740, 30);
            welcomeLabel.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            
            // Date/Time group box
            dateTimeGroupBox.Location = new System.Drawing.Point(20, 100);
            dateTimeGroupBox.Size = new System.Drawing.Size(360, 150);
            
            shortDateLabel.Location = new System.Drawing.Point(10, 30);
            shortDateLabel.Size = new System.Drawing.Size(340, 25);
            
            longDateLabel.Location = new System.Drawing.Point(10, 60);
            longDateLabel.Size = new System.Drawing.Size(340, 25);
            
            timeLabel.Location = new System.Drawing.Point(10, 90);
            timeLabel.Size = new System.Drawing.Size(340, 25);
            
            dateTimeGroupBox.Controls.AddRange(new Control[] { shortDateLabel, longDateLabel, timeLabel });
            
            // Number group box
            numberGroupBox.Location = new System.Drawing.Point(400, 100);
            numberGroupBox.Size = new System.Drawing.Size(360, 150);
            
            numberLabel.Location = new System.Drawing.Point(10, 30);
            numberLabel.Size = new System.Drawing.Size(340, 25);
            
            currencyLabel.Location = new System.Drawing.Point(10, 60);
            currencyLabel.Size = new System.Drawing.Size(340, 25);
            
            percentLabel.Location = new System.Drawing.Point(10, 90);
            percentLabel.Size = new System.Drawing.Size(340, 25);
            
            numberGroupBox.Controls.AddRange(new Control[] { numberLabel, currencyLabel, percentLabel });
            
            // Regional info group box
            regionalGroupBox.Location = new System.Drawing.Point(20, 270);
            regionalGroupBox.Size = new System.Drawing.Size(740, 250);
            
            regionalInfoTextBox.Location = new System.Drawing.Point(10, 30);
            regionalInfoTextBox.Size = new System.Drawing.Size(720, 210);
            regionalInfoTextBox.Multiline = true;
            regionalInfoTextBox.ReadOnly = true;
            regionalInfoTextBox.ScrollBars = ScrollBars.Vertical;
            regionalInfoTextBox.Font = new System.Drawing.Font("Consolas", 9F);
            
            regionalGroupBox.Controls.Add(regionalInfoTextBox);
            
            // Refresh button
            refreshButton.Location = new System.Drawing.Point(650, 530);
            refreshButton.Size = new System.Drawing.Size(110, 30);
            refreshButton.Click += RefreshButton_Click;
            
            // Add all controls to form
            this.Controls.AddRange(new Control[]
            {
                languageComboBox, welcomeLabel, dateTimeGroupBox,
                numberGroupBox, regionalGroupBox, refreshButton
            });
        }
        
        private void LanguageComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (languageComboBox.SelectedItem is LanguageItem selectedLanguage)
            {
                // Set new culture
                CultureHelper.SetCulture(selectedLanguage.CultureCode);
                
                // Save preference
                Properties.Settings.Default.PreferredLanguage = selectedLanguage.CultureCode;
                Properties.Settings.Default.Save();
                
                // Apply RTL layout if needed
                RTLHelper.ApplyRTLLayout(this);
            }
        }
        
        private void RefreshButton_Click(object? sender, EventArgs e)
        {
            RefreshFormattedContent();
        }
        
        private void OnCultureChanged(object? sender, EventArgs e)
        {
            RefreshUIText();
            RefreshFormattedContent();
        }
        
        private void RefreshUIText()
        {
            // Update form title
            this.Text = Strings.ApplicationTitle;
            
            // Update labels
            welcomeLabel.Text = Strings.WelcomeMessage;
            dateTimeGroupBox.Text = Strings.DateTimeFormatting;
            numberGroupBox.Text = Strings.NumberFormatting;
            regionalGroupBox.Text = Strings.RegionalInformation;
            refreshButton.Text = Strings.RefreshButton;
        }
        
        private void RefreshFormattedContent()
        {
            DateTime now = DateTime.Now;
            
            // Date/Time formatting
            shortDateLabel.Text = $"{Strings.ShortDate}: {LocalizationHelper.FormatDate(now)}";
            longDateLabel.Text = $"{Strings.LongDate}: {LocalizationHelper.FormatDateTime(now)}";
            timeLabel.Text = $"{Strings.Time}: {LocalizationHelper.FormatTime(now)}";
            
            // Number formatting
            double sampleNumber = 1234567.89;
            decimal sampleCurrency = 1234.56m;
            double samplePercent = 0.7534;
            
            numberLabel.Text = $"{Strings.Number}: {LocalizationHelper.FormatNumber(sampleNumber)}";
            currencyLabel.Text = $"{Strings.Currency}: {LocalizationHelper.FormatCurrency(sampleCurrency)}";
            percentLabel.Text = $"{Strings.Percent}: {LocalizationHelper.FormatPercent(samplePercent)}";
            
            // Regional information
            var regionalInfo = LocalizationHelper.GetRegionalInfo();
            regionalInfoTextBox.Text = $@"{Strings.CultureName}: {regionalInfo.CultureName}
{Strings.DisplayName}: {regionalInfo.DisplayName}
{Strings.EnglishName}: {regionalInfo.EnglishName}
{Strings.NativeName}: {regionalInfo.NativeName}
{Strings.CountryName}: {regionalInfo.CountryName}
{Strings.CurrencySymbol}: {regionalInfo.CurrencySymbol} ({regionalInfo.ISOCurrencySymbol})
{Strings.DecimalSeparator}: '{regionalInfo.NumberDecimalSeparator}'
{Strings.ThousandsSeparator}: '{regionalInfo.NumberGroupSeparator}'
{Strings.ShortDatePattern}: {regionalInfo.ShortDatePattern}
{Strings.LongDatePattern}: {regionalInfo.LongDatePattern}
{Strings.ShortTimePattern}: {regionalInfo.ShortTimePattern}
{Strings.LongTimePattern}: {regionalInfo.LongTimePattern}
{Strings.FirstDayOfWeek}: {regionalInfo.FirstDayOfWeek}
{Strings.IsRightToLeft}: {regionalInfo.IsRightToLeft}";
        }
        
        /// <summary>
        /// Language item for combo box
        /// </summary>
        private class LanguageItem
        {
            public string DisplayName { get; set; }
            public string CultureCode { get; set; }
            
            public LanguageItem(string displayName, string cultureCode)
            {
                DisplayName = displayName;
                CultureCode = cultureCode;
            }
            
            public override string ToString() => DisplayName;
        }
    }
}
