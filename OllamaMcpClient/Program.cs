using ModelContextProtocol.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.AI;
using System.Text.Json;

// MCP Client Transport
string currentDirectory = Directory.GetCurrentDirectory();
string serverDirectory = Path.GetFullPath(Path.Combine(currentDirectory, "..", "StudentsMcpServer"));
var clientTransport = new StdioClientTransport(new StdioClientTransportOptions
{
    Name = "Students MCP Server",
    Command = "dotnet",
    Arguments = ["run"],
    WorkingDirectory = serverDirectory,
});

// Logger
using var loggerFactory = LoggerFactory.Create(builder =>
    builder.AddConsole().SetMinimumLevel(LogLevel.Information));

// Create MCP Client
var mcpClient = await McpClientFactory.CreateAsync(clientTransport);

// Get available tools from MCP Server
var mcpTools = await mcpClient.ListToolsAsync();

var toolsJson = JsonSerializer.Serialize(mcpTools, new JsonSerializerOptions { WriteIndented = true });
Console.WriteLine("\nAvailable Tools:\n" + toolsJson);

await Task.Delay(100);

// Configure Ollama LLM Client
var ollamaChatClient = new OllamaChatClient(
    new Uri("http://localhost:11434/"),
    "llama3.2:3b"
);

var chatClient = new ChatClientBuilder(ollamaChatClient)
    .UseLogging(loggerFactory)
    .UseFunctionInvocation()
    .Build();

// Prompt loop
Console.WriteLine("Type your message below (type 'exit' to quit):");

while (true)
{
    Console.Write("\n You: ");
    var userInput = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(userInput))
        continue;

    if (userInput.Trim().ToLower() == "exit")
    {
        Console.WriteLine("Exiting chat...");
        break;
    }

    var messages = new List<ChatMessage>
    {
        new(ChatRole.System, "You are a helpful assistant."),
        new(ChatRole.User, userInput)
    };

    try
    {
        var response = await chatClient.GetResponseAsync(
            messages,
            new ChatOptions { Tools = mcpTools.ToArray<AITool>() });

        var assistantMessage = response.Messages.LastOrDefault(m => m.Role == ChatRole.Assistant);

        if (assistantMessage != null)
        {
            var textOutput = string.Join($" ", assistantMessage.Contents.Select(c => c.ToString()));
            Console.WriteLine("\n AI: " + textOutput);
        }
        else
        {
            Console.WriteLine("\n AI: (no assistant message received)");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n Error: {ex.Message}");
    }
}
