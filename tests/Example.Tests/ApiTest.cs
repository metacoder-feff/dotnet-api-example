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

    public static bool IsCI =>
        Environment.GetEnvironmentVariable("IS_CI_TEST")?
        .ToLowerInvariant()
        == "true";

    public async ValueTask DisposeAsync()
    {
        await _appFactory.DisposeAsync();
    }
}

public class InfrastructrureApiTest : ApiTestBase
{
    [Theory]
    [InlineData(AspEnvironment.Development, HttpStatusCode.OK)]
    [InlineData(AspEnvironment.Production , HttpStatusCode.NotFound)]
    public async Task Test_swagger_ui_enabled(AspEnvironment env, HttpStatusCode res)
    {
        _appFactory.SetAspEnvironment(env);

        var body = await Client.GetStringAsync("/swagger", res);

        if (env == AspEnvironment.Development)
            body.Should().Contain("""
            <div id="swagger-ui">
            """);
    }

    [Theory]
    [InlineData(AspEnvironment.Development, HttpStatusCode.OK)]
    [InlineData(AspEnvironment.Production , HttpStatusCode.NotFound)]
    public async Task Test_openapi_enabled(AspEnvironment env, HttpStatusCode res)
    {
        _appFactory.SetAspEnvironment(env);

        var body = await Client.GetStringAsync("/openapi/v1.json", res);

        if (env == AspEnvironment.Development)
            body.Should().Contain("openapi");
    }
    
    /// <summary>
    /// If intended to change API, run this test locally and push updated "tests/Files/openapi.json" to repo.
    /// </summary>
    [Fact]
    public async Task OpenAPI_json__update()
    {
//TODO: attribute, see tests/tmp/Ext.Exa/SupportedOS.cs        
        Assert.SkipWhen(IsCI, "Only for local-dev");
        await AssertOrUpdateOpenAPI(false);
    }

    /// <summary>
    /// This check aims to avoid accidentally changing of public API. Runs on CI only.
    /// If intended to change API, then run test "OpenAPI_json__update" locally and push updated "tests/Files/openapi.json" to repo.
    /// This patch would be reviewed at MR/PR.
    /// </summary>
    [Fact]
    public async Task OpenAPI_json__should_not_change()
    {
//TODO: attribute
        Assert.SkipUnless(IsCI, "Only for CI");
        await AssertOrUpdateOpenAPI(true);
    }

    private async Task AssertOrUpdateOpenAPI(bool isCI)
    {
        var body = await Client.GetStringAsync("/openapi/v1.json");

        var targetFile = "../../../../Files/openapi.json";
        var changesFile = targetFile + ".modified.json";

        // for local dev
        if(isCI == false)
        {
            File.WriteAllText(targetFile, body);
            return;
        }

        // for CI
        var stored = File.ReadAllText(targetFile);
        if (stored == body)
            return;

        File.WriteAllText(changesFile, body);
        body
            .ParseJToken()
            .Should()
            .BeEquivalentTo(stored);
            //.BeEquivalentToEX(stored);
    }

    [Fact]
    public async Task Test_Metrics()
    {
        var body = await Client.GetStringAsync("/metrics");

        body.Should().Contain("dotnet_collection_count_total counter");
    }
}