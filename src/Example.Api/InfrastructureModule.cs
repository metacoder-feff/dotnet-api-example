using Example.Utils;
using Prometheus;

namespace Example.Api;

static class InfrastructureModule
{
    public static void SetupServices(IServiceCollection services)
    {
        services.AddStdCloudLogging();

        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        services.AddOpenApi();
    }

    public static void SetupPipeline(WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "v1");
            });
        }

        app.UseHttpsRedirection();

        // Enable the /metrics page to export Prometheus metrics.
        // Metrics published in this sample:
        // * built-in process metrics giving basic information about the .NET runtime (enabled by default)
        // * metrics from .NET Event Counters (enabled by default, updated every 10 seconds)
        // * metrics from .NET Meters (enabled by default)
        // ref: https://github.com/prometheus-net/prometheus-net/blob/master/Sample.Web/Program.cs
        app.MapMetrics();
    }
}
