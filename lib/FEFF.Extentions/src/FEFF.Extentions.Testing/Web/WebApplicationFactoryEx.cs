using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace FEFF.Extentions.Testing;

public interface IBuilderOverrider
{
    void AddBuilderOverride(Action<IWebHostBuilder> action);
}

internal class BuilderOverrideList : List<Action<IWebHostBuilder>>, IBuilderOverrider
{
    public void AddBuilderOverride(Action<IWebHostBuilder> action)
    {
        this.Add(action);
    }
}

public class WebApplicationFactoryEx<TEntryPoint> : WebApplicationFactory<TEntryPoint>
where TEntryPoint: class
{
    private readonly BuilderOverrideList _builderOverrides = [];

    public IBuilderOverrider BuilderOverrider => _builderOverrides;

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
    public static void UseSetting(this IBuilderOverrider factory, string key, string? value)
    {
        factory.AddBuilderOverride(
            b => b.UseSetting(key, value)
        );
    }

    public static void UseAspEnvironment(this IBuilderOverrider factory, AspEnvironment env)
    {
        factory.AddBuilderOverride(
            b => b.UseEnvironment(env.ToString())
        );
    }

    public static void ConfigureServices(this IBuilderOverrider factory, Action<IServiceCollection> configureServices)
    {
        factory.AddBuilderOverride(
            b => b.ConfigureServices(configureServices)
        );
    }

    public static void ConfigureServices(this IBuilderOverrider factory, Action<WebHostBuilderContext, IServiceCollection> configureServices)
    {
        factory.AddBuilderOverride(
            b => b.ConfigureServices(configureServices)
        );
    }
}