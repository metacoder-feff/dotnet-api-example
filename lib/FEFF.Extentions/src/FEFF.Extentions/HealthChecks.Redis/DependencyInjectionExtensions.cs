namespace Microsoft.Extensions.DependencyInjection;

using FEFF.Extentions.HealthChecks.Redis;

public static class RedisHealthchecksExtentions
{
    public static IHealthChecksBuilder AddRedisConnectionManagerHealthCheck(
        this IHealthChecksBuilder builder,
        string name,
        IEnumerable<string>? tags = null,
        TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(name);

        return builder.AddCheck<RedisConnectionManagerHealthCheck>(name, null, tags, timeout);
    }
}