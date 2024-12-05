using Azure.Monitor.OpenTelemetry.AspNetCore;
using OpenTelemetry.Instrumentation.Runtime;
using OpenTelemetry.Metrics;

namespace LoggingAndMonitoringWithDotNet.Modules;

public class MetricsModule : IModule
{
    public bool IsEnabled => true;
    public int Order => 0;
    
    public IServiceCollection Register(WebApplicationBuilder builder)
    {
        builder.Services.Configure<Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration>(config =>
        {
            config.SetAzureTokenCredential(new DefaultAzureCredential());
        });

        builder.Services.AddApplicationInsightsTelemetry(new Microsoft.ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions
        {
            ConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]
        });
        
        // Configure the OpenTelemetry meter provider to add runtime instrumentation.
        builder.Services.ConfigureOpenTelemetryMeterProvider((_, provider) =>  provider.AddRuntimeInstrumentation()); 

        builder.Services.AddOpenTelemetry()
            // Configure OpenTelemetry to use Azure Monitor.
            .UseAzureMonitor()
            // Add Metrics for ASP.NET Core and our custom metrics and export to Prometheus
            .WithMetrics(meterProviderBuilder =>
            {
                // Metrics provider from OpenTelemetry
                meterProviderBuilder.AddAspNetCoreInstrumentation();
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