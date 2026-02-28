using Microsoft.Extensions.DependencyInjection;

using FEFF.Extentions.Redis;
using FEFF.Extentions.SignalR.Redis;

namespace Example.Tests.Fixures;

public sealed class FixtureContainer : IAsyncDisposable
{
    private readonly ServiceProvider _provider;

    public FixtureContainer()
    {    
        var services = new ServiceCollection();

//TODO: auto
        services.AddSingleton<ITestApplicationBuilder, TestApplicationBuilder<Program>>();
        services.AddSingleton<TestApplicationFixture>();
        services.AddSingleton<FakeTimeFixture>();
        services.AddSingleton<FakeRandomFixture>();
        services.AddSingleton<ClientFixture>();
        services.AddSingleton<AppServiceScopeFixture>();
        services.AddSingleton<AuthorizedClientFixture>();
        
// TODO: fixture as an Action ??
        services.AddSingleton<TestIdFixture>();
        services.AddSingleton<DbNameFixture>();
        services.AddSingleton<RedisPrefixFixture<RedisConnectionManager>>();
        services.AddSingleton<RedisChannelPrefixFixture<SignalRedisProviderProxy>>();

        _provider = services.BuildServiceProvider();
    }

    public ValueTask DisposeAsync()
    {
        return _provider.DisposeAsync();
    }

    public T GetFixture<T>()
    where T : notnull
    {
        return _provider.GetRequiredService<T>();
    }
}
