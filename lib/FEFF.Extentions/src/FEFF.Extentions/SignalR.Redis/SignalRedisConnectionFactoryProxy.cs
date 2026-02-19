namespace FEFF.Extentions.SignalR.Redis;

using FEFF.Extentions.HealthChecks.Redis;
using Microsoft.Extensions.Options;

// for RedisConnectionFactoryHealthCheck<>
// use subclass to distinguish from other redis connections (e.g. distributed chache)
internal class SignalRedisConnectionFactoryProxy : RedisConnectionFactoryProxy
{
    public SignalRedisConnectionFactoryProxy(IOptionsFactory<Options> factory) : base(factory)
    {
    }
}