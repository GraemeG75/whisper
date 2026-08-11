namespace WhisperWinForms
{
    public sealed class BrowserLoginResult
    {
        public string CookieFilePath { get; init; } = string.Empty;
        public string RefererUrl { get; init; } = string.Empty;
        public string UserAgent { get; init; } = string.Empty;
    }
}
