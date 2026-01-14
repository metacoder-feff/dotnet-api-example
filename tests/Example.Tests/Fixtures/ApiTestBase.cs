using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Time.Testing;

namespace Example.Tests;

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


public class WebApplicationFactoryEx<TEntryPoint> : WebApplicationFactory<TEntryPoint>
where TEntryPoint: class
{
    private readonly List<Action<IWebHostBuilder>> _builderOverrides = [];

    public void OverrideBuilder(Action<IWebHostBuilder> action)
    {
        _builderOverrides.Add(action);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        foreach(var a in _builderOverrides)
            a(builder);
    }
}

public class AppFactory : WebApplicationFactoryEx<Program>
{
    public readonly FakeRandom        FakeRandom = new();
    public readonly FakeTimeProvider  FakeTime   = new(new DateTimeOffset(2000, 1, 1, 0, 0, 0, 0, TimeSpan.Zero));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureAppConfiguration((ctx, configBuilder) =>
        {
            //configBuilder.AddInMemoryCollection(AdditionalSettings);

            // WORKAROUND for vscode error:
            //   "The configured user limit (128) on the number of inotify..."
            //Environment.SetEnvironmentVariable("DOTNET_hostBuilder:reloadConfigOnChange", "false");
            var ccc = configBuilder.Sources.OfType<FileConfigurationSource>();
            foreach (var x in ccc)
            {
                x.ReloadOnChange = false;
            }
        });

        
        builder.ConfigureServices( (ctx, services) =>
        {
            services.TryReplaceSingleton<Random>(FakeRandom);
            services.TryReplaceSingleton<TimeProvider>(FakeTime);
        });
    }


    public XUnitHttpClient CreateTestClient()
    {
        var httpClient = CreateClient();
        return new XUnitHttpClient(httpClient);
    }
}

//TODO: disposable pattern/DI of 'AppFactory' 
public class ApiTestBase: IAsyncDisposable
{
    protected readonly AppFactory _appFactory = new();

    /// <summary>
    /// Runs AppFactory, creates, memoizes and returns Client.
    /// </summary>
    protected XUnitHttpClient Client
    {
        get
        {
            field ??= _appFactory.CreateTestClient();
            return field;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _appFactory.DisposeAsync();
    }
}

public enum AspEnvironment { Development, Production };

public static class WebApplicationFactoryExtention
{
    public static void OverrideAspEnvironment<T>(this WebApplicationFactoryEx<T> factory, AspEnvironment env)
    where T: class
    {
        factory.OverrideBuilder(
            b => b.UseEnvironment(env.ToString())
        );
    }
}