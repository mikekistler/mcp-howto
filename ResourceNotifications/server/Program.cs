using ModelContextProtocol.AspNetCore;
using ResourceNotifications.Resources;
using ResourceNotifications.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithResources<LiveResources>();

// Register the background service
builder.Services.AddHostedService<BackgroundTaskService>();

// Configure session timeout
builder.Services.Configure<HttpServerTransportOptions>(options =>
{
    options.IdleTimeout = Timeout.InfiniteTimeSpan; // Never timeout
});

builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Information;
});

var app = builder.Build();

app.UseHttpsRedirection();

app.MapMcp();

app.Run();
