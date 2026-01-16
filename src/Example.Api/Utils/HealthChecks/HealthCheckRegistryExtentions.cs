using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Utils.HealthChecks;

public static class HealthCheckRegistryExtentions
{
//TODO: Readiness

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