using System.Text.Json;
using ModelContextProtocol.Protocol;
using Subscriptions.Resources;

namespace Subscriptions.Services;

public class BackgroundTaskService : BackgroundService
{
    private readonly ILogger<BackgroundTaskService> _logger;

    public BackgroundTaskService(ILogger<BackgroundTaskService> logger)
    {
        _logger = logger;
    }

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
        foreach (var resource in ResourceManager.ListResources())
        {
            resource.Text = $"Updated at {DateTime.UtcNow:O}";
            if (ResourceManager.Subscriptions.TryGetValue(resource.Uri, out var mcpServer))
            {
                ResourceUpdatedNotificationParams notificationParams = new() { Uri = resource.Uri };
                _logger.LogInformation("Sending ResourceUpdatedNotifcation to the client");
                await mcpServer.SendMessageAsync(new JsonRpcNotification
                {
                    Method = NotificationMethods.ResourceUpdatedNotification,
                    Params = JsonSerializer.SerializeToNode(notificationParams),
                }, token);
            }
        }

        await Task.CompletedTask; // Placeholder for async work
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background task service is stopping.");
        await base.StopAsync(stoppingToken);
    }
}
