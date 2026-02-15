using StackExchange.Redis;

namespace FEFF.Extentions.Redis;

partial class RedisConnectionManager
{
    public class Options
    {
        public ConfigurationOptions ConfigurationOptions { get; set; } = new();

        /// <summary>
        /// For testing purposes:
        /// User can define this prefix in test-setup and prepend it to every key in redis
        ///   to avoid interference between tests.
        /// </summary>
        public string? KeyPrefix { get; set; }
    }
}