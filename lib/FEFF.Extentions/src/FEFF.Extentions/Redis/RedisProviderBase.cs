using StackExchange.Redis;

namespace FEFF.Extentions.Redis;

/// <summary>
/// 1. Get redis options.<br/>
/// 2. Organize task cancellation.<br/>
/// 3. Create and return connection.
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