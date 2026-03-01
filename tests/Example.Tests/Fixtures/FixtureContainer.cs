using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Example.Tests.Fixures;

public sealed class FixtureContainer : IAsyncDisposable
{
    private readonly ServiceProvider _provider;

    public FixtureContainer()
    {
        var services = new ServiceCollection();

        //TODO: auto
        //services.AddSingleton<ITestApplicationBuilder, TestApplicationBuilder<Program>>();
        services.AddSingleton<TestApplicationBuilder<Program>>();
        services.AddSingleton<ITestApplicationBuilder>(sp => sp.GetRequiredService<TestApplicationBuilder<Program>>());

        var types = FindFixtureTypes();
        var tt = types.ToList();

        foreach (var t in types)
            services.AddSingleton(t);


        // // TODO: fixture as an Action ??
        //         services.AddSingleton<TestIdFixture>();
        //         services.AddSingleton<DbNameFixture>();
                 services.AddSingleton(typeof(RedisPrefixFixture<>));
        //         services.AddSingleton(typeof(RedisChannelPrefixFixture<>));

        _provider = services.BuildServiceProvider();
    }

    private static IEnumerable<Type> FindFixtureTypes()
    {
        var atr = typeof(FixtureAttribute);
        
        var types = GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsDefined(atr, false));
        return types;
    }

    private static IEnumerable<Assembly> GetAssemblies()
    {
        // var assembly = Assembly.GetExecutingAssembly();
        // IEnumerable<Assembly> aa = [assembly];
        // return aa;
        return AppDomain.CurrentDomain.GetAssemblies();
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
