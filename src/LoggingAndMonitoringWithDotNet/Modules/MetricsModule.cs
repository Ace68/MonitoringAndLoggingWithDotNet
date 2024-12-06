using System.Net;
using LoggingAndMonitoringWithDotNet.Endpoints;
using Microsoft.Extensions.Http.Resilience;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Simmy;

namespace LoggingAndMonitoringWithDotNet.Modules;

public class MetricsModule : IModule
{
    public bool IsEnabled => true;
    public int Order => 0;
    
    public IServiceCollection Register(WebApplicationBuilder builder)
    {
        const string key = "brewup-chaos-telemetry";
        const string httpOrdersClientName = "BrewUpOrders";
        const string httpBeersClientName = "BrewUpBeers";
        
        builder.Services.AddHttpClient(httpOrdersClientName, configure =>
            {
                configure.BaseAddress = new Uri(builder.Configuration.GetValue<string>("BrewUpBaseUri") 
                                                ?? throw new InvalidOperationException());
                configure.DefaultRequestHeaders.Add("accept", "application/json");
            })
            .AddResilienceHandler("orders-pipeline", (ResiliencePipelineBuilder<HttpResponseMessage> builder) => 
            {
                // Start with configuring standard resilience strategies
                builder
                    .AddConcurrencyLimiter(10, 100)
                    .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
                    {
                        MaxRetryAttempts = 4,
                        BackoffType = DelayBackoffType.Exponential,
                        UseJitter = true,
                        Delay = TimeSpan.FromSeconds(3),
                        MaxDelay = TimeSpan.FromSeconds(5)
                    });
                // .AddTimeout(TimeSpan.FromSeconds(5));

                // // Next, configure chaos strategies to introduce controlled disruptions.
                // // Place these after the standard resilience strategies.
                //
                // // Inject chaos into 2% of invocations
                // const double InjectionRate = 0.02;
                //
                // builder
                //     .AddChaosLatency(InjectionRate, TimeSpan.FromMinutes(1)) // Introduce a delay as chaos latency
                //     .AddChaosFault(InjectionRate, () => new InvalidOperationException("Chaos strategy injection!")) // Introduce a fault as chaos
                //     .AddChaosOutcome(InjectionRate, () => new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)) // Simulate an outcome as chaos
                //     .AddChaosBehavior(0.001, cancellationToken => RestartRedisAsync(cancellationToken)); // Introduce a specific behavior as chaos            
            });

            builder.Services.AddHttpClient(httpBeersClientName, configure =>
            {
                configure.BaseAddress = new Uri(builder.Configuration.GetValue<string>("BrewUpBaseUri") 
                                                ?? throw new InvalidOperationException());
                configure.DefaultRequestHeaders.Add("accept", "application/json");
            })
            .AddResilienceHandler("beers-pipeline", (ResiliencePipelineBuilder<HttpResponseMessage> configure) => 
            {
                // Start with configuring standard resilience strategies
                configure
                    .AddConcurrencyLimiter(10, 100)
                    .AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
                    {
                        FailureRatio = 0.5,
                        MinimumThroughput = 8,
                        SamplingDuration = TimeSpan.FromSeconds(10),
                        BreakDurationGenerator = static args => new ValueTask<TimeSpan>(TimeSpan.FromMinutes(args.FailureCount))
                    });
                // .AddTimeout(TimeSpan.FromSeconds(5));

                // Next, configure chaos strategies to introduce controlled disruptions.
                // Place these after the resilience strategies.
                
                // Inject chaos into 2% of invocations
                const double InjectionRate = 0.02;
                
                configure
                    .AddChaosLatency(InjectionRate, TimeSpan.FromMinutes(1)) // Introduce a delay as chaos latency
                    .AddChaosFault(InjectionRate, () => new InvalidOperationException("Chaos strategy injection!")) // Introduce a fault as chaos
                    .AddChaosOutcome(InjectionRate, () => new HttpResponseMessage(HttpStatusCode.InternalServerError)); // Simulate an outcome as chaos
            });
        
        builder.Services.AddOpenTelemetry().WithMetrics(opts => opts
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Pollyv8.WebApi"))
            .AddMeter("Polly")
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(builder.Configuration.GetValue<string>("OtlpEndpointUri") 
                                           ?? throw new InvalidOperationException());
            }));
		
        return builder.Services;
    }

    public WebApplication Configure(WebApplication app) => app;
}