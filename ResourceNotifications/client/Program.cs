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
        Name = "ResourceNotificationUpdateClient",
        Version = "1.0.0"
    }
};

await using var mcpClient = await McpClientFactory.CreateAsync(clientTransport, options);

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
Console.WriteLine($"Resource ({resourceUri}): {resource}");


