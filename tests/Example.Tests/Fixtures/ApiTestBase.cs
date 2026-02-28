using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

using FEFF.Extentions.Redis;
using FEFF.Extentions.SignalR.Redis;

using Example.Api;

namespace Example.Tests;

public class ApiTestBase: IAsyncDisposable //IAsyncLifetime
{
    private readonly string DbName = $"Weather-test-{Guid.NewGuid()}";

    #region Stored fixtures
    protected ITestApplicationBuilder AppBuilder {get; }
    protected TestApplicationFixture AppFixture {get; }
    protected FakeTimeFixture FakeTimeFixture {get; }
    protected FakeRandomFixture FakeRandomFixture {get; }
    protected ClientFixture ClientFixture {get; }
    protected AppServiceScopeFixture ScopeFixture {get; }
    #endregion

    #region props from fixtures for smart access

    protected FakeRandom FakeRandom => FakeRandomFixture.FakeRandom;

    protected FakeTimeProvider FakeTime => FakeTimeFixture.FakeTime;

    /// <summary>
    /// Build, memoize and return ITestApplication.
    /// </summary>
    protected ITestApplication TestApplication => AppFixture.LazyTestApplication;

    /// <summary>
    /// Build&Run TestApp, create, memoize and return HttpClient connected to TestApp.
    /// </summary>
    protected virtual HttpClient Client => ClientFixture.Client;

    /// <summary>
    /// Build&Run TestApp, get, memoize and return DbContext instance form TestApp.
    /// </summary>
    public WeatherContext DbCtx
    {
        get
        {
            field ??= ScopeFixture.LazyScopeServiceProvider.GetRequiredService<WeatherContext>();
            return field;
        }
    }
    #endregion

    public ApiTestBase()
    {
        // build fixtures tree
        AppBuilder = new TestApplicationBuilder<Program>();
        FakeRandomFixture = new(AppBuilder);
        FakeTimeFixture = new(AppBuilder);

// TODO: fixture as an Action ??
        _ = new DbNameFixture(AppBuilder, DbName, InfrastructureModule.PgConnectionStringName);
        // KeyPrefix and ChannelPrefix for main redis connection
        _ = new RedisPrefixFixture<RedisConnectionManager>(AppBuilder, DbName);
        // channel prefix for SignalR redis connection
        _ = new RedisChannelPrefixFixture<SignalRedisProviderProxy>(AppBuilder, DbName);
        
        AppFixture = new(AppBuilder);
        ScopeFixture = new(AppFixture);
        ClientFixture = new(AppFixture);
    }

    #region IAsyncDisposable //IAsyncLifetime
    // public virtual ValueTask InitializeAsync()
    // {
    //     return ValueTask.CompletedTask;
    // }

    // Public implementation of Dispose pattern callable by consumers.
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    // Protected implementation of Dispose pattern.
    protected virtual async Task DisposeAsyncCore()
    {
        ClientFixture.Dispose();
        await ScopeFixture.DisposeAsync().ConfigureAwait(false);
        await AppFixture.DisposeAsync().ConfigureAwait(false);
    }
    #endregion

    /// <summary>
    /// Run TestApp, get, memoize and return TService instance form TestApp.
    /// </summary>
    public TService GetRequiredService<TService>() where TService : notnull =>
        ScopeFixture.LazyScopeServiceProvider.GetRequiredService<TService>();
}