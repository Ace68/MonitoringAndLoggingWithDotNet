using Polly.Telemetry;

namespace BrewUp.ChaosAndTelemetry;

internal class BrewUpTelemetryListener : TelemetryListener
{
    public override void Write<TResult, TArgs>(in TelemetryEventArguments<TResult, TArgs> args)
    {
        Console.WriteLine($"Telemetry event occurred: {args.Event.EventName}");
    }
}