namespace Microsoft.Extensions.DependencyInjection;

using FEFF.Extentions.HealthChecks.Redis;
using FEFF.Extentions.Redis;

public static class RedisHealthchecksExtentions
{
    public static IHealthChecksBuilder AddRedisConnectionHealthCheck<T>(
        this IHealthChecksBuilder builder,
        string? name = null,
        IEnumerable<string>? tags = null,
        TimeSpan? timeout = null)
    where T: RedisConnectionManager
    {
        ArgumentNullException.ThrowIfNull(builder);
        
        name ??= TypeHelper.GetTypeName<T>();

        return builder.AddCheck<RedisConnectionManagerHealthCheck<T>>(name, null, tags, timeout);
    }
}