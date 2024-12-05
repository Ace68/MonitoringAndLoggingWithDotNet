using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;
using Polly.Timeout;

var services = new ServiceCollection();

const string key = "Retry-Timeout";
const string httpClientName = "BrewUpClient";

// Polly documentation
// Retry: https://www.pollydocs.org/strategies/retry.html
// Timeout: https://www.pollydocs.org/strategies/timeout.html
services.AddResiliencePipeline(key, static builder =>
{
    builder.AddRetry(new RetryStrategyOptions
    {
        ShouldHandle = new PredicateBuilder().Handle<TimeoutRejectedException>()
    });

    builder.AddTimeout(TimeSpan.FromSeconds(1.5));
})
    .AddHttpClient();

// // The AddStandardResilienceHandler method will work only if you get your HttpClient instances
// // by using the IHttpCientFactory you get from the service provider
var serviceProvider = services.BuildServiceProvider();
var clientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
var httpClient = clientFactory.CreateClient( httpClientName );
var response = await httpClient.GetAsync( "https://brewupapi.ambitiousocean-9c685401.italynorth.azurecontainerapps.io/v1/brewup" );
Console.WriteLine( await response.Content.ReadAsStringAsync() );
