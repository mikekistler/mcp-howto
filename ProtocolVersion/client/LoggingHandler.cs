using Microsoft.Extensions.Logging;

public class LoggingHandler : DelegatingHandler
{
    private readonly ILogger _logger;

    public LoggingHandler(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger("HttpLogger");
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Add a trace id to ensure we can correlate requests to responses
        var traceId = Guid.NewGuid().ToString();
        request.Headers.Add("X-Trace-Id", traceId);

        // Build a log message for the request
        var requestLog = new System.Text.StringBuilder();
        requestLog.AppendLine($"HTTP Request:\n{request.Method} {request.RequestUri}");

        // Add request headers
        foreach (var header in request.Headers)
        {
            requestLog.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
        }

        // Add content headers if content is present
        if (request.Content != null)
        {
            foreach (var header in request.Content.Headers)
            {
                requestLog.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }

            // Add request body
            var requestBody = await request.Content.ReadAsStringAsync();
            requestLog.AppendLine($"\n{requestBody}");
        }

        _logger.LogTrace(requestLog.ToString());

        var response = await base.SendAsync(request, cancellationToken);

        // Build a log message for the response
        var responseLog = new System.Text.StringBuilder();
        responseLog.AppendLine($"HTTP Response: {response.StatusCode}");

        // Add response headers
        foreach (var header in response.Headers)
        {
            responseLog.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
        }

        // Add content headers and body if content is present
        if (response.Content != null)
        {
            foreach (var header in response.Content.Headers)
            {
                responseLog.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }

            // Add response body
            var responseBody = await response.Content.ReadAsStringAsync();
            responseLog.AppendLine($"\n{responseBody}");
        }

        _logger.LogTrace(responseLog.ToString());

        return response;
    }
}
