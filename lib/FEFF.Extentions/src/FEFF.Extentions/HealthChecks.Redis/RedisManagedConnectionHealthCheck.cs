// some ideas from: 
// https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/blob/master/src/HealthChecks.Redis/RedisHealthCheck.cs

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FEFF.Extentions.HealthChecks.Redis;
using FEFF.Extentions.Redis;

internal class RedisManagedConnectionHealthCheck<T> : IHealthCheck
where T: IRedisConnectionProvider
{
    private T _redis;

    public RedisManagedConnectionHealthCheck(T m)
    {
        _redis = m;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        try
        {
            var conn = await _redis.GetConnectionAsync(cancellationToken);

            return await RedisHealthHelper.TryCheckConnection(context.Registration, conn, cancellationToken);
        }
        catch (Exception ex)
        {
                return new HealthCheckResult(context.Registration.FailureStatus, "Redis HealthCheck exception.", ex);
        }
    }
}