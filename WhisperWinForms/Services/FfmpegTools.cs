using System.Diagnostics;
using System.Text.RegularExpressions;

namespace WhisperWinForms.Services
{
    /// <summary>Direct FFmpeg/FFprobe process wrappers mirroring the command shapes used by the Python transcriber.</summary>
    public static partial class FfmpegTools
    {
        public static async Task<(int ExitCode, string StdErr)> RunAsync(string fileName, IReadOnlyList<string> arguments)
        {
            ProcessStartInfo startInfo = new()
                        {
                FileName = fileName,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            foreach (string argument in arguments)
            {
                startInfo.ArgumentList.Add(argument);
            }

            using Process process = new() { StartInfo = startInfo };
            process.Start();

            Task<string> stdErrTask = process.StandardError.ReadToEndAsync();

            await process.StandardOutput.ReadToEndAsync();

            string stdErr = await stdErrTask;

            await process.WaitForExitAsync();

            return (process.ExitCode, stdErr);
        }

        public static async Task<string> NormalizeAndAmplifyAudioAsync(string inputFile, string tempDir)
        {
            Directory.CreateDirectory(tempDir);

            string outputFile = Path.Combine(tempDir, $"{Path.GetFileNameWithoutExtension(inputFile)}_normalized.wav");

            string[] arguments =
            [
                "-y", "-i", inputFile,
                "-ac", "1", "-ar", "16000",
                "-af", "highpass=f=100,anlmdn=s=0.003:p=0.002,loudnorm=I=-20:TP=-3:LRA=4,volume=1.6",
                outputFile,
            ];

            (int exitCode, string stdErr) = await RunAsync("ffmpeg", arguments);

            if (exitCode != 0)
            {
                throw new InvalidOperationException($"Audio normalization failed: {Tail(stdErr)}");
            }

            return outputFile;
        }

        public static async Task<string> ExtractAudioFromMp4Async(string inputFile, string tempDir)
        {
            Directory.CreateDirectory(tempDir);

            string outputFile = Path.Combine(tempDir, $"{Path.GetFileNameWithoutExtension(inputFile)}_extracted.wav");

            string[] arguments =
            [
                "-y", "-i", inputFile,
                "-vn", "-acodec", "pcm_s16le", "-ar", "16000", "-ac", "1",
                "-af", "highpass=f=80,anlmdn=s=0.004:p=0.0015,volume=1.5",
                outputFile,
            ];

            (int exitCode, string stdErr) = await RunAsync("ffmpeg", arguments);

            if (exitCode != 0)
            {
                throw new InvalidOperationException($"MP4 audio extraction failed: {Tail(stdErr)}");
            }

            return outputFile;
        }

        /// <summary>Resolves an M3U/M3U8 playlist into a single WAV file via FFmpeg's concat/HLS demuxers.</summary>
        public static async Task<string?> ExtractAudioFromPlaylistAsync(string inputFile, string tempDir)
        {
            Directory.CreateDirectory(tempDir);
            string outputFile = Path.Combine(tempDir, $"{Path.GetFileNameWithoutExtension(inputFile)}_playlist.wav");
            bool isM3u = Path.GetExtension(inputFile).Equals(".m3u", StringComparison.OrdinalIgnoreCase);
            string playlistInput = inputFile;
            string? concatFile = null;

            if (isM3u)
            {
                List<string> entries = [];
                foreach (string rawLine in await File.ReadAllLinesAsync(inputFile))
                {
                    string entry = rawLine.Trim();
                    if (entry.Length == 0 || entry.StartsWith('#'))
                    {
                        continue;
                    }
                    Uri resolved = new(new Uri(inputFile), entry);
                    string resolvedPath = resolved.IsFile ? resolved.LocalPath : resolved.ToString();
                    string escaped = resolvedPath.Replace("'", "'\\''");
                    entries.Add($"file '{escaped}'");
                }

                if (entries.Count == 0)
                {
                    return null;
                }

                concatFile = Path.Combine(tempDir, $"{Path.GetFileNameWithoutExtension(inputFile)}_playlist.txt");
                await File.WriteAllTextAsync(concatFile, string.Join('\n', entries) + "\n");
                playlistInput = concatFile;
            }

            string[] arguments =
            [
                "-y",
                "-protocol_whitelist", "file,http,https,tcp,tls,crypto",
                "-f", isM3u ? "concat" : "hls",
                "-i", playlistInput,
                "-vn", "-acodec", "pcm_s16le", "-ar", "16000", "-ac", "1",
                outputFile,
            ];

            (int exitCode, string stdErr) = await RunAsync("ffmpeg", arguments);

            if (concatFile != null && File.Exists(concatFile))
            {
                File.Delete(concatFile);
            }

            if (exitCode != 0)
            {
                throw new InvalidOperationException($"Playlist audio extraction failed: {Tail(stdErr)}");
            }

            return outputFile;
        }

        public static async Task<double> GetAudioDurationSecondsAsync(string audioFile)
        {
            string[] arguments =
            [
                "-v", "error", "-show_entries", "format=duration",
                "-of", "default=noprint_wrappers=1:nokey=1", audioFile,
            ];

            ProcessStartInfo startInfo = new()
            {
                FileName = "ffprobe",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            foreach (string argument in arguments)
            {
                startInfo.ArgumentList.Add(argument);
            }

            using Process process = new() { StartInfo = startInfo };

            process.Start();

            Task<string> stdOutTask = process.StandardOutput.ReadToEndAsync();

            await process.StandardError.ReadToEndAsync();

            string stdOut = await stdOutTask;

            await process.WaitForExitAsync();

            return process.ExitCode == 0 && double.TryParse(stdOut.Trim(), out double duration) ? duration : 0.0;
        }

        public static async Task<string> ExportAudioChunkAsync(string sourceAudio, string chunkFile, double startSec, double endSec)
        {
            string[] arguments =
            [
                "-y", "-i", sourceAudio,
                "-ss", startSec.ToString("F3"), "-to", endSec.ToString("F3"),
                "-ac", "1", "-ar", "16000",
                chunkFile,
            ];

            (int exitCode, _) = await RunAsync("ffmpeg", arguments);

            if (exitCode != 0)
            {
                throw new InvalidOperationException($"Failed to export chunk {startSec:F3}-{endSec:F3} from {sourceAudio}");
            }
            return chunkFile;
        }

        public static async Task<List<(double Start, double End)>> DetectSilencesAsync(string audioFile, double silenceDb, double minSilenceSec)
        {
            string[] arguments =
            [
                "-i", audioFile,
                "-af", $"silencedetect=noise={silenceDb}dB:d={minSilenceSec}",
                "-f", "null", "-",
            ];

            (_, string stdErr) = await RunAsync("ffmpeg", arguments);

            List<double> starts = [];
            List<double> ends = [];

            foreach (Match match in MyRegex().Matches(stdErr))
            {
                starts.Add(double.Parse(match.Groups[1].Value));
            }

            foreach (Match match in Regex.Matches(stdErr, @"silence_end:\s*([0-9.]+)"))
            {
                ends.Add(double.Parse(match.Groups[1].Value));
            }

            List<(double Start, double End)> silences = [];

            for (int i = 0; i < starts.Count; i++)
            {
                if (i < ends.Count && ends[i] >= starts[i])
                {
                    silences.Add((starts[i], ends[i]));
                }
            }

            return silences;
        }

        public static List<(double Start, double End)> BuildChunkRanges(double duration, List<(double Start, double End)> silences, double targetChunkSec, double maxChunkSec)
        {
            if (duration <= 0)
            {
                return [(0.0, 0.0)];
            }

            if (duration <= maxChunkSec)
            {
                return [(0.0, duration)];
            }

            List<(double Start, double End)> ranges = [];
            double start = 0.0;
            while (start < duration)
            {
                double desired = start + targetChunkSec;
                double hardLimit = Math.Min(start + maxChunkSec, duration);
                double cut = hardLimit;

                foreach ((double silenceStart, double silenceEnd) in silences)
                {
                    if (silenceStart < desired)
                    {
                        continue;
                    }
                    if (silenceStart > hardLimit)
                    {
                        break;
                    }
                    cut = silenceStart;
                    break;
                }

                if (cut <= start)
                {
                    cut = Math.Min(start + maxChunkSec, duration);
                }

                ranges.Add((start, cut));
                start = cut;
            }
            return ranges;
        }

        private static string Tail(string text, int length = 500)
        {
            return text.Length <= length ? text : text[^length..];
        }

        [GeneratedRegex(@"silence_start:\s*([0-9.]+)")]
        private static partial Regex MyRegex();
    }
}
