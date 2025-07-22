using Microsoft.Extensions.Logging;

using ModelContextProtocol.Client;

var endpoint = Environment.GetEnvironmentVariable("ENDPOINT") ?? "http://localhost:3001";

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddConsole()
        .SetMinimumLevel(LogLevel.Trace);
});

var loggingHandler = new LoggingHandler(loggerFactory)
{
    InnerHandler = new HttpClientHandler() // required
};

var httpClient = new HttpClient(loggingHandler);

var clientTransport = new SseClientTransport(new()
{
    Endpoint = new Uri(endpoint),
    TransportMode = HttpTransportMode.StreamableHttp,
}, httpClient);

await using var mcpClient = await McpClientFactory.CreateAsync(clientTransport, new()
{
    // Comment the following line to get the latest protocol version
    // ProtocolVersion = "2025-03-26"
});

await mcpClient.PingAsync();

var tools = await mcpClient.ListToolsAsync();
foreach (var tool in tools)
{
    Console.WriteLine($"Connected to server with tools: {tool.Name}");
}
