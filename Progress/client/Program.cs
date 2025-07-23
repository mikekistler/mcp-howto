using System.Text.Json;
using System.Text.Json.Nodes;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

var endpoint = Environment.GetEnvironmentVariable("ENDPOINT") ?? "http://localhost:3001";

var clientTransport = new SseClientTransport(new()
{
    Endpoint = new Uri(endpoint),
    TransportMode = HttpTransportMode.StreamableHttp,
});

McpClientOptions options = new()
{
    ClientInfo = new()
    {
        Name = "ProgressClient",
        Version = "1.0.0"
    }
};

await using var mcpClient = await McpClientFactory.CreateAsync(clientTransport, options);

var tools = await mcpClient.ListToolsAsync();
foreach (var tool in tools)
{
    Console.WriteLine($"Connected to server with tools: {tool.Name}");
}

Console.WriteLine($"Calling tool: {tools.First().Name}");

ProgressToken progressToken = new(Guid.NewGuid().ToString("N"));

mcpClient.RegisterNotificationHandler(NotificationMethods.ProgressNotification,
    (notification, cancellationToken) =>
    {
        if (JsonSerializer.Deserialize<ProgressNotificationParams>(notification.Params) is { } pn &&
            pn.ProgressToken == progressToken)
        {
            // progress.Report(pn.Progress);
            Console.WriteLine($"Tool progress: {pn.Progress.Progress} of {pn.Progress.Total} - {pn.Progress.Message}");
            if (pn.Meta is { } meta)
            {
                Console.WriteLine($"Meta data: {JsonSerializer.Serialize(meta)}");
            }
        }
        return ValueTask.CompletedTask;
    }).ConfigureAwait(false);

var request = new JsonRpcRequest
{
    Method = RequestMethods.ToolsCall,
    Params = new JsonObject
    {
        ["Name"] = tools.First().Name,
        ["Arguments"] = new JsonObject(),
        ["_meta"] = new JsonObject
        {
            ["ProgressToken"] = progressToken.ToString(),
        }
    }
};

var response = await mcpClient.SendRequestAsync(request, cancellationToken: default);

var result = JsonSerializer.Deserialize<CallToolResult>(response.Result);

foreach (var block in result.Content)
{
    if (block is TextContentBlock textBlock)
    {
        Console.WriteLine(textBlock.Text);
    }
    else
    {
        Console.WriteLine($"Received unexpected result content of type {block.GetType()}");
    }
}
