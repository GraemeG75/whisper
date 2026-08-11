using Microsoft.Web.WebView2.WinForms;

namespace WhisperWinForms
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null;

        private WebView2 _webView;
        private Button _buttonUseSession;
        private Button _buttonCancel;
        private FlowLayoutPanel buttons;
        private Panel toolbar;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            _webView = new WebView2();
            _buttonUseSession = new Button();
            _buttonCancel = new Button();
            buttons = new FlowLayoutPanel();
            toolbar = new Panel();
            ((System.ComponentModel.ISupportInitialize)_webView).BeginInit();
            buttons.SuspendLayout();
            toolbar.SuspendLayout();
            SuspendLayout();
            // 
            // _webView
            // 
            _webView.AllowExternalDrop = true;
            _webView.CreationProperties = null;
            _webView.DefaultBackgroundColor = Color.White;
            _webView.Dock = DockStyle.Fill;
            _webView.Location = new Point(0, 0);
            _webView.Name = "_webView";
            _webView.Size = new Size(1100, 674);
            _webView.TabIndex = 0;
            _webView.ZoomFactor = 1D;
            // 
            // _buttonUseSession
            // 
            _buttonUseSession.AutoSize = true;
            _buttonUseSession.Location = new Point(11, 11);
            _buttonUseSession.Name = "_buttonUseSession";
            _buttonUseSession.Size = new Size(135, 25);
            _buttonUseSession.TabIndex = 0;
            _buttonUseSession.Text = "Use logged-in session";
            _buttonUseSession.UseVisualStyleBackColor = true;
            _buttonUseSession.Click += ButtonUseSession_Click;
            // 
            // _buttonCancel
            // 
            _buttonCancel.AutoSize = true;
            _buttonCancel.Location = new Point(11, 42);
            _buttonCancel.Name = "_buttonCancel";
            _buttonCancel.Size = new Size(54, 25);
            _buttonCancel.TabIndex = 1;
            _buttonCancel.Text = "Cancel";
            _buttonCancel.UseVisualStyleBackColor = true;
            _buttonCancel.Click += ButtonCancel_Click;
            // 
            // buttons
            // 
            buttons.AutoSize = true;
            buttons.Controls.Add(_buttonUseSession);
            buttons.Controls.Add(_buttonCancel);
            buttons.Dock = DockStyle.Right;
            buttons.Location = new Point(943, 0);
            buttons.Name = "buttons";
            buttons.Padding = new Padding(8);
            buttons.Size = new Size(157, 86);
            buttons.TabIndex = 0;
            // 
            // toolbar
            // 
            toolbar.Controls.Add(buttons);
            toolbar.Dock = DockStyle.Bottom;
            toolbar.Location = new Point(0, 674);
            toolbar.Name = "toolbar";
            toolbar.Size = new Size(1100, 86);
            toolbar.TabIndex = 1;
            // 
            // LoginForm
            // 
            AcceptButton = _buttonUseSession;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = _buttonCancel;
            ClientSize = new Size(1100, 760);
            Controls.Add(_webView);
            Controls.Add(toolbar);
            MinimumSize = new Size(800, 500);
            Name = "LoginForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Sign in to stream provider";
            Load += LoginForm_Load;
            ((System.ComponentModel.ISupportInitialize)_webView).EndInit();
            buttons.ResumeLayout(false);
            buttons.PerformLayout();
            toolbar.ResumeLayout(false);
            toolbar.PerformLayout();
            ResumeLayout(false);
        }
    }
}