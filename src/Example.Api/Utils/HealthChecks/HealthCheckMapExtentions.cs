using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Utils.HealthChecks;

// No need to add "startup" check because it dublicates "liveness" waiting for a bit more time this is configured at cloud.
public static class HealthCheckMapExtentions
{
    /// <summary>
    /// HealthStatus.Degraded: not needed for Liveness/Readiness.
    /// HealthStatus.Unhealthy: 
    /// - Asp default status '503' makes cloud balacer (yandex) to stop a traffic immediately,
    /// - while status '500' should be repeated a number of times to stop the traffic.
    /// </summary>
    private static ReadOnlyDictionary<HealthStatus, int> GetStatusCodesMapping()
    {
        return new Dictionary<HealthStatus, int>
        {
            {HealthStatus.Healthy  , StatusCodes.Status200OK},
            {HealthStatus.Degraded , StatusCodes.Status200OK},
            {HealthStatus.Unhealthy, StatusCodes.Status500InternalServerError},
        }
        .AsReadOnly();
    }

    /// <summary>
    /// Add standard healthchecks:
    /// <list type="bullet">
    ///     <item>
    ///         <description>"/health/liveness" - Only health checks tagged with the "Liveness" tag must pass for app to be considered alive.</description>
    ///     </item>
    ///     <item>
    ///         <description>"/health/readiness" - Only health checks tagged with the "Readiness" tag must pass for app to be considered ready to accept traffic after starting.</description>
    ///     </item>
    ///     <item>
    ///         <description>"/health/overal" - All health checks are exported for detaliled diagnostics.</description>
    ///     </item>
    /// </list>
    /// Unhealthy status returns "HTTP-500". Json format is contolled by "services.ConfigureHttpJsonOptions".
    /// </summary>
    public static void MapStdHealthChecks(this IEndpointRouteBuilder app)
    {
        var mapping = GetStatusCodesMapping();

        app.MapHealthChecks("/health/liveness", new HealthCheckOptions
        {
            Predicate = IsLiveness,
            ResponseWriter = WriteAsync,
            ResultStatusCodes = mapping,
        });

        app.MapHealthChecks("/health/readiness", new HealthCheckOptions
        {
            Predicate = IsReadiness,
            ResponseWriter = WriteAsync,        
            ResultStatusCodes = mapping,
        });

        app.MapHealthChecks("/health/overal", new HealthCheckOptions
        {
            // no Predicate => all
            ResponseWriter = WriteAsync,
            ResultStatusCodes = mapping,
        });
    }

    private static bool IsLiveness(HealthCheckRegistration check)
        => check.Tags.Contains(HealthCheckTag.Liveness);

    private static bool IsReadiness(HealthCheckRegistration check)
        => check.Tags.Contains(HealthCheckTag.Readiness);

    private static async Task WriteAsync(HttpContext context, HealthReport report)
    {
        var obj = new
        {
            Status   = report.Status,
            Duration = report.TotalDuration,
            Checks = report.Entries
                    .Select(e =>
                        new
                        {
                            Name        = e.Key,
                            Description = e.Value.Description,
                            Duration    = e.Value.Duration,
                            Status      = e.Value.Status,
                            Error       = e.Value.Exception?.Message,
                            Data        = e.Value.Data,
                            //Tags = e.Value.Tags,
                        }
                    )
                    .ToList()
        };

        var cancellationToken = context.RequestAborted;

        // uses 'SerializerOptions' from 'services.ConfigureHttpJsonOptions'
        // same as MinimalApi
        await context.Response.WriteAsJsonAsync(obj, cancellationToken);
    }
}