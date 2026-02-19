using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace FEFF.Extentions.Redis;

// DI: Named options -> Typed options
public class RedisProviderOptions<TRedisProvider> : IRedisProviderOptions
where TRedisProvider : RedisProviderBase
{
    public ConfigurationOptions ConfigurationOptions => GetConfigurationOptions();

    private readonly IOptionsFactory<RedisConfigurationOptions> _optionsFactory;

    public RedisProviderOptions(IOptionsFactory<RedisConfigurationOptions> optionsFactory)
    {
        _optionsFactory = optionsFactory;
    }

    public ConfigurationOptions GetConfigurationOptions()
    {
        var name = TypeHelper.GetTypeName<TRedisProvider>();
        var opts = _optionsFactory.Create(name);
        return opts.ConfigurationOptions;
    }
}

// DI: Named options
public class RedisConfigurationOptions
{
    public ConfigurationOptions ConfigurationOptions { get; set; } = new();
}