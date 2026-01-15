
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Utils.HealthChecks;

public class SimpleHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        return Task.FromResult(
                HealthCheckResult.Healthy("AspNet is alive.")
        );
    }
}