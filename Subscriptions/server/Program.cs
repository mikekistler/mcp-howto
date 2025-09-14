using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using Subscriptions.Resources;
using Subscriptions.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
builder.Services.AddMcpServer()
    .WithHttpTransport(options =>
    {
        // Configure session timeout
        options.IdleTimeout = Timeout.InfiniteTimeSpan; // Never timeout
        // Remove all subscriptions for the session when it ends
        options.RunSessionHandler = async (httpContext, mcpServer, token) =>
        {
            // Code that should run before a session starts should go here
            try
            {
                await mcpServer.RunAsync(token);
            }
            finally
            {
                // Code that should run running after a session completes should go here.
                SubscriptionManager.RemoveAllSubscriptions(mcpServer);
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
            SubscriptionManager.AddSubscription(uri, context.Server);
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
            SubscriptionManager.RemoveSubscription(uri, context.Server);
        }
        return new EmptyResult();
    }
    );
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

// Register the background service
builder.Services.AddHostedService<BackgroundTaskService>();

builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Information;
});

var app = builder.Build();

app.UseHttpsRedirection();

app.MapMcp();

app.Run();
