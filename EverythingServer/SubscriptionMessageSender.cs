using ModelContextProtocol;
using ModelContextProtocol.Server;

internal class SubscriptionMessageSender(IServiceProvider serviceProvider, HashSet<string> subscriptions) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait for the application to fully start before trying to access the MCP server
        await Task.Delay(2000, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Try to get the server from the service provider
                var server = serviceProvider.GetService<IMcpServer>();
                if (server != null)
                {
                    foreach (var uri in subscriptions)
                    {
                        await server.SendNotificationAsync("notifications/resource/updated",
                            new
                            {
                                Uri = uri,
                            }, cancellationToken: stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception but don't crash the service
                Console.WriteLine($"Error in SubscriptionMessageSender: {ex.Message}");
            }

            await Task.Delay(5000, stoppingToken);
        }
    }
}
