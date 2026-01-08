using StackExchange.Redis;

namespace FEFF.Extensions.Redis;

/// <summary>
/// 1. Get redis options.<br/>
/// 2. Use ConnectAsync cancellation extension.
/// </summary>
public class RedisProviderBase
{
    private readonly ConfigurationOptions _options;

    public RedisProviderBase(IRedisProviderOptions options)
    {
        _options = options.ConfigurationOptions.Clone();
    }

    protected async Task<ConnectionMultiplexer> ConnectAsync(TextWriter? log = null, CancellationToken cancellationToken = default)
    {
        return await RedisExtensions.ConnectAsync(_options, log, cancellationToken);
    }
}