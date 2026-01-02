namespace NovelWriterAssistant
{
    partial class LoadDatabaseForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ListBox mainTitleListBox;
        private System.Windows.Forms.ListBox subTitleListBox;
        private System.Windows.Forms.ListBox versionListBox;
        private System.Windows.Forms.Label mainTitleLabel;
        private System.Windows.Forms.Label subTitleLabel;
        private System.Windows.Forms.Label versionLabel;
        private System.Windows.Forms.Button loadButton;
        private System.Windows.Forms.Button cancelButton;

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
            mainTitleListBox = new System.Windows.Forms.ListBox();
            subTitleListBox = new System.Windows.Forms.ListBox();
            versionListBox = new System.Windows.Forms.ListBox();
            mainTitleLabel = new System.Windows.Forms.Label();
            subTitleLabel = new System.Windows.Forms.Label();
            versionLabel = new System.Windows.Forms.Label();
            loadButton = new System.Windows.Forms.Button();
            cancelButton = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // mainTitleListBox
            // 
            mainTitleListBox.FormattingEnabled = true;
            mainTitleListBox.Location = new System.Drawing.Point(12, 27);
            mainTitleListBox.Name = "mainTitleListBox";
            mainTitleListBox.Size = new System.Drawing.Size(250, 439);
            mainTitleListBox.TabIndex = 1;
            mainTitleListBox.SelectedIndexChanged += MainTitleListBox_SelectedIndexChanged;
            // 
            // subTitleListBox
            // 
            subTitleListBox.FormattingEnabled = true;
            subTitleListBox.Location = new System.Drawing.Point(268, 27);
            subTitleListBox.Name = "subTitleListBox";
            subTitleListBox.Size = new System.Drawing.Size(250, 439);
            subTitleListBox.TabIndex = 3;
            subTitleListBox.SelectedIndexChanged += SubTitleListBox_SelectedIndexChanged;
            // 
            // versionListBox
            // 
            versionListBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            versionListBox.FormattingEnabled = true;
            versionListBox.HorizontalScrollbar = true;
            versionListBox.Location = new System.Drawing.Point(524, 27);
            versionListBox.Name = "versionListBox";
            versionListBox.Size = new System.Drawing.Size(629, 439);
            versionListBox.TabIndex = 5;
            versionListBox.SelectedIndexChanged += VersionListBox_SelectedIndexChanged;
            versionListBox.DoubleClick += OnVersionDoubleClick;
            // 
            // mainTitleLabel
            // 
            mainTitleLabel.AutoSize = true;
            mainTitleLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            mainTitleLabel.Location = new System.Drawing.Point(12, 9);
            mainTitleLabel.Name = "mainTitleLabel";
            mainTitleLabel.Size = new System.Drawing.Size(103, 15);
            mainTitleLabel.TabIndex = 0;
            mainTitleLabel.Text = "Select Main Title:";
            // 
            // subTitleLabel
            // 
            subTitleLabel.AutoSize = true;
            subTitleLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            subTitleLabel.Location = new System.Drawing.Point(268, 9);
            subTitleLabel.Name = "subTitleLabel";
            subTitleLabel.Size = new System.Drawing.Size(92, 15);
            subTitleLabel.TabIndex = 2;
            subTitleLabel.Text = "Select Subtitle:";
            // 
            // versionLabel
            // 
            versionLabel.AutoSize = true;
            versionLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            versionLabel.Location = new System.Drawing.Point(524, 9);
            versionLabel.Name = "versionLabel";
            versionLabel.Size = new System.Drawing.Size(89, 15);
            versionLabel.TabIndex = 4;
            versionLabel.Text = "Select Version:";
            // 
            // loadButton
            // 
            loadButton.Enabled = false;
            loadButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            loadButton.Location = new System.Drawing.Point(997, 472);
            loadButton.Name = "loadButton";
            loadButton.Size = new System.Drawing.Size(75, 30);
            loadButton.TabIndex = 6;
            loadButton.Text = "Load";
            loadButton.UseVisualStyleBackColor = true;
            loadButton.Click += LoadButton_Click;
            // 
            // cancelButton
            // 
            cancelButton.Location = new System.Drawing.Point(1078, 472);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(75, 30);
            cancelButton.TabIndex = 7;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += CancelButton_Click;
            // 
            // LoadDatabaseForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1165, 514);
            Controls.Add(cancelButton);
            Controls.Add(loadButton);
            Controls.Add(versionListBox);
            Controls.Add(versionLabel);
            Controls.Add(subTitleListBox);
            Controls.Add(subTitleLabel);
            Controls.Add(mainTitleListBox);
            Controls.Add(mainTitleLabel);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "LoadDatabaseForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Load from Database";
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
