using Azure.Identity;

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

    return builder.Services;
  }

  public WebApplication Configure(WebApplication app)
  {

    return app;
  }
}