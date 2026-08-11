namespace WhisperWinForms.Services
{
    public sealed class StreamOptions
    {
        public string Url { get; set; } = string.Empty;
        public string? RefererUrl { get; set; }
        public string? UserAgent { get; set; }
        public string? CookiesFile { get; set; }
        public string? OutputFile { get; set; }
        public int BufferMs { get; set; } = 10000;
    }
}
