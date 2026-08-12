using System.Diagnostics;

namespace WhisperWinForms.Services
{
    /// <summary>Fetches and refreshes a live M3U/M3U8 manifest, rewriting segment URLs to absolute
    /// addresses and preserving signed query parameters, so FFmpeg can read it as a local file.</summary>
    public sealed class HlsManifestClient
    {
        private readonly IReadOnlyDictionary<string, string>? _cookies;
        private readonly string? _refererUrl;
        private readonly string? _userAgent;
        private readonly HashSet<string> _loggedSegmentUrls = new(StringComparer.OrdinalIgnoreCase);

        public HlsManifestClient(IReadOnlyDictionary<string, string>? cookies, string? refererUrl, string? userAgent)
        {
            _cookies = cookies;
            _refererUrl = refererUrl;
            _userAgent = userAgent;
        }

        public Task<double> RefreshManifestAsync(string url, string localManifestPath, CancellationToken cancellationToken)
            => this.RefreshManifestAsync(url, localManifestPath, log: null, cancellationToken);

        public async Task<double> RefreshManifestAsync(string url, string localManifestPath, IProgress<string>? log, CancellationToken cancellationToken)
        {
            log?.Report($"Fetching playlist: {url}");
            string text = await DownloadManifestWithCurlAsync(url, cancellationToken);
            Uri playlistUri = new(url);

            double targetDuration = 5.0;
            List<string> manifestLines = [];

            foreach (string rawLine in text.Split('\n'))
            {
                string line = rawLine.TrimEnd('\r');
                string trimmed = line.Trim();

                if (trimmed.StartsWith("#EXT-X-TARGETDURATION:"))
                {
                    if (double.TryParse(trimmed.Split(':', 2)[1], out double parsed))
                    {
                        targetDuration = parsed;
                    }
                }

                if (trimmed.Length > 0 && !trimmed.StartsWith('#'))
                {
                    line = ResolveSegmentUrl(playlistUri, trimmed);
                    if (log != null && _loggedSegmentUrls.Add(line))
                    {
                        log.Report($"Segment: {line}");
                    }
                }

                manifestLines.Add(line);
            }

            await File.WriteAllTextAsync(localManifestPath, string.Join('\n', manifestLines) + "\n", cancellationToken);
            return Math.Max(1.0, targetDuration / 2.0);
        }

        private async Task<string> DownloadManifestWithCurlAsync(string url, CancellationToken cancellationToken)
        {
            string userAgent = string.IsNullOrWhiteSpace(_userAgent)
                ? "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/151.0.0.0 Safari/537.36"
                : _userAgent.Trim();
            string referer = ResolveRefererUri(url, _refererUrl)?.ToString() ?? string.Empty;
            string origin = string.Empty;
            if (Uri.TryCreate(referer, UriKind.Absolute, out Uri? refererUri))
            {
                origin = new UriBuilder(refererUri.Scheme, refererUri.Host, refererUri.Port).Uri.GetLeftPart(UriPartial.Authority);
            }

            ProcessStartInfo startInfo = new()
            {
                FileName = "curl.exe",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            string[] arguments =
            [
                "--fail-with-body", "--silent", "--show-error", "--location", "--max-time", "30",
                "-H", $"User-Agent: {userAgent}", "-H", "Accept: */*",
                "-H", "Accept-Language: en-US,en;q=0.9", "-H", "DNT: 1",
                "-H", "Priority: u=1, i",
                "-H", "Sec-CH-UA: \"Not=A?Brand\";v=\"99\", \"Google Chrome\";v=\"151\", \"Chromium\";v=\"151\"",
                "-H", "Sec-CH-UA-Mobile: ?0", "-H", "Sec-CH-UA-Platform: \"Windows\"", "-H", "Sec-GPC: 1",
            ];
            foreach (string argument in arguments)
            {
                startInfo.ArgumentList.Add(argument);
            }
            if (referer.Length > 0)
            {
                startInfo.ArgumentList.Add("-H");
                startInfo.ArgumentList.Add($"Referer: {referer}");
                startInfo.ArgumentList.Add("-H");
                startInfo.ArgumentList.Add($"Origin: {origin}");
                startInfo.ArgumentList.Add("-H");
                startInfo.ArgumentList.Add("Sec-Fetch-Dest: empty");
                startInfo.ArgumentList.Add("-H");
                startInfo.ArgumentList.Add("Sec-Fetch-Mode: cors");
                startInfo.ArgumentList.Add("-H");
                startInfo.ArgumentList.Add("Sec-Fetch-Site: same-site");
            }
            foreach (KeyValuePair<string, string> cookie in _cookies ?? new Dictionary<string, string>())
            {
                startInfo.ArgumentList.Add("-H");
                startInfo.ArgumentList.Add($"Cookie: {cookie.Key}={cookie.Value}");
            }
            startInfo.ArgumentList.Add(url);

            using Process process = new() { StartInfo = startInfo };
            process.Start();
            Task<string> outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            string error = await process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);
            string output = await outputTask;
            if (process.ExitCode != 0)
            {
                string detail = error.Trim();
                if (detail.Length > 500)
                {
                    detail = detail[^500..];
                }
                throw new HttpRequestException(string.Format(GlobalResources.GetString("errorPlaylistRequest"), process.ExitCode, detail));
            }
            return output;
        }

        private static string ResolveSegmentUrl(Uri playlistUri, string entry)
        {
            Uri segmentUri = new(playlistUri, entry);

            string playlistQuery = playlistUri.Query;
            if (!string.IsNullOrEmpty(playlistQuery) && string.IsNullOrEmpty(segmentUri.Query))
            {
                UriBuilder builder = new UriBuilder(segmentUri)
                {
                    Query = playlistQuery.TrimStart('?'),
                };
                return builder.Uri.ToString();
            }

            return segmentUri.ToString();
        }

        private static Uri? ResolveRefererUri(string streamUrl, string? configuredRefererUrl)
        {
            string candidate = string.IsNullOrWhiteSpace(configuredRefererUrl) ? streamUrl : configuredRefererUrl;
            if (!Uri.TryCreate(candidate, UriKind.Absolute, out Uri? candidateUri))
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(configuredRefererUrl))
            {
                return candidateUri;
            }

            return new UriBuilder(candidateUri.Scheme, candidateUri.Host, candidateUri.Port).Uri;
        }

        private static IEnumerable<string> ToCookiePairs(IReadOnlyDictionary<string, string> cookies)
        {
            foreach (KeyValuePair<string, string> cookie in cookies)
            {
                yield return $"{cookie.Key}={cookie.Value}";
            }
        }
    }
}
