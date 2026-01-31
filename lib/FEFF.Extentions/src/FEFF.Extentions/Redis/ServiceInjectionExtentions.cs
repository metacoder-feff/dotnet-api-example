using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FEFF.Extentions.Redis;

public static class ServiceInjectionExtentions
{
    public static IServiceCollection AddRedisConnectrionManager(this IServiceCollection services, string cfgSection = "Redis")
    {
        services.AddOptions<RedisConnectrionManager.Options>()
                    .BindConfiguration(cfgSection)
                    .ValidateDataAnnotations() //TODO:??
                    .Validate().With<RedisConnectrionManager.Options.Validator>()
                    .ValidateOnStart()
                    ;

        services.TryAddSingleton<RedisConnectrionManager>();

        return services;
    }
}