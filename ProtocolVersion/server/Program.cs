using System.Net.Http.Headers;

using QuickstartWeatherServer.Tools;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<WeatherTools>();

builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddSingleton(_ =>
{
    var client = new HttpClient { BaseAddress = new Uri("https://api.weather.gov") };
    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("weather-tool", "1.0"));
    return client;
});

var app = builder.Build();

// Middleware to pass through X-Trace-Id header from request to response
app.Use(async (context, next) =>
{
    // Check if the request contains an X-Trace-Id header
    if (context.Request.Headers.TryGetValue("X-Trace-Id", out var traceIdValues))
    {
        var traceId = traceIdValues.FirstOrDefault();
        if (!string.IsNullOrEmpty(traceId))
        {
            // Add the X-Trace-Id header to the response
            context.Response.Headers.Append("X-Trace-Id", traceId);
        }
    }

    await next();
});

app.MapMcp();

app.Run();
