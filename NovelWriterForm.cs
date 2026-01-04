using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic;

namespace NovelWriterAssistant
{
    public partial class NovelWriterForm : Form
    {
        private HttpClient httpClient;
        private const string OLLAMA_URL = "http://localhost:11434/api/generate";
        private const string OLLAMA_CHAT_URL = "http://localhost:11434/api/chat";
        private const string OLLAMA_TAGS_URL = "http://localhost:11434/api/tags";
        private const string autoSaveTitle = "autosave";
        private readonly string dbFilePath;
        private readonly string connectionString;
        private string autoSaveLoadedModel = "";
        private bool novelWasChangedFromLastSave;

        // Session management fields
        private OllamaSessionManager sessionManager;
        private string lastNovelText = "";
        private string currentModelName = "";

        // Class to hold generation response with token counts
        private class GenerationResponse
        {
            public string Text { get; set; } = "";
            public int PromptTokens { get; set; } = 0;
            public int ResponseTokens { get; set; } = 0;
            public int TotalTokens => PromptTokens + ResponseTokens;
        }
        #region Init
        public NovelWriterForm()
        {
            InitializeComponent();
            httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(5); // Longer timeout for LLM generation

            // Initialize session manager
            sessionManager = new OllamaSessionManager();

            // Set up database file path
            string tempFolder = System.IO.Path.GetTempPath();
            dbFilePath = System.IO.Path.Combine(tempFolder, "NovelWriterAssistant_AutoSave.db");
            connectionString = $"Data Source={dbFilePath}";

            // Initialize database
            InitializeDatabase();

            // Load available models from Ollama
            LoadAvailableModels();
        }

