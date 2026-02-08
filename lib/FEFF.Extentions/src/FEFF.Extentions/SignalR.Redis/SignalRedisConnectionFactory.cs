using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace FEFF.Extentions.SignalR.Redis;
using FEFF.Extentions.Redis;

// for RedisConnectionFactoryHealthCheck<>
internal class SignalRedisConnectionFactory : RedisConnectionFactoryProxy
{
    public SignalRedisConnectionFactory(IOptions<ConfigurationOptions> opt) : base(opt)
    {
    }
}