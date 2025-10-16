using System.Collections.Concurrent;
using System.Text.Json;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Subscriptions.Resources;

namespace Subscriptions.Services;

public class NotificationService(ILogger<NotificationService> logger, McpServer server, ConcurrentDictionary<string, byte> subscriptions): BackgroundService
{
    private readonly ILogger<NotificationService> _logger = logger;
    private readonly McpServer _server = server;
    private readonly ConcurrentDictionary<string, byte> _subscriptions = subscriptions;

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5)); // every 5 seconds

        try
        {
            while (await timer.WaitForNextTickAsync(token))
            {
                await UpdateExistingResourcesAsync(token);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Background task service is stopping.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred in the background task service.");
        }
    }

    private async Task UpdateExistingResourcesAsync(CancellationToken token)
    {
        _logger.LogDebug("Updating existing resources with current timestamp");

        // Iterate through existing resources and update them
        foreach (var resourceUri in _subscriptions.Keys)
        {
            var resource = ResourceManager.GetResource(resourceUri);
            if (resource is null)
            {
                _logger.LogWarning("Resource {ResourceUri} not found", resourceUri);
                continue;
            }
            resource.Text = $"Updated at {DateTime.UtcNow:O}";

            ResourceUpdatedNotificationParams notificationParams = new() { Uri = resource.Uri };
            _logger.LogInformation("Sending ResourceUpdatedNotification to the client");
            await _server.SendMessageAsync(new JsonRpcNotification
            {
                Method = NotificationMethods.ResourceUpdatedNotification,
                Params = JsonSerializer.SerializeToNode(notificationParams),
            }, token);

        }

        await Task.CompletedTask; // Placeholder for async work
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background task service is stopping.");
        await base.StopAsync(stoppingToken);
    }
}
