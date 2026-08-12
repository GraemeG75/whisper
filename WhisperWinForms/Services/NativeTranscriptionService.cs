using System.Diagnostics;
using System.Text.Json;
using Whisper.net;

namespace WhisperWinForms.Services
{
    /// <summary>Native (Whisper.net + FFmpeg) replacement for the Python transcription worker.</summary>
    public sealed class NativeTranscriptionService
    {
        public async Task RunBatchAsync(TranscriptionOptions options, BatchOptions batch, IProgress<string> log, CancellationToken cancellationToken)
        {
            string modelPath = await GgmlModelManager.EnsureModelAsync(options.ModelName, options.ModelsDirectory, log);
            using WhisperFactory factory = GgmlModelManager.CreateFactory(modelPath, options.UseGpu);
            using WhisperProcessor processor = BuildProcessor(factory, options);

            Directory.CreateDirectory(batch.OutputFolder);
            Directory.CreateDirectory(batch.TempFolder);

            IEnumerable<string> files = Directory.EnumerateFiles(batch.InputFolder)
                .Where(f => BatchOptions.SupportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase);

            List<string> fileList = files.ToList();
            if (fileList.Count == 0)
            {
                log.Report($"No input files found in '{batch.InputFolder}'.");
                return;
            }

            foreach (string filePath in fileList)
            {
                cancellationToken.ThrowIfCancellationRequested();
                log.Report($"\nProcessing: {Path.GetFileName(filePath)}");

                string extension = Path.GetExtension(filePath).ToLowerInvariant();
                string sourceAudio = filePath;

                try
                {
                    if (extension == ".mp4")
                    {
                        log.Report("  Extracting audio from MP4...");
                        sourceAudio = await FfmpegTools.ExtractAudioFromMp4Async(filePath, batch.TempFolder);
                    }
                    else if (extension is ".m3u" or ".m3u8")
                    {
                        log.Report("  Resolving playlist with FFmpeg...");
                        string? playlistAudio = await FfmpegTools.ExtractAudioFromPlaylistAsync(filePath, batch.TempFolder);
                        if (playlistAudio == null)
                        {
                            log.Report("  Warning: playlist contained no media entries, skipping.");
                            continue;
                        }
                        sourceAudio = playlistAudio;
                    }

                    log.Report("  Normalizing and amplifying audio...");
                    sourceAudio = await FfmpegTools.NormalizeAndAmplifyAudioAsync(sourceAudio, batch.TempFolder);

                    List<(string ChunkFile, double OffsetSec)> chunks = [(sourceAudio, 0.0)];
                    if (batch.ChunkOnSilence)
                    {
                        chunks = await BuildSilenceChunksAsync(sourceAudio, batch);
                        log.Report($"Detected {chunks.Count} chunk(s) for {Path.GetFileName(filePath)}");
                    }

                    List<TranscribedSegment> mergedSegments = [];
                    int chunkIndex = 0;
                    foreach ((string chunkFile, double offsetSec) in chunks)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        chunkIndex++;
                        await foreach (SegmentData segment in processor.ProcessAsync(File.OpenRead(chunkFile), cancellationToken))
                        {
                            mergedSegments.Add(new TranscribedSegment(
                                segment.Start.TotalSeconds + offsetSec,
                                segment.End.TotalSeconds + offsetSec,
                                segment.Text.Trim(),
                                segment.Probability));
                        }
                        log.Report($"Chunk progress: {chunkIndex}/{chunks.Count}");
                    }

                    WriteTranscript(filePath, batch, options.Language, mergedSegments, log);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    log.Report($"  Error processing {Path.GetFileName(filePath)}: {ex.Message}");
                }
                finally
                {
                    CleanupTempFiles(filePath, batch.TempFolder);
                }
            }

            log.Report("\nAll files transcribed.");
        }

