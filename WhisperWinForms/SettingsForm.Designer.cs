namespace WhisperWinForms
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;
        private Label labelSettingsLanguage;
        private ComboBox comboBoxLanguage;
        private Button buttonOk;
        private Button buttonCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            labelSettingsLanguage = new Label();
            comboBoxLanguage = new ComboBox();
            buttonOk = new Button();
            buttonCancel = new Button();
            SuspendLayout();
            // 
            // labelSettingsLanguage
            // 
            labelSettingsLanguage.AutoSize = true;
            labelSettingsLanguage.Location = new Point(16, 20);
            labelSettingsLanguage.Name = "labelSettingsLanguage";
            labelSettingsLanguage.Size = new Size(60, 15);
            labelSettingsLanguage.TabIndex = 0;
            labelSettingsLanguage.Text = GlobalResources.GetString("labelSettingsLanguage.Text");
            // 
            // comboBoxLanguage
            // 
            comboBoxLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxLanguage.FormattingEnabled = true;
            comboBoxLanguage.Items.AddRange(new object[]
            {
                GlobalResources.GetString("languageEnglish"),
                GlobalResources.GetString("languageFrench"),
                GlobalResources.GetString("languageGerman"),
                GlobalResources.GetString("languageSpanish")
            });
            comboBoxLanguage.Location = new Point(100, 16);
            comboBoxLanguage.Name = "comboBoxLanguage";
            comboBoxLanguage.Size = new Size(190, 23);
            comboBoxLanguage.TabIndex = 1;
            // 
            // buttonOk
            // 
            buttonOk.Location = new Point(134, 58);
            buttonOk.Name = "buttonOk";
            buttonOk.Size = new Size(75, 28);
            buttonOk.TabIndex = 2;
            buttonOk.Text = GlobalResources.GetString("buttonSettingsOk.Text");
            buttonOk.UseVisualStyleBackColor = true;
            buttonOk.Click += buttonOk_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(215, 58);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 28);
            buttonCancel.TabIndex = 3;
            buttonCancel.Text = GlobalResources.GetString("buttonSettingsCancel.Text");
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // SettingsForm
            // 
            AcceptButton = buttonOk;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new Size(306, 104);
            Controls.Add(labelSettingsLanguage);
            Controls.Add(comboBoxLanguage);
            Controls.Add(buttonOk);
            Controls.Add(buttonCancel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SettingsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = GlobalResources.GetString("titleSettings");
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
