using Microsoft.Extensions.DependencyInjection.Extensions;

namespace StackExchange.Redis.KeyspaceIsolation;

public static class DatabaseServiceInjectionExtentions
{
    public static IServiceCollection AddRedisDatabaseFactory(this IServiceCollection services, Action<RedisDatabaseFactory.Options>? config = null)
    {
        services.AddRedisConnectionFactory();
        services.TryAddTransient<RedisDatabaseFactory>();
        if(config != null)
        {
            services
                .AddOptions<RedisDatabaseFactory.Options>()
                .Configure(config);
        }
        return services;
    }
}