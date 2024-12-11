namespace LoggingAndMonitoringWithDotNet.Modules;

public class MetricsModule : IModule
{
  public bool IsEnabled => true;
  public int Order => 0;

  public IServiceCollection Register(WebApplicationBuilder builder)
  {

    return builder.Services;
  }

  public WebApplication Configure(WebApplication app)
  {
    return app;
  }
}