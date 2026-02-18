
using FEFF.Extentions.Redis;

namespace Example.Api;

// example to use multiple connections
internal class RedisConnectionManager2 : RedisConnectionManager
{
    public RedisConnectionManager2(RedisConnectionFactory<RedisConnectionManager2> factory) : base(factory)
    {
    }
}