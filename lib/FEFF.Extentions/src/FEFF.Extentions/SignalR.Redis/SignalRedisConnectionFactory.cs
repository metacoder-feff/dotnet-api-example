using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace FEFF.Extentions.SignalR.Redis;
using FEFF.Extentions.HealthChecks.Redis;

// for RedisConnectionFactoryHealthCheck<>
public class SignalRedisConnectionFactory : RedisConnectionFactoryProxy
{
    public SignalRedisConnectionFactory(IOptions<ConfigurationOptions> opt) : base(opt)
    {
    }
}