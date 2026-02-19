using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace FEFF.Extentions.Redis;

//TODO (SRE): split connector/optionsfactory

/// <summary>
/// 1. Get options for consumer connection.<br/>
/// 2. Organize task cancellation.<br/>
/// 2. Create and return connection.
/// </summary>
public class RedisProviderBase
{
    private IOptionsFactory<Options> _factory;

    public RedisProviderBase(IOptionsFactory<Options> factory)
    {
        _factory = factory;
    }

    protected async Task<IConnectionMultiplexer> ConnectAsync(TextWriter? log = null, CancellationToken cancellationToken = default)
    {
        var optionsDiscriminator = GetType();
        var name = GetTypeName(optionsDiscriminator);
        var opts = _factory.Create(name);
        // thread-safe guard
        var config = opts.ConfigurationOptions.Clone();

//TODO (StackExchange.Redis): cancellationToken
        var t = ConnectionMultiplexer.ConnectAsync(config, log);
        return await t.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    // use consumer's TypeName as a key for named options
    internal static string GetTypeName(Type? optionsDiscriminator)
    {
        if(optionsDiscriminator == null)
            return Microsoft.Extensions.Options.Options.DefaultName;
        return TypeHelper.GetTypeName(optionsDiscriminator);
    }

    // use consumer's TypeName as a key for named options
    internal static string GetTypeName<TOptionsDiscriminator>() where TOptionsDiscriminator : class
    {
        return GetTypeName(typeof(TOptionsDiscriminator));
    }

    public class Options
    {
        public ConfigurationOptions ConfigurationOptions { get; set; } = new();
    }
}