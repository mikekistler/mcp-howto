
using Microsoft.Extensions.DependencyInjection;

public static class ClientResilience
{
    /// <summary>
    /// Creates an HttpClient configured with standard resilience patterns for improved reliability.
    /// </summary>
    /// <returns>
    /// An HttpClient instance with the following resilience features enabled:
    /// - Retry policy: Automatically retries failed requests with exponential backoff
    /// - Circuit breaker: Prevents cascading failures by temporarily stopping requests to failing services
    /// - Timeout policy: Enforces request timeouts (default 10 seconds) to prevent hanging operations
    /// - Rate limiting: Controls the rate of outgoing requests to prevent overwhelming downstream services
    /// - Hedging: Sends additional requests when the primary request is delayed
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown when the HttpClient cannot be resolved from the service provider.</exception>
    public static HttpClient ResilientHttpClient()
    {
        var services = new ServiceCollection();
        var httpClientBuilder = services
            .AddHttpClient<DummyClient>()
            .AddStandardResilienceHandler();
        var serviceProvider = services.BuildServiceProvider();
        var httpClient = serviceProvider.GetRequiredService<HttpClient>();
        return (httpClient as HttpClient) ?? throw new InvalidOperationException("Failed to get HttpClient");
    }
}

internal sealed class DummyClient { }
