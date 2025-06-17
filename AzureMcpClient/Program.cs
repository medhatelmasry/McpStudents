using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Client;

// Load configuration from appsettings.json
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

// Get appsettings.json variables
string? endpoint = config["AI:Endpoint"];
string? apiKey = config["AI:ApiKey"];
string? deploymentName = config["AI:DeploymentName"] ?? "gpt-4o";

if (string.IsNullOrEmpty(endpoint))
{
    Console.WriteLine("Error: Azure OpenAI endpoint is required.");
    Console.WriteLine("Please provide it using --endpoint parameter or AZURE_OPENAI_ENDPOINT environment variable.");
    Environment.Exit(1);
}

// Create an IChatClient using Azure OpenAI
IChatClient client = CreateChatClient(endpoint, apiKey, deploymentName);

// Helper method to create chat client
static IChatClient CreateChatClient(string endpoint, string? apiKey, string deploymentName)
{
    try
    {
        AzureOpenAIClient azureClient;

        if (!string.IsNullOrEmpty(apiKey))
        {
            // Use API key authentication if provided
            azureClient = new AzureOpenAIClient(
                new Uri(endpoint),
                new AzureKeyCredential(apiKey));

            Console.WriteLine("Connected to Azure OpenAI using API Key authentication");
        }
        else
        {
            // Fall back to DefaultAzureCredential if no API key is provided
            azureClient = new AzureOpenAIClient(
                new Uri(endpoint),
                new DefaultAzureCredential());

            Console.WriteLine("Connected to Azure OpenAI using DefaultAzureCredential");
        }

        Console.WriteLine($"Using deployment: {deploymentName}");

        return new ChatClientBuilder(
            azureClient.GetChatClient(deploymentName).AsIChatClient())
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

    messages.Add(new(ChatRole.User, userInput));

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