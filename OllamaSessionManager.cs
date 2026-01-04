using System;
using System.Collections.Generic;
using System.Linq;

namespace NovelWriterAssistant
{
    /// <summary>
    /// Manages Ollama chat session state and conversation history for persistent context
    /// </summary>
    public class OllamaSessionManager
    {
        private const int MAX_MESSAGE_PAIRS = 15;
        private const int CHARS_PER_TOKEN = 4; // Rough estimate for token counting

        private List<ChatMessage> conversationHistory;
        private string lastNovelContextSent;
        private int initialContextTokens;
        private string sessionKey;

        public OllamaSessionManager()
        {
            conversationHistory = new List<ChatMessage>();
            lastNovelContextSent = "";
            initialContextTokens = 0;
            sessionKey = "";
        }

        /// <summary>
        /// Gets the current number of messages in the conversation history
        /// </summary>
        public int MessageCount => conversationHistory.Count;

        /// <summary>
        /// Checks if the session is empty (not initialized)
        /// </summary>
        public bool IsEmpty() => conversationHistory.Count == 0;

        /// <summary>
        /// Initializes a new session with the full novel context and system prompt
        /// </summary>
        public void InitializeSession(string novelContext, string systemPrompt)
        {
            conversationHistory.Clear();

            // Add system message with the system prompt or default
            string systemMessage = string.IsNullOrWhiteSpace(systemPrompt)
                ? "You are a creative fiction writer. Continue the story naturally, maintaining consistency with the established characters, plot, and writing style."
                : systemPrompt;

            conversationHistory.Add(new ChatMessage
            {
                role = "system",
                content = systemMessage
            });

            // Store the initial novel context
            lastNovelContextSent = novelContext;
            initialContextTokens = EstimateTokens(novelContext);
        }

        /// <summary>
        /// Builds chat messages for the next generation request
        /// </summary>
        public List<ChatMessage> GetChatMessages(string currentNovelText, string instructions, int paragraphCount)
        {
            // Clone the conversation history to avoid modifying the original
            var messages = conversationHistory.Select(m => new ChatMessage
            {
                role = m.role,
                content = m.content
            }).ToList();

            // Determine if this is the first request or an incremental update
            if (conversationHistory.Count <= 1)
            {
                // First request: include full context
                string contextMessage = BuildInitialContextMessage(currentNovelText, instructions, paragraphCount);
                messages.Add(new ChatMessage
                {
                    role = "user",
                    content = contextMessage
                });
            }
            else
            {
                // Incremental request: include only the delta
                string delta = GetNovelDelta(currentNovelText);
                string incrementalMessage = BuildIncrementalMessage(delta, instructions, paragraphCount);
                messages.Add(new ChatMessage
                {
                    role = "user",
                    content = incrementalMessage
                });
            }

            // Trim history if it's getting too long
            TrimHistoryIfNeeded(messages);

            return messages;
        }

        /// <summary>
        /// Adds the assistant's response to the conversation history
        /// </summary>
        public void AddAssistantResponse(string response)
        {
            conversationHistory.Add(new ChatMessage
            {
                role = "assistant",
                content = response
            });

            // Note: We don't add the user's request here because it's already in the history
            // from the GetChatMessages call that preceded this response

            // However, we need to add the last user message if it's not already there
            // This happens because we clone messages in GetChatMessages
            if (conversationHistory.Count > 0 &&
                conversationHistory[conversationHistory.Count - 1].role != "user")
            {
                // The last user message was added in GetChatMessages but not persisted
                // We need to reconstruct it from the last generation
                // For now, we'll just ensure the assistant message is added
                // The user message will be added on the next GetChatMessages call
            }
        }

        /// <summary>
        /// Updates the session after a user message was sent (should be called before AddAssistantResponse)
        /// </summary>
        public void UpdateAfterUserMessage(string userMessage)
        {
            // Add the user message to the history if it's not already the last message
            if (conversationHistory.Count == 0 ||
                conversationHistory[conversationHistory.Count - 1].content != userMessage)
            {
                conversationHistory.Add(new ChatMessage
                {
                    role = "user",
                    content = userMessage
                });
            }
        }

        /// <summary>
        /// Resets the session, clearing all conversation history
        /// </summary>
        public void ResetSession()
        {
            conversationHistory.Clear();
            lastNovelContextSent = "";
            initialContextTokens = 0;
        }

