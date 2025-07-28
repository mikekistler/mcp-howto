using ModelContextProtocol.Protocol;
using ModelContextProtocol.AspNetCore;
using ResourceNotifications.Resources;
using ResourceNotifications.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddMcpServer()
    .WithHttpTransport(options =>
        // Configure session timeout
        options.IdleTimeout = Timeout.InfiniteTimeSpan // Never timeout
    )
    .WithResources<LiveResources>()
    .WithSubscribeToResourcesHandler((context, token) =>
        {
            if (context.Params?.Uri is string uri)
            {
                ResourceManager.Subscriptions.Add(uri);
            }
            return ValueTask.FromResult(new EmptyResult());
        }
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
