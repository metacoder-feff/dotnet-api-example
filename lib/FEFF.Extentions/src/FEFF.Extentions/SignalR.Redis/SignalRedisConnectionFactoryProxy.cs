namespace FEFF.Extentions.SignalR.Redis;

using FEFF.Extentions.HealthChecks.Redis;
using FEFF.Extentions.Redis;

// for RedisConnectionFactoryHealthCheck<>
// use subclass to distinguish from other redis connections (e.g. distributed chache)
internal class SignalRedisConnectionFactoryProxy : RedisConnectionFactoryProxy
{
    public SignalRedisConnectionFactoryProxy(RedisProviderOptions<SignalRedisConnectionFactoryProxy> options) : base(options)
    {
    }
}