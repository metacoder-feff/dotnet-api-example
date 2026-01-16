using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using NodaTime.Serialization.SystemTextJson;
using Prometheus;

using Example.Utils;
using Utils.HealthChecks;

namespace Example.Api;

static class InfrastructureModule
{
    /// <summary>
    /// HealthStatus.Degraded: not needed for Liveness/Readiness.
    /// HealthStatus.Unhealthy: 
    /// - Asp default status '503' makes cloud balacer (yandex) to stop a traffic immediately,
    /// - while status '500' should be repeated a number of times to stop the traffic.
    /// </summary>
    private static readonly IReadOnlyDictionary<HealthStatus, int> StatusCodesMapping = new Dictionary<HealthStatus, int>
    {
        {HealthStatus.Healthy  , StatusCodes.Status200OK},
        {HealthStatus.Degraded , StatusCodes.Status200OK},
        {HealthStatus.Unhealthy, StatusCodes.Status500InternalServerError},
    };
    
    public static void SetupServices(IServiceCollection services)
    {
        services.AddStdCloudLogging();
        services.ConfigureHttpJsonOptions(o => 
            ConfigureJsonSerializer(o.SerializerOptions)
        );

        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        services.AddOpenApi();

        services.AddHealthChecks()
                .AddSimpleLivenessCheck()
                ;
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

        var healthCheckWriter = HealthCheckHelper.CreateWriter(ConfigureJsonSerializer);
        app.MapHealthChecks("/health/liveness", new HealthCheckOptions
        {
            Predicate = HealthCheckHelper.IsLiveness,
            ResponseWriter = healthCheckWriter
        });
     
        app.MapHealthChecks("/health/readiness", new HealthCheckOptions
        {
            Predicate = HealthCheckHelper.IsReadiness,
            ResponseWriter = healthCheckWriter,
            // workaround: retry on yandex if 500
            // 503 - immediately stops traffic           
            ResultStatusCodes = new Dictionary<HealthStatus, int>(StatusCodesMapping),
        });
        
        app.MapHealthChecks("/health/overal", new HealthCheckOptions
        {
            // no Predicate => all
            ResponseWriter = healthCheckWriter
        });
    }

    internal static void ConfigureJsonSerializer(JsonSerializerOptions o)
    {
        o.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        o.Converters.Add(
            new JsonStringEnumConverter(namingPolicy: JsonNamingPolicy.SnakeCaseLower)
        );
        o.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        o.AllowTrailingCommas    = true;
        o.ReadCommentHandling    = JsonCommentHandling.Skip;

        o.ConfigureForNodaTime(NodaTime.DateTimeZoneProviders.Tzdb);
    }
}
