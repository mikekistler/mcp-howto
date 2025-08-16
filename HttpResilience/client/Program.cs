using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

var endpoint = Environment.GetEnvironmentVariable("ENDPOINT") ?? "http://localhost:3001";

var httpClient = ClientResilience.ResilientHttpClient();

var clientTransport = new SseClientTransport(new()
{
    Endpoint = new Uri(endpoint),
    TransportMode = HttpTransportMode.StreamableHttp,
}, httpClient);

await using var mcpClient = await McpClientFactory.CreateAsync(clientTransport);

await mcpClient.PingAsync();

var tools = await mcpClient.ListToolsAsync();

while (true)
{
    Console.WriteLine("\nAvailable tools:");
    for (int i = 0; i < tools.Count; i++)
    {
        Console.WriteLine($"{i + 1}. {tools[i].Name} - {tools[i].Description}");
    }

    Console.Write("Select a tool to invoke or 0 to exit (enter number): ");

    var input = Console.ReadLine();
    if (!int.TryParse(input, out int choice) || choice < 0 || choice > tools.Count)
    {
        Console.WriteLine("Invalid selection. Please try again.");
        continue;
    }

    if (choice == 0)
        break;

    var selectedTool = tools[choice - 1];

    try
    {
        var result = await mcpClient.CallToolAsync(selectedTool.Name, new Dictionary<string, object?>());
        var textContent = result.Content?.OfType<TextContentBlock>().FirstOrDefault()?.Text ?? "No text content";
        Console.WriteLine($"Tool '{selectedTool.Name}' result: {textContent}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error invoking tool: {ex.Message}");
    }
}
