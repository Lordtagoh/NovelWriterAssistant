namespace NovelWriterAssistant
{
    partial class NovelWriterForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            if (disposing)
            {
                httpClient?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            mainTitleLabel = new System.Windows.Forms.Label();
            mainTitleTextBox = new System.Windows.Forms.TextBox();
            subtitleLabel = new System.Windows.Forms.Label();
            subtitleTextBox = new System.Windows.Forms.TextBox();
            novelTextBox = new System.Windows.Forms.TextBox();
            promptTextBox = new System.Windows.Forms.TextBox();
            systemPromptLabel = new System.Windows.Forms.Label();
            systemPromptTextBox = new System.Windows.Forms.TextBox();
            generateOneParagraphButton = new System.Windows.Forms.Button();
            saveButton = new System.Windows.Forms.Button();
            loadJsonButton = new System.Windows.Forms.Button();
            loadAutoSaveButton = new System.Windows.Forms.Button();
            clearAllButton = new System.Windows.Forms.Button();
            option1TextBox = new System.Windows.Forms.TextBox();
            option2TextBox = new System.Windows.Forms.TextBox();
            option3TextBox = new System.Windows.Forms.TextBox();
            promptLabel = new System.Windows.Forms.Label();
            optionsLabel = new System.Windows.Forms.Label();
            useOption1Button = new System.Windows.Forms.Button();
            useOption2Button = new System.Windows.Forms.Button();
            useOption3Button = new System.Windows.Forms.Button();
            modelLabel = new System.Windows.Forms.Label();
            modelComboBox = new System.Windows.Forms.ComboBox();
            generationTimeLabel = new System.Windows.Forms.Label();
            generateTwoParagraphButton = new System.Windows.Forms.Button();
            generateThreeParagraphButton = new System.Windows.Forms.Button();
            loadDBButton = new System.Windows.Forms.Button();
            sessionStatusLabel = new System.Windows.Forms.Label();
            resetSessionButton = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // mainTitleLabel
            // 
            mainTitleLabel.AutoSize = true;
            mainTitleLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            mainTitleLabel.Location = new System.Drawing.Point(10, 10);
            mainTitleLabel.Name = "mainTitleLabel";
            mainTitleLabel.Size = new System.Drawing.Size(65, 15);
            mainTitleLabel.TabIndex = 22;
            mainTitleLabel.Text = "Main Title:";
            // 
            // mainTitleTextBox
            // 
            mainTitleTextBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            mainTitleTextBox.Location = new System.Drawing.Point(85, 7);
            mainTitleTextBox.Name = "mainTitleTextBox";
            mainTitleTextBox.Size = new System.Drawing.Size(400, 25);
            mainTitleTextBox.TabIndex = 23;
            // 
            // subtitleLabel
            // 
            subtitleLabel.AutoSize = true;
            subtitleLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            subtitleLabel.Location = new System.Drawing.Point(500, 10);
            subtitleLabel.Name = "subtitleLabel";
            subtitleLabel.Size = new System.Drawing.Size(54, 15);
            subtitleLabel.TabIndex = 24;
            subtitleLabel.Text = "Subtitle:";
            // 
            // subtitleTextBox
            // 
            subtitleTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            subtitleTextBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            subtitleTextBox.Location = new System.Drawing.Point(562, 7);
            subtitleTextBox.Name = "subtitleTextBox";
            subtitleTextBox.Size = new System.Drawing.Size(408, 25);
            subtitleTextBox.TabIndex = 25;
            // 
            // novelTextBox
            // 
            novelTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            novelTextBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            novelTextBox.Location = new System.Drawing.Point(10, 40);
            novelTextBox.Multiline = true;
            novelTextBox.Name = "novelTextBox";
            novelTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            novelTextBox.Size = new System.Drawing.Size(960, 300);
            novelTextBox.TabIndex = 0;
            // 
            // promptTextBox
            // 
            promptTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            promptTextBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            promptTextBox.Location = new System.Drawing.Point(10, 375);
            promptTextBox.Multiline = true;
            promptTextBox.Name = "promptTextBox";
            promptTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            promptTextBox.Size = new System.Drawing.Size(960, 60);
            promptTextBox.TabIndex = 1;
            // 
            // systemPromptLabel
            // 
            systemPromptLabel.AutoSize = true;
            systemPromptLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            systemPromptLabel.Location = new System.Drawing.Point(10, 445);
            systemPromptLabel.Name = "systemPromptLabel";
            systemPromptLabel.Size = new System.Drawing.Size(223, 15);
            systemPromptLabel.TabIndex = 13;
            systemPromptLabel.Text = "System Prompt (constant instructions):";
            // 
            // systemPromptTextBox
            // 
            systemPromptTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            systemPromptTextBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            systemPromptTextBox.Location = new System.Drawing.Point(10, 470);
            systemPromptTextBox.Multiline = true;
            systemPromptTextBox.Name = "systemPromptTextBox";
            systemPromptTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            systemPromptTextBox.Size = new System.Drawing.Size(960, 60);
            systemPromptTextBox.TabIndex = 14;
            // 
            // generateOneParagraphButton
            // 
            generateOneParagraphButton.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            generateOneParagraphButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            generateOneParagraphButton.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            generateOneParagraphButton.ForeColor = System.Drawing.Color.White;
            generateOneParagraphButton.Location = new System.Drawing.Point(10, 540);
            generateOneParagraphButton.Name = "generateOneParagraphButton";
            generateOneParagraphButton.Size = new System.Drawing.Size(290, 35);
            generateOneParagraphButton.TabIndex = 2;
            generateOneParagraphButton.Text = "Generate Next 1 Paragraph";
            generateOneParagraphButton.UseVisualStyleBackColor = false;
            generateOneParagraphButton.Click += GenerateButton_Click;
            // 
            // saveButton
            // 
            saveButton.BackColor = System.Drawing.Color.FromArgb(0, 150, 136);
            saveButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            saveButton.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            saveButton.ForeColor = System.Drawing.Color.White;
            saveButton.Location = new System.Drawing.Point(520, 594);
            saveButton.Name = "saveButton";
            saveButton.Size = new System.Drawing.Size(100, 35);
            saveButton.TabIndex = 15;
            saveButton.Text = "Save";
            saveButton.UseVisualStyleBackColor = false;
            saveButton.Click += SaveButton_Click;
            // 
            // loadJsonButton
            // 
            loadJsonButton.BackColor = System.Drawing.Color.FromArgb(156, 39, 176);
            loadJsonButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            loadJsonButton.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            loadJsonButton.ForeColor = System.Drawing.Color.White;
            loadJsonButton.Location = new System.Drawing.Point(736, 595);
            loadJsonButton.Name = "loadJsonButton";
            loadJsonButton.Size = new System.Drawing.Size(100, 35);
            loadJsonButton.TabIndex = 16;
            loadJsonButton.Text = "Load JSON";
            loadJsonButton.UseVisualStyleBackColor = false;
            loadJsonButton.Click += LoadFromJsonButton_Click;
            // 
            // loadAutoSaveButton
            // 
            loadAutoSaveButton.BackColor = System.Drawing.Color.FromArgb(255, 152, 0);
            loadAutoSaveButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            loadAutoSaveButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            loadAutoSaveButton.ForeColor = System.Drawing.Color.White;
            loadAutoSaveButton.Location = new System.Drawing.Point(650, 595);
            loadAutoSaveButton.Name = "loadAutoSaveButton";
            loadAutoSaveButton.Size = new System.Drawing.Size(80, 35);
            loadAutoSaveButton.TabIndex = 20;
            loadAutoSaveButton.Text = "Load Auto";
            loadAutoSaveButton.UseVisualStyleBackColor = false;
            loadAutoSaveButton.Click += LoadAutoSaveButton_Click;
            // 
            // clearAllButton
            // 
            clearAllButton.BackColor = System.Drawing.Color.FromArgb(244, 67, 54);
            clearAllButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            clearAllButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            clearAllButton.ForeColor = System.Drawing.Color.White;
            clearAllButton.Location = new System.Drawing.Point(335, 598);
            clearAllButton.Name = "clearAllButton";
            clearAllButton.Size = new System.Drawing.Size(150, 30);
            clearAllButton.TabIndex = 21;
            clearAllButton.Text = "Clear All Textboxes";
            clearAllButton.UseVisualStyleBackColor = false;
            clearAllButton.Click += ClearAllButton_Click;
            // 
            // option1TextBox
            // 
            option1TextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            option1TextBox.BackColor = System.Drawing.Color.White;
            option1TextBox.Font = new System.Drawing.Font("Segoe UI", 9F);
            option1TextBox.Location = new System.Drawing.Point(10, 640);
            option1TextBox.Multiline = true;
            option1TextBox.Name = "option1TextBox";
            option1TextBox.ReadOnly = true;
            option1TextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            option1TextBox.Size = new System.Drawing.Size(310, 220);
            option1TextBox.TabIndex = 3;
            // 
            // option2TextBox
            // 
            option2TextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            option2TextBox.BackColor = System.Drawing.Color.White;
            option2TextBox.Font = new System.Drawing.Font("Segoe UI", 9F);
            option2TextBox.Location = new System.Drawing.Point(330, 640);
            option2TextBox.Multiline = true;
            option2TextBox.Name = "option2TextBox";
            option2TextBox.ReadOnly = true;
            option2TextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            option2TextBox.Size = new System.Drawing.Size(310, 220);
            option2TextBox.TabIndex = 4;
            // 
            // option3TextBox
            // 
            option3TextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            option3TextBox.BackColor = System.Drawing.Color.White;
            option3TextBox.Font = new System.Drawing.Font("Segoe UI", 9F);
            option3TextBox.Location = new System.Drawing.Point(650, 640);
            option3TextBox.Multiline = true;
            option3TextBox.Name = "option3TextBox";
            option3TextBox.ReadOnly = true;
            option3TextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            option3TextBox.Size = new System.Drawing.Size(310, 220);
            option3TextBox.TabIndex = 5;
            // 
            // promptLabel
            // 
            promptLabel.AutoSize = true;
            promptLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            promptLabel.Location = new System.Drawing.Point(10, 350);
            promptLabel.Name = "promptLabel";
            promptLabel.Size = new System.Drawing.Size(185, 15);
            promptLabel.TabIndex = 6;
            promptLabel.Text = "How should the novel continue?";
            // 
            // optionsLabel
            // 
            optionsLabel.AutoSize = true;
            optionsLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            optionsLabel.Location = new System.Drawing.Point(10, 585);
            optionsLabel.Name = "optionsLabel";
            optionsLabel.Size = new System.Drawing.Size(116, 15);
            optionsLabel.TabIndex = 7;
            optionsLabel.Text = "Generated Options:";
            // 
            // useOption1Button
            // 
            useOption1Button.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            useOption1Button.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            useOption1Button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            useOption1Button.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            useOption1Button.ForeColor = System.Drawing.Color.White;
            useOption1Button.Location = new System.Drawing.Point(10, 835);
            useOption1Button.Name = "useOption1Button";
            useOption1Button.Size = new System.Drawing.Size(310, 30);
            useOption1Button.TabIndex = 8;
            useOption1Button.Text = "Use This Option";
            useOption1Button.UseVisualStyleBackColor = false;
            useOption1Button.Click += UseThisOption_AddToNovel;
            // 
            // useOption2Button
            // 
            useOption2Button.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            useOption2Button.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            useOption2Button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            useOption2Button.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            useOption2Button.ForeColor = System.Drawing.Color.White;
            useOption2Button.Location = new System.Drawing.Point(330, 835);
            useOption2Button.Name = "useOption2Button";
            useOption2Button.Size = new System.Drawing.Size(310, 30);
            useOption2Button.TabIndex = 9;
            useOption2Button.Text = "Use This Option";
            useOption2Button.UseVisualStyleBackColor = false;
            useOption2Button.Click += UseThisOption_AddToNovel;
            // 
            // useOption3Button
            // 
            useOption3Button.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            useOption3Button.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            useOption3Button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            useOption3Button.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            useOption3Button.ForeColor = System.Drawing.Color.White;
            useOption3Button.Location = new System.Drawing.Point(650, 835);
            useOption3Button.Name = "useOption3Button";
            useOption3Button.Size = new System.Drawing.Size(310, 30);
            useOption3Button.TabIndex = 10;
            useOption3Button.Text = "Use This Option";
            useOption3Button.UseVisualStyleBackColor = false;
            useOption3Button.Click += UseThisOption_AddToNovel;
            // 
            // modelLabel
            // 
            modelLabel.AutoSize = true;
            modelLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            modelLabel.Location = new System.Drawing.Point(10, 610);
            modelLabel.Name = "modelLabel";
            modelLabel.Size = new System.Drawing.Size(45, 15);
            modelLabel.TabIndex = 11;
            modelLabel.Text = "Model:";
            // 
            // modelComboBox
            // 
            modelComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            modelComboBox.Font = new System.Drawing.Font("Segoe UI", 9F);
            modelComboBox.FormattingEnabled = true;
            modelComboBox.Location = new System.Drawing.Point(65, 607);
            modelComboBox.Name = "modelComboBox";
            modelComboBox.Size = new System.Drawing.Size(235, 23);
            modelComboBox.TabIndex = 12;
            // 
            // generationTimeLabel
            // 
            generationTimeLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            generationTimeLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            generationTimeLabel.ForeColor = System.Drawing.Color.Gray;
            generationTimeLabel.Location = new System.Drawing.Point(10, 865);
            generationTimeLabel.Name = "generationTimeLabel";
            generationTimeLabel.Size = new System.Drawing.Size(932, 20);
            generationTimeLabel.TabIndex = 17;
            // 
            // generateTwoParagraphButton
            // 
            generateTwoParagraphButton.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            generateTwoParagraphButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            generateTwoParagraphButton.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            generateTwoParagraphButton.ForeColor = System.Drawing.Color.White;
            generateTwoParagraphButton.Location = new System.Drawing.Point(330, 540);
            generateTwoParagraphButton.Name = "generateTwoParagraphButton";
            generateTwoParagraphButton.Size = new System.Drawing.Size(290, 35);
            generateTwoParagraphButton.TabIndex = 18;
            generateTwoParagraphButton.Text = "Generate Next 2 Paragraph";
            generateTwoParagraphButton.UseVisualStyleBackColor = false;
            generateTwoParagraphButton.Click += GenerateButton_Click;
            // 
            // generateThreeParagraphButton
            // 
            generateThreeParagraphButton.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            generateThreeParagraphButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            generateThreeParagraphButton.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            generateThreeParagraphButton.ForeColor = System.Drawing.Color.White;
            generateThreeParagraphButton.Location = new System.Drawing.Point(652, 540);
            generateThreeParagraphButton.Name = "generateThreeParagraphButton";
            generateThreeParagraphButton.Size = new System.Drawing.Size(290, 35);
            generateThreeParagraphButton.TabIndex = 19;
            generateThreeParagraphButton.Text = "Generate Next 3 Paragraph";
            generateThreeParagraphButton.UseVisualStyleBackColor = false;
            generateThreeParagraphButton.Click += GenerateButton_Click;
            // 
            // loadDBButton
            // 
            loadDBButton.BackColor = System.Drawing.Color.MediumPurple;
            loadDBButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            loadDBButton.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            loadDBButton.ForeColor = System.Drawing.Color.White;
            loadDBButton.Location = new System.Drawing.Point(842, 594);
            loadDBButton.Name = "loadDBButton";
            loadDBButton.Size = new System.Drawing.Size(100, 35);
            loadDBButton.TabIndex = 26;
            loadDBButton.Text = "Load DB";
            loadDBButton.UseVisualStyleBackColor = false;
            loadDBButton.Click += LoadDBButton_Click;
            //
            // sessionStatusLabel
            //
            sessionStatusLabel.AutoSize = true;
            sessionStatusLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            sessionStatusLabel.ForeColor = System.Drawing.Color.DarkGreen;
            sessionStatusLabel.Location = new System.Drawing.Point(10, 850);
            sessionStatusLabel.Name = "sessionStatusLabel";
            sessionStatusLabel.Size = new System.Drawing.Size(120, 15);
            sessionStatusLabel.TabIndex = 27;
            sessionStatusLabel.Text = "Session: Not started";
            //
            // resetSessionButton
            //
            resetSessionButton.BackColor = System.Drawing.Color.FromArgb(255, 128, 0);
            resetSessionButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            resetSessionButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            resetSessionButton.ForeColor = System.Drawing.Color.White;
            resetSessionButton.Location = new System.Drawing.Point(150, 845);
            resetSessionButton.Name = "resetSessionButton";
            resetSessionButton.Size = new System.Drawing.Size(110, 25);
            resetSessionButton.TabIndex = 28;
            resetSessionButton.Text = "Reset Session";
            resetSessionButton.UseVisualStyleBackColor = false;
            resetSessionButton.Click += ResetSessionButton_Click;
            //
            // NovelWriterForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(984, 880);
            Controls.Add(resetSessionButton);
            Controls.Add(sessionStatusLabel);
            Controls.Add(loadDBButton);
            Controls.Add(generateThreeParagraphButton);
            Controls.Add(generateTwoParagraphButton);
            Controls.Add(generationTimeLabel);
            Controls.Add(clearAllButton);
            Controls.Add(loadAutoSaveButton);
            Controls.Add(loadJsonButton);
            Controls.Add(saveButton);
            Controls.Add(systemPromptTextBox);
            Controls.Add(systemPromptLabel);
            Controls.Add(modelComboBox);
            Controls.Add(modelLabel);
            Controls.Add(useOption3Button);
            Controls.Add(useOption2Button);
            Controls.Add(useOption1Button);
            Controls.Add(optionsLabel);
            Controls.Add(promptLabel);
            Controls.Add(option3TextBox);
            Controls.Add(option2TextBox);
            Controls.Add(option1TextBox);
            Controls.Add(generateOneParagraphButton);
            Controls.Add(promptTextBox);
            Controls.Add(novelTextBox);
            Controls.Add(subtitleTextBox);
            Controls.Add(subtitleLabel);
            Controls.Add(mainTitleTextBox);
            Controls.Add(mainTitleLabel);
            MinimumSize = new System.Drawing.Size(800, 600);
            Name = "NovelWriterForm";
            Text = "Novel Writer Assistant";
            FormClosing += OnClosing_AutoSave;
            Load += NovelWriterForm_Load;
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label mainTitleLabel;
        private System.Windows.Forms.TextBox mainTitleTextBox;
        private System.Windows.Forms.Label subtitleLabel;
        private System.Windows.Forms.TextBox subtitleTextBox;
        private System.Windows.Forms.TextBox novelTextBox;
        private System.Windows.Forms.TextBox promptTextBox;
        private System.Windows.Forms.Label systemPromptLabel;
        private System.Windows.Forms.TextBox systemPromptTextBox;
        private System.Windows.Forms.Button generateOneParagraphButton;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button loadJsonButton;
        private System.Windows.Forms.Button loadAutoSaveButton;
        private System.Windows.Forms.Button clearAllButton;
        private System.Windows.Forms.TextBox option1TextBox;
        private System.Windows.Forms.TextBox option2TextBox;
        private System.Windows.Forms.TextBox option3TextBox;
        private System.Windows.Forms.Label promptLabel;
        private System.Windows.Forms.Label optionsLabel;
        private System.Windows.Forms.Button useOption1Button;
        private System.Windows.Forms.Button useOption2Button;
        private System.Windows.Forms.Button useOption3Button;
        private System.Windows.Forms.Label modelLabel;
        private System.Windows.Forms.ComboBox modelComboBox;
        private System.Windows.Forms.Label generationTimeLabel;
        private System.Windows.Forms.Button generateTwoParagraphButton;
        private System.Windows.Forms.Button generateThreeParagraphButton;
        private System.Windows.Forms.Button loadDBButton;
        private System.Windows.Forms.Label sessionStatusLabel;
        private System.Windows.Forms.Button resetSessionButton;
    }
}
