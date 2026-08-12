namespace WhisperWinForms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private System.Windows.Forms.TabControl tabControlMode;
        private System.Windows.Forms.TabPage tabPageBatch;
        private System.Windows.Forms.TabPage tabPageStream;

        private System.Windows.Forms.Label labelInputFolder;
        private System.Windows.Forms.TextBox textBoxInputFolder;
        private System.Windows.Forms.Button buttonBrowseInputFolder;

        private System.Windows.Forms.Label labelOutputFolder;
        private System.Windows.Forms.TextBox textBoxOutputFolder;
        private System.Windows.Forms.Button buttonBrowseOutputFolder;

        private System.Windows.Forms.CheckBox checkBoxPreprocess;
        private System.Windows.Forms.CheckBox checkBoxChunkOnSilence;
        private System.Windows.Forms.CheckBox checkBoxFastMode;
        private System.Windows.Forms.CheckBox checkBoxDetailedOutput;

        private System.Windows.Forms.Label labelStreamUrl;
        private System.Windows.Forms.TextBox textBoxStreamUrl;
        private System.Windows.Forms.Label labelRefererUrl;
        private System.Windows.Forms.TextBox textBoxRefererUrl;
        private System.Windows.Forms.Label labelUserAgent;
        private System.Windows.Forms.TextBox textBoxUserAgent;

        private System.Windows.Forms.Label labelCookiesFile;
        private System.Windows.Forms.TextBox textBoxCookiesFile;
        private System.Windows.Forms.Button buttonBrowseCookiesFile;

        private System.Windows.Forms.Label labelStreamOutputFile;
        private System.Windows.Forms.TextBox textBoxStreamOutputFile;
        private System.Windows.Forms.Button buttonBrowseStreamOutputFile;

        private System.Windows.Forms.Label labelChunkMs;
        private System.Windows.Forms.NumericUpDown numericChunkMs;
        private System.Windows.Forms.Button buttonLogin;

        private System.Windows.Forms.GroupBox groupBoxCommon;
        private System.Windows.Forms.Label labelModel;
        private System.Windows.Forms.ComboBox comboBoxModel;
        private System.Windows.Forms.Label labelLanguage;
        private System.Windows.Forms.TextBox textBoxLanguage;
        private System.Windows.Forms.Label labelPrompt;
        private System.Windows.Forms.TextBox textBoxPrompt;

        private System.Windows.Forms.GroupBox groupBoxEngine;
        private System.Windows.Forms.Label labelModelsDir;
        private System.Windows.Forms.TextBox textBoxModelsDir;
        private System.Windows.Forms.Button buttonBrowseModelsDir;
        private System.Windows.Forms.CheckBox checkBoxUseGpu;

        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Button buttonSettings;
        private System.Windows.Forms.TextBox textBoxLog;

        private System.Windows.Forms.SplitContainer splitContainerOutput;
        private System.Windows.Forms.Label labelTranscript;
        private System.Windows.Forms.Label labelLog;
        private System.Windows.Forms.Label labelAudioActivity;
        private System.Windows.Forms.TextBox textBoxTranscript;
        private System.Windows.Forms.Timer timerAudioActivity;

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            tabControlMode = new TabControl();
            tabPageBatch = new TabPage();
            labelInputFolder = new Label();
            textBoxInputFolder = new TextBox();
            buttonBrowseInputFolder = new Button();
            labelOutputFolder = new Label();
            textBoxOutputFolder = new TextBox();
            buttonBrowseOutputFolder = new Button();
            checkBoxPreprocess = new CheckBox();
            checkBoxChunkOnSilence = new CheckBox();
            checkBoxFastMode = new CheckBox();
            checkBoxDetailedOutput = new CheckBox();
            tabPageStream = new TabPage();
            labelStreamUrl = new Label();
            textBoxStreamUrl = new TextBox();
            labelRefererUrl = new Label();
            textBoxRefererUrl = new TextBox();
            labelUserAgent = new Label();
            textBoxUserAgent = new TextBox();
            labelCookiesFile = new Label();
            textBoxCookiesFile = new TextBox();
            buttonBrowseCookiesFile = new Button();
            labelStreamOutputFile = new Label();
            textBoxStreamOutputFile = new TextBox();
            buttonBrowseStreamOutputFile = new Button();
            labelChunkMs = new Label();
            numericChunkMs = new NumericUpDown();
            buttonLogin = new Button();
            groupBoxCommon = new GroupBox();
            labelModel = new Label();
            comboBoxModel = new ComboBox();
            labelLanguage = new Label();
            textBoxLanguage = new TextBox();
            labelPrompt = new Label();
            textBoxPrompt = new TextBox();
            groupBoxEngine = new GroupBox();
            labelModelsDir = new Label();
            textBoxModelsDir = new TextBox();
            buttonBrowseModelsDir = new Button();
            checkBoxUseGpu = new CheckBox();
            buttonStart = new Button();
            buttonStop = new Button();
            buttonSettings = new Button();
            textBoxLog = new TextBox();
            splitContainerOutput = new SplitContainer();
            labelTranscript = new Label();
            labelAudioActivity = new Label();
            textBoxTranscript = new TextBox();
            labelLog = new Label();
            timerAudioActivity = new System.Windows.Forms.Timer(components);
            tabControlMode.SuspendLayout();
            tabPageBatch.SuspendLayout();
            tabPageStream.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericChunkMs).BeginInit();
            groupBoxCommon.SuspendLayout();
            groupBoxEngine.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerOutput).BeginInit();
            splitContainerOutput.Panel1.SuspendLayout();
            splitContainerOutput.Panel2.SuspendLayout();
            splitContainerOutput.SuspendLayout();
            SuspendLayout();
            // 
            // tabControlMode
            // 
            tabControlMode.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tabControlMode.Controls.Add(tabPageBatch);
            tabControlMode.Controls.Add(tabPageStream);
            tabControlMode.Location = new Point(12, 100);
            tabControlMode.Name = "tabControlMode";
            tabControlMode.SelectedIndex = 0;
            tabControlMode.Size = new Size(790, 268);
            tabControlMode.TabIndex = 1;
            // 
            // tabPageBatch
            // 
            tabPageBatch.Controls.Add(labelInputFolder);
            tabPageBatch.Controls.Add(textBoxInputFolder);
            tabPageBatch.Controls.Add(buttonBrowseInputFolder);
            tabPageBatch.Controls.Add(labelOutputFolder);
            tabPageBatch.Controls.Add(textBoxOutputFolder);
            tabPageBatch.Controls.Add(buttonBrowseOutputFolder);
            tabPageBatch.Controls.Add(checkBoxPreprocess);
            tabPageBatch.Controls.Add(checkBoxChunkOnSilence);
            tabPageBatch.Controls.Add(checkBoxFastMode);
            tabPageBatch.Controls.Add(checkBoxDetailedOutput);
            tabPageBatch.Location = new Point(4, 24);
            tabPageBatch.Name = "tabPageBatch";
            tabPageBatch.Size = new Size(782, 240);
            tabPageBatch.TabIndex = 0;
            tabPageBatch.Text = GlobalResources.GetString("tabPageBatch.Text");
            // 
            // labelInputFolder
            // 
            labelInputFolder.AutoSize = true;
            labelInputFolder.Location = new Point(12, 20);
            labelInputFolder.Name = "labelInputFolder";
            labelInputFolder.Size = new Size(72, 15);
            labelInputFolder.TabIndex = 0;
            labelInputFolder.Text = GlobalResources.GetString("labelInputFolder.Text");
            // 
            // textBoxInputFolder
            // 
            textBoxInputFolder.Location = new Point(140, 17);
            textBoxInputFolder.Name = "textBoxInputFolder";
            textBoxInputFolder.Size = new Size(500, 23);
            textBoxInputFolder.TabIndex = 1;
            textBoxInputFolder.Text = GlobalResources.GetString("textBoxInputFolder.Text");
            // 
            // buttonBrowseInputFolder
            // 
            buttonBrowseInputFolder.Location = new Point(650, 15);
            buttonBrowseInputFolder.Name = "buttonBrowseInputFolder";
            buttonBrowseInputFolder.Size = new Size(75, 23);
            buttonBrowseInputFolder.TabIndex = 2;
            buttonBrowseInputFolder.Text = GlobalResources.GetString("buttonBrowseInputFolder.Text");
            buttonBrowseInputFolder.Click += buttonBrowseInputFolder_Click;
            // 
            // labelOutputFolder
            // 
            labelOutputFolder.AutoSize = true;
            labelOutputFolder.Location = new Point(12, 52);
            labelOutputFolder.Name = "labelOutputFolder";
            labelOutputFolder.Size = new Size(82, 15);
            labelOutputFolder.TabIndex = 3;
            labelOutputFolder.Text = GlobalResources.GetString("labelOutputFolder.Text");
            // 
            // textBoxOutputFolder
            // 
            textBoxOutputFolder.Location = new Point(140, 49);
            textBoxOutputFolder.Name = "textBoxOutputFolder";
            textBoxOutputFolder.Size = new Size(500, 23);
            textBoxOutputFolder.TabIndex = 4;
            textBoxOutputFolder.Text = GlobalResources.GetString("textBoxOutputFolder.Text");
            // 
            // buttonBrowseOutputFolder
            // 
            buttonBrowseOutputFolder.Location = new Point(650, 47);
            buttonBrowseOutputFolder.Name = "buttonBrowseOutputFolder";
            buttonBrowseOutputFolder.Size = new Size(75, 23);
            buttonBrowseOutputFolder.TabIndex = 5;
            buttonBrowseOutputFolder.Text = GlobalResources.GetString("buttonBrowseOutputFolder.Text");
            buttonBrowseOutputFolder.Click += buttonBrowseOutputFolder_Click;
            // 
            // checkBoxPreprocess
            // 
            checkBoxPreprocess.AutoSize = true;
            checkBoxPreprocess.Enabled = false;
            checkBoxPreprocess.Location = new Point(140, 84);
            checkBoxPreprocess.Name = "checkBoxPreprocess";
            checkBoxPreprocess.Size = new Size(298, 19);
            checkBoxPreprocess.TabIndex = 6;
            checkBoxPreprocess.Text = GlobalResources.GetString("checkBoxPreprocess.Text");
            // 
            // checkBoxChunkOnSilence
            // 
            checkBoxChunkOnSilence.AutoSize = true;
            checkBoxChunkOnSilence.Location = new Point(140, 108);
            checkBoxChunkOnSilence.Name = "checkBoxChunkOnSilence";
            checkBoxChunkOnSilence.Size = new Size(117, 19);
            checkBoxChunkOnSilence.TabIndex = 7;
            checkBoxChunkOnSilence.Text = GlobalResources.GetString("checkBoxChunkOnSilence.Text");
            // 
            // checkBoxFastMode
            // 
            checkBoxFastMode.AutoSize = true;
            checkBoxFastMode.Enabled = false;
            checkBoxFastMode.Location = new Point(140, 132);
            checkBoxFastMode.Name = "checkBoxFastMode";
            checkBoxFastMode.Size = new Size(203, 19);
            checkBoxFastMode.TabIndex = 8;
            checkBoxFastMode.Text = GlobalResources.GetString("checkBoxFastMode.Text");
            // 
            // checkBoxDetailedOutput
            // 
            checkBoxDetailedOutput.AutoSize = true;
            checkBoxDetailedOutput.Location = new Point(140, 156);
            checkBoxDetailedOutput.Name = "checkBoxDetailedOutput";
            checkBoxDetailedOutput.Size = new Size(178, 19);
            checkBoxDetailedOutput.TabIndex = 9;
            checkBoxDetailedOutput.Text = GlobalResources.GetString("checkBoxDetailedOutput.Text");
            // 
            // tabPageStream
            // 
            tabPageStream.Controls.Add(labelStreamUrl);
            tabPageStream.Controls.Add(textBoxStreamUrl);
            tabPageStream.Controls.Add(labelRefererUrl);
            tabPageStream.Controls.Add(textBoxRefererUrl);
            tabPageStream.Controls.Add(labelUserAgent);
            tabPageStream.Controls.Add(textBoxUserAgent);
            tabPageStream.Controls.Add(labelCookiesFile);
            tabPageStream.Controls.Add(textBoxCookiesFile);
            tabPageStream.Controls.Add(buttonBrowseCookiesFile);
            tabPageStream.Controls.Add(labelStreamOutputFile);
            tabPageStream.Controls.Add(textBoxStreamOutputFile);
            tabPageStream.Controls.Add(buttonBrowseStreamOutputFile);
            tabPageStream.Controls.Add(labelChunkMs);
            tabPageStream.Controls.Add(numericChunkMs);
            tabPageStream.Controls.Add(buttonLogin);
            tabPageStream.Location = new Point(4, 24);
            tabPageStream.Name = "tabPageStream";
            tabPageStream.Size = new Size(782, 240);
            tabPageStream.TabIndex = 1;
            tabPageStream.Text = GlobalResources.GetString("tabPageStream.Text");
            // 
            // labelStreamUrl
            // 
            labelStreamUrl.AutoSize = true;
            labelStreamUrl.Location = new Point(12, 20);
            labelStreamUrl.Name = "labelStreamUrl";
            labelStreamUrl.Size = new Size(172, 15);
            labelStreamUrl.TabIndex = 0;
            labelStreamUrl.Text = GlobalResources.GetString("labelStreamUrl.Text");
            // 
            // textBoxStreamUrl
            // 
            textBoxStreamUrl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxStreamUrl.Location = new Point(200, 17);
            textBoxStreamUrl.Name = "textBoxStreamUrl";
            textBoxStreamUrl.Size = new Size(560, 23);
            textBoxStreamUrl.TabIndex = 1;
            // 
            // labelRefererUrl
            // 
            labelRefererUrl.AutoSize = true;
            labelRefererUrl.Location = new Point(12, 52);
            labelRefererUrl.Name = "labelRefererUrl";
            labelRefererUrl.Size = new Size(135, 15);
            labelRefererUrl.TabIndex = 2;
            labelRefererUrl.Text = GlobalResources.GetString("labelRefererUrl.Text");
            // 
            // textBoxRefererUrl
            // 
            textBoxRefererUrl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxRefererUrl.Location = new Point(200, 49);
            textBoxRefererUrl.Name = "textBoxRefererUrl";
            textBoxRefererUrl.PlaceholderText = GlobalResources.GetString("textBoxRefererUrl.PlaceholderText");
            textBoxRefererUrl.Size = new Size(560, 23);
            textBoxRefererUrl.TabIndex = 3;
            // 
            // labelUserAgent
            // 
            labelUserAgent.AutoSize = true;
            labelUserAgent.Location = new Point(12, 84);
            labelUserAgent.Name = "labelUserAgent";
            labelUserAgent.Size = new Size(131, 15);
            labelUserAgent.TabIndex = 4;
            labelUserAgent.Text = GlobalResources.GetString("labelUserAgent.Text");
            // 
            // textBoxUserAgent
            // 
            textBoxUserAgent.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxUserAgent.Location = new Point(200, 81);
            textBoxUserAgent.Name = "textBoxUserAgent";
            textBoxUserAgent.PlaceholderText = GlobalResources.GetString("textBoxUserAgent.PlaceholderText");
            textBoxUserAgent.Size = new Size(560, 23);
            textBoxUserAgent.TabIndex = 5;
            // 
            // labelCookiesFile
            // 
            labelCookiesFile.AutoSize = true;
            labelCookiesFile.Location = new Point(12, 116);
            labelCookiesFile.Name = "labelCookiesFile";
            labelCookiesFile.Size = new Size(126, 15);
            labelCookiesFile.TabIndex = 6;
            labelCookiesFile.Text = GlobalResources.GetString("labelCookiesFile.Text");
            // 
            // textBoxCookiesFile
            // 
            textBoxCookiesFile.Location = new Point(200, 113);
            textBoxCookiesFile.Name = "textBoxCookiesFile";
            textBoxCookiesFile.Size = new Size(440, 23);
            textBoxCookiesFile.TabIndex = 7;
            // 
            // buttonBrowseCookiesFile
            // 
            buttonBrowseCookiesFile.Location = new Point(650, 111);
            buttonBrowseCookiesFile.Name = "buttonBrowseCookiesFile";
            buttonBrowseCookiesFile.Size = new Size(75, 23);
            buttonBrowseCookiesFile.TabIndex = 4;
            buttonBrowseCookiesFile.Text = GlobalResources.GetString("buttonBrowseCookiesFile.Text");
            buttonBrowseCookiesFile.Click += buttonBrowseCookiesFile_Click;
            // 
            // labelStreamOutputFile
            // 
            labelStreamOutputFile.AutoSize = true;
            labelStreamOutputFile.Location = new Point(12, 148);
            labelStreamOutputFile.Name = "labelStreamOutputFile";
            labelStreamOutputFile.Size = new Size(122, 15);
            labelStreamOutputFile.TabIndex = 5;
            labelStreamOutputFile.Text = GlobalResources.GetString("labelStreamOutputFile.Text");
            // 
            // textBoxStreamOutputFile
            // 
            textBoxStreamOutputFile.Location = new Point(200, 145);
            textBoxStreamOutputFile.Name = "textBoxStreamOutputFile";
            textBoxStreamOutputFile.Size = new Size(440, 23);
            textBoxStreamOutputFile.TabIndex = 6;
            // 
            // buttonBrowseStreamOutputFile
            // 
            buttonBrowseStreamOutputFile.Location = new Point(650, 143);
            buttonBrowseStreamOutputFile.Name = "buttonBrowseStreamOutputFile";
            buttonBrowseStreamOutputFile.Size = new Size(75, 23);
            buttonBrowseStreamOutputFile.TabIndex = 7;
            buttonBrowseStreamOutputFile.Text = GlobalResources.GetString("buttonBrowseStreamOutputFile.Text");
            buttonBrowseStreamOutputFile.Click += buttonBrowseStreamOutputFile_Click;
            // 
            // labelChunkMs
            // 
            labelChunkMs.AutoSize = true;
            labelChunkMs.Location = new Point(12, 180);
            labelChunkMs.Name = "labelChunkMs";
            labelChunkMs.Size = new Size(91, 15);
            labelChunkMs.TabIndex = 8;
            labelChunkMs.Text = GlobalResources.GetString("labelChunkMs.Text");
            // 
            // numericChunkMs
            // 
            numericChunkMs.Increment = new decimal(new int[] { 1000, 0, 0, 0 });
            numericChunkMs.Location = new Point(200, 177);
            numericChunkMs.Maximum = new decimal(new int[] { 60000, 0, 0, 0 });
            numericChunkMs.Minimum = new decimal(new int[] { 1000, 0, 0, 0 });
            numericChunkMs.Name = "numericChunkMs";
            numericChunkMs.Size = new Size(100, 23);
            numericChunkMs.TabIndex = 9;
            numericChunkMs.Value = new decimal(new int[] { 10000, 0, 0, 0 });
            // 
            // buttonLogin
            // 
            buttonLogin.AutoSize = true;
            buttonLogin.Location = new Point(330, 177);
            buttonLogin.Name = "buttonLogin";
            buttonLogin.TabIndex = 10;
            buttonLogin.Text = GlobalResources.GetString("buttonLogin.Text");
            buttonLogin.Click += buttonLogin_Click;
            // 
            // groupBoxCommon
            // 
            groupBoxCommon.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBoxCommon.Controls.Add(labelModel);
            groupBoxCommon.Controls.Add(comboBoxModel);
            groupBoxCommon.Controls.Add(labelLanguage);
            groupBoxCommon.Controls.Add(textBoxLanguage);
            groupBoxCommon.Controls.Add(labelPrompt);
            groupBoxCommon.Controls.Add(textBoxPrompt);
            groupBoxCommon.Location = new Point(12, 374);
            groupBoxCommon.Name = "groupBoxCommon";
            groupBoxCommon.Size = new Size(790, 60);
            groupBoxCommon.TabIndex = 2;
            groupBoxCommon.TabStop = false;
            groupBoxCommon.Text = GlobalResources.GetString("groupBoxCommon.Text");
            // 
            // labelModel
            // 
            labelModel.AutoSize = true;
            labelModel.Location = new Point(12, 26);
            labelModel.Name = "labelModel";
            labelModel.Size = new Size(44, 15);
            labelModel.TabIndex = 0;
            labelModel.Text = GlobalResources.GetString("labelModel.Text");
            // 
            // comboBoxModel
            // 
            comboBoxModel.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxModel.Items.AddRange(new object[]
            {
                GlobalResources.GetString("modelTiny"),
                GlobalResources.GetString("modelBase"),
                GlobalResources.GetString("modelSmall"),
                GlobalResources.GetString("modelMedium"),
                GlobalResources.GetString("modelLarge")
            });
            comboBoxModel.Location = new Point(70, 23);
            comboBoxModel.Name = "comboBoxModel";
            comboBoxModel.Size = new Size(110, 23);
            comboBoxModel.TabIndex = 1;
            // 
            // labelLanguage
            // 
            labelLanguage.AutoSize = true;
            labelLanguage.Location = new Point(200, 26);
            labelLanguage.Name = "labelLanguage";
            labelLanguage.Size = new Size(62, 15);
            labelLanguage.TabIndex = 2;
            labelLanguage.Text = GlobalResources.GetString("labelLanguage.Text");
            // 
            // textBoxLanguage
            // 
            textBoxLanguage.Location = new Point(270, 23);
            textBoxLanguage.Name = "textBoxLanguage";
            textBoxLanguage.Size = new Size(60, 23);
            textBoxLanguage.TabIndex = 3;
            textBoxLanguage.Text = "en";
            // 
            // labelPrompt
            // 
            labelPrompt.AutoSize = true;
            labelPrompt.Location = new Point(350, 26);
            labelPrompt.Name = "labelPrompt";
            labelPrompt.Size = new Size(82, 15);
            labelPrompt.TabIndex = 4;
            labelPrompt.Text = GlobalResources.GetString("labelPrompt.Text");
            // 
            // textBoxPrompt
            // 
            textBoxPrompt.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxPrompt.Location = new Point(430, 23);
            textBoxPrompt.Name = "textBoxPrompt";
            textBoxPrompt.Size = new Size(330, 23);
            textBoxPrompt.TabIndex = 5;
            textBoxPrompt.Text = GlobalResources.GetString("textBoxPrompt.Text");
            // 
            // groupBoxEngine
            // 
            groupBoxEngine.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBoxEngine.Controls.Add(labelModelsDir);
            groupBoxEngine.Controls.Add(textBoxModelsDir);
            groupBoxEngine.Controls.Add(buttonBrowseModelsDir);
            groupBoxEngine.Controls.Add(checkBoxUseGpu);
            groupBoxEngine.Location = new Point(12, 12);
            groupBoxEngine.Name = "groupBoxEngine";
            groupBoxEngine.Size = new Size(790, 82);
            groupBoxEngine.TabIndex = 0;
            groupBoxEngine.TabStop = false;
            groupBoxEngine.Text = GlobalResources.GetString("groupBoxEngine.Text");
            // 
            // labelModelsDir
            // 
            labelModelsDir.AutoSize = true;
            labelModelsDir.Location = new Point(12, 22);
            labelModelsDir.Name = "labelModelsDir";
            labelModelsDir.Size = new Size(129, 15);
            labelModelsDir.TabIndex = 0;
            labelModelsDir.Text = GlobalResources.GetString("labelModelsDir.Text");
            // 
            // textBoxModelsDir
            // 
            textBoxModelsDir.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxModelsDir.Location = new Point(160, 19);
            textBoxModelsDir.Name = "textBoxModelsDir";
            textBoxModelsDir.Size = new Size(540, 23);
            textBoxModelsDir.TabIndex = 1;
            // 
            // buttonBrowseModelsDir
            // 
            buttonBrowseModelsDir.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonBrowseModelsDir.Location = new Point(706, 17);
            buttonBrowseModelsDir.Name = "buttonBrowseModelsDir";
            buttonBrowseModelsDir.Size = new Size(75, 23);
            buttonBrowseModelsDir.TabIndex = 2;
            buttonBrowseModelsDir.Text = GlobalResources.GetString("buttonBrowseModelsDir.Text");
            buttonBrowseModelsDir.Click += buttonBrowseModelsDir_Click;
            // 
            // checkBoxUseGpu
            // 
            checkBoxUseGpu.AutoSize = true;
            checkBoxUseGpu.Checked = true;
            checkBoxUseGpu.CheckState = CheckState.Checked;
            checkBoxUseGpu.Location = new Point(160, 52);
            checkBoxUseGpu.Name = "checkBoxUseGpu";
            checkBoxUseGpu.Size = new Size(114, 19);
            checkBoxUseGpu.TabIndex = 3;
            checkBoxUseGpu.Text = GlobalResources.GetString("checkBoxUseGpu.Text");
            // 
            // buttonStart
            // 
            buttonStart.Location = new Point(12, 446);
            buttonStart.Name = "buttonStart";
            buttonStart.Size = new Size(100, 30);
            buttonStart.TabIndex = 3;
            buttonStart.Text = GlobalResources.GetString("buttonStart.Text");
            buttonStart.Click += buttonStart_Click;
            // 
            // buttonStop
            // 
            buttonStop.Enabled = false;
            buttonStop.Location = new Point(120, 446);
            buttonStop.Name = "buttonStop";
            buttonStop.Size = new Size(100, 30);
            buttonStop.TabIndex = 4;
            buttonStop.Text = GlobalResources.GetString("buttonStop.Text");
            buttonStop.Click += buttonStop_Click;
            // 
            // buttonSettings
            // 
            buttonSettings.Location = new Point(228, 446);
            buttonSettings.Name = "buttonSettings";
            buttonSettings.Size = new Size(100, 30);
            buttonSettings.TabIndex = 5;
            buttonSettings.Text = GlobalResources.GetString("buttonSettings.Text");
            buttonSettings.Click += buttonSettings_Click;
            // 
            // textBoxLog
            // 
            textBoxLog.Dock = DockStyle.Fill;
            textBoxLog.Font = new Font("Consolas", 9F);
            textBoxLog.Location = new Point(0, 20);
            textBoxLog.Multiline = true;
            textBoxLog.Name = "textBoxLog";
            textBoxLog.ReadOnly = true;
            textBoxLog.ScrollBars = ScrollBars.Vertical;
            textBoxLog.Size = new Size(390, 182);
            textBoxLog.TabIndex = 1;
            // 
            // splitContainerOutput
            // 
            splitContainerOutput.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            splitContainerOutput.Location = new Point(12, 470);
            splitContainerOutput.Name = "splitContainerOutput";
            splitContainerOutput.Panel1.Controls.Add(textBoxTranscript);
            splitContainerOutput.Panel1.Controls.Add(labelAudioActivity);
            splitContainerOutput.Panel1.Controls.Add(labelTranscript);
            splitContainerOutput.Panel2.Controls.Add(textBoxLog);
            splitContainerOutput.Panel2.Controls.Add(labelLog);
            splitContainerOutput.Size = new Size(790, 202);
            splitContainerOutput.SplitterDistance = 392;
            splitContainerOutput.TabIndex = 6;
            // 
            // labelTranscript
            // 
            labelTranscript.AutoSize = true;
            labelTranscript.Dock = DockStyle.Top;
            labelTranscript.Location = new Point(0, 0);
            labelTranscript.Name = "labelTranscript";
            labelTranscript.Padding = new Padding(0, 3, 0, 0);
            labelTranscript.Size = new Size(69, 18);
            labelTranscript.TabIndex = 0;
            labelTranscript.Text = GlobalResources.GetString("labelTranscript.Text");
            // 
            // labelAudioActivity
            // 
            labelAudioActivity.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            labelAudioActivity.AutoSize = true;
            labelAudioActivity.ForeColor = Color.Gray;
            labelAudioActivity.Location = new Point(282, 3);
            labelAudioActivity.Name = "labelAudioActivity";
            labelAudioActivity.Size = new Size(108, 15);
            labelAudioActivity.TabIndex = 1;
            labelAudioActivity.Text = GlobalResources.GetString("labelAudioActivity.Idle");
            labelAudioActivity.TextAlign = ContentAlignment.MiddleRight;
            // 
            // textBoxTranscript
            // 
            textBoxTranscript.Dock = DockStyle.Fill;
            textBoxTranscript.Font = new Font("Consolas", 9F);
            textBoxTranscript.Location = new Point(0, 20);
            textBoxTranscript.Multiline = true;
            textBoxTranscript.Name = "textBoxTranscript";
            textBoxTranscript.ReadOnly = true;
            textBoxTranscript.ScrollBars = ScrollBars.Vertical;
            textBoxTranscript.Size = new Size(392, 182);
            textBoxTranscript.TabIndex = 2;
            // 
            // labelLog
            // 
            labelLog.AutoSize = true;
            labelLog.Dock = DockStyle.Top;
            labelLog.Location = new Point(0, 0);
            labelLog.Name = "labelLog";
            labelLog.Padding = new Padding(0, 3, 0, 0);
            labelLog.Size = new Size(31, 18);
            labelLog.TabIndex = 0;
            labelLog.Text = GlobalResources.GetString("labelLog.Text");
            // 
            // timerAudioActivity
            // 
            timerAudioActivity.Interval = 800;
            timerAudioActivity.Tick += timerAudioActivity_Tick;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(814, 684);
            Controls.Add(groupBoxEngine);
            Controls.Add(tabControlMode);
            Controls.Add(groupBoxCommon);
            Controls.Add(buttonStart);
            Controls.Add(buttonStop);
            Controls.Add(buttonSettings);
            Controls.Add(splitContainerOutput);
            MinimumSize = new Size(700, 500);
            Name = "MainForm";
            Text = GlobalResources.GetString("titleMainForm");
            tabControlMode.ResumeLayout(false);
            tabPageBatch.ResumeLayout(false);
            tabPageBatch.PerformLayout();
            tabPageStream.ResumeLayout(false);
            tabPageStream.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericChunkMs).EndInit();
            groupBoxCommon.ResumeLayout(false);
            groupBoxCommon.PerformLayout();
            groupBoxEngine.ResumeLayout(false);
            groupBoxEngine.PerformLayout();
            splitContainerOutput.Panel1.ResumeLayout(false);
            splitContainerOutput.Panel1.PerformLayout();
            splitContainerOutput.Panel2.ResumeLayout(false);
            splitContainerOutput.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerOutput).EndInit();
            splitContainerOutput.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
