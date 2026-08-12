using System.Globalization;
using System.Text.Json;

namespace WhisperWinForms
{
    /// <summary>Persists app-wide user preferences (UI language, last-used batch folders) in a single JSON file.</summary>
    internal static class WhisperSettings
    {
        private static readonly string SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WhisperWinForms", "settings.json");

        public static readonly string[] SupportedLanguages = ["en", "fr", "de", "es"];

        public static string LoadLanguage()
        {
            string language = LoadData().Language;
            return SupportedLanguages.Contains(language) ? language : "en";
        }

        public static void SaveLanguage(string language)
        {
            if (!SupportedLanguages.Contains(language))
            {
                return;
            }

            WhisperSettingsData data = LoadData();
            data.Language = language;
            SaveData(data);
        }

        public static (string? InputFolder, string? OutputFolder) LoadBatchFolders()
        {
            WhisperSettingsData data = LoadData();
            return (data.InputFolder, data.OutputFolder);
        }

        public static void SaveBatchFolders(string inputFolder, string outputFolder)
        {
            WhisperSettingsData data = LoadData();
            data.InputFolder = inputFolder;
            data.OutputFolder = outputFolder;
            SaveData(data);
        }

        public static void Apply()
        {
            string language = LoadLanguage();
            CultureInfo culture = CultureInfo.GetCultureInfo(language);
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }

        private static WhisperSettingsData LoadData()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    WhisperSettingsData? settings = JsonSerializer.Deserialize<WhisperSettingsData>(json);
                    if (settings != null)
                    {
                        return settings;
                    }
                }
            }
            catch (IOException)
            {
            }
            catch (JsonException)
            {
            }

            return new WhisperSettingsData();
        }

        private static void SaveData(WhisperSettingsData data)
        {
            string? directory = Path.GetDirectoryName(SettingsPath);
            if (directory != null)
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonSerializer.Serialize(data);
            File.WriteAllText(SettingsPath, json);
        }

        private sealed class WhisperSettingsData
        {
            public string Language { get; set; } = "en";
            public string? InputFolder { get; set; }
            public string? OutputFolder { get; set; }
        }
    }
}
