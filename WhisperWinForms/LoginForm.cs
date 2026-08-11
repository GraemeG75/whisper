using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Globalization;
using System.Text.Json;

namespace WhisperWinForms
{
    public sealed class LoginForm : Form
    {
        private readonly string _startUrl;
        private readonly WebView2 _webView;
        private readonly Button _buttonUseSession;
        private readonly Button _buttonCancel;
        private BrowserLoginResult? _result;

        public BrowserLoginResult? Result => _result;

        public LoginForm(string startUrl)
        {
            _startUrl = startUrl;
            _webView = new WebView2
            {
                Dock = DockStyle.Fill,
            };
            _buttonUseSession = new Button
            {
                AutoSize = true,
                Text = "Use logged-in session",
            };
            _buttonCancel = new Button
            {
                AutoSize = true,
                Text = "Cancel",
            };

            FlowLayoutPanel buttons = new()
            {
                AutoSize = true,
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(8),
            };
            buttons.Controls.Add(_buttonUseSession);
            buttons.Controls.Add(_buttonCancel);

            Panel toolbar = new()
            {
                Dock = DockStyle.Bottom,
                Height = 52,
            };
            toolbar.Controls.Add(buttons);

            Controls.Add(_webView);
            Controls.Add(toolbar);
            AcceptButton = _buttonUseSession;
            CancelButton = _buttonCancel;
            ClientSize = new Size(1100, 760);
            MinimumSize = new Size(800, 500);
            StartPosition = FormStartPosition.CenterParent;
            Text = "Sign in to stream provider";

            Load += LoginForm_Load;
            _buttonUseSession.Click += ButtonUseSession_Click;
            _buttonCancel.Click += ButtonCancel_Click;
        }

        private async void LoginForm_Load(object? sender, EventArgs e)
        {
            try
            {
                await _webView.EnsureCoreWebView2Async();
                _webView.CoreWebView2.Navigate(_startUrl);
            }
            catch (WebView2RuntimeNotFoundException)
            {
                MessageBox.Show(this,
                    "Microsoft Edge WebView2 Runtime is required for provider login.",
                    "WebView2 runtime missing",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                DialogResult = DialogResult.Abort;
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, "Could not open login page",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Abort;
            }
        }

        private async void ButtonUseSession_Click(object? sender, EventArgs e)
        {
            if (_webView.CoreWebView2 == null)
            {
                return;
            }

            try
            {
                string cookieFilePath = Path.Combine(Path.GetTempPath(), $"whisper-cookies-{Guid.NewGuid():N}.txt");
                IReadOnlyList<CoreWebView2Cookie> cookies = await _webView.CoreWebView2.CookieManager.GetCookiesAsync(_webView.Source?.ToString() ?? _startUrl);
                if (cookies.Count == 0)
                {
                    MessageBox.Show(this, "No cookies were found for this login session.", "Login required",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                await File.WriteAllTextAsync(cookieFilePath, BuildNetscapeCookieFile(cookies));
                string userAgent = await GetUserAgentAsync();
                _result = new BrowserLoginResult
                {
                    CookieFilePath = cookieFilePath,
                    RefererUrl = _webView.Source?.ToString() ?? _startUrl,
                    UserAgent = userAgent,
                };
                DialogResult = DialogResult.OK;
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, "Could not export login session",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ButtonCancel_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private async Task<string> GetUserAgentAsync()
        {
            string scriptResult = await _webView.CoreWebView2.ExecuteScriptAsync("navigator.userAgent");
            return JsonSerializer.Deserialize<string>(scriptResult) ?? string.Empty;
        }

        private static string BuildNetscapeCookieFile(IEnumerable<CoreWebView2Cookie> cookies)
        {
            List<string> lines =
            [
                "# Netscape HTTP Cookie File",
                "# Generated by WhisperWinForms WebView2 login",
            ];

            foreach (CoreWebView2Cookie cookie in cookies)
            {
                string domain = cookie.IsHttpOnly ? $"#HttpOnly_{cookie.Domain}" : cookie.Domain;
                string includeSubdomains = cookie.Domain.StartsWith('.') ? "TRUE" : "FALSE";
                string secure = cookie.IsSecure ? "TRUE" : "FALSE";
                string expiration = cookie.Expires != DateTime.MinValue
                    ? new DateTimeOffset(cookie.Expires).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)
                    : "0";
                lines.Add(string.Join("\t", domain, includeSubdomains, cookie.Path, secure,
                    expiration, cookie.Name, cookie.Value));
            }

            return string.Join(Environment.NewLine, lines) + Environment.NewLine;
        }
    }
}
