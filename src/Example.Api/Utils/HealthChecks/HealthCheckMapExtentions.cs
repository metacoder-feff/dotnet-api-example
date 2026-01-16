using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Utils.HealthChecks;

public static class HealthCheckMapExtentions
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

    public static bool IsLiveness(HealthCheckRegistration check)
        => check.Tags.Contains(HealthCheckTag.Liveness);

    public static bool IsReadiness(HealthCheckRegistration check)
        => check.Tags.Contains(HealthCheckTag.Readiness);

    public static void MapStdHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health/liveness", new HealthCheckOptions
        {
            Predicate = IsLiveness,
            ResponseWriter = WriteAsync,
            ResultStatusCodes = new Dictionary<HealthStatus, int>(StatusCodesMapping),
        });

        app.MapHealthChecks("/health/readiness", new HealthCheckOptions
        {
            Predicate = IsReadiness,
            ResponseWriter = WriteAsync,        
            ResultStatusCodes = new Dictionary<HealthStatus, int>(StatusCodesMapping),
        });

        app.MapHealthChecks("/health/overal", new HealthCheckOptions
        {
            // no Predicate => all
            ResponseWriter = WriteAsync,
            ResultStatusCodes = new Dictionary<HealthStatus, int>(StatusCodesMapping),
        });
    }

    private static async Task WriteAsync(HttpContext context, HealthReport report)
    {
        var obj = new
        {
            Status   = report.Status,
            Duration = report.TotalDuration,
            // check = report.Entries.Value+name(==report.Entries.Key)
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

// TODO: CancellationToken ??

        // uses 'SerializerOptions' from 'services.ConfigureHttpJsonOptions'
        // same as MinimalApi
        await context.Response.WriteAsJsonAsync(obj);
    }
}