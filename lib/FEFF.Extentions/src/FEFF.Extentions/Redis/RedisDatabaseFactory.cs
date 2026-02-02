using Microsoft.Extensions.Options;
using StackExchange.Redis;
using StackExchange.Redis.KeyspaceIsolation;

namespace FEFF.Extentions.Redis;

/// <summary>
/// Create Prefixed Database (for testing)
/// </summary>
//TODO: interface
public partial class RedisDatabaseFactory
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
    private readonly RedisConnectionFactory _redis;

    public RedisDatabaseFactory(RedisConnectionFactory redis, IOptions<Options> opts)
    {
        _prefix = opts.Value.KeyPrefix;
        _redis = redis;
    }

    /// <summary>
    /// Returns a Database using 'options.KeyPrefix' (for test support).
    /// </summary>
    public async Task<IDatabase> GetDatabaseAsync(TextWriter? log = null, CancellationToken cancellationToken = default)
    {
        var c = await _redis.GetConnectionAsync(log, cancellationToken).ConfigureAwait(false);
        var res = c.GetDatabase();

        if (_prefix.IsNullOrEmpty())
            return res;

        // namespace for Redis keyspace DB API
        return res.WithKeyPrefix(_prefix);
    }
}