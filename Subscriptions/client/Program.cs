using System.Text.Json;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

var endpoint = Environment.GetEnvironmentVariable("ENDPOINT") ?? "http://localhost:3001";

var consoleLoggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});

var clientTransport = new SseClientTransport(new()
{
    Endpoint = new Uri(endpoint),
    TransportMode = HttpTransportMode.StreamableHttp,
}, consoleLoggerFactory);

McpClientOptions options = new()
{
    ClientInfo = new()
    {
        Name = "SubscriptionsClient",
        Version = "1.0.0"
    },
    // Indicate that we will handle notifications, but wait until the client is created to set them
    Capabilities = new()
    {
        NotificationHandlers = []
    }
};

await using var mcpClient = await McpClientFactory.CreateAsync(clientTransport, options, loggerFactory: consoleLoggerFactory);

// Emitting resource subscription notifications requires the server to declare subscription support
// Check if the server supports resource subscription notifications
if (mcpClient.ServerCapabilities.Resources?.Subscribe != true)
{
    Console.WriteLine("Server does not support subscription to resource notifications.");
    return;
}

// List the resource templates
var templates = await mcpClient.ListResourceTemplatesAsync();

// Bail out if no templates
if (!templates.Any())
{
    Console.WriteLine("No resource templates found.");
    return;
}

foreach (var template in templates)
{
    Console.WriteLine($"Template: {template.Name}, UriTemplate: {template.UriTemplate}");
}

// Read a resource for the first template

var firstTemplate = templates.First();
var uriTemplate = firstTemplate.UriTemplate;

// This template only has one placeholder. Replace it with a number between 1 and 99
var rng = new Random();
var number = rng.Next(1, 100);

// Replace the single placeholder with the generated number
var resourceUri = System.Text.RegularExpressions.Regex.Replace(
    uriTemplate,
    @"\{.*?\}",
    number.ToString());

// Retrieve and print the resource
var resource = await mcpClient.ReadResourceAsync(resourceUri);

// Extract the first text block from the resource contents
var resourceText = resource.Contents
    .OfType<TextResourceContents>()
    .FirstOrDefault()?.Text ?? "No text content found";

Console.WriteLine($"Resource ({resourceUri}): {resourceText}");

// Register a notification handler for ResourceUpdatedNotifications
mcpClient.RegisterNotificationHandler(NotificationMethods.ResourceUpdatedNotification,
    async (notification, token) =>
    {
        var notificationParams = JsonSerializer.Deserialize<ResourceUpdatedNotificationParams>(notification.Params, McpJsonUtilities.DefaultOptions);
        if (notificationParams is not null)
        {
            Console.WriteLine($"Resource updated: {notificationParams.Uri}");
            try
            {
                var updatedResource = await mcpClient.ReadResourceAsync(notificationParams.Uri!);
                var updatedText = updatedResource.Contents
                    .OfType<TextResourceContents>()
                    .FirstOrDefault()?.Text ?? "No text content found";
                Console.WriteLine($"Updated content: {updatedText}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading updated resource: {ex.Message}");
            }
        }
    });

// Now subscribe for resource notifications
await mcpClient.SubscribeToResourceAsync(resourceUri);

// Keep the client running to receive notifications
Console.WriteLine("Subscribed to resource notifications. Press Ctrl+C to exit...");
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};
try
{
    while (!cts.IsCancellationRequested)
    {
        await Task.Delay(TimeSpan.FromSeconds(1));
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("\nShutting down...");
}
