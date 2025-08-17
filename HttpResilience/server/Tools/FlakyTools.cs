using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace FlakyServer.Tools;

[McpServerToolType]
public class FlakyTools
{
    private readonly ILogger<FlakyTools> _logger;

    public FlakyTools(ILogger<FlakyTools> logger)
    {
        _logger = logger;
    }

    [McpServerTool, Description("Tool that randomly fails 50% of the time to demonstrate error handling")]
    public async Task<string> RandomFailureTool(
        IMcpServer server,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("RandomFailureTool invoked");

        var random = new Random();
        if (random.Next(2) == 0)
        {
            _logger.LogError("RandomFailureTool failed - random failure occurred");
            throw new InvalidOperationException("Random failure occurred - this tool fails 50% of the time");
        }

        await Task.Delay(100, cancellationToken); // Small delay to simulate work

        _logger.LogInformation("RandomFailureTool completed successfully");
        return "Tool executed successfully";
    }

    [McpServerTool, Description("Tool that simulates a lost request by waiting 5 minutes before responding")]
    public async Task<string> LostRequestTool(
        IMcpServer server,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("LostRequestTool invoked - will wait 5 minutes");

        await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);

        _logger.LogInformation("LostRequestTool completed after 5 minutes");
        return "Request finally completed after 5 minutes";
    }
}