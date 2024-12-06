using BrewUp.ChaosAndTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Polly.Telemetry;
using Polly.Timeout;

const string key = "brewup-chaos-telemetry";
const string httpClientName = "BrewUpClient";

var services = new ServiceCollection()
    .AddLogging(builder => builder.AddConsole())
    .AddResiliencePipeline(key, builder =>
    {
        builder.AddRetry(new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<TimeoutRejectedException>(),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            MaxRetryAttempts = 4,
            Delay = TimeSpan.FromSeconds(2)
        });

        // builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
        // {
        //     FailureRatio = 0.5,
        //     SamplingDuration = TimeSpan.FromSeconds(10),
        //     MinimumThroughput = 8,
        //     BreakDuration = TimeSpan.FromSeconds(30)
        // });
    })
    .Configure<TelemetryOptions>(telemetryOptions =>
    {
        // Configure enrichers
        telemetryOptions.MeteringEnrichers.Add(new BrewUpMeteringEnricher());

        // Configure telemetry listeners
        telemetryOptions.TelemetryListeners.Add(new BrewUpTelemetryListener());
        
        // Configure telemetry severity
        telemetryOptions.SeverityProvider = args => args.Event.EventName switch
        {
            "OnRetry" => ResilienceEventSeverity.Debug,
            "ExecutionAttempt" => ResilienceEventSeverity.Debug,
            _ => args.Event.Severity
        };
    })
    .AddHttpClient();

var builder = new ResiliencePipelineBuilder
{
    Name = key,
    InstanceName = httpClientName
};

var serviceProvider = services.BuildServiceProvider();
var clientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
var httpClient = clientFactory.CreateClient(httpClientName);

ResilienceContext resilienceContext = ResilienceContextPool.Shared.Get(key);

ResiliencePipeline pipeline = builder.Build();
await pipeline.Execute(
    async _ =>
    {
        for (var i = 0; i < 30; i++)
        {
            Console.WriteLine($"Attempt # {i + 1}");
            var response = await httpClient.GetAsync("https://brewupapi.ambitiousocean-9c685401.italynorth.azurecontainerapps.io/v1/brewup");
            Console.WriteLine(await response.Content.ReadAsStringAsync());
    
            Thread.Sleep(500);
    
            if (i < 29)
                Console.Clear();
        }
    }, resilienceContext);
    