        public async Task RunStreamAsync(TranscriptionOptions options, StreamOptions stream, IProgress<string> log, CancellationToken cancellationToken)
        {
            string modelPath = await GgmlModelManager.EnsureModelAsync(options.ModelName, options.ModelsDirectory, log);
            using WhisperFactory factory = GgmlModelManager.CreateFactory(modelPath, options.UseGpu);
            using WhisperProcessor processor = BuildProcessor(factory, options);

            Dictionary<string, string>? cookies = null;
            if (!string.IsNullOrWhiteSpace(stream.CookiesFile))
            {
                log.Report($"Loading authentication cookies from {stream.CookiesFile}");
                cookies = CookieFileLoader.Load(stream.CookiesFile);
                log.Report($"Loaded {cookies.Count} cookie(s)");
            }

            StreamWriter? outputWriter = null;
            if (!string.IsNullOrWhiteSpace(stream.OutputFile))
            {
                outputWriter = new StreamWriter(stream.OutputFile, append: false);
            }

            bool isPlaylist = stream.Url.Contains(".m3u", StringComparison.OrdinalIgnoreCase);
            try
            {
                if (Uri.TryCreate(stream.Url, UriKind.Absolute, out Uri? streamUri))
                {
                    string safeUrl = streamUri.GetLeftPart(UriPartial.Path);
                    log.Report($"Requesting playlist: {safeUrl}");
                }
                log.Report($"Referer: {stream.RefererUrl ?? "derived from stream URL"}; User-Agent: {(string.IsNullOrWhiteSpace(stream.UserAgent) ? "default browser profile" : "custom browser profile")}");
                Task? refreshTask = null;
                CancellationTokenSource? refreshCts = null;
                string? manifestPath = null;
                HlsManifestClient? manifestClient = null;
                double refreshInterval = 0.0;
                if (isPlaylist)
                {
                    manifestPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.m3u8");
                    refreshCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    manifestClient = new(cookies, stream.RefererUrl, stream.UserAgent);
                    refreshInterval = await manifestClient.RefreshManifestAsync(stream.Url, manifestPath, cancellationToken);
                }

                using Process ffmpegProcess = StartFfmpegDecoder(stream.Url, isPlaylist, cookies, stream.RefererUrl, stream.UserAgent, manifestPath, out manifestPath);
                if (isPlaylist && manifestClient != null && manifestPath != null && refreshCts != null)
                {
                    refreshTask = RefreshManifestLoopAsync(manifestClient, stream.Url, manifestPath, refreshInterval, log, refreshCts.Token);
                }

                log.Report("Listening for audio stream...");
                int bytesPerBufferWindow = 16000 * 2 * stream.BufferMs / 1000;
                byte[] pcmBuffer = new byte[bytesPerBufferWindow];
                int filled = 0;
                string lastNorm = string.Empty;
                List<string> recentNorms = [];

                Stream ffmpegOutput = ffmpegProcess.StandardOutput.BaseStream;
                byte[] readBuffer = new byte[8192];
                int bytesRead;
                while ((bytesRead = await ffmpegOutput.ReadAsync(readBuffer, cancellationToken)) > 0)
                {
                    int offset = 0;
                    while (offset < bytesRead)
                    {
                        int toCopy = Math.Min(bytesRead - offset, pcmBuffer.Length - filled);
                        Array.Copy(readBuffer, offset, pcmBuffer, filled, toCopy);
                        filled += toCopy;
                        offset += toCopy;

                        if (filled >= pcmBuffer.Length)
                        {
                            await TranscribeStreamChunkAsync(processor, pcmBuffer, recentNorms, log, outputWriter, cancellationToken);
                            filled = 0;
                        }
                    }
                }

                string ffmpegError = await ffmpegProcess.StandardError.ReadToEndAsync(cancellationToken);
                await ffmpegProcess.WaitForExitAsync(cancellationToken);
                if (ffmpegProcess.ExitCode != 0)
                {
                    string detail = ffmpegError.Trim();
                    if (detail.Length > 1000)
                    {
                        detail = detail[^1000..];
                    }
                    throw new InvalidOperationException(string.Format(GlobalResources.GetString("errorFfmpegStreamDecoding"), ffmpegProcess.ExitCode, detail));
                }

                if (filled > 0)
                {
                    byte[] remaining = pcmBuffer[..filled];
                    await TranscribeStreamChunkAsync(processor, remaining, recentNorms, log, outputWriter, cancellationToken);
                }

                refreshCts?.Cancel();
                if (refreshTask != null)
                {
                    try
                    {
                        await refreshTask;
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected when the stream stops.
                    }
                }

                if (!ffmpegProcess.HasExited)
                {
                    ffmpegProcess.Kill(entireProcessTree: true);
                }

                if (manifestPath != null && File.Exists(manifestPath))
                {
                    File.Delete(manifestPath);
                }

                log.Report("Stream transcription complete.");
            }
            finally
            {
                outputWriter?.Dispose();
            }
        }

