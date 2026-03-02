using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

using FEFF.Experimental.TestFixtures;
using FEFF.Experimental.TestFixtures.AspNetCore;
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
    TestApplicationFixture TestApplication,
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

public class ApiTestBase
{
//TODO: memoize
    protected FixtureContainer FixtureContainer => TestContext.Current.GetFixtureContainer();
    protected FixtureSet FixtureSet {get;}

    #region props from fixtures for smart access
    protected ITestApplicationBuilder AppBuilder => FixtureSet.TestApplication.ApplicationBuilder;

//TODO: rename
    protected FakeRandom FakeRandom => FixtureSet.FakeRandom.Value;

    protected FakeTimeProvider FakeTime => FixtureSet.FakeTime.Value;

    /// <summary>
    /// Build, memoize and return ITestApplication.
    /// </summary>
    protected ITestApplication TestApplication => FixtureSet.TestApplication.LazyCreatedApplication;

//TODO: memoize
    /// <summary>
    /// Build&Run TestApp, create, memoize and return HttpClient connected to TestApp.
    /// </summary>
    protected virtual HttpClient Client => GetFixture<AppClientFixture>().Value;

    /// <summary>
    /// Build&Run TestApp, get, memoize and return DbContext instance form TestApp.
    /// </summary>
    public WeatherContext DbCtx
    {
        get
        {
            field ??= GetFixture<AppServicesFixture>().ServiceProvider.GetRequiredService<WeatherContext>();
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

    /// <summary>
    /// Run TestApp, get, memoize and return TService instance form TestApp.
    /// </summary>
    public TService GetRequiredService<TService>() where TService : notnull =>
        GetFixture<AppServicesFixture>().ServiceProvider.GetRequiredService<TService>();
}