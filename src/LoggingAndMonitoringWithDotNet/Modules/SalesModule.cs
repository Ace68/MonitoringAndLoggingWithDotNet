using LoggingAndMonitoringWithDotNet.Endpoints;

namespace LoggingAndMonitoringWithDotNet.Modules;

public class SalesModule : IModule
{
    public bool IsEnabled => true;
    public int Order => 0;
    
    public IServiceCollection Register(WebApplicationBuilder builder) => builder.Services;

    public WebApplication Configure(WebApplication app)
    {
        app.MapBrewUpEndpoints();

        return app;
    }
}