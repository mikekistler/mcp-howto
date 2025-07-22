## Elicitation: Interactive User Engagement

One of the most significant additions is the **elicitation** feature, which allows servers to request additional information from users during interactions. This enables more dynamic and interactive AI experiences, making it easier to gather necessary context before executing tasks.

### Server Support for Elicitation

Servers request structured data from users with the [ElicitAsync] extension method on [IMcpServer].
The C# SDK registers an instance of [IMcpServer] with the dependency injection container,
so tools can simply add a parameter of type [IMcpServer] to their method signature to access it.

[ElicitAsync]: https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.Server.McpServerExtensions.html#ModelContextProtocol_Server_McpServerExtensions_ElicitAsync_ModelContextProtocol_Server_IMcpServer_ModelContextProtocol_Protocol_ElicitRequestParams_System_Threading_CancellationToken_
[IMcpServer]: https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.Server.IMcpServer.html

The MCP Server must specify the schema of each input value it is requesting from the user.
Only primitive types (string, number, boolean) are supported for elicitation requests.
The schema may include a description to help the user understand what is being requested.

The server can request a single input or multiple inputs at once.
To help distinguish multiple inputs, each input has a unique name.

The following example demonstrates how a server could request a boolean response from the user.

```csharp
[McpServerTool, Description("A simple game where the user has to guess a number between 1 and 10.")]
public async Task<string> GuessTheNumber(
    IMcpServer server, // Get the McpServer from DI container
    CancellationToken token
)
{
    // First ask the user if they want to play
    var playSchema = new RequestSchema
    {
        Properties =
        {
            ["Answer"] = new BooleanSchema()
        }
    };

    var playResponse = await server.ElicitAsync(new ElicitRequestParams
    {
        Message = "Do you want to play a game?",
        RequestedSchema = playSchema
    }, token);

    // Check if user wants to play
    if (playResponse.Action != "accept" || playResponse.Content?["Answer"].ValueKind != JsonValueKind.True)
    {
        return "Maybe next time!";
    }

    // remaining implementation of GuessTheNumber method
```

### Client Support for Elicitation

Elicitation is an optional feature so clients declare their support for it in their capabilities as part of the `initialize` request. In the MCP C# SDK, this is done by configuring an [ElicitationHandler] in the [McpClientOptions]:

[ElicitationHandler]: https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.Protocol.ElicitationCapability.html#ModelContextProtocol_Protocol_ElicitationCapability_ElicitationHandler
[McpClientOptions]: https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.Client.McpClientOptions.html


```csharp
McpClientOptions options = new()
{
    ClientInfo = new()
    {
        Name = "ElicitationClient",
        Version = "1.0.0"
    },
    Capabilities = new()
    {
        Elicitation = new()
        {
            ElicitationHandler = HandleElicitationAsync
        }
    }
};
```

The ElicitationHandler is an asynchronous method that will be called when the server requests additional information.
The ElicitationHandler must request input from the user and return the data in a format that matches the requested schema.
This will be highly dependent on the client application and how it interacts with the user.

If the user provides the requested information, the ElicitationHandler should return an [ElicitResult] with the action set to "accept" and the content containing the user's input.
If the user does not provide the requested information, the ElicitationHandler should return an [ElicitResult] with the action set to "reject" and no content.

Below is an example of how a console application might handle elicitation requests.
Here's an example implementation:

```csharp
async ValueTask<ElicitResult> HandleElicitationAsync(ElicitRequestParams? requestParams, CancellationToken token)
{
    // Bail out if the requestParams is null or if the requested schema has no properties
    if (requestParams?.RequestedSchema?.Properties == null)
    {
        return new ElicitResult(); // New ElicitResult with default Action "reject"
    }

    // Process the elicitation request
    if (requestParams?.Message is not null)
    {
        Console.WriteLine(requestParams.Message);
    }

    var content = new Dictionary<string, JsonElement>();

    // Loop through requestParams.requestSchema.Properties dictionary requesting values for each property
    foreach (var property in requestParams.RequestedSchema.Properties)
    {
        if (property.Value is ElicitRequestParams.BooleanSchema booleanSchema)
        {
            Console.Write($"{booleanSchema.Description}: ");
            var clientInput = Console.ReadLine();
            bool parsedBool;
            if (bool.TryParse(clientInput, out parsedBool))
            {
                content[property.Key] = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(parsedBool));
            }
        }
        else if (property.Value is ElicitRequestParams.NumberSchema numberSchema)
        {
            Console.Write($"{numberSchema.Description}: ");
            var clientInput = Console.ReadLine();
            double parsedNumber;
            if (double.TryParse(clientInput, out parsedNumber))
            {
                content[property.Key] = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(parsedNumber));
            }
        }
        else if (property.Value is ElicitRequestParams.StringSchema stringSchema)
        {
            Console.Write($"{stringSchema.Description}: ");
            var clientInput = Console.ReadLine();
            content[property.Key] = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(clientInput));
        }
    }

    // Return the user's input
    return new ElicitResult
    {
        Action = "accept",
        Content = content
    };
}
```
