namespace FEFF.Extentions.SignalR.Redis;

using FEFF.Extentions.HealthChecks.Redis;
using FEFF.Extentions.Redis;

// for RedisConnectionFactoryHealthCheck<>
// use subclass to distinguish from other redis connections (e.g. distributed chache)
internal class SignalRedisProviderProxy : RedisProviderProxy
{
    public SignalRedisProviderProxy(RedisProviderOptions<SignalRedisProviderProxy> options) : base(options)
    {
    }
}