using Microsoft.Extensions.Options;
using ResourceNotifications.Resources;

namespace ResourceNotifications.Services;

public class BackgroundTaskService : BackgroundService
{
    private readonly ILogger<BackgroundTaskService> _logger;
    private int _resourceCounter = 1;

    public BackgroundTaskService(ILogger<BackgroundTaskService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5)); // every 5 seconds

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await UpdateExistingResourcesAsync();
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

    private async Task UpdateExistingResourcesAsync()
    {
        _logger.LogDebug("Updating existing resources with current timestamp");

        // You could iterate through existing resources and update them
        // Example: Update all resources with a new description containing the current timestamp
        foreach (var resource in ResourceManager.ListResources())
        {
            resource.Text = $"Updated at {DateTime.UtcNow:O}";
        }

        await Task.CompletedTask; // Placeholder for async work
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background task service is stopping.");
        await base.StopAsync(stoppingToken);
    }
}
