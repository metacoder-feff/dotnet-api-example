using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

using FEFF.Extentions.Redis;
using FEFF.Extentions.SignalR.Redis;

using Example.Api;

namespace Example.Tests;

public class ApiTestBase: IAsyncDisposable //IAsyncLifetime
{
    private readonly string DbName = $"Weather-test-{Guid.NewGuid()}";

    // fixures
    protected ITestApplicationBuilder AppBuilder {get; }
    private readonly TestApplicationFixture _appFixture;
    private readonly FakeTimeFixture _fft;
    private readonly FakeRandomFixture _frf;

    // props from fixtures for smart access
    protected ITestApplication TestApplication => _appFixture.LazyTestApplication;
    
    protected FakeRandom FakeRandom => _frf.FakeRandom;

    protected FakeTimeProvider  FakeTime => _fft.FakeTime;

    /// <summary>
    /// Run TestApp, create, memoize and return HttpClient connected to TestApp.
    /// </summary>
    protected HttpClient Client => _appFixture.LazyClient;

    /// <summary>
    /// Run TestApp, get, memoize and return DbContext instance form TestApp.
    /// </summary>
    public WeatherContext DbCtx
    {
        get
        {
            field ??= _appFixture.LazyScopeServiceProvider.GetRequiredService<WeatherContext>();
            return field;
        }
    }

    public ApiTestBase()
    {
        // build fixtures tree
        AppBuilder = new TestApplicationBuilder<Program>();
        _appFixture = new(AppBuilder);
        _frf = new(AppBuilder);
        _fft = new(AppBuilder);
// TODO: fixture as an Action
        _ = new DbNameFixture(AppBuilder, DbName, InfrastructureModule.PgConnectionStringName);
        // KeyPrefix and ChannelPrefix for main redis connection
        _ = new RedisPrefixFixture<RedisConnectionManager>(AppBuilder, DbName);
        // channel prefix for SignalR redis connection
        _ = new RedisChannelPrefixFixture<SignalRedisProviderProxy>(AppBuilder, DbName);
    }

    #region IAsyncLifetime
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
        await _appFixture.DisposeAsync().ConfigureAwait(false);
    }
    #endregion

    /// <summary>
    /// Run TestApp, get, memoize and return TService instance form TestApp.
    /// </summary>
    public TService GetRequiredService<TService>() where TService : notnull =>
        _appFixture.LazyScopeServiceProvider.GetRequiredService<TService>();
}