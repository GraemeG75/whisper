namespace WhisperWinForms.Services
{
    public sealed class BatchOptions
    {
        public string InputFolder { get; set; } = "audio_files";
        public string OutputFolder { get; set; } = "transcripts";
        public string TempFolder { get; set; } = "processed";
        public bool ChunkOnSilence { get; set; }
        public bool DetailedOutput { get; set; }
        public double SilenceDb { get; set; } = -32.0;
        public double MinSilenceSec { get; set; } = 0.45;
        public double TargetChunkSec { get; set; } = 55.0;
        public double MaxChunkSec { get; set; } = 80.0;

        public static readonly string[] SupportedExtensions =
        [
            ".mp3", ".wav", ".m4a", ".flac", ".mp4", ".m3u", ".m3u8",
        ];
    }
}
