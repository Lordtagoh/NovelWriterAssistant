# Novel Writer Assistant

A WinForms application to help writers continue their novels using AI-powered suggestions via Ollama.

## Prerequisites

1. **.NET 6.0 or later** - Download from https://dotnet.microsoft.com/download
2. **Ollama** - Download and install from https://ollama.ai
3. **An LLM model** - Install a model in Ollama (recommended: qwen2.5:7b)

## Setup Instructions

### 1. Install Ollama and Model

```bash
# Download and install Ollama from https://ollama.ai

# Pull the recommended model (or choose another under 10B)
ollama pull qwen2.5:7b

# Alternatively, you can use:
# ollama pull llama3.2:3b
# ollama pull mistral:7b
# ollama pull phi3.5
```

### 2. Start Ollama Server

```bash
ollama serve
```

This will start the Ollama server on http://localhost:11434

### 3. Build and Run the Application

#### Option A: Using Visual Studio
1. Open Visual Studio
2. Create a new WinForms project
3. Replace the auto-generated files with `NovelWriterApp.cs` and `NovelWriterAssistant.csproj`
4. Build and run (F5)

#### Option B: Using Command Line
1. Open a terminal in the project directory
2. Build the project:
   ```bash
   dotnet build
   ```
3. Run the application:
   ```bash
   dotnet run
   ```

## Usage

1. **Write your novel** in the top text box (or paste existing content)
2. **Enter instructions** in the middle text box describing how you want the story to continue
   - Example: "The hero discovers a hidden door"
   - Example: "Add a plot twist involving the mysterious stranger"
3. **Click "Generate Next Paragraph"** button
4. **Review three different options** that appear in the bottom text boxes
5. **Choose your favorite** and copy it to the novel text box to continue

## Configuration

You can customize the model and generation parameters in the code:

### Change the Model
In `NovelWriterApp.cs`, line 23:
```csharp
private const string MODEL_NAME = "qwen2.5:7b"; // Change to your preferred model
```

### Adjust Generation Parameters
In the `GenerateParagraph` method (around line 196):
```csharp
options = new
{
    temperature = 0.8,  // Higher = more creative (0.0-2.0)
    top_p = 0.9,        // Diversity of word choice (0.0-1.0)
    num_predict = 200   // Max tokens to generate
}
```

## Troubleshooting

### "Connection refused" or "Cannot connect to Ollama"
- Make sure Ollama is running (`ollama serve`)
- Check that Ollama is accessible at http://localhost:11434

### "Model not found"
- Install the model: `ollama pull qwen2.5:7b`
- Check installed models: `ollama list`

### Generation is slow
- Smaller models (3B) are faster than larger ones (7B)
- GPU acceleration significantly improves speed
- Consider reducing `num_predict` parameter

### Generated text quality is poor
- Try a different model (qwen2.5:7b is recommended for creative writing)
- Adjust temperature (higher = more creative but less coherent)
- Provide more context in the novel text box
- Be more specific in your instructions

## Features

- **Three variations**: Get multiple creative options for each continuation
- **Context-aware**: Uses your existing novel as context
- **Customizable instructions**: Guide the AI with specific directions
- **Simple interface**: Clean, focused design for writers
- **Local and private**: All processing happens on your machine

## Future Enhancements

Possible additions:
- Save/load novel files
- Button to copy selected option to novel text
- Model selection dropdown
- Temperature/creativity slider
- Character and plot tracking
- Export to various formats

## License

Free to use and modify for personal or commercial projects.
