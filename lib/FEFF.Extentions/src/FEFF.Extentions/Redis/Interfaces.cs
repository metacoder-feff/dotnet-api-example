namespace FEFF.Extentions.Redis;

public interface IRedisConnectionFactoryBuilder
{
    IServiceCollection Services { get; }
}

public interface IRedisConfigurationFactoryBuilder
{
    IServiceCollection Services { get; }
}