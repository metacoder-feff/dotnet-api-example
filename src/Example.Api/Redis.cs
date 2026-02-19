
using FEFF.Extentions.Redis;

namespace Example.Api;

// example to use multiple self-managed connections
internal class RedisConnectionManager2 : RedisConnectionManager
{
    public RedisConnectionManager2(RedisProviderOptions<RedisConnectionManager2> options) : base(options)
    {
    }
}