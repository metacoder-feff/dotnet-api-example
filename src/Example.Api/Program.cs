using Prometheus;

using Example.Api;

var builder = WebApplication.CreateBuilder(args);
//TODO: setup logger

//TODO: log errors?
InfrastructureModule.SetupServices(builder.Services);
ExampleApiModule.SetupServices(builder.Services);

var app = builder.Build();
//TODO: log errors!

InfrastructureModule.SetupPipeline(app);
ExampleApiModule.SetupPipeline(app);


// Enable the /metrics page to export Prometheus metrics.
// Metrics published in this sample:
// * built-in process metrics giving basic information about the .NET runtime (enabled by default)
// * metrics from .NET Event Counters (enabled by default, updated every 10 seconds)
// * metrics from .NET Meters (enabled by default)
// ref: https://github.com/prometheus-net/prometheus-net/blob/master/Sample.Web/Program.cs
app.MapMetrics();

app.Run();