        private static async Task TranscribeStreamChunkAsync(
            WhisperProcessor processor, byte[] pcmData, List<string> recentNorms, IProgress<string> log,
            StreamWriter? outputWriter, CancellationToken cancellationToken)
        {
            using MemoryStream wavStream = WavUtils.CreatePcm16WavStream(pcmData);
            await foreach (SegmentData segment in processor.ProcessAsync(wavStream, cancellationToken))
            {
                string text = segment.Text.Trim();
                if (TranscriptFilter.ShouldSkipSegment(text, segment.Probability))
                {
                    continue;
                }
                if (TranscriptFilter.IsLikelyHallucination(text, segment.Probability))
                {
                    continue;
                }

                string norm = TranscriptFilter.NormalizeSegmentText(text);
                recentNorms.Add(norm);
                if (recentNorms.Count > 10)
                {
                    recentNorms.RemoveAt(0);
                }
                if (TranscriptFilter.DetectRepeatingLoop(recentNorms))
                {
                    continue;
                }

                string line = $"[{TranscriptFilter.FormatTimestamp(segment.Start)}] {text}";
                log.Report(line);
                if (outputWriter != null)
                {
                    await outputWriter.WriteLineAsync(line);
                    await outputWriter.FlushAsync();
                }
            }
        }

