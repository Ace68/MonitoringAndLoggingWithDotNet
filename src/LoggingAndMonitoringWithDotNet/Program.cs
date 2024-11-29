using LoggingAndMonitoringWithDotNet.Modules;

var builder = WebApplication.CreateBuilder(args);

// Register Modules
builder.RegisterModules();

var app = builder.Build();

app.ConfigureModules();

await app.RunAsync();