using Microsoft.Extensions.Options;
using StackExchange.Redis;
using StackExchange.Redis.KeyspaceIsolation;

namespace FEFF.Extentions.Redis;

/// <summary>
/// Create Prefixed Database (for testing)
/// </summary>
public class RedisDatabaseProvider<T>: IRedisDatabaseProvider
where T: RedisConnectionManager
{
    public class Options
    {
        /// <summary>
        /// For testing purposes:
        /// User can define this prefix in test-setup and prepend it to every key in redis
        ///   to avoid interference between tests. <br/>
        /// SeeAlso: 'IDatabase.WithKeyPrefix()' <br/>
        /// SeeAlso: 'ConfigurationOptions.ChannelPrefix'
        /// </summary>
        public string? KeyPrefix { get; set; }
    }

    private readonly string? _prefix;
    private readonly T _redis;

    public RedisDatabaseProvider(T redis, IOptions<Options> opts)
    {
        _redis = redis;
        _prefix = opts.Value.KeyPrefix;
    }

    /// <summary>
    /// Returns a Database using 'options.KeyPrefix' (for testing support).
    /// </summary>
    public async Task<IDatabase> GetDatabaseAsync(CancellationToken cancellationToken = default)
    {
        var c = await _redis.GetConnectionAsync(cancellationToken).ConfigureAwait(false);
        var res = c.GetDatabase();

        if (_prefix.IsNullOrEmpty())
            return res;

        // namespace for Redis keyspace DB API
        return res.WithKeyPrefix(_prefix);
    }
}