using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

var services = new ServiceCollection();

const string key = "Retry-Timeout-CircuitBreaker";
const string httpClientName = "BrewUpClient";

// Polly documentation
// Retry: https://www.pollydocs.org/strategies/retry.html
// Timeout: https://www.pollydocs.org/strategies/timeout.html
// CircuitBreaker: https://www.pollydocs.org/strategies/circuit-breaker.html
services.AddResiliencePipeline(key, static builder =>
    {
        builder.AddRetry(new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<TimeoutRejectedException>()
        });

        builder.AddTimeout(TimeSpan.FromSeconds(1.5));

        builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            SamplingDuration = TimeSpan.FromSeconds(10),
            MinimumThroughput = 8,
            BreakDuration = TimeSpan.FromSeconds(30)
        });
    })
    .AddHttpClient();

var serviceProvider = services.BuildServiceProvider();
var clientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
var httpClient = clientFactory.CreateClient(httpClientName);

for (var i = 0; i < 30; i++)
{
    Console.WriteLine($"Attempt # {i + 1}");
    var response = await httpClient.GetAsync("https://brewupapi.ambitiousocean-9c685401.italynorth.azurecontainerapps.io/v1/brewup");
    Console.WriteLine(await response.Content.ReadAsStringAsync());
    
    Thread.Sleep(500);
    
    if (i < 29)
        Console.Clear();
}