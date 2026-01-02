using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace NovelWriterAssistant
{
    public partial class LoadDatabaseForm : Form
    {
        private readonly string connectionString;

        public class StoryVersion
        {
            public long Id { get; set; }
            public int Version { get; set; }
            public string SavedDateTime { get; set; } = null!;
            public string LastParagraph { get; set; } = null!;
            public int IsAutoSave { get; set; }

            public override string ToString()
            {
                string autoSaveMarker = IsAutoSave == 1 ? " [AutoSave]" : "";
                return $"v{Version} - {SavedDateTime}{autoSaveMarker} - {LastParagraph}";
            }
        }

        public class SubTitleItem
        {
            public long Id { get; set; }
            public string Subtitle { get; set; } = null!;

            public override string ToString()
            {
                return Subtitle;
            }
        }

        public class MainTitleItem
        {
            public long Id { get; set; }
            public string MainTitle { get; set; } = null!;

            public override string ToString()
            {
                return MainTitle;
            }
        }

        public long? SelectedStoryVersionId { get; private set; }

        public LoadDatabaseForm(string connectionString)
        {
            this.connectionString = connectionString;
            InitializeComponent();
            LoadMainTitles();
        }

        private void LoadMainTitles()
        {
            mainTitleListBox.Items.Clear();
            subTitleListBox.Items.Clear();
            versionListBox.Items.Clear();

            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        SELECT Id, MainTitle
                        FROM MainTitles
                        ORDER BY UpdatedDateTime DESC
                    ";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new MainTitleItem
                            {
                                Id = reader.GetInt64(0),
                                MainTitle = reader.GetString(1)
                            };
                            mainTitleListBox.Items.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading main titles: {ex.Message}",
                    "Load Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void LoadSubTitles(long mainTitleId)
        {
            subTitleListBox.Items.Clear();
            versionListBox.Items.Clear();

            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        SELECT Id, Subtitle
                        FROM SubTitles
                        WHERE MainTitleId = $mainTitleId
                        ORDER BY UpdatedDateTime DESC
                    ";
                    command.Parameters.AddWithValue("$mainTitleId", mainTitleId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new SubTitleItem
                            {
                                Id = reader.GetInt64(0),
                                Subtitle = reader.GetString(1)
                            };
                            subTitleListBox.Items.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading subtitles: {ex.Message}",
                    "Load Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void LoadVersions(long subTitleId)
        {
            versionListBox.Items.Clear();

            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        SELECT Id, Version, SavedDateTime, Novel, IsAutoSave
                        FROM StoryVersions
                        WHERE SubTitleId = $subTitleId
                        ORDER BY Version DESC, IsAutoSave ASC
                    ";
                    command.Parameters.AddWithValue("$subTitleId", subTitleId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string novel = reader.IsDBNull(3) ? "" : reader.GetString(3);
                            string lastParagraph = GetLastParagraph(novel);

                            var item = new StoryVersion
                            {
                                Id = reader.GetInt64(0),
                                Version = reader.GetInt32(1),
                                SavedDateTime = reader.GetString(2),
                                LastParagraph = lastParagraph,
                                IsAutoSave = reader.GetInt32(4)
                            };
                            versionListBox.Items.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading versions: {ex.Message}",
                    "Load Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private string GetLastParagraph(string novel)
        {
            if (string.IsNullOrWhiteSpace(novel))
                return "(empty)";

            // Split by double newlines to get paragraphs
            string[] paragraphs = novel.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);

            if (paragraphs.Length == 0)
            {
                // No paragraphs, try single lines
                string[] lines = novel.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 0)
                {
                    string lastLine = lines[lines.Length - 1].Trim();
                    return lastLine.Length > 60 ? lastLine.Substring(0, 60) + "..." : lastLine;
                }
                return "(empty)";
            }

            string lastParagraph = paragraphs[paragraphs.Length - 1].Trim();
            // Limit to 60 characters for display
            return lastParagraph.Length > 60 ? lastParagraph.Substring(0, 60) + "..." : lastParagraph;
        }

        private void MainTitleListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (mainTitleListBox.SelectedItem is MainTitleItem selectedItem)
            {
                LoadSubTitles(selectedItem.Id);
            }
        }

        private void SubTitleListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (subTitleListBox.SelectedItem is SubTitleItem selectedItem)
            {
                LoadVersions(selectedItem.Id);
            }
        }

        private void VersionListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Enable load button when a version is selected
            loadButton.Enabled = versionListBox.SelectedItem != null;
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            if (versionListBox.SelectedItem is StoryVersion selectedVersion)
            {
                SelectedStoryVersionId = selectedVersion.Id;
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
