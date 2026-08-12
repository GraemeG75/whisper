namespace WhisperWinForms
{
    public sealed partial class SettingsForm : Form
    {
        private readonly string _initialLanguage;

        public bool LanguageChanged { get; private set; }

        public SettingsForm()
        {
            InitializeComponent();
            _initialLanguage = WhisperSettings.LoadLanguage();
            int selectedIndex = Array.IndexOf(WhisperSettings.SupportedLanguages, _initialLanguage);
            comboBoxLanguage.SelectedIndex = selectedIndex >= 0 ? selectedIndex : 0;
        }

        private void buttonOk_Click(object? sender, EventArgs e)
        {
            string language = WhisperSettings.SupportedLanguages[comboBoxLanguage.SelectedIndex];
            WhisperSettings.SaveLanguage(language);
            LanguageChanged = !string.Equals(language, _initialLanguage, StringComparison.Ordinal);
            DialogResult = DialogResult.OK;
        }
    }
}
