using System.Text.Json;

namespace WhisperWinForms.Services
{
    /// <summary>Loads cookies from a JSON object file or a Netscape cookie-jar text file.</summary>
    public static class CookieFileLoader
    {
        public static Dictionary<string, string> Load(string cookieFilePath)
        {
            if (!File.Exists(cookieFilePath))
            {
                throw new FileNotFoundException($"Cookie file not found: {cookieFilePath}");
            }

            string content = File.ReadAllText(cookieFilePath).Trim();

            if (content.StartsWith('{'))
            {
                Dictionary<string, string>? cookies = JsonSerializer.Deserialize<Dictionary<string, string>>(content);
                return cookies ?? [];
            }

            Dictionary<string, string> result = [];
            foreach (string rawLine in content.Split('\n'))
            {
                string line = rawLine.Trim();
                if (line.Length == 0 || (line.StartsWith('#') && !line.StartsWith("#HttpOnly_", StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                string[] parts = line.Split('\t');
                if (parts.Length >= 7)
                {
                    if (parts[0].StartsWith("#HttpOnly_", StringComparison.OrdinalIgnoreCase))
                    {
                        parts[0] = parts[0]["#HttpOnly_".Length..];
                    }
                    result[parts[5]] = parts[6];
                }
            }

            if (result.Count == 0)
            {
                throw new InvalidDataException($"No cookies found in {cookieFilePath}");
            }

            return result;
        }
    }
}
