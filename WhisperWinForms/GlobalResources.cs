using System.Globalization;
using System.Resources;

namespace WhisperWinForms
{
    internal static class GlobalResources
    {
        private static readonly ResourceManager ResourceManager = new("WhisperWinForms.GlobalResources", typeof(GlobalResources).Assembly);

        public static string GetString(string name)
        {
            return ResourceManager.GetString(name, CultureInfo.CurrentUICulture) ?? name;
        }
    }
}
