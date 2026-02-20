// some ideas from: 
// https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/blob/master/src/HealthChecks.Redis/RedisHealthCheck.cs

using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace FEFF.Extentions.HealthChecks.Redis;

// A connection is not created and/or disposed by implementation of this interface.
public interface IRedisConnectionObservable
{
    bool IsConnectionRequested { get; }
    IConnectionMultiplexer? ActiveConnection { get; }
}

internal class RedisObservedConnectionHealthCheck<T> : IHealthCheck
where T: IRedisConnectionObservable
{
    private IRedisConnectionObservable _redis;

    public RedisObservedConnectionHealthCheck(T m)
    {
        _redis = m;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        if(_redis.IsConnectionRequested == false)
            return HealthCheckResult.Healthy("RedisConnection has not been requested yet.");

        var conn = _redis.ActiveConnection;
        if(conn == null)
            return HealthCheckResult.Unhealthy("RedisConnection is starting.");

//TODO: _redis.LastConnectionException

        return await RedisHealthHelper.TryCheckConnection(context.Registration, conn, cancellationToken);
    }
}