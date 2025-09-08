using ModelContextProtocol.Protocol;
using ModelContextProtocol.AspNetCore;
using Subscriptions.Resources;
using Subscriptions.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddMcpServer()
    .WithHttpTransport(options =>
        // Configure session timeout
        options.IdleTimeout = Timeout.InfiniteTimeSpan // Never timeout
    )
    .WithResources<LiveResources>()
    // Add dummy ListResourcesHandler as workaround for https://github.com/modelcontextprotocol/csharp-sdk/issues/656
    .WithListResourcesHandler(async (_, __) => new ListResourcesResult())
    .WithSubscribeToResourcesHandler((context, token) =>
        {
            if (context.Params?.Uri is string uri)
            {
                ResourceManager.Subscriptions.Add(uri, context.Server);
            }
            return ValueTask.FromResult(new EmptyResult());
        }
    // )
    // .WithUnsubscribeFromResourcesHandler((context, token) =>
    //     {
    //         if (context.Params?.Uri is string uri)
    //         {
    //             ResourceManager.Subscriptions.Add(uri, context.Server);
    //         }
    //         return ValueTask.FromResult(new EmptyResult());
    //     }
    );

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
