using ModelContextProtocol.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.AI;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

// Load configuration from appsettings.json
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

string? githubToken = config["AI:GitHubToken"];
string? modelName = config["AI:ModelName"] ?? "gpt-4o";

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

// Create OpenAI chat client for GitHub Models and convert to IChatClient
var openAIClient = new OpenAI.OpenAIClient(
    new System.ClientModel.ApiKeyCredential(githubToken!),
    new OpenAI.OpenAIClientOptions
    {
        Endpoint = new Uri("https://models.inference.ai.azure.com")
    });

// Use the extension method to convert to Microsoft.Extensions.AI.IChatClient
var baseChatClient = openAIClient.GetChatClient(modelName).AsIChatClient();
var chatClient = new ChatClientBuilder(baseChatClient)
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
