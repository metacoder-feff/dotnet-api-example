using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace FEFF.Extentions.Redis;

public class RedisConnectionFactory /*: IRedisConnectionFactory*/
{
//TODO: 'TextWriter? log' from options?
    private readonly ConfigurationOptions _options;

    public RedisConnectionFactory(IOptions<Options> o)
    {
        _options = o.Value.ConfigurationOptions.Clone();
    }

//TODO (StackExchange.Redis): cancellationToken
    public async Task<ConnectionMultiplexer> ConnectAsync(TextWriter? log = null)
    {
        return await ConnectionMultiplexer.ConnectAsync(_options, log).ConfigureAwait(false);
    }

    public class Options
    {
        public ConfigurationOptions ConfigurationOptions { get; set; } = new();
    }
}

