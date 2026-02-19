using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace FEFF.Extentions.Redis;

/// <summary>
/// 1. Get options for consumer connection.<br/>
/// 2. Organize task cancellation.<br/>
/// 2. Create and return connection.
/// </summary>
public class RedisProviderBase
{
    private readonly ConfigurationOptions _options;

    public RedisProviderBase(IRedisProviderOptions options)
    {
        _options = options.ConfigurationOptions.Clone();
    }

    protected async Task<IConnectionMultiplexer> ConnectAsync(TextWriter? log = null, CancellationToken cancellationToken = default)
    {
//TODO (StackExchange.Redis): cancellationToken
        var t = ConnectionMultiplexer.ConnectAsync(_options, log);
        return await t.WaitAsync(cancellationToken).ConfigureAwait(false);
    }
}

public class RedisProviderOptions<T> : IRedisProviderOptions
{
    public ConfigurationOptions ConfigurationOptions => GetConfigurationOptions();

    private readonly IOptionsFactory<RedisConfigurationOptions> _optionsFactory;

    public RedisProviderOptions(IOptionsFactory<RedisConfigurationOptions> optionsFactory)
    {
        _optionsFactory = optionsFactory;
    }

    public ConfigurationOptions GetConfigurationOptions()
    {
        var name = OptionsNameHelper.GetTypeName<T>();
        var opts = _optionsFactory.Create(name);
        return opts.ConfigurationOptions;
    }
}

public class RedisConfigurationOptions
{
    public ConfigurationOptions ConfigurationOptions { get; set; } = new();
}

// use consumer's TypeName as a key for named options
internal static class OptionsNameHelper
{
    internal static string GetTypeName(Type? optionsDiscriminator)
    {
        if(optionsDiscriminator == null)
            return Options.DefaultName; //Microsoft.Extensions.Options.Options.DefaultName = ""
        return TypeHelper.GetTypeName(optionsDiscriminator);
    }

    internal static string GetTypeName<TOptionsDiscriminator>()
    {
        return GetTypeName(typeof(TOptionsDiscriminator));
    }
}