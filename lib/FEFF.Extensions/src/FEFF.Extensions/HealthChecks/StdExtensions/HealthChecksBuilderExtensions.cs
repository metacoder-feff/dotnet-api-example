using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

using FEFF.Extensions.HealthChecks;

public static class HealthChecksBuilderExtensions
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