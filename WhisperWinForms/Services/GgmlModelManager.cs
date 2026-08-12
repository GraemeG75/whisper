using Whisper.net;
using Whisper.net.Ggml;
using Whisper.net.LibraryLoader;
using Whisper.net.Logger;

namespace WhisperWinForms.Services
{
    /// <summary>Downloads and caches ggml Whisper models, and creates the native transcription factory.</summary>
    public static class GgmlModelManager
    {
        private static readonly object NativeLogLock = new();
        private static bool _nativeLoggerRegistered;
        private static IProgress<string>? _currentLog;

        public static GgmlType MapModelName(string modelName)
        {
            return modelName.ToLowerInvariant() switch
            {
                "tiny" => GgmlType.Tiny,
                "base" => GgmlType.Base,
                "small" => GgmlType.Small,
                "medium" => GgmlType.Medium,
                "large" => GgmlType.LargeV3,
                _ => GgmlType.Base,
            };
        }

        public static async Task<string> EnsureModelAsync(string modelName, string modelsDirectory, IProgress<string> log)
        {
            Directory.CreateDirectory(modelsDirectory);
            GgmlType ggmlType = MapModelName(modelName);
            string modelPath = Path.Combine(modelsDirectory, $"ggml-{modelName.ToLowerInvariant()}.bin");

            if (!File.Exists(modelPath))
            {
                log.Report($"Downloading Whisper model '{modelName}'...");
                using HttpClient httpClient = new HttpClient();
                WhisperGgmlDownloader downloader = new WhisperGgmlDownloader(httpClient);
                using Stream modelStream = await downloader.GetGgmlModelAsync(ggmlType);
                await using FileStream fileStream = File.Create(modelPath);
                await modelStream.CopyToAsync(fileStream);
                log.Report($"Model downloaded: {modelPath}");
            }

            return modelPath;
        }


        public static WhisperFactory CreateFactory(string modelPath, bool useGpu, IProgress<string> log)
        {
            _currentLog = log;
            lock (NativeLogLock)
            {
                if (!_nativeLoggerRegistered)
                {
                    LogProvider.AddLogger((level, message) => _currentLog?.Report($"[whisper-native:{level}] {message.TrimEnd()}"));
                    _nativeLoggerRegistered = true;
                }
            }

            WhisperFactoryOptions options = new WhisperFactoryOptions
            {
                UseGpu = useGpu,
            };
            WhisperFactory factory = WhisperFactory.FromPath(modelPath, options);
            log.Report($"Native runtime library loaded: {RuntimeOptions.LoadedLibrary?.ToString() ?? "unknown"}");
            return factory;
        }
    }
}
