// some ideas from: 
// https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/blob/master/src/HealthChecks.Redis/RedisHealthCheck.cs

using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace FEFF.Extentions.HealthChecks.Redis;

public interface IRedisHealthConnectionProvider
{
    bool IsConnectionRequested { get; }
    IConnectionMultiplexer? ActiveConnection { get; }
}

//TODO: IRedisHealthConnectionProvider
internal class RedisProviderProxyHealthCheck<T> : IHealthCheck
where T: IRedisHealthConnectionProvider
{
    private IRedisHealthConnectionProvider _redis;

    public RedisProviderProxyHealthCheck(T m)
    {
        _redis = m;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        if(_redis.IsConnectionRequested == false)
            return HealthCheckResult.Healthy("RedisConnectionFactory has not been requested yet.");

        var conn = _redis.ActiveConnection;
        if(conn == null)
            return HealthCheckResult.Unhealthy("RedisConnectionFactory is starting a connection.");

//TODO: _redis.LastConnectionException

        return await RedisHealthHelper.TryCheckConnection(context.Registration, conn, cancellationToken);
    }
}