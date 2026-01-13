using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Time.Testing;

namespace Example.Tests;

public sealed class TestClient : IDisposable
{
    private readonly HttpClient _client;

    public TestClient(HttpClient client)
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

public enum AspEnvironment { Development, Production };

public class AppFactory : WebApplicationFactory<Program>
{
    //public readonly Dictionary<string, string?> AdditionalSettings = [];

    private string? _aspEnvironment;

    public readonly FakeRandom        FakeRandom = new();
    public readonly FakeTimeProvider  FakeTime   = new(new DateTimeOffset(2000, 1, 1, 0, 0, 0, 0, TimeSpan.Zero));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        if(_aspEnvironment != null)
            builder.UseEnvironment(_aspEnvironment);

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

    internal void SetAspEnvironment(AspEnvironment env)
    {
        _aspEnvironment = env.ToString();
    }

    public TestClient CreateTestClient()
    {
        var httpClient = CreateClient();
        return new TestClient(httpClient);
    }
}

//TODO: disposable pattern/DI of 'AppFactory' 
public class ApiTestBase: IAsyncDisposable
{
    protected readonly AppFactory _appFactory = new();

    /// <summary>
    /// Runs AppFactory, creates, memoizes and returns Client.
    /// </summary>
    protected TestClient Client
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