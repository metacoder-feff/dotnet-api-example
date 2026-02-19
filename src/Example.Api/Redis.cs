
using FEFF.Extentions.Redis;
using Microsoft.Extensions.Options;

namespace Example.Api;

// example to use multiple self-managed connections
internal class RedisConnectionManager2 : RedisConnectionManager
{
    public RedisConnectionManager2(IOptionsFactory<Options> factory) : base(factory)
    {
    }
}