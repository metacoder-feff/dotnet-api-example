using Microsoft.Extensions.DependencyInjection;

namespace Example.Tests.Fixures;

public sealed class FixtureContainer : IAsyncDisposable
{
    private readonly ServiceProvider _provider;

    public FixtureContainer()
    {    
//TODO: auto
        var services = new ServiceCollection();
        
        services.AddSingleton<TestApplicationBuilder<Program>>();
        services.AddSingleton<ITestApplicationBuilder>(sp => sp.GetRequiredService<TestApplicationBuilder<Program>>());

        services.AddSingleton<TestApplicationFixture>();
        services.AddSingleton<FakeTimeFixture>();
        services.AddSingleton<FakeRandomFixture>();
        services.AddSingleton<ClientFixture>();
        services.AddSingleton<AppServiceScopeFixture>();
        services.AddSingleton<AuthorizedClientFixture>();
        
// TODO: fixture as an Action ??
        services.AddSingleton<TestIdFixture>();
        services.AddSingleton<DbNameFixture>();
        services.AddSingleton(typeof(RedisPrefixFixture<>));
        services.AddSingleton(typeof(RedisChannelPrefixFixture<>));

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
