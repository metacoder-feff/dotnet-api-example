using StackExchange.Redis;

namespace FEFF.Extentions.Redis;

partial class RedisConnectrionManager
{
    /// <summary>
    /// https://redis.io/docs/latest/develop/connect/clients/dotnet/
    /// https://stackexchange.github.io/StackExchange.Redis/
    /// ConfigurationString examples:
    /// 1. This will connect to a single server on the local machine using the default redis port (6379). 
    ///     "localhost"
    ///
    /// 2. Additional options are simply appended (comma-delimited). Ports are represented with a colon (:) as is usual.
    /// Configuration options include an = after the name. For example:
    ///     "redis0:6380,redis1:6380,allowAdmin=true"
    ///
    /// 3. If you specify a serviceName in the connection string, it will trigger sentinel mode.
    /// This example will connect to a sentinel server on the local machine using the default sentinel port (26379), 
    /// discover the current primary server for the myprimary service and return a managed connection pointing to that 
    /// primary server that will automatically be updated if the primary changes:
    ///     "localhost,serviceName=myprimary"
    ///
    ///  4.
    ///     $"{HOST_NAME}:{PORT_NUMBER},password={PASSWORD}"
    /// </summary>


    public class Options
    {
        /// <summary>
        /// For testing purposes:
        /// User can define this prefix in test-setup and prepend it to every key in redis
        ///   to avoid interference between tests.
        /// SeeAlso: 'ConfigurationOptions.ChannelPrefix'
        /// </summary>
        public string? KeyPrefix { get; set; }

        public ConfigurationOptions ConfigurationOptions { get; set; } = new();
    }
}