namespace WhisperWinForms.Services
{
    public sealed class TranscriptionOptions
    {
        public string ModelName { get; set; } = "base";
        public string ModelsDirectory { get; set; } = "models";
        public bool UseGpu { get; set; } = true;
        public string Language { get; set; } = "en";
        public string Prompt { get; set; } = string.Empty;
    }
}
