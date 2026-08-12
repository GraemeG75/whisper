using System.Globalization;
using System.Text.Json;

namespace WhisperWinForms
{
    internal static class UiLanguageSettings
    {
        private static readonly string SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WhisperWinForms", "settings.json");

        public static string Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    UiLanguageSettingsData? settings = JsonSerializer.Deserialize<UiLanguageSettingsData>(json);
                    if (settings != null && SupportedLanguages.Contains(settings.Language))
                    {
                        return settings.Language;
                    }
                }
            }
            catch (IOException)
            {
            }
            catch (JsonException)
            {
            }

            return "en";
        }

        public static void Save(string language)
        {
            if (!SupportedLanguages.Contains(language))
            {
                return;
            }

            string? directory = Path.GetDirectoryName(SettingsPath);
            if (directory != null)
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonSerializer.Serialize(new UiLanguageSettingsData { Language = language });
            File.WriteAllText(SettingsPath, json);
        }

        public static void Apply()
        {
            string language = Load();
            CultureInfo culture = CultureInfo.GetCultureInfo(language);
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }

        public static readonly string[] SupportedLanguages = ["en", "fr", "de", "es"];

        private sealed class UiLanguageSettingsData
        {
            public string Language { get; set; } = "en";
        }
    }
}
