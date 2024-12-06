using Polly.Retry;
using Polly.Telemetry;

namespace BrewUp.ChaosAndTelemetry;

internal class BrewUpMeteringEnricher : MeteringEnricher
{
    public override void Enrich<TResult, TArgs>(in EnrichmentContext<TResult, TArgs> context)
    {
        context.Tags.Add(new("brewup-custom-tag", "brewup-custom-value"));
        
        // You can read additional details from any resilience event and use it to enrich the telemetry
        if (context.TelemetryEvent.Arguments is OnRetryArguments<TResult> retryArgs)
        {
            // See https://github.com/open-telemetry/semantic-conventions/blob/main/docs/general/metrics.md for more details
            // on how to name the tags.
            context.Tags.Add(new("retry.attempt", retryArgs.AttemptNumber));
        }
    }
}