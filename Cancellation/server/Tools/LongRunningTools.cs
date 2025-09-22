using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Cancellation.Tools;

public class LongRunningTools
{
    private readonly ILogger<LongRunningTools> _logger;

    public LongRunningTools(ILogger<LongRunningTools> logger)
    {
        _logger = logger;
    }

    [McpServerTool, Description("Demonstrates a long running tool that supports cancellation")]
    public async Task<string> LongRunningTool(
        IMcpServer server,
        RequestContext<CallToolRequestParams> context,
        int duration = 10,
        int steps = 5,
        CancellationToken cancellationToken = default)
    {
        var stepDuration = duration / steps;

        for (int i = 1; i <= steps; i++)
        {
            await Task.Delay(stepDuration * 1000);

            _logger.LogInformation("Long running tool step {Step}/{TotalSteps} completed", i, steps);

            // Check for cancellation
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Long running tool was cancelled at step {Step}/{TotalSteps}", i, steps);
            }
            cancellationToken.ThrowIfCancellationRequested();
        }

        return $"Long running tool completed. Duration: {duration} seconds. Steps: {steps}.";
    }
}