using LoggingAndMonitoringWithDotNet.Modules;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Register Modules
builder.RegisterModules();

var app = builder.Build();

app.MapDefaultEndpoints();

app.ConfigureModules();

await app.RunAsync();