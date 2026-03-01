using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

using FEFF.Extentions.Redis;
using FEFF.Extentions.SignalR.Redis;

namespace Example.Tests;
using Example.Api;
using Example.Tests.Fixures;

/// <summary>
/// This fixures are required by most of tests
/// </summary>
[Fixture]
public record FixtureSet(
    FakeRandomFixture FakeRandom,
    FakeTimeFixture FakeTime,

// TODO: fixture as an Action ??
    // just override services at app builder
    DbNameFixture DbName,
    // KeyPrefix and ChannelPrefix for main redis connection
    RedisPrefixFixture<RedisConnectionManager> SecondRedisPrefix,
    // channel prefix for SignalR redis connection
    RedisChannelPrefixFixture<SignalRedisProviderProxy> SignalRedisPrefix
);

public class ApiTestBase: IAsyncDisposable //IAsyncLifetime
{
    protected FixtureContainer FixtureContainer {get;} = new ();
    protected FixtureSet FixtureSet {get;}

//TODO: memoize
    #region props from fixtures for smart access
    protected ITestApplicationBuilder AppBuilder => GetFixture<ITestApplicationBuilder>();

    protected FakeRandom FakeRandom => GetFixture<FakeRandomFixture>().FakeRandom;

    protected FakeTimeProvider FakeTime => GetFixture<FakeTimeFixture>().FakeTime;

    /// <summary>
    /// Build, memoize and return ITestApplication.
    /// </summary>
    protected ITestApplication TestApplication => GetFixture<TestApplicationFixture>().LazyTestApplication;

    /// <summary>
    /// Build&Run TestApp, create, memoize and return HttpClient connected to TestApp.
    /// </summary>
    protected virtual HttpClient Client => GetFixture<ClientFixture>().Client;

    /// <summary>
    /// Build&Run TestApp, get, memoize and return DbContext instance form TestApp.
    /// </summary>
    public WeatherContext DbCtx
    {
        get
        {
            field ??= GetFixture<AppServiceScopeFixture>().LazyScopeServiceProvider.GetRequiredService<WeatherContext>();
            return field;
        }
    }
    #endregion

    public ApiTestBase()
    {
        FixtureSet = GetFixture<FixtureSet>();
    }

    protected T GetFixture<T>()
    where T : notnull
    {
        return FixtureContainer.GetFixture<T>();
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
    protected ValueTask DisposeAsyncCore()
    {
        return FixtureContainer.DisposeAsync();
    }
    #endregion

    /// <summary>
    /// Run TestApp, get, memoize and return TService instance form TestApp.
    /// </summary>
    public TService GetRequiredService<TService>() where TService : notnull =>
        GetFixture<AppServiceScopeFixture>().LazyScopeServiceProvider.GetRequiredService<TService>();
}