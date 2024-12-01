using LoggingAndMonitoringWithDotNet.Modules;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Register Modules
builder.RegisterModules();
builder.Services.Configure<Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration>(config =>
{
config.SetAzureTokenCredential(new DefaultAzureCredential());
});

builder.Services.AddApplicationInsightsTelemetry(new Microsoft.ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions
{
  ConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]
});

var app = builder.Build();

app.MapDefaultEndpoints();

app.ConfigureModules();

await app.RunAsync();