namespace WhisperWinForms.Services
{
    /// <summary>Ported segment-quality heuristics from the Python transcriber (hallucination/loop filtering).</summary>
    public static class TranscriptFilter
    {
        private static readonly HashSet<string> CommonHallucinations =
        [
            "thank you",
            "okay",
            "roger",
            "yes",
        ];

        public static string NormalizeSegmentText(string text)
        {
            return string.Join(' ', text.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        /// <summary>Whisper.net exposes an average token probability rather than OpenAI's log-prob/no-speech metrics,
        /// so low confidence is approximated with a probability threshold instead of the original log-prob cutoffs.</summary>
        public static bool ShouldSkipSegment(string text, float probability)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return true;
            }
            return probability < 0.35f;
        }

        public static bool IsLikelyHallucination(string text, float probability)
        {
            string norm = NormalizeSegmentText(text);
            if (CommonHallucinations.Contains(norm) && probability < 0.75f)
            {
                return true;
            }
            return norm.Split(' ').Length <= 2 && probability < 0.5f;
        }

        public static bool DetectRepeatingLoop(IReadOnlyList<string> recentNorms, int windowSize = 10)
        {
            if (recentNorms.Count < windowSize)
            {
                return false;
            }

            IEnumerable<string> recent = recentNorms.Skip(recentNorms.Count - windowSize);
            string[] window = recent.ToArray();
            int alternationCount = 0;
            for (int i = 1; i < window.Length; i++)
            {
                if (window[i] != window[i - 1])
                {
                    alternationCount++;
                }
            }
            return alternationCount >= window.Length * 0.7;
        }

        public static string FormatTimestamp(TimeSpan time)
        {
            long totalMs = Math.Max(0, (long)time.TotalMilliseconds);
            long hours = totalMs / 3600000;
            long minutes = (totalMs % 3600000) / 60000;
            long seconds = (totalMs % 60000) / 1000;
            long millis = totalMs % 1000;
            return $"{hours:D2}:{minutes:D2}:{seconds:D2}.{millis:D3}";
        }
    }
}
