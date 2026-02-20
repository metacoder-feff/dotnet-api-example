namespace FEFF.Extentions.SignalR.Redis;

using FEFF.Extentions.HealthChecks.Redis;
using FEFF.Extentions.Redis;

/// <summary>
/// For <see cref="RedisObservedConnectionHealthCheck&lt;&gt;"/>.<br/>
/// Use subclass to distinguish from other redis connections (e.g. distributed chache)
/// </summary>
internal class SignalRedisProviderProxy : RedisProviderProxy
{
    public SignalRedisProviderProxy(RedisProviderOptions<SignalRedisProviderProxy> options) : base(options)
    {
    }
}