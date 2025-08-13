## Logging

MCP servers may expose log messages to clients through the [Logging utility].

[Logging utility]: https://modelcontextprotocol.io/specification/2025-06-18/server/utilities/logging

This directory demonstrates a Model Context Protocol (MCP) client and server that support structured logging and log message exchange.

The logging example consists of two components:

- **Client** (`client/`): Connects to the MCP server, sets the logging level, subscribes to log notifications, and invokes a tool that generates log messages.
- **Server** (`server/`): Implements an MCP server with a tool that emits log messages at various levels, and supports dynamic logging configuration.

### Logging Levels

MCP uses the logging levels defined in [RFC 5424](https://tools.ietf.org/html/rfc5424).

The MCP C# SDK uses the standard .NET [ILogger] and [ILoggerProvider] abstractions, which support a slightly
different set of logging levels. Here's the levels and how they map to standard .NET logging levels.

| Level     | .NET | Description                       | Example Use Case             |
|-----------|------|-----------------------------------|------------------------------|
| debug     | ✓    | Detailed debugging information    | Function entry/exit points   |
| info      | ✓    | General informational messages    | Operation progress updates   |
| notice    |      | Normal but significant events     | Configuration changes        |
| warning   | ✓    | Warning conditions                | Deprecated feature usage     |
| error     | ✓    | Error conditions                  | Operation failures           |
| critical  | ✓    | Critical conditions               | System component failures    |
| alert     |      | Action must be taken immediately  | Data corruption detected     |
| emergency |      | System is unusable                |                              |

**Note:** .NET's [ILogger] also supports a `Trace` level (more verbose than Debug) log level.
As there is no equivalent level in the MCP logging levels, Trace level logs messages are silently
dropped when sending messages to the client.

[ILogger]: https://learn.microsoft.com/dotnet/api/microsoft.extensions.logging.ilogger
[ILoggerProvider]: https://learn.microsoft.com/dotnet/api/microsoft.extensions.logging.iloggerprovider

### Server configuration and logging

MCP servers that implement the Logging utility must declare this in the capabilities send in the
[Initialization] phase at the beginning of the MCP session.

[Initialization]: https://modelcontextprotocol.io/specification/2025-06-18/basic/lifecycle#initialization

Servers built with the C# SDK always declare the logging capability. The C# SDK provides an extension method
[WithSetLoggingLevelHandler] on [IMcpServerBuilder] if the server has some special logic it wants to perform
when a client sets the logging level. However, the SDK already takes care of setting the [LoggingLevel]
in the [IMcpServer], so most servers will not need to implement this.

[IMcpServer]: https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.Server.IMcpServer.html
[IMcpServerBuilder]: https://modelcontextprotocol.github.io/csharp-sdk/api/Microsoft.Extensions.DependencyInjection.IMcpServerBuilder.html
[WithSetLoggingLevelHandler]: https://modelcontextprotocol.github.io/csharp-sdk/api/Microsoft.Extensions.DependencyInjection.McpServerBuilderExtensions.html#Microsoft_Extensions_DependencyInjection_McpServerBuilderExtensions_WithSetLoggingLevelHandler_Microsoft_Extensions_DependencyInjection_IMcpServerBuilder_System_Func_ModelContextProtocol_Server_RequestContext_ModelContextProtocol_Protocol_SetLevelRequestParams__System_Threading_CancellationToken_System_Threading_Tasks_ValueTask_ModelContextProtocol_Protocol_EmptyResult___
[LoggingLevel]: https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.Server.IMcpServer.html#ModelContextProtocol_Server_IMcpServer_LoggingLevel

MCP Servers using the MCP C# SDK can obtain an [ILoggerProvider] from the IMcpServer [AsClientLoggerProvider] extension method,
and from that can create an [ILogger] instance for logging messages that should be sent to the MCP client.

```csharp
    ILoggerProvider loggerProvider = context.Server.AsClientLoggerProvider();
    ILogger logger = loggerProvider.CreateLogger("LoggingTools");
```

[ILoggerProvider]: https://learn.microsoft.com/dotnet/api/microsoft.extensions.logging.iloggerprovider
[AsClientLoggerProvider]: https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.Server.McpServerExtensions.html#ModelContextProtocol_Server_McpServerExtensions_AsClientLoggerProvider_ModelContextProtocol_Server_IMcpServer_
[ILogger]: https://learn.microsoft.com/dotnet/api/microsoft.extensions.logging.ilogger

### Client support for logging

Clients that wish to receive log messages from the server must first check if logging is supported.
This is done by checking the [Logging] property of the [ServerCapabilities] field of [IMcpClient].

[IMcpClient]: https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.Client.IMcpClient.html
[ServerCapabilities]: https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.Client.IMcpClient.html#ModelContextProtocol_Client_IMcpClient_ServerCapabilities
[Logging]: https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.Protocol.ServerCapabilities.html#ModelContextProtocol_Protocol_ServerCapabilities_Logging

```csharp
// Verify that the server supports logging
if (mcpClient.ServerCapabilities.Logging is null)
{
    Console.WriteLine("Server does not support logging.");
    return;
}
```

If the server supports logging, the client can set the level of log messages it wishes to receive with
the [SetLoggingLevel] method on [IMcpClient]. The `loggingLevel` specified here is an MCP logging level.
See the [Logging Levels](#logging-levels) section above for the mapping between MCP and .NET logging levels.
In the sample client, the logging level can be passed as a command line argument (e.g., `dotnet run Debug`).

[SetLoggingLevel]: https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.Client.McpClientExtensions.html#ModelContextProtocol_Client_McpClientExtensions_SetLoggingLevel_ModelContextProtocol_Client_IMcpClient_Microsoft_Extensions_Logging_LogLevel_System_Threading_CancellationToken_

```csharp
await mcpClient.SetLoggingLevel(loggingLevel);
```

Lastly, the client must configure a notification handler for [NotificationMethods.LoggingMessageNotification] notifications.

[NotificationMethods.LoggingMessageNotification]: https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.Protocol.NotificationMethods.html#ModelContextProtocol_Protocol_NotificationMethods_LoggingMessageNotification

```csharp
mcpClient.RegisterNotificationHandler(NotificationMethods.LoggingMessageNotification,
    (notification, ct) =>
    {
        if (JsonSerializer.Deserialize<LoggingMessageNotificationParams>(notification.Params) is { } ln)
        {
            Console.WriteLine($"[{ln.Level}] {ln.Logger} {ln.Data}");
        }
        return default;
    });
```

### Running the Example

1. **Start the server:**
	```sh
	cd server
	dotnet run
	```
2. **Run the client:**
	```sh
	cd ../client
	dotnet run [LoggingLevel]
	# Example: dotnet run Debug
	```
	If no logging level is specified, the default from the server configuration is used.
