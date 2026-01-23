using System.Collections.Frozen;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace FEFF.Extentions.Testing;

public interface ITestApplicationBuilder
{
    void ConfigureWebHost(Action<IWebHostBuilder> action);
    ITestApplication Build(); 
}

public interface ITestApplication : IAsyncDisposable
{
    IServiceProvider Services { get; }
    TestServer Server { get; }

    HttpClient CreateClient();
    void StartServer();
}

public class TestApplicationBuilder<TEntryPoint> : ITestApplicationBuilder
where TEntryPoint: class
{
    private readonly List<Action<IWebHostBuilder>> _builderOverrides = [];

    public void ConfigureWebHost(Action<IWebHostBuilder> action)
    {
        _builderOverrides.Add(action);
    }

    public ITestApplication Build()
    {
        return new OverridenWebApplication<TEntryPoint>(_builderOverrides);
    }
}

internal class OverridenWebApplication<TEntryPoint> : WebApplicationFactory<TEntryPoint>, ITestApplication
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
    public static void UseSetting(this ITestApplicationBuilder builder, string key, string? value)
    {
        builder.ConfigureWebHost(
            b => b.UseSetting(key, value)
        );
    }

    public static void UseAspEnvironment(this ITestApplicationBuilder builder, AspEnvironment env)
    {
        builder.ConfigureWebHost(
            b => b.UseEnvironment(env.ToString())
        );
    }

    public static void ConfigureServices(this ITestApplicationBuilder builder, Action<IServiceCollection> configureServices)
    {
        builder.ConfigureWebHost(
            b => b.ConfigureServices(configureServices)
        );
    }

    public static void ConfigureServices(this ITestApplicationBuilder builder, Action<WebHostBuilderContext, IServiceCollection> configureServices)
    {
        builder.ConfigureWebHost(
            b => b.ConfigureServices(configureServices)
        );
    }
}