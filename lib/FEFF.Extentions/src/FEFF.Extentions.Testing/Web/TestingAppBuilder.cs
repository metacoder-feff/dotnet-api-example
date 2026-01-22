using System.Collections.Frozen;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace FEFF.Extentions.Testing;

public interface ITestingAppBuilder
{
    void AddOverride(Action<IWebHostBuilder> action);
}

public class TestingAppBuilder : ITestingAppBuilder
{
    private readonly List<Action<IWebHostBuilder>> _builderOverrides = [];

    public void AddOverride(Action<IWebHostBuilder> action)
    {
        _builderOverrides.Add(action);
    }

    public WebApplicationFactory<TEntryPoint> CreateApp<TEntryPoint>()
    where TEntryPoint: class
    {
        return new TestingWebApplication<TEntryPoint>(_builderOverrides);
    }
}

public class TestingWebApplication<TEntryPoint> : WebApplicationFactory<TEntryPoint>
where TEntryPoint: class
{
    private FrozenSet<Action<IWebHostBuilder>> _builderOverrides;

    public TestingWebApplication(List<Action<IWebHostBuilder>> builderOverrides)
    {
        _builderOverrides = builderOverrides.ToFrozenSet();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        foreach(var a in _builderOverrides)
            a(builder);
    }
}

public enum AspEnvironment { Development, Production };

public static class WebApplicationFactoryExtention
{
    public static void UseSetting(this ITestingAppBuilder builder, string key, string? value)
    {
        builder.AddOverride(
            b => b.UseSetting(key, value)
        );
    }

    public static void UseAspEnvironment(this ITestingAppBuilder builder, AspEnvironment env)
    {
        builder.AddOverride(
            b => b.UseEnvironment(env.ToString())
        );
    }

    public static void ConfigureServices(this ITestingAppBuilder builder, Action<IServiceCollection> configureServices)
    {
        builder.AddOverride(
            b => b.ConfigureServices(configureServices)
        );
    }

    public static void ConfigureServices(this ITestingAppBuilder builder, Action<WebHostBuilderContext, IServiceCollection> configureServices)
    {
        builder.AddOverride(
            b => b.ConfigureServices(configureServices)
        );
    }
}