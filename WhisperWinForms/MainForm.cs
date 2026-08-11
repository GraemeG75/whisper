using WhisperWinForms.Services;

namespace WhisperWinForms
{
    public partial class MainForm : Form
    {
        private readonly NativeTranscriptionService _transcriptionService;
        private CancellationTokenSource? _cancellationTokenSource;

        public MainForm()
        {
            InitializeComponent();

            _transcriptionService = new NativeTranscriptionService();

            string? repoRoot = FindRepositoryRoot();
            if (repoRoot != null)
            {
                this.textBoxInputFolder.Text = Path.Combine(repoRoot, "audio_files");
                this.textBoxOutputFolder.Text = Path.Combine(repoRoot, "transcripts");
                this.textBoxModelsDir.Text = Path.Combine(repoRoot, "WhisperWinForms", "models");
            }
            else
            {
                this.textBoxModelsDir.Text = Path.Combine(AppContext.BaseDirectory, "models");
            }

            this.FormClosing += this.MainForm_FormClosing;
        }

        private static string? FindRepositoryRoot()
        {
            DirectoryInfo? directory = new DirectoryInfo(AppContext.BaseDirectory);
            for (int i = 0; i < 8 && directory != null; i++)
            {
                string candidate = Path.Combine(directory.FullName, "transcribe_whisper.py");
                if (File.Exists(candidate))
                {
                    return directory.FullName;
                }
                directory = directory.Parent;
            }
            return null;
        }

        private void buttonBrowseModelsDir_Click(object? sender, EventArgs e)
        {
            using FolderBrowserDialog dialog = new FolderBrowserDialog
            {
                SelectedPath = this.textBoxModelsDir.Text,
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                this.textBoxModelsDir.Text = dialog.SelectedPath;
            }
        }

        private void buttonBrowseInputFolder_Click(object? sender, EventArgs e)
        {
            using FolderBrowserDialog dialog = new FolderBrowserDialog
            {
                SelectedPath = this.textBoxInputFolder.Text,
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                this.textBoxInputFolder.Text = dialog.SelectedPath;
            }
        }

        private void buttonBrowseOutputFolder_Click(object? sender, EventArgs e)
        {
            using FolderBrowserDialog dialog = new FolderBrowserDialog
            {
                SelectedPath = this.textBoxOutputFolder.Text,
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                this.textBoxOutputFolder.Text = dialog.SelectedPath;
            }
        }

        private void buttonBrowseCookiesFile_Click(object? sender, EventArgs e)
        {
            using OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Cookie files|*.json;*.txt|All files|*.*",
                FileName = this.textBoxCookiesFile.Text,
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                this.textBoxCookiesFile.Text = dialog.FileName;
            }
        }

        private void buttonBrowseStreamOutputFile_Click(object? sender, EventArgs e)
        {
            using SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "Text files|*.txt|All files|*.*",
                FileName = this.textBoxStreamOutputFile.Text,
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                this.textBoxStreamOutputFile.Text = dialog.FileName;
            }
        }

        private void buttonLogin_Click(object? sender, EventArgs e)
        {
            string startUrl = string.IsNullOrWhiteSpace(this.textBoxRefererUrl.Text)
                ? "https://www.broadcastify.com/"
                : this.textBoxRefererUrl.Text;
            using LoginForm dialog = new(startUrl);
            if (dialog.ShowDialog(this) == DialogResult.OK && dialog.Result != null)
            {
                this.textBoxCookiesFile.Text = dialog.Result.CookieFilePath;
                this.textBoxRefererUrl.Text = dialog.Result.RefererUrl;
                this.textBoxUserAgent.Text = dialog.Result.UserAgent;
                this.AppendLog($"Browser login session imported ({dialog.Result.CookieFilePath}).");
            }
        }

        private async void buttonStart_Click(object? sender, EventArgs e)
        {
            if (_cancellationTokenSource != null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(this.textBoxModelsDir.Text))
            {
                MessageBox.Show(this, "Set a folder for downloaded Whisper models.", "Missing models folder",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            TranscriptionOptions options = new TranscriptionOptions
            {
                ModelName = (this.comboBoxModel.SelectedItem as string) ?? "base",
                ModelsDirectory = this.textBoxModelsDir.Text,
                UseGpu = this.checkBoxUseGpu.Checked,
                Language = this.textBoxLanguage.Text,
                Prompt = this.textBoxPrompt.Text,
            };

            this.textBoxLog.Clear();
            _cancellationTokenSource = new CancellationTokenSource();
            this.buttonStart.Enabled = false;
            this.buttonStop.Enabled = true;

            IProgress<string> log = new Progress<string>(this.AppendLog);
            bool isStream = this.tabControlMode.SelectedTab == this.tabPageStream;

            try
            {
                if (isStream)
                {
                    if (string.IsNullOrWhiteSpace(this.textBoxStreamUrl.Text))
                    {
                        MessageBox.Show(this, "Enter a stream URL.", "Missing URL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    StreamOptions streamOptions = new StreamOptions
                    {
                        Url = this.textBoxStreamUrl.Text,
                        RefererUrl = string.IsNullOrWhiteSpace(this.textBoxRefererUrl.Text) ? null : this.textBoxRefererUrl.Text,
                        UserAgent = string.IsNullOrWhiteSpace(this.textBoxUserAgent.Text) ? null : this.textBoxUserAgent.Text,
                        CookiesFile = string.IsNullOrWhiteSpace(this.textBoxCookiesFile.Text) ? null : this.textBoxCookiesFile.Text,
                        OutputFile = string.IsNullOrWhiteSpace(this.textBoxStreamOutputFile.Text) ? null : this.textBoxStreamOutputFile.Text,
                        BufferMs = (int)this.numericChunkMs.Value,
                    };
                    await _transcriptionService.RunStreamAsync(options, streamOptions, log, _cancellationTokenSource.Token);
                }
                else
                {
                    BatchOptions batchOptions = new BatchOptions
                    {
                        InputFolder = this.textBoxInputFolder.Text,
                        OutputFolder = this.textBoxOutputFolder.Text,
                        ChunkOnSilence = this.checkBoxChunkOnSilence.Checked,
                        DetailedOutput = this.checkBoxDetailedOutput.Checked,
                    };
                    await _transcriptionService.RunBatchAsync(options, batchOptions, log, _cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                this.AppendLog("Cancelled.");
            }
            catch (Exception ex)
            {
                this.AppendLog($"Error: {ex.Message}");
            }
            finally
            {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
                this.buttonStart.Enabled = true;
                this.buttonStop.Enabled = false;
            }
        }

        private void buttonStop_Click(object? sender, EventArgs e)
        {
            this.AppendLog("Stopping...");
            _cancellationTokenSource?.Cancel();
        }

        private void AppendLog(string line)
        {
            if (this.textBoxLog.InvokeRequired)
            {
                this.textBoxLog.BeginInvoke(new Action(() => this.AppendLogInternal(line)));
            }
            else
            {
                this.AppendLogInternal(line);
            }
        }

        private void AppendLogInternal(string line)
        {
            this.textBoxLog.AppendText(line + Environment.NewLine);
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}
