var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.LoggingAndMonitoringWithDotNet>("loggingandmonitoringwithdotnet");

builder.Build().Run();
