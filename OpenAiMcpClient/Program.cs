using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Client;
using OpenAI;

// Load configuration from appsettings.json
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

// Get appsettings.json variables
string? apiKey = config["AI:ApiKey"];
string? modelName = config["AI:ModelName"] ?? "gpt-4o";

if (string.IsNullOrEmpty(apiKey))
{
    Console.WriteLine("Error: OpenAI API key is required.");
    Console.WriteLine("Please provide it in the appsettings.json file under AI:ApiKey.");
    Environment.Exit(1);
}

// Create an IChatClient using OpenAI
IChatClient client = CreateChatClient(apiKey, modelName);

// Helper method to create chat client
static IChatClient CreateChatClient(string apiKey, string modelName)
{
    try
    {
        // Use API key authentication for OpenAI
        var openAIClient = new OpenAIClient(apiKey);

        Console.WriteLine("Connected to OpenAI using API Key authentication");
        Console.WriteLine($"Using model: {modelName}");

        return new ChatClientBuilder(
            openAIClient.GetChatClient(modelName).AsIChatClient())
            .UseFunctionInvocation()
            .Build();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error connecting to Azure OpenAI: {ex.Message}");
        Environment.Exit(1);
        return null!; // This line will never execute due to the Environment.Exit above
    }
}

// Get the path to the MCP server directory 
string currentDirectory = Directory.GetCurrentDirectory();
string serverDirectory = Path.GetFullPath(Path.Combine(currentDirectory, "..", "StudentsMcpServer"));

IMcpClient mcpClient = await McpClientFactory.CreateAsync(
    new StdioClientTransport(new()
    {
        Name = "Students MCP Server",
        Command = "dotnet",
        Arguments = ["run"],
        WorkingDirectory = serverDirectory,
    }));

// List all available tools from the MCP server.
Console.WriteLine("Available tools:");
IList<McpClientTool> tools = await mcpClient.ListToolsAsync();
foreach (McpClientTool tool in tools)
{
    Console.WriteLine($"{tool}");
}
Console.WriteLine();

// Conversational loop that can utilize the tools via prompts.
List<ChatMessage> messages = [];

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

    messages.Add(new(Microsoft.Extensions.AI.ChatRole.User, userInput));

    try
    {
        List<ChatResponseUpdate> updates = [];
        await foreach (ChatResponseUpdate update in client
            .GetStreamingResponseAsync(messages, new() { Tools = [.. tools] }))
        {
            Console.Write(update);
            updates.Add(update);
        }
        Console.WriteLine();

        messages.AddMessages(updates);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n Error: {ex.Message}");
    }
}