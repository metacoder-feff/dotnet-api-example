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
    private readonly TestApplicationFixture _appFixture;
    private readonly FakeTimeFixture _fakeTimeFixture;
    private readonly FakeRandomFixture _fakeRandomFixture;
    private readonly ClientFixture _clientFixture;
    private readonly AppServiceScopeFixture _scopeFixture;
    #endregion

    #region props from fixtures for smart access

    protected FakeRandom FakeRandom => _fakeRandomFixture.FakeRandom;

    protected FakeTimeProvider FakeTime => _fakeTimeFixture.FakeTime;

    /// <summary>
    /// Build, memoize and return ITestApplication.
    /// </summary>
    protected ITestApplication TestApplication => _appFixture.LazyTestApplication;

    /// <summary>
    /// Build&Run TestApp, create, memoize and return HttpClient connected to TestApp.
    /// </summary>
    protected HttpClient Client => _clientFixture.Client;

    /// <summary>
    /// Build&Run TestApp, get, memoize and return DbContext instance form TestApp.
    /// </summary>
    public WeatherContext DbCtx
    {
        get
        {
            field ??= _scopeFixture.LazyScopeServiceProvider.GetRequiredService<WeatherContext>();
            return field;
        }
    }
    #endregion

    public ApiTestBase()
    {
        // build fixtures tree
        AppBuilder = new TestApplicationBuilder<Program>();
        _fakeRandomFixture = new(AppBuilder);
        _fakeTimeFixture = new(AppBuilder);

// TODO: fixture as an Action ??
        _ = new DbNameFixture(AppBuilder, DbName, InfrastructureModule.PgConnectionStringName);
        // KeyPrefix and ChannelPrefix for main redis connection
        _ = new RedisPrefixFixture<RedisConnectionManager>(AppBuilder, DbName);
        // channel prefix for SignalR redis connection
        _ = new RedisChannelPrefixFixture<SignalRedisProviderProxy>(AppBuilder, DbName);
        
        _appFixture = new(AppBuilder);
        _clientFixture = new(_appFixture);
        _scopeFixture = new(_appFixture);
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
    protected async Task DisposeAsyncCore()
    {
        _clientFixture.Dispose();
        await _scopeFixture.DisposeAsync().ConfigureAwait(false);
        await _appFixture.DisposeAsync().ConfigureAwait(false);
    }
    #endregion

    /// <summary>
    /// Run TestApp, get, memoize and return TService instance form TestApp.
    /// </summary>
    public TService GetRequiredService<TService>() where TService : notnull =>
        _scopeFixture.LazyScopeServiceProvider.GetRequiredService<TService>();
}