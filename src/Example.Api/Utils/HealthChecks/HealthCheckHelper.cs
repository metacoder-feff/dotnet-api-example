using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Utils.HealthChecks;

public static class HealthCheckHelper
{
    public static Func<HttpContext, HealthReport, Task> CreateWriter(Action<JsonSerializerOptions> cfgAction)
    {
        var writer = new JsonHealthCheckWriter(cfgAction);
        return writer.WriteAsync;
    }

    public static bool IsLiveness(HealthCheckRegistration check)
        => check.Tags.Contains(HealthCheckTag.Liveness);

    public static bool IsReadiness(HealthCheckRegistration check)
        => check.Tags.Contains(HealthCheckTag.Readiness);

    public static IHealthChecksBuilder AddLivenessCheck<T>(this IHealthChecksBuilder builder)
    where T : class, IHealthCheck
    {
        return builder.AddCheck<T>(typeof(T).Name, tags: [HealthCheckTag.Liveness]);
    }

    public static IHealthChecksBuilder AddSimpleLivenessCheck(this IHealthChecksBuilder builder)
    {
        return builder.AddLivenessCheck<SimpleHealthCheck>();
    }
}