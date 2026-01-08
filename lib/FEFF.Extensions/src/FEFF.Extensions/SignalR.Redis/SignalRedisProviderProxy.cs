namespace FEFF.Extensions.SignalR.Redis;

using FEFF.Extensions.HealthChecks.Redis;
using FEFF.Extensions.Redis;

/// <summary>
/// For <see cref="RedisObservedConnectionHealthCheck&lt;&gt;"/>.<br/>
/// Use subclass to distinguish from other redis connections (e.g. distributed chache)
/// </summary>
public class SignalRedisProviderProxy : RedisProviderProxy
{
    public SignalRedisProviderProxy(RedisProviderOptions<SignalRedisProviderProxy> options) : base(options)
    {
    }
}