        /// <summary>
        /// Detects if the novel has been edited (not just appended to)
        /// </summary>
        public bool IsNovelEdited(string currentNovelText)
        {
            if (string.IsNullOrEmpty(lastNovelContextSent))
                return false;

            // If current text doesn't start with what we sent before, it was edited
            return !currentNovelText.StartsWith(lastNovelContextSent);
        }

        /// <summary>
        /// Updates the tracked novel text (call after successful generation)
        /// </summary>
        public void UpdateNovelContext(string newNovelText)
        {
            lastNovelContextSent = newNovelText;
        }

        /// <summary>
        /// Estimates the number of tokens saved by using session persistence
        /// </summary>
        public int GetEstimatedTokensSaved()
        {
            if (conversationHistory.Count <= 1)
                return 0;

            // If we have a session, we're saving the initial context tokens on each request
            return initialContextTokens;
        }

        /// <summary>
        /// Gets the session key
        /// </summary>
        public string GetSessionKey() => sessionKey;

        /// <summary>
        /// Sets the session key (e.g., based on novel title)
        /// </summary>
        public void SetSessionKey(string key)
        {
            sessionKey = key;
        }

        // Private helper methods

        private string BuildInitialContextMessage(string novelContext, string instructions, int paragraphCount)
        {
            var message = "Here is the story so far:\n\n" + novelContext + "\n\n";

            if (!string.IsNullOrWhiteSpace(instructions))
            {
                message += "Instructions for the next paragraph(s):\n" + instructions + "\n\n";
            }

            message += $"Write the next {paragraphCount} paragraph(s) of the story. ";
            message += "Continue naturally from where the story left off, maintaining the established tone, style, and character voices. ";
            message += "Each paragraph should be well-developed and move the story forward.";

            return message;
        }

        private string BuildIncrementalMessage(string newContent, string instructions, int paragraphCount)
        {
            var message = "";

            if (!string.IsNullOrWhiteSpace(newContent))
            {
                message = "The story has been updated. Here is the new content:\n\n" + newContent + "\n\n";
            }

            if (!string.IsNullOrWhiteSpace(instructions))
            {
                message += "Instructions for the next paragraph(s):\n" + instructions + "\n\n";
            }
            else if (string.IsNullOrWhiteSpace(message))
            {
                message = "Continue the story.\n\n";
            }

            message += $"Write the next {paragraphCount} paragraph(s) of the story, ";
            message += "maintaining consistency with everything that has come before.";

            return message;
        }

        private string GetNovelDelta(string currentNovelText)
        {
            if (string.IsNullOrEmpty(lastNovelContextSent))
                return currentNovelText;

            if (currentNovelText.StartsWith(lastNovelContextSent))
            {
                // Novel was appended to
                string delta = currentNovelText.Substring(lastNovelContextSent.Length).Trim();
                return delta;
            }

            // Novel was edited, return full text
            return currentNovelText;
        }

        private void TrimHistoryIfNeeded(List<ChatMessage> messages)
        {
            // Keep system message, initial context, and last N message pairs
            if (messages.Count > MAX_MESSAGE_PAIRS * 2 + 2)
            {
                var systemMessage = messages[0];
                var initialContext = messages[1];
                var recentMessages = messages.Skip(messages.Count - (MAX_MESSAGE_PAIRS * 2)).ToList();

                messages.Clear();
                messages.Add(systemMessage);
                messages.Add(initialContext);
                messages.AddRange(recentMessages);
            }
        }

        private int EstimateTokens(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            return text.Length / CHARS_PER_TOKEN;
        }

        /// <summary>
        /// Represents a chat message with role and content
        /// </summary>
        public class ChatMessage
        {
            public string role { get; set; }
            public string content { get; set; }
        }

        /// <summary>
        /// Represents a chat request to the Ollama API
        /// </summary>
        public class ChatRequest
        {
            public string model { get; set; }
            public List<ChatMessage> messages { get; set; }
            public bool stream { get; set; }
            public object options { get; set; }
            public string keep_alive { get; set; }
        }

        /// <summary>
        /// Represents a chat response from the Ollama API
        /// </summary>
        public class ChatResponse
        {
            public string model { get; set; }
            public DateTime created_at { get; set; }
            public ChatMessage message { get; set; }
            public bool done { get; set; }
            public int prompt_eval_count { get; set; }
            public int eval_count { get; set; }
        }
    }
}
