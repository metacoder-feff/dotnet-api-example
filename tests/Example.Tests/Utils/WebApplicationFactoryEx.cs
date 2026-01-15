using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Utils.Testing;

public sealed class XUnitHttpClient : IDisposable
{
    private readonly HttpClient _client;

    public XUnitHttpClient(HttpClient client)
    {
        _client = client;
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    public async Task<string> GetStringAsync(string requestUri, HttpStatusCode expectedStatus = HttpStatusCode.OK)
    {
        var resp = await _client.GetAsync(requestUri, TestContext.Current.CancellationToken);
        var body = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        resp.StatusCode
            .Should().Be(expectedStatus, body);
        return body;
    }
}

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

    public XUnitHttpClient CreateTestClient()
    {
        var httpClient = CreateClient();
        return new XUnitHttpClient(httpClient);
    }
}

public enum AspEnvironment { Development, Production };

public static class WebApplicationFactoryExtention
{
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