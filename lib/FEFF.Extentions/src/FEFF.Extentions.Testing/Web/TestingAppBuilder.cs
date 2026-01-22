using System.Collections.Frozen;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace FEFF.Extentions.Testing;

public interface ITestingAppBuilder
{
    void ConfigureWebHost(Action<IWebHostBuilder> action);
}

public class TestingAppBuilder : ITestingAppBuilder
{
    private readonly List<Action<IWebHostBuilder>> _builderOverrides = [];

    public void ConfigureWebHost(Action<IWebHostBuilder> action)
    {
        _builderOverrides.Add(action);
    }

    //TODO: encapsulate TestingApp->WebApplicationFactory
    public WebApplicationFactory<TEntryPoint> Build<TEntryPoint>()
    where TEntryPoint: class
    {
        return new OverridenWebApplication<TEntryPoint>(_builderOverrides);
    }
}

internal class OverridenWebApplication<TEntryPoint> : WebApplicationFactory<TEntryPoint>
where TEntryPoint: class
{
    private FrozenSet<Action<IWebHostBuilder>> _builderOverrides;

    public OverridenWebApplication(List<Action<IWebHostBuilder>> builderOverrides)
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

public static class TestingAppBuilderExtention
{
    public static void UseSetting(this ITestingAppBuilder builder, string key, string? value)
    {
        builder.ConfigureWebHost(
            b => b.UseSetting(key, value)
        );
    }

    public static void UseAspEnvironment(this ITestingAppBuilder builder, AspEnvironment env)
    {
        builder.ConfigureWebHost(
            b => b.UseEnvironment(env.ToString())
        );
    }

    public static void ConfigureServices(this ITestingAppBuilder builder, Action<IServiceCollection> configureServices)
    {
        builder.ConfigureWebHost(
            b => b.ConfigureServices(configureServices)
        );
    }

    public static void ConfigureServices(this ITestingAppBuilder builder, Action<WebHostBuilderContext, IServiceCollection> configureServices)
    {
        builder.ConfigureWebHost(
            b => b.ConfigureServices(configureServices)
        );
    }
}