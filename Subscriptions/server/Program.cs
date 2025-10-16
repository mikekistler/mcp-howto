using System.Collections.Concurrent;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using Subscriptions.Resources;
using Subscriptions.Services;

// Dictionary of session IDs to a set of resource URIs they are subscribed to
// The value is a ConcurrentDictionary used as a thread-safe HashSet
// because .NET does not have a built-in concurrent HashSet
ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> subscriptions = new();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
builder.Services.AddMcpServer()
    .WithHttpTransport(options =>
    {
        // Add a RunSessionHandler to remove all subscriptions for the session when it ends
        options.RunSessionHandler = async (httpContext, mcpServer, token) =>
        {
            if (mcpServer.SessionId == null)
            {
                // There is no sessionId if the serverOptions.Stateless is true
                await mcpServer.RunAsync(token);
                return;
            }
            try
            {
                subscriptions[mcpServer.SessionId] = new ConcurrentDictionary<string, byte>();
                // Get logger from DI -- not sure why I need to do it this way
                var loggerFactory = httpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<NotificationService>();
                // Start an instance of NotificationService for this session
                using var notificationSender = new NotificationService(logger, mcpServer, subscriptions[mcpServer.SessionId]);
                await notificationSender.StartAsync(token);
                await mcpServer.RunAsync(token);
            }
            finally
            {
                // This code runs when the session ends
                subscriptions.TryRemove(mcpServer.SessionId, out _);
            }
        };
    })
    .WithResources<LiveResources>()
    // Add dummy ListResourcesHandler as workaround for https://github.com/modelcontextprotocol/csharp-sdk/issues/656
    .WithListResourcesHandler(async (_, __) => new ListResourcesResult())
    .WithSubscribeToResourcesHandler(async (context, token) =>
    {
        if (context.Server.SessionId == null)
        {
            throw new McpException("Cannot add subscription for server with null SessionId");
        }
        if (context.Params?.Uri is { } uri)
        {
            subscriptions[context.Server.SessionId].TryAdd(uri, 0);
        }

        return new EmptyResult();
    })
    .WithUnsubscribeFromResourcesHandler(async (context, token) =>
    {
        if (context.Server.SessionId == null)
        {
            throw new McpException("Cannot remove subscription for server with null SessionId");
        }
        if (context.Params?.Uri is { } uri)
        {
            subscriptions[context.Server.SessionId].TryRemove(uri, out _);
        }
        return new EmptyResult();
    }
    );
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Information;
});

var app = builder.Build();

app.UseHttpsRedirection();

app.MapMcp();

app.Run();
