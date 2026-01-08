namespace Microsoft.Extensions.DependencyInjection;

using FEFF.Extensions.HealthChecks.Redis;
using FEFF.Extensions.Redis;

public static class RedisHealthchecksExtensions
{
    public static IHealthChecksBuilder AddRedisConnectionCheck<T>(
        this IHealthChecksBuilder builder,
        string? name = null,
        IEnumerable<string>? tags = null,
        TimeSpan? timeout = null
        )
        where T: IRedisConnectionProvider
    {
        ArgumentNullException.ThrowIfNull(builder);
        
        name ??= TypeHelper.GetTypeName<T>();

        return builder.AddCheck<RedisManagedConnectionHealthCheck<T>>(name, null, tags, timeout);
    }
}