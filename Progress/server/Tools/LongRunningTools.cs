using System.ComponentModel;
using System.Text.Json.Nodes;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Progress.Tools;

[McpServerToolType]
public class LongRunningTools
{
    [McpServerTool, Description("Demonstrates a long running tool with progress updates")]
    public static async Task<string> LongRunningTool(
        IMcpServer server,
        RequestContext<CallToolRequestParams> context,
        int duration = 10,
        int steps = 5)
    {
        var progressToken = context.Params?.ProgressToken;
        var stepDuration = duration / steps;

        for (int i = 1; i <= steps; i++)
        {
            await Task.Delay(stepDuration * 1000);

            if (progressToken is not null)
            {
                await server.SendNotificationAsync("notifications/progress", new ProgressNotificationParams
                    {
                        ProgressToken = progressToken.GetValueOrDefault(),
                        Progress = new ProgressNotificationValue
                        {
                            Progress = i,
                            Total = steps,
                            Message = $"Step {i} of {steps} completed.",
                        },
                        Meta = new JsonObject
                        {
                            ["partialResults"] = "...",
                        }
                    });
            }
        }

        return $"Long running tool completed. Duration: {duration} seconds. Steps: {steps}.";
    }
}