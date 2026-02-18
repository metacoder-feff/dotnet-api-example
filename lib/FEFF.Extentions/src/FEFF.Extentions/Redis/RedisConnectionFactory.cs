using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace FEFF.Extentions.Redis;

public class RedisConnectionFactory
{
    private IOptionsFactory<Options> _factory;

    public RedisConnectionFactory(IOptionsFactory<Options> factory)
    {
        _factory = factory;
    }

//TODO (StackExchange.Redis): cancellationToken
    public async Task<IConnectionMultiplexer> ConnectAsync(Type? optionsDiscriminator = null, TextWriter? log = null)
    {
        var name = GetTypeName(optionsDiscriminator);
        var opts = _factory.Create(name);
        // thread-safe guard
        var config = opts.ConfigurationOptions.Clone();

        return await ConnectionMultiplexer.ConnectAsync(config, log).ConfigureAwait(false);
    }

    // use consumer's TypeName as a key for named options
    internal static string GetTypeName(Type? t)
    {
        if(t == null)
            return Microsoft.Extensions.Options.Options.DefaultName;
        return TypeHelper.GetTypeName(t);
    }

    // use consumer's TypeName as a key for named options
    internal static string GetTypeName<TDiscriminator>() where TDiscriminator : class
    {
        return GetTypeName(typeof(TDiscriminator));
    }

    public class Options
    {
        public ConfigurationOptions ConfigurationOptions { get; set; } = new();
    }
}