        private void NovelWriterForm_Load(object sender, EventArgs e)
        {
            // Subscribe to text changed event for auto-save (only novel textbox)
            novelTextBox.TextChanged += NovelTextBox_TextChanged;

            // Try to load from temp file if it exists
            LoadFromDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    // Enable foreign key support
                    var pragmaCommand = connection.CreateCommand();
                    pragmaCommand.CommandText = "PRAGMA foreign_keys = ON";
                    pragmaCommand.ExecuteNonQuery();

                    var command = connection.CreateCommand();

                    // Create MainTitles table
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS MainTitles (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            MainTitle TEXT NOT NULL UNIQUE,
                            CreatedDateTime TEXT NOT NULL,
                            UpdatedDateTime TEXT NOT NULL
                        );
                        CREATE INDEX IF NOT EXISTS idx_maintitles_title ON MainTitles(MainTitle);
                    ";
                    command.ExecuteNonQuery();

                    // Create SubTitles table
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS SubTitles (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            MainTitleId INTEGER NOT NULL,
                            Subtitle TEXT NOT NULL,
                            CreatedDateTime TEXT NOT NULL,
                            UpdatedDateTime TEXT NOT NULL,
                            FOREIGN KEY (MainTitleId) REFERENCES MainTitles(Id) ON DELETE CASCADE,
                            UNIQUE(MainTitleId, Subtitle)
                        );
                        CREATE INDEX IF NOT EXISTS idx_subtitles_maintitleid ON SubTitles(MainTitleId);
                        CREATE INDEX IF NOT EXISTS idx_subtitles_composite ON SubTitles(MainTitleId, Subtitle);
                    ";
                    command.ExecuteNonQuery();

                    // Create StoryVersions table
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS StoryVersions (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            SubTitleId INTEGER NOT NULL,
                            Version INTEGER NOT NULL,
                            Novel TEXT,
                            SystemPrompt TEXT,
                            Prompt TEXT,
                            SelectedModel TEXT,
                            SavedDateTime TEXT NOT NULL,
                            IsAutoSave INTEGER NOT NULL DEFAULT 0,
                            FOREIGN KEY (SubTitleId) REFERENCES SubTitles(Id) ON DELETE CASCADE,
                            UNIQUE(SubTitleId, Version, IsAutoSave)
                        );
                        CREATE INDEX IF NOT EXISTS idx_storyversions_subtitleid ON StoryVersions(SubTitleId);
                        CREATE INDEX IF NOT EXISTS idx_storyversions_version ON StoryVersions(SubTitleId, Version DESC);
                        CREATE INDEX IF NOT EXISTS idx_storyversions_autosave ON StoryVersions(SubTitleId, IsAutoSave, Version DESC);
                    ";
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to initialize database: {ex.Message}",
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        /// <summary>
        /// Gets the ID of an existing MainTitle or creates a new one.
        /// </summary>
        /// <param name="connection">Open SQLite connection</param>
        /// <param name="mainTitle">Main title text</param>
        /// <returns>MainTitle ID</returns>
        private long GetOrCreateMainTitleId(SqliteConnection connection, string mainTitle)
        {
            // Try to get existing MainTitle
            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = @"
                SELECT Id FROM MainTitles WHERE MainTitle = $mainTitle
            ";
            selectCommand.Parameters.AddWithValue("$mainTitle", mainTitle);

            var result = selectCommand.ExecuteScalar();
            if (result != null)
            {
                return (long)result;
            }

            // Create new MainTitle
            var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = @"
                INSERT INTO MainTitles (MainTitle, CreatedDateTime, UpdatedDateTime)
                VALUES ($mainTitle, $dateTime, $dateTime);
                SELECT last_insert_rowid();
            ";
            insertCommand.Parameters.AddWithValue("$mainTitle", mainTitle);
            insertCommand.Parameters.AddWithValue("$dateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            // Return the newly created ID
            return (long)insertCommand.ExecuteScalar();
        }

        /// <summary>
        /// Gets the ID of an existing SubTitle or creates a new one.
        /// </summary>
        /// <param name="connection">Open SQLite connection</param>
        /// <param name="mainTitleId">MainTitle foreign key</param>
        /// <param name="subtitle">Subtitle text</param>
        /// <returns>SubTitle ID</returns>
        private long GetOrCreateSubTitleId(SqliteConnection connection, long mainTitleId, string subtitle)
        {
            // Try to get existing SubTitle
            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = @"
                SELECT Id FROM SubTitles
                WHERE MainTitleId = $mainTitleId AND Subtitle = $subtitle
            ";
            selectCommand.Parameters.AddWithValue("$mainTitleId", mainTitleId);
            selectCommand.Parameters.AddWithValue("$subtitle", subtitle);

            var result = selectCommand.ExecuteScalar();
            if (result != null)
            {
                return (long)result;
            }

            // Create new SubTitle
            var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = @"
                INSERT INTO SubTitles (MainTitleId, Subtitle, CreatedDateTime, UpdatedDateTime)
                VALUES ($mainTitleId, $subtitle, $dateTime, $dateTime);
                SELECT last_insert_rowid();
            ";
            insertCommand.Parameters.AddWithValue("$mainTitleId", mainTitleId);
            insertCommand.Parameters.AddWithValue("$subtitle", subtitle);
            insertCommand.Parameters.AddWithValue("$dateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            // Return the newly created ID
            return (long)insertCommand.ExecuteScalar();
        }

        /// <summary>
        /// Updates the UpdatedDateTime for a MainTitle when a save occurs.
        /// </summary>
        /// <param name="connection">Open SQLite connection</param>
        /// <param name="mainTitleId">MainTitle ID to update</param>
        private void UpdateMainTitleTimestamp(SqliteConnection connection, long mainTitleId)
        {
            var updateCommand = connection.CreateCommand();
            updateCommand.CommandText = @"
                UPDATE MainTitles
                SET UpdatedDateTime = $dateTime
                WHERE Id = $id
            ";
            updateCommand.Parameters.AddWithValue("$dateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            updateCommand.Parameters.AddWithValue("$id", mainTitleId);
            updateCommand.ExecuteNonQuery();
        }

        /// <summary>
        /// Updates the UpdatedDateTime for a SubTitle when a save occurs.
        /// </summary>
        /// <param name="connection">Open SQLite connection</param>
        /// <param name="subTitleId">SubTitle ID to update</param>
        private void UpdateSubTitleTimestamp(SqliteConnection connection, long subTitleId)
        {
            var updateCommand = connection.CreateCommand();
            updateCommand.CommandText = @"
                UPDATE SubTitles
                SET UpdatedDateTime = $dateTime
                WHERE Id = $id
            ";
            updateCommand.Parameters.AddWithValue("$dateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            updateCommand.Parameters.AddWithValue("$id", subTitleId);
            updateCommand.ExecuteNonQuery();
        }

        #endregion
        #region Load Available Models
        /// <summary>Load from the server all the aviable models and populate the combobox.</summary>
        private async void LoadAvailableModels()
        {
            try
            {
                // Disable the combobox while loading
                modelComboBox.Enabled = false;
                modelComboBox.Items.Clear();
                modelComboBox.Items.Add("Loading models...");
                modelComboBox.SelectedIndex = 0;

                // Fetch available models from Ollama
                HttpResponseMessage response = await httpClient.GetAsync(OLLAMA_TAGS_URL);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();

                using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
                {
                    JsonElement root = doc.RootElement;
                    if (root.TryGetProperty("models", out JsonElement modelsElement))
                    {
                        modelComboBox.Items.Clear();
                        int autoSelectIndex = -1;
                        foreach (JsonElement modelElement in modelsElement.EnumerateArray())
                        {
                            if (modelElement.TryGetProperty("name", out JsonElement nameElement))
                            {
                                string modelName = nameElement.GetString();
                                if (!string.IsNullOrEmpty(modelName))
                                {
                                    modelComboBox.Items.Add(modelName);
                                    //Check if this model was loaded from auto-save
                                    if (autoSaveLoadedModel.Length > 0 && modelName == autoSaveLoadedModel)
                                        autoSelectIndex = modelComboBox.Items.Count - 1;
                                }
                            }
                        }

                        // Set default model if any are available
                        if (modelComboBox.Items.Count > 0)
                            modelComboBox.SelectedIndex = autoSelectIndex >= 0 ? autoSelectIndex : 0;
                        else
                        {
                            modelComboBox.Items.Add("No models found");
                            modelComboBox.SelectedIndex = 0;
                        }
                    }
                }

                modelComboBox.Enabled = true;
            }
            catch (Exception ex)
            {
                modelComboBox.Items.Clear();
                modelComboBox.Items.Add("Error loading models");
                modelComboBox.SelectedIndex = 0;
                modelComboBox.Enabled = true;

                MessageBox.Show(
                    $"Failed to load models from Ollama:\n{ex.Message}\n\nMake sure Ollama is running.",
                    "Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }
        #endregion

        #region Generate the next Paragraph(s)

        private async void GenerateButton_Click(object sender, EventArgs e)
        {
            int numberOfParagraph = 1;
            if (sender == generateTwoParagraphButton)
                numberOfParagraph = 2;
            if (sender == generateThreeParagraphButton)
                numberOfParagraph = 3;


            // Validate model selection
            if (modelComboBox.SelectedItem == null)
            {
                MessageBox.Show(
                    "Please select a model from the dropdown.",
                    "No Model Selected",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            // Clear previous options
            option1TextBox.Clear();
            option2TextBox.Clear();
            option3TextBox.Clear();
            generationTimeLabel.Text = "";

            // Disable all generate buttons during generation
            generateOneParagraphButton.Enabled = false;
            generateTwoParagraphButton.Enabled = false;
            generateThreeParagraphButton.Enabled = false;

            // Disable all "use this option" buttons during generation
            useOption1Button.Enabled = false;
            useOption2Button.Enabled = false;
            useOption3Button.Enabled = false;

            // Update the clicked button text
            if (sender == generateOneParagraphButton)
                generateOneParagraphButton.Text = "Generating...";
            else if (sender == generateTwoParagraphButton)
                generateTwoParagraphButton.Text = "Generating...";
            else if (sender == generateThreeParagraphButton)
                generateThreeParagraphButton.Text = "Generating...";

            // Start timing
            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                // Check if session needs reset
                string selectedModel = modelComboBox.SelectedItem?.ToString() ?? "llama3.2:latest";
                bool modelChanged = !string.IsNullOrEmpty(currentModelName) && currentModelName != selectedModel;
                bool novelEdited = sessionManager.IsNovelEdited(novelTextBox.Text);

                // Reset session if needed
                if (modelChanged || novelEdited || sessionManager.IsEmpty())
                {
                    sessionManager.ResetSession();
                    string context = ProcessNovelContextForSummary(novelTextBox.Text);
                    sessionManager.InitializeSession(context, systemPromptTextBox.Text);
                    lastNovelText = novelTextBox.Text;
                    currentModelName = selectedModel;
                }

                // Get chat messages from session manager
                var messages = sessionManager.GetChatMessages(
                    novelTextBox.Text,
                    promptTextBox.Text,
                    numberOfParagraph
                );

                // Store the user message for later (before it's sent)
                var lastUserMessage = messages[messages.Count - 1].content;

                // Generate three different variations with temperature variation
                var task1 = GenerateParagraphChat(messages, 0.7);
                var task2 = GenerateParagraphChat(messages, 0.85);
                var task3 = GenerateParagraphChat(messages, 1.0);
                generationTimeLabel.Text = $"{DateTime.Now.ToLongTimeString()} Generating...";
                await Task.WhenAll(task1, task2, task3);

                var result1 = await task1;
                var result2 = await task2;
                var result3 = await task3;

                // Update session with the user message that was sent
                sessionManager.UpdateAfterUserMessage(lastUserMessage);

                option1TextBox.Text = result1.Text;
                option2TextBox.Text = result2.Text;
                option3TextBox.Text = result3.Text;

                // Calculate total token usage from all three generations
                int totalPromptTokens = result1.PromptTokens + result2.PromptTokens + result3.PromptTokens;
                int totalResponseTokens = result1.ResponseTokens + result2.ResponseTokens + result3.ResponseTokens;
                int totalTokens = totalPromptTokens + totalResponseTokens;

                // Get estimated tokens saved
                int tokensSaved = sessionManager.GetEstimatedTokensSaved();

                // Stop timing and display the result with token usage
                stopwatch.Stop();
                double totalSeconds = stopwatch.Elapsed.TotalSeconds;
                string statsMessage = $"Generation completed in {totalSeconds:F2} seconds ({stopwatch.Elapsed.TotalMilliseconds:F0} ms) | " +
                                     $"Tokens: {totalTokens:N0} (Prompt: {totalPromptTokens:N0}/3, Response: {totalResponseTokens:N0}/3)";

                if (tokensSaved > 0)
                {
                    statsMessage += $" | Saved ~{tokensSaved:N0} tokens/request";
                }

                statsMessage += $" | Session: {sessionManager.MessageCount} messages";

                generationTimeLabel.Text = statsMessage;

                // Update session status label
                sessionStatusLabel.Text = $"Session: Active ({sessionManager.MessageCount} messages)";
                sessionStatusLabel.ForeColor = System.Drawing.Color.DarkGreen;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                string selectedModel = modelComboBox.SelectedItem?.ToString() ?? "unknown";
                MessageBox.Show(
                    $"Error generating text: {ex.Message}\n\nMake sure Ollama is running and the model '{selectedModel}' is installed.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                generationTimeLabel.Text = "Generation failed";
            }
            finally
            {
                // Re-enable all generate buttons
                generateOneParagraphButton.Enabled = true;
                generateTwoParagraphButton.Enabled = true;
                generateThreeParagraphButton.Enabled = true;

                // Restore original button text
                generateOneParagraphButton.Text     = "Generate Next Paragraph";
                generateTwoParagraphButton.Text     = "Generate Next 2 Paragraphs";
                generateThreeParagraphButton.Text   = "Generate Next 3 Paragraphs";

                // Re-enable all "use this option" buttons
                useOption1Button.Enabled = true;
                useOption2Button.Enabled = true;
                useOption3Button.Enabled = true;
            }
        }

        private string ProcessNovelContextForSummary(string novelContext)
        {
            if (string.IsNullOrEmpty(novelContext))
                return novelContext;

            string[] lines = novelContext.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            // Find the last line that starts with 'SUMMARY'
            int lastSummaryIndex = -1;
            for (int i = lines.Length - 1; i >= 0; i--)
            {
                if (lines[i].StartsWith("SUMMARY", StringComparison.OrdinalIgnoreCase))
                {
                    lastSummaryIndex = i;
                    break;
                }
            }

            // If no SUMMARY line found, return original context
            if (lastSummaryIndex == -1)
                return novelContext;

            // Process the lines
            StringBuilder result = new StringBuilder();

            // Process all lines up to and including the last SUMMARY line
            int SUMMARY_PREFIX_LENGTH = "SUMMARY^1:".Length;
            for (int i = 0; i <= lastSummaryIndex; i++)
            {
                if (lines[i].StartsWith("SUMMARY", StringComparison.OrdinalIgnoreCase))
                {
                    // Strip the first 10 characters from SUMMARY lines
                    string processedLine = lines[i].Length > 10 ? lines[i].Substring(SUMMARY_PREFIX_LENGTH) : "";
                    if (!string.IsNullOrEmpty(processedLine) || i < lastSummaryIndex)
                    {
                        result.AppendLine(processedLine);
                    }
                }
                // Skip non-SUMMARY lines before the last SUMMARY line
            }

            // Add all lines after the last SUMMARY line as-is
            for (int i = lastSummaryIndex + 1; i < lines.Length; i++)
            {
                result.AppendLine(lines[i]);
            }

            return result.ToString().TrimEnd();
        }

        private string BuildPrompt(int numberOfParagraph)
        {
            string novelContext = novelTextBox.Text.Trim();
            string userInstructions = promptTextBox.Text.Trim();
            string systemPrompt = systemPromptTextBox.Text.Trim();

            // Process novel context: filter SUMMARY lines
            novelContext = ProcessNovelContextForSummary(novelContext);

            StringBuilder prompt = new StringBuilder();

            // Add system prompt if provided (constant instructions)
            if (!string.IsNullOrEmpty(systemPrompt))
            {
                prompt.AppendLine(systemPrompt);
                prompt.AppendLine();
            }
            else
            {
                // Default system prompt if none provided
                prompt.AppendLine("You are a creative fiction writer helping to continue a novel.");
                prompt.AppendLine();
            }

            if (!string.IsNullOrEmpty(novelContext))
            {
                prompt.AppendLine("Here is the story so far:");
                prompt.AppendLine(novelContext);
                prompt.AppendLine();
            }

            if (!string.IsNullOrEmpty(userInstructions))
            {
                if (numberOfParagraph == 1)
                    prompt.AppendLine("Instructions for the next paragraph:");
                else
                    prompt.AppendLine($"Instructions for the next {numberOfParagraph} paragraphs:");
                prompt.AppendLine(userInstructions);
                prompt.AppendLine();
            }

            if (numberOfParagraph == 1)
                prompt.AppendLine("Write the next paragraph of the story. Be creative and engaging. Write only the paragraph itself, nothing else.");
            else
                prompt.AppendLine($"Write the next {numberOfParagraph} paragraphs of the story. Each paragraph MUST be separated by an empty line. Be creative and engaging. Write only the paragraphs itself, nothing else.");

            return prompt.ToString();
        }

        private async Task<GenerationResponse> GenerateParagraph(string prompt)
        {
            string selectedModel = modelComboBox.SelectedItem?.ToString() ?? "llama3.2:latest";

            var requestBody = new
            {
                model = selectedModel,
                prompt = prompt,
                stream = false,
                options = new
                {
                    temperature = 0.8,
                    top_p = 0.9,
                    num_predict = 200
                }
            };

            string jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync(OLLAMA_URL, content);
            response.EnsureSuccessStatusCode();

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var result = new GenerationResponse();

            using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
            {
                JsonElement root = doc.RootElement;

                // Extract response text
                if (root.TryGetProperty("response", out JsonElement responseElement))
                {
                    result.Text = responseElement.GetString()?.Trim() ?? "No response generated";
                }
                else
                {
                    result.Text = "Error parsing response";
                }

                // Extract token counts
                if (root.TryGetProperty("prompt_eval_count", out JsonElement promptTokensElement))
                {
                    result.PromptTokens = promptTokensElement.GetInt32();
                }

                if (root.TryGetProperty("eval_count", out JsonElement responseTokensElement))
                {
                    result.ResponseTokens = responseTokensElement.GetInt32();
                }
            }

            return result;
        }

        private async Task<GenerationResponse> GenerateParagraphChat(
            System.Collections.Generic.List<OllamaSessionManager.ChatMessage> messages,
            double temperature)
        {
            string selectedModel = modelComboBox.SelectedItem?.ToString() ?? "llama3.2:latest";

            var requestBody = new OllamaSessionManager.ChatRequest
            {
                model = selectedModel,
                messages = messages,
                stream = false,
                options = new
                {
                    temperature = temperature,
                    top_p = 0.9,
                    num_predict = 200
                },
                keep_alive = "10m" // Keep model loaded in memory for 10 minutes
            };

            string jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync(OLLAMA_CHAT_URL, content);
            response.EnsureSuccessStatusCode();

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var result = new GenerationResponse();

            using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
            {
                JsonElement root = doc.RootElement;

                // Extract response text from message.content
                if (root.TryGetProperty("message", out JsonElement messageElement))
                {
                    if (messageElement.TryGetProperty("content", out JsonElement contentElement))
                    {
                        result.Text = contentElement.GetString()?.Trim() ?? "No response generated";
                    }
                    else
                    {
                        result.Text = "Error: No content in message";
                    }
                }
                else
                {
                    result.Text = "Error parsing response";
                }

                // Extract token counts
                if (root.TryGetProperty("prompt_eval_count", out JsonElement promptTokensElement))
                {
                    result.PromptTokens = promptTokensElement.GetInt32();
                }

                if (root.TryGetProperty("eval_count", out JsonElement responseTokensElement))
                {
                    result.ResponseTokens = responseTokensElement.GetInt32();
                }
            }

            return result;
        }
        #endregion


        #region Save and Load
        private void SaveButton_Click(object sender, EventArgs e)
        {
            // Validate MainTitle and Subtitle
            string mainTitle = mainTitleTextBox.Text.Trim();
            string subtitle = subtitleTextBox.Text.Trim();

            if (string.IsNullOrEmpty(mainTitle) || string.IsNullOrEmpty(subtitle))
            {
                MessageBox.Show(
                    "Please enter both Main Title and Subtitle before saving.",
                    "Missing Information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Novel Project (*.json)|*.json|All files (*.*)|*.*";
                saveDialog.DefaultExt = "json";
                saveDialog.Title = "Save Novel Project";
                saveDialog.FileName = $"{mainTitle}_{subtitle}.json";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Save to JSON file
                        var projectData = new
                        {
                            MainTitle = mainTitle,
                            Subtitle = subtitle,
                            Novel = novelTextBox.Text,
                            Prompt = promptTextBox.Text,
                            SystemPrompt = systemPromptTextBox.Text,
                            SelectedModel = modelComboBox.SelectedItem?.ToString() ?? ""
                        };

                        string json = JsonSerializer.Serialize(projectData, new JsonSerializerOptions { WriteIndented = true });
                        System.IO.File.WriteAllText(saveDialog.FileName, json);

                        // Save to SQLite database
                        long newVersion = 0;
                        using (var connection = new SqliteConnection(connectionString))
                        {
                            connection.Open();

                            // Enable foreign key support
                            var pragmaCommand = connection.CreateCommand();
                            pragmaCommand.CommandText = "PRAGMA foreign_keys = ON";
                            pragmaCommand.ExecuteNonQuery();

                            using (var transaction = connection.BeginTransaction())
                            {
                                try
                                {
                                    // Get or create MainTitle
                                    long mainTitleId = GetOrCreateMainTitleId(connection, mainTitle);

                                    // Get or create SubTitle
                                    long subTitleId = GetOrCreateSubTitleId(connection, mainTitleId, subtitle);

                                    // Get the maximum version for this SubTitle (only manual saves, IsAutoSave = 0)
                                    var queryCommand = connection.CreateCommand();
                                    queryCommand.CommandText = @"
                                        SELECT COALESCE(MAX(Version), 0)
                                        FROM StoryVersions
                                        WHERE SubTitleId = $subTitleId AND IsAutoSave = 0
                                    ";
                                    queryCommand.Parameters.AddWithValue("$subTitleId", subTitleId);

                                    long maxVersion = (long)queryCommand.ExecuteScalar();
                                    newVersion = maxVersion + 1;

                                    // Insert the new version record
                                    var insertCommand = connection.CreateCommand();
                                    insertCommand.CommandText = @"
                                        INSERT INTO StoryVersions
                                        (SubTitleId, Version, Novel, SystemPrompt, Prompt, SelectedModel, SavedDateTime, IsAutoSave)
                                        VALUES ($subTitleId, $version, $novel, $systemPrompt, $prompt, $selectedModel, $savedDateTime, $isAutoSave)
                                    ";
                                    insertCommand.Parameters.AddWithValue("$subTitleId", subTitleId);
                                    insertCommand.Parameters.AddWithValue("$version", newVersion);
                                    insertCommand.Parameters.AddWithValue("$novel", novelTextBox.Text);
                                    insertCommand.Parameters.AddWithValue("$systemPrompt", systemPromptTextBox.Text);
                                    insertCommand.Parameters.AddWithValue("$prompt", promptTextBox.Text);
                                    insertCommand.Parameters.AddWithValue("$selectedModel", modelComboBox.SelectedItem?.ToString() ?? "");
                                    insertCommand.Parameters.AddWithValue("$savedDateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                    insertCommand.Parameters.AddWithValue("$isAutoSave", 0);

                                    insertCommand.ExecuteNonQuery();

                                    // Update timestamps
                                    UpdateMainTitleTimestamp(connection, mainTitleId);
                                    UpdateSubTitleTimestamp(connection, subTitleId);

                                    transaction.Commit();
                                }
                                catch
                                {
                                    transaction.Rollback();
                                    throw;
                                }
                            }
                        }
                        novelWasChangedFromLastSave = false;

                        MessageBox.Show(
                            $"Project saved successfully!\nMain Title: {mainTitle}\nSubtitle: {subtitle}\nVersion: {newVersion}",
                            "Save Successful",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Error saving project: {ex.Message}",
                            "Save Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }
                }
            }
        }

        private void LoadFromJsonButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openDialog = new OpenFileDialog())
            {
                openDialog.Filter = "Novel Project (*.json)|*.json|All files (*.*)|*.*";
                openDialog.Title = "Load Novel Project";

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string json = System.IO.File.ReadAllText(openDialog.FileName);
                        using (JsonDocument doc = JsonDocument.Parse(json))
                        {
                            JsonElement root = doc.RootElement;

                            // Load main title
                            if (root.TryGetProperty("MainTitle", out JsonElement mainTitleElement))
                            {
                                mainTitleTextBox.Text = mainTitleElement.GetString() ?? "";
                            }

                            // Load subtitle
                            if (root.TryGetProperty("Subtitle", out JsonElement subtitleElement))
                            {
                                subtitleTextBox.Text = subtitleElement.GetString() ?? "";
                            }

                            // Load novel text
                            if (root.TryGetProperty("Novel", out JsonElement novelElement))
                            {
                                novelTextBox.Text = novelElement.GetString() ?? "";
                                // Scroll to the end of the text
                                novelTextBox.SelectionStart = novelTextBox.Text.Length;
                                novelTextBox.SelectionLength = 0;
                                novelTextBox.ScrollToCaret();
                            }
                            else
                            {
                                // Load novel text
                                if (root.TryGetProperty("storySoFar", out JsonElement storySoFarElement))
                                {
                                    string text = storySoFarElement.GetString() ?? "";
                                    text = text.Replace("\n\n","\r\n\r\n");
                                    novelTextBox.Text = text;
                                    // Scroll to the end of the text
                                    novelTextBox.SelectionStart = novelTextBox.Text.Length;
                                    novelTextBox.SelectionLength = 0;
                                    novelTextBox.ScrollToCaret();
                                }
                            }

                            // Load prompt text
                            if (root.TryGetProperty("Prompt", out JsonElement promptElement))
                            {
                                promptTextBox.Text = promptElement.GetString() ?? "";
                            }
                            else
                            {
                                if (root.TryGetProperty("whatHappensNext", out JsonElement nextElement))
                                {
                                    string nextText = nextElement.GetString() ?? string.Empty;
                                    if (string.IsNullOrEmpty(nextText) == false)
                                    {
                                        int instructionsIndex = nextText.IndexOf("<instructions>", StringComparison.OrdinalIgnoreCase);
                                        if (instructionsIndex >= 0)
                                        {                                             
                                            promptTextBox.Text = nextText.Substring(0,instructionsIndex).Trim();
                                            systemPromptTextBox.Text = nextText.Substring(instructionsIndex).Trim();
                                        }
                                        else
                                            promptTextBox.Text = nextText;
                                    }
                                }
                            }

                            // Load system prompt
                            if (root.TryGetProperty("SystemPrompt", out JsonElement systemPromptElement))
                            {
                                systemPromptTextBox.Text = systemPromptElement.GetString() ?? "";
                            }

                            // Load selected model
                            if (root.TryGetProperty("SelectedModel", out JsonElement modelElement))
                            {
                                string modelName = modelElement.GetString();
                                if (!string.IsNullOrEmpty(modelName))
                                {
                                    // Try to find and select the model in the combobox
                                    bool modelFound = false;
                                    for (int i = 0; i < modelComboBox.Items.Count; i++)
                                    {
                                        if (modelComboBox.Items[i].ToString() == modelName)
                                        {
                                            modelComboBox.SelectedIndex = i;
                                            modelFound = true;
                                            break;
                                        }
                                    }

                                    // If model not found, select first available
                                    if (!modelFound && modelComboBox.Items.Count > 0)
                                    {
                                        modelComboBox.SelectedIndex = 0;
                                    }
                                }
                            }
                        }

                        MessageBox.Show(
                            "Project loaded successfully!",
                            "Load Successful",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Error loading project: {ex.Message}",
                            "Load Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }
                }
            }
        }

        private void LoadAutoSaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (System.IO.File.Exists(dbFilePath))
                {
                    LoadFromDatabase();
                    MessageBox.Show(
                        "Auto-save loaded successfully!",
                        "Load Auto-Save",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                else
                {
                    MessageBox.Show(
                        "No auto-save database found.",
                        "Load Auto-Save",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading auto-save: {ex.Message}",
                    "Load Auto-Save Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void AutoSaveToDatabase()
        {
            if (mainTitleTextBox.TextLength == 0 && subtitleTextBox.TextLength == 0 && novelTextBox.TextLength == 0
                && systemPromptTextBox.TextLength == 0 && promptTextBox.TextLength == 0)
            {
                // Nothing to save
                return;
            }

            try
            {
                // Get MainTitle and Subtitle from textboxes
                string mainTitle = mainTitleTextBox.Text.Trim();
                string subtitle = subtitleTextBox.Text.Trim();

                // If MainTitle or Subtitle is empty, use "autosave" as default
                if (string.IsNullOrEmpty(mainTitle))
                    mainTitle = autoSaveTitle;
                if (string.IsNullOrEmpty(subtitle))
                    subtitle = autoSaveTitle;

                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    // Enable foreign key support
                    var pragmaCommand = connection.CreateCommand();
                    pragmaCommand.CommandText = "PRAGMA foreign_keys = ON";
                    pragmaCommand.ExecuteNonQuery();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Get or create MainTitle
                            long mainTitleId = GetOrCreateMainTitleId(connection, mainTitle);

                            // Get or create SubTitle
                            long subTitleId = GetOrCreateSubTitleId(connection, mainTitleId, subtitle);

                            // Get the last Version number where IsAutoSave = 0 for this SubTitle
                            var queryCommand = connection.CreateCommand();
                            queryCommand.CommandText = @"
                                SELECT COALESCE(MAX(Version), 0)
                                FROM StoryVersions
                                WHERE SubTitleId = $subTitleId AND IsAutoSave = 0
                            ";
                            queryCommand.Parameters.AddWithValue("$subTitleId", subTitleId);

                            long lastManualVersion = (long)queryCommand.ExecuteScalar();

                            // If no manual save exists yet, use version 0
                            long versionToUse = lastManualVersion > 0 ? lastManualVersion : 0;

                            // Insert the new auto-save record
                            var insertCommand = connection.CreateCommand();
                            insertCommand.CommandText = @"
                                INSERT INTO StoryVersions
                                (SubTitleId, Version, Novel, SystemPrompt, Prompt, SelectedModel, SavedDateTime, IsAutoSave)
                                VALUES ($subTitleId, $version, $novel, $systemPrompt, $prompt, $selectedModel, $savedDateTime, $isAutoSave)
                            ";
                            insertCommand.Parameters.AddWithValue("$subTitleId", subTitleId);
                            insertCommand.Parameters.AddWithValue("$version", versionToUse);
                            insertCommand.Parameters.AddWithValue("$novel", novelTextBox.Text);
                            insertCommand.Parameters.AddWithValue("$systemPrompt", systemPromptTextBox.Text);
                            insertCommand.Parameters.AddWithValue("$prompt", promptTextBox.Text);
                            insertCommand.Parameters.AddWithValue("$selectedModel", modelComboBox.SelectedItem?.ToString() ?? "");
                            insertCommand.Parameters.AddWithValue("$savedDateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            insertCommand.Parameters.AddWithValue("$isAutoSave", 1);

                            insertCommand.ExecuteNonQuery();

                            // Update timestamps
                            UpdateMainTitleTimestamp(connection, mainTitleId);
                            UpdateSubTitleTimestamp(connection, subTitleId);

                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Silently fail for auto-save - don't interrupt user workflow
            }
        }

        private void LoadFromDatabase()
        {
            try
            {
                if (System.IO.File.Exists(dbFilePath))
                {
                    using (var connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();

                        // Enable foreign key support
                        var pragmaCommand = connection.CreateCommand();
                        pragmaCommand.CommandText = "PRAGMA foreign_keys = ON";
                        pragmaCommand.ExecuteNonQuery();

                        var command = connection.CreateCommand();
                        command.CommandText = @"
                            SELECT
                                mt.MainTitle,
                                st.Subtitle,
                                sv.Novel,
                                sv.SystemPrompt,
                                sv.Prompt,
                                sv.SelectedModel
                            FROM StoryVersions sv
                            INNER JOIN SubTitles st ON sv.SubTitleId = st.Id
                            INNER JOIN MainTitles mt ON st.MainTitleId = mt.Id
                            ORDER BY sv.Id DESC
                            LIMIT 1
                        ";

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Temporarily unsubscribe from event
                                novelTextBox.TextChanged -= NovelTextBox_TextChanged;

                                // Load main title and subtitle
                                mainTitleTextBox.Text = reader.GetString(0);
                                //DO NOT write autosave as title
                                if (mainTitleTextBox.Text == autoSaveTitle)
                                    mainTitleTextBox.Text = "";
                                subtitleTextBox.Text = reader.GetString(1);
                                if (subtitleTextBox.Text == autoSaveTitle)
                                    subtitleTextBox.Text = "";

                                // Load novel text
                                novelTextBox.Text = reader.GetString(2);
                                // Scroll to the end of the text
                                novelTextBox.SelectionStart = novelTextBox.Text.Length;
                                novelTextBox.SelectionLength = 0;
                                novelTextBox.ScrollToCaret();
                                // Mark as not changed
                                novelWasChangedFromLastSave = false;

                                // Load system prompt
                                systemPromptTextBox.Text = reader.GetString(3);

                                // Load prompt
                                promptTextBox.Text = reader.GetString(4);

                                // Load selected model
                                autoSaveLoadedModel = reader.GetString(5);
                                if (!string.IsNullOrEmpty(autoSaveLoadedModel))
                                {
                                    // Try to find and select the model in the combobox
                                    bool modelFound = false;
                                    for (int i = 0; i < modelComboBox.Items.Count; i++)
                                    {
                                        if (modelComboBox.Items[i].ToString() == autoSaveLoadedModel)
                                        {
                                            modelComboBox.SelectedIndex = i;
                                            modelFound = true;
                                            break;
                                        }
                                    }

                                    // If model not found, select first available
                                    if (!modelFound && modelComboBox.Items.Count > 0)
                                    {
                                        modelComboBox.SelectedIndex = 0;
                                    }
                                }

                                // Resubscribe to event
                                novelTextBox.TextChanged += NovelTextBox_TextChanged;

                                // Reset session when loading a novel
                                sessionManager.ResetSession();
                                lastNovelText = "";
                                currentModelName = autoSaveLoadedModel;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Silently fail - database is optional
            }
        }

        private void LoadDBButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!System.IO.File.Exists(dbFilePath))
                {
                    MessageBox.Show(
                        "No database file found.",
                        "Load from Database",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    return;
                }

                using (var loadForm = new LoadDatabaseForm(connectionString))
                {
                    if (loadForm.ShowDialog() == DialogResult.OK && loadForm.SelectedStoryVersionId.HasValue)
                    {
                        LoadSpecificVersion(loadForm.SelectedStoryVersionId.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error opening load database form: {ex.Message}",
                    "Load Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void LoadSpecificVersion(long storyVersionId)
        {
            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    // Enable foreign key support
                    var pragmaCommand = connection.CreateCommand();
                    pragmaCommand.CommandText = "PRAGMA foreign_keys = ON";
                    pragmaCommand.ExecuteNonQuery();

                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        SELECT
                            mt.MainTitle,
                            st.Subtitle,
                            sv.Novel,
                            sv.SystemPrompt,
                            sv.Prompt,
                            sv.SelectedModel,
                            sv.Version,
                            sv.IsAutoSave
                        FROM StoryVersions sv
                        INNER JOIN SubTitles st ON sv.SubTitleId = st.Id
                        INNER JOIN MainTitles mt ON st.MainTitleId = mt.Id
                        WHERE sv.Id = $id
                    ";
                    command.Parameters.AddWithValue("$id", storyVersionId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Temporarily unsubscribe from event
                            novelTextBox.TextChanged -= NovelTextBox_TextChanged;

                            // Load main title and subtitle
                            mainTitleTextBox.Text = reader.GetString(0);
                            // DO NOT write autosave as title
                            if (mainTitleTextBox.Text == autoSaveTitle)
                                mainTitleTextBox.Text = "";
                            subtitleTextBox.Text = reader.GetString(1);
                            if (subtitleTextBox.Text == autoSaveTitle)
                                subtitleTextBox.Text = "";

                            // Load novel text
                            novelTextBox.Text = reader.IsDBNull(2) ? "" : reader.GetString(2);
                            // Scroll to the end of the text
                            novelTextBox.SelectionStart = novelTextBox.Text.Length;
                            novelTextBox.SelectionLength = 0;
                            novelTextBox.ScrollToCaret();
                            // Mark as not changed
                            novelWasChangedFromLastSave = false;

                            // Load system prompt
                            systemPromptTextBox.Text = reader.IsDBNull(3) ? "" : reader.GetString(3);

                            // Load prompt
                            promptTextBox.Text = reader.IsDBNull(4) ? "" : reader.GetString(4);

                            // Load selected model
                            autoSaveLoadedModel = reader.IsDBNull(5) ? "" : reader.GetString(5);
                            if (!string.IsNullOrEmpty(autoSaveLoadedModel))
                            {
                                // Try to find and select the model in the combobox
                                bool modelFound = false;
                                for (int i = 0; i < modelComboBox.Items.Count; i++)
                                {
                                    if (modelComboBox.Items[i].ToString() == autoSaveLoadedModel)
                                    {
                                        modelComboBox.SelectedIndex = i;
                                        modelFound = true;
                                        break;
                                    }
                                }

                                // If model not found, select first available
                                if (!modelFound && modelComboBox.Items.Count > 0)
                                {
                                    modelComboBox.SelectedIndex = 0;
                                }
                            }

                            // Resubscribe to event
                            novelTextBox.TextChanged += NovelTextBox_TextChanged;

                            int version = reader.GetInt32(6);
                            int isAutoSave = reader.GetInt32(7);
                            string autoSaveText = isAutoSave == 1 ? " (AutoSave)" : "";

                            MessageBox.Show(
                                $"Loaded version {version}{autoSaveText} successfully!",
                                "Load Successful",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information
                            );
                        }
                        else
                        {
                            MessageBox.Show(
                                "Story version not found.",
                                "Load Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading story version: {ex.Message}",
                    "Load Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        #endregion
        #region Miscellaneous UI Events

        private void NovelTextBox_TextChanged(object sender, EventArgs e)
        {
            novelWasChangedFromLastSave = false;
        }
        /// <summary>If i selected this Option i want to add it to the novel</summary>
        /// <param name="sender">The button that was pressed</param>
        /// <param name="e"></param>
        private void UseThisOption_AddToNovel(object sender, EventArgs e)
        {
            if (sender == null)
                return;
            string text = sender switch
            {
                Button btn when btn == useOption1Button => option1TextBox.Text,
                Button btn when btn == useOption2Button => option2TextBox.Text,
                Button btn when btn == useOption3Button => option3TextBox.Text,
                _ => ""
            };
            //If there is no text, do nothing
            if (string.IsNullOrEmpty(text))
                return;
            AppendOptionToNovel(text);

            // Update session with the selected assistant response
            sessionManager.AddAssistantResponse(text);

            // Update novel context tracking
            sessionManager.UpdateNovelContext(novelTextBox.Text);
            lastNovelText = novelTextBox.Text;

            AutoSaveToDatabase();
        }

        private void AppendOptionToNovel(string optionText)
        {
            if (string.IsNullOrWhiteSpace(optionText))
            {
                return;
            }

            // Check if this content is already in the last 3 paragraphs
            if (!string.IsNullOrWhiteSpace(novelTextBox.Text))
            {
                if (novelTextBox.Text.Contains(optionText.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show(
                        "This content is already present in the last few paragraphs of your novel. It was not added again.",
                        "Duplicate Content",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    return;
                }
            }

            // Add a newline if there's already content
            if (!string.IsNullOrWhiteSpace(novelTextBox.Text))
            {
                novelTextBox.Text += Environment.NewLine + Environment.NewLine;
            }

            // Append the selected option
            novelTextBox.Text += optionText;

            // Scroll to the end
            novelTextBox.SelectionStart = novelTextBox.Text.Length;
            novelTextBox.ScrollToCaret();
            NovelTextBox_TextChanged(this, EventArgs.Empty);
        }

        private void ResetSessionButton_Click(object sender, EventArgs e)
        {
            // Reset the session manager
            sessionManager.ResetSession();
            lastNovelText = "";

            // Update session status label
            sessionStatusLabel.Text = "Session: Reset - will reinitialize on next generation";
            sessionStatusLabel.ForeColor = System.Drawing.Color.DarkOrange;

            MessageBox.Show(
                "Session reset successfully. The next generation will send the full novel context to start a fresh session.",
                "Session Reset",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void ClearAllButton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Are you sure you want to clear all textboxes? This cannot be undone.",
                "Clear All Textboxes",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                // Temporarily unsubscribe from event to avoid auto-save during clear
                novelTextBox.TextChanged -= NovelTextBox_TextChanged;

                // Clear all textboxes
                novelTextBox.Clear();
                promptTextBox.Clear();
                systemPromptTextBox.Clear();
                option1TextBox.Clear();
                option2TextBox.Clear();
                option3TextBox.Clear();
                generationTimeLabel.Text = "";

                // Resubscribe to event
                novelTextBox.TextChanged += NovelTextBox_TextChanged;

                // Reset session when clearing all
                sessionManager.ResetSession();
                lastNovelText = "";
                sessionStatusLabel.Text = "Session: Not started";
                sessionStatusLabel.ForeColor = System.Drawing.Color.Gray;

                // Save the cleared state
                AutoSaveToDatabase();
            }
        }

        private void OnClosing_AutoSave(object sender, FormClosingEventArgs e)
        {
            //Save on exit to avoid data loss
            AutoSaveToDatabase();
        }
        #endregion
    }


}