        private static async Task RefreshManifestLoopAsync(
            HlsManifestClient client, string url, string manifestPath, double intervalSeconds, IProgress<string> log, CancellationToken cancellationToken)
        {
            double interval = intervalSeconds;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(interval), cancellationToken);
                    interval = await client.RefreshManifestAsync(url, manifestPath, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    log.Report($"Warning: could not refresh playlist: {ex.Message}");
                }
            }
        }

        private static Process StartFfmpegDecoder(string url, bool isPlaylist, IReadOnlyDictionary<string, string>? cookies, string? refererUrl, string? userAgent, string? existingManifestPath, out string? manifestPath)
        {
            string cookieHeader = cookies is { Count: > 0 } ? string.Join("; ", cookies.Select(c => $"{c.Key}={c.Value}")) : string.Empty;
            string userAgentHeader = string.IsNullOrWhiteSpace(userAgent)
                ? "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/151.0.0.0 Safari/537.36"
                : userAgent.Trim();
            string refererCandidate = string.IsNullOrWhiteSpace(refererUrl) ? url : refererUrl;
            string headers = $"User-Agent: {userAgentHeader}\r\nAccept: */*\r\nAccept-Language: en-US,en;q=0.9\r\nDNT: 1\r\nPriority: u=1, i\r\nSec-CH-UA: \"Not=A?Brand\";v=\"99\", \"Google Chrome\";v=\"151\", \"Chromium\";v=\"151\"\r\nSec-CH-UA-Mobile: ?0\r\nSec-CH-UA-Platform: \"Windows\"\r\nSec-GPC: 1\r\n";
            if (Uri.TryCreate(refererCandidate, UriKind.Absolute, out Uri? refererUri))
            {
                if (string.IsNullOrWhiteSpace(refererUrl))
                {
                    refererUri = new UriBuilder(refererUri.Scheme, refererUri.Host, refererUri.Port).Uri;
                }
                headers += $"Referer: {refererUri}\r\n";
                Uri originUri = new(refererUri.GetLeftPart(UriPartial.Authority));
                headers += $"Origin: {originUri}\r\nSec-Fetch-Dest: empty\r\nSec-Fetch-Mode: cors\r\nSec-Fetch-Site: same-site\r\n";
            }
            if (cookieHeader.Length > 0)
            {
                headers += $"Cookie: {cookieHeader}\r\n";
            }

            string input = url;
            manifestPath = existingManifestPath;
            if (isPlaylist)
            {
                manifestPath ??= Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.m3u8");
                input = manifestPath;
            }

            ProcessStartInfo startInfo = new()
            {
                FileName = "ffmpeg",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            string[] arguments =
            [
                "-hide_banner", "-loglevel", "error",
                "-protocol_whitelist", "file,http,https,tcp,tls,crypto",
                "-headers", headers,
                "-i", input,
                "-vn", "-f", "s16le", "-ar", "16000", "-ac", "1", "pipe:1",
            ];
            foreach (string argument in arguments)
            {
                startInfo.ArgumentList.Add(argument);
            }

            Process process = new() { StartInfo = startInfo };
            process.Start();
            return process;
        }

        private static async Task<List<(string ChunkFile, double OffsetSec)>> BuildSilenceChunksAsync(string sourceAudio, BatchOptions batch)
        {
            double duration = await FfmpegTools.GetAudioDurationSecondsAsync(sourceAudio);
            List<(double Start, double End)> silences = await FfmpegTools.DetectSilencesAsync(sourceAudio, batch.SilenceDb, batch.MinSilenceSec);
            List<(double Start, double End)> ranges = FfmpegTools.BuildChunkRanges(duration, silences, batch.TargetChunkSec, batch.MaxChunkSec);

            string chunkDir = Path.Combine(batch.TempFolder, "chunks", Path.GetFileNameWithoutExtension(sourceAudio));
            Directory.CreateDirectory(chunkDir);

            List<(string, double)> chunkPaths = [];
            int index = 0;
            foreach ((double start, double end) in ranges)
            {
                if (end - start < 0.3)
                {
                    continue;
                }
                index++;
                string chunkFile = Path.Combine(chunkDir, $"chunk_{index:D4}.wav");
                await FfmpegTools.ExportAudioChunkAsync(sourceAudio, chunkFile, start, end);
                chunkPaths.Add((chunkFile, start));
            }

            return chunkPaths.Count > 0 ? chunkPaths : [(sourceAudio, 0.0)];
        }

        private static WhisperProcessor BuildProcessor(WhisperFactory factory, TranscriptionOptions options)
        {
            WhisperProcessorBuilder builder = factory.CreateBuilder()
                .WithLanguage(options.Language)
                .WithProbabilities();

            if (!string.IsNullOrWhiteSpace(options.Prompt))
            {
                builder = builder.WithPrompt(options.Prompt);
            }

            return builder.Build();
        }

        private static void WriteTranscript(string filePath, BatchOptions batch, string language, List<TranscribedSegment> segments, IProgress<string> log)
        {
            List<string> parts = [];
            double lastEnd = 0.0;

            foreach (TranscribedSegment segment in segments)
            {
                double gap = segment.Start - lastEnd;
                if (gap > 1.0 && parts.Count > 0)
                {
                    parts.Add("\n");
                }
                parts.Add($"[{TranscriptFilter.FormatTimestamp(TimeSpan.FromSeconds(segment.Start))}] {segment.Text}\n");
                lastEnd = segment.End;
            }

            string outputFilename = Path.Combine(batch.OutputFolder, $"{Path.GetFileNameWithoutExtension(filePath)}_{language}.txt");
            File.WriteAllText(outputFilename, string.Concat(parts).Trim());
            log.Report($"Saved to: {outputFilename}");

            if (batch.DetailedOutput)
            {
                WriteDetailedOutputs(outputFilename, segments);
                log.Report($"Saved segment diagnostics: {Path.ChangeExtension(outputFilename, null)}.segments.tsv");
            }
        }

        private static void WriteDetailedOutputs(string outputBase, List<TranscribedSegment> segments)
        {
            string baseNoExt = outputBase[..^Path.GetExtension(outputBase).Length];
            string tsvFile = $"{baseNoExt}.segments.tsv";
            string jsonlFile = $"{baseNoExt}.segments.jsonl";

            using (StreamWriter tsvWriter = new StreamWriter(tsvFile))
            {
                tsvWriter.WriteLine("start\tend\tprobability\ttext");
                foreach (TranscribedSegment segment in segments)
                {
                    tsvWriter.WriteLine(
                        $"{TranscriptFilter.FormatTimestamp(TimeSpan.FromSeconds(segment.Start))}\t" +
                        $"{TranscriptFilter.FormatTimestamp(TimeSpan.FromSeconds(segment.End))}\t" +
                        $"{segment.Probability:F3}\t{segment.Text}");
                }
            }

            using StreamWriter jsonlWriter = new StreamWriter(jsonlFile);
            foreach (TranscribedSegment segment in segments)
            {
                var row = new
                {
                    start = segment.Start,
                    end = segment.End,
                    start_hms = TranscriptFilter.FormatTimestamp(TimeSpan.FromSeconds(segment.Start)),
                    end_hms = TranscriptFilter.FormatTimestamp(TimeSpan.FromSeconds(segment.End)),
                    probability = segment.Probability,
                    text = segment.Text,
                };
                jsonlWriter.WriteLine(JsonSerializer.Serialize(row));
            }
        }

        private static void CleanupTempFiles(string filePath, string tempDir)
        {
            string stem = Path.GetFileNameWithoutExtension(filePath);
            string[] filesToRemove =
            [
                Path.Combine(tempDir, $"{stem}_playlist.wav"),
                Path.Combine(tempDir, $"{stem}_playlist.txt"),
                Path.Combine(tempDir, $"{stem}_normalized.wav"),
                Path.Combine(tempDir, $"{stem}_extracted.wav"),
            ];
            foreach (string file in filesToRemove)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }

            string chunkDir = Path.Combine(tempDir, "chunks", stem);
            if (Directory.Exists(chunkDir))
            {
                Directory.Delete(chunkDir, recursive: true);
            }
        }

        private readonly record struct TranscribedSegment(double Start, double End, string Text, float Probability);
    }
}
