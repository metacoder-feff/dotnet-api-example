using StackExchange.Redis;

namespace FEFF.Extentions.Redis;

//TODO: Move KeyPrefix to Redis
//TODO: Then use only ConfigurationOptions
partial class RedisConnectionManager
{
    /// <summary>
    /// Extend standard 'Redis.ConfigurationOptions'
    /// </summary>
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

        public ConfigurationOptions ConfigurationOptions { get; set; } = new();
    }
}