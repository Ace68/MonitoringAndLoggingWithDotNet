using System.Diagnostics.Metrics;
using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using OpenTelemetry.Metrics;
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
            .AddResilienceHandler("orders-pipeline", (ResiliencePipelineBuilder<HttpResponseMessage> pipelineBuilder) => 
            {
                // Start with configuring standard resilience strategies
                pipelineBuilder
                    .ConfigureTelemetry(LoggerFactory.Create(bld => bld.AddOpenTelemetry().AddConsole()))
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

                // Next, configure chaos strategies to introduce controlled disruptions.
                // Place these after the standard resilience strategies.
                
                // Inject chaos into 20% of invocations
                const double InjectionRate = 0.2;
                
                pipelineBuilder
                    .AddChaosLatency(InjectionRate, TimeSpan.FromMinutes(1)); // Introduce a delay as chaos latency
                    // .AddChaosFault(InjectionRate, () => new InvalidOperationException("Chaos strategy injection!")) // Introduce a fault as chaos
                    // .AddChaosOutcome(InjectionRate, () => new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)); // Simulate an outcome as chaos
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
                    .ConfigureTelemetry(LoggerFactory.Create(bld => bld.AddOpenTelemetry().AddConsole()))
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
                    .AddChaosFault(InjectionRate, () => new InvalidOperationException("Chaos strategy injection!")) // Introduce a fault as chaos
                    .AddChaosOutcome(InjectionRate, () => new HttpResponseMessage(HttpStatusCode.InternalServerError)); // Simulate an outcome as chaos
            });
            
            builder.Services.AddOpenTelemetry()
                // Add Metrics for ASP.NET Core and our custom metrics and export to Prometheus
                .WithMetrics(meterProviderBuilder =>
                {
                    // Metrics provider from OpenTelemetry
                    meterProviderBuilder.AddAspNetCoreInstrumentation();
                    meterProviderBuilder.AddMeter("Polly");
                    meterProviderBuilder.AddMeter();
                    // Metrics provides by ASP.NET Core in .NET 8
                    meterProviderBuilder.AddMeter("Microsoft.AspNetCore.Hosting");
                    meterProviderBuilder.AddMeter("Microsoft.AspNetCore.Server.Kestrel");
                    meterProviderBuilder.AddPrometheusExporter();
                });
		
        return builder.Services;
    }

    public WebApplication Configure(WebApplication app)
    {
        app.MapPrometheusScrapingEndpoint();
        
        return app;
    }
}