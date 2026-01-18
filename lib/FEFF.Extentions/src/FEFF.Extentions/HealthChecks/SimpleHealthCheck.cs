
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FEFF.Extentions.HealthChecks;

public class SimpleHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        return Task.FromResult(
                HealthCheckResult.Healthy("AspNet is alive.")
        );
    }
}