using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Example.Tests.Fixures;

public sealed class FixtureContainer : IAsyncDisposable
{
    // thread-safe by default
    private static readonly Lazy<ServiceCollection> __services = new(CreateServiceCollection);
    private readonly ServiceProvider _provider;

    public FixtureContainer()
    {
        _provider = __services.Value.BuildServiceProvider();
    }

    private static ServiceCollection CreateServiceCollection()
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
        return services;
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
        // return [assembly];
        
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
