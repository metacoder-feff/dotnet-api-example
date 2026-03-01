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

        var types = FindFixtureTypes<FixtureAttribute>();
        foreach (var t in types)
            RegisterFixureType(services, t);
        
        return services;
    }

    private static void RegisterFixureType(ServiceCollection services, Type t)
    {
        services.AddSingleton(t);
        var attribute = t.GetCustomAttribute<FixtureAttribute>();
        if (attribute?.FixtureType is null)
            return;

        if(attribute.FixtureType.IsAssignableFrom(t) == false)
            throw new InvalidOperationException($"Implementation type'{t}' should be subtype or implement {nameof(FixtureAttribute.FixtureType)} '{attribute.FixtureType}'.");

        services.AddSingleton(attribute.FixtureType, sp => sp.GetRequiredService(t));
    }

    private static IEnumerable<Type> FindFixtureTypes<TAttribute>()
    {
        var atr = typeof(TAttribute);
        
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
