using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace FEFF.Extentions.Redis;

public class RedisConnectionFactory : IRedisConnectionFactory
{
    private readonly ConfigurationOptions _options;

    public RedisConnectionFactory(Options o)
    {
        _options = o.ConfigurationOptions.Clone();
    }

    public RedisConnectionFactory(IOptions<Options> o) : this(o.Value)
    {
    }

//TODO (StackExchange.Redis): cancellationToken
    public async Task<IConnectionMultiplexer> ConnectAsync(TextWriter? log = null)
    {
        return await ConnectionMultiplexer.ConnectAsync(_options, log).ConfigureAwait(false);
    }

    public class Options
    {
        public ConfigurationOptions ConfigurationOptions { get; set; } = new();
    }
}

public class RedisConnectionFactory<TDiscriminator> : RedisConnectionFactory
where TDiscriminator : class
{
    public RedisConnectionFactory(IOptionsFactory<Options> factory) : base(GetOptions(factory))
    {
    }

    private static Options GetOptions(IOptionsFactory<Options> factory)
    {
        var name = TypeHelper.GetTypeName<TDiscriminator>();
        return factory.Create(name);
    }
}