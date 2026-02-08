namespace FEFF.Extentions.Redis;

public interface IRedisConfigurationBuilder
{
    IServiceCollection Services { get; }
}

public interface IRedisConfigurationFactoryBuilder
{
    IServiceCollection Services { get; }
}

//TODO: IRedisConnectionFactory
//TODO: IRedisDatabaseFactory
//TODO: public ConfigurationOptionsFactory.Options
//TODO: internal RedisConnectionFactory RedisDatabaseFactory ConfigurationOptionsFactory