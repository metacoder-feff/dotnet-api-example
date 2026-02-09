using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace FEFF.Extentions.SignalR.Redis;
using FEFF.Extentions.HealthChecks.Redis;

// for RedisConnectionFactoryHealthCheck<>
// use subclass to distinguish from other redis connections (e.g. distributed chache)
internal class SignalRedisConnectionFactoryProxy : RedisConnectionFactoryProxy
{
    public SignalRedisConnectionFactoryProxy(IOptions<ConfigurationOptions> opt) : base(opt)
    {
    }
}