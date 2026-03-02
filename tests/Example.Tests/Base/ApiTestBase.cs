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
    //----------------------------------------------
    /// <summary>
    /// Get a TestCase-Fixture. It would be destroyed after TestCase finishes.
    /// </summary>
    /// <remarks>
    /// TestCase-Fixure integration requires:
    /// <code>
    /// [assembly: FEFF.Experimental.TestFixtures.FixturesXUnitExtension]
    /// </code>
    /// Otherwise manage <see cref="IFixtureProvider"/> manually.
    /// </remarks>
    protected static T GetFixture<T>()
    where T : notnull
    {
        return TestContext.Current.GetTestCaseFixtureProvider().GetFixture<T>();
    }
    //----------------------------------------

    protected FixtureSet FixtureSet { get; } = GetFixture<FixtureSet>();

    #region smart access to most used fixtures

    // This only fixture may be accessed only before test application is built otherwise thows
    protected ITestApplicationBuilder AppBuilder => FixtureSet.TestApplication.ApplicationBuilder;

    protected FakeRandom FakeRandom => FixtureSet.FakeRandom.Value;
    protected FakeTimeProvider FakeTime => FixtureSet.FakeTime.Value;

    /// <summary>
    /// Build and return ITestApplication.
    /// </summary>
    protected ITestApplication TestApplication => FixtureSet.TestApplication.LazyCreatedApplication;

    /// <summary>
    /// Build&Run TestApp and return HttpClient connected to TestApp.
    /// </summary>
    protected virtual HttpClient Client => GetFixture<AppClientFixture>().Value;

    /// <summary>
    /// Build&Run TestApp and return DbContext instance form TestApp.
    /// </summary>
    public WeatherContext DbCtx
    {
        get
        {
            field ??= GetAppService<WeatherContext>();
            return field;
        }
    }

    /// <summary>
    /// Build&Run TestApp and return TService instance form TestApp.
    /// </summary>
    public static TService GetAppService<TService>() where TService : notnull =>
        GetFixture<AppServicesFixture>().ServiceProvider.GetRequiredService<TService>();
    #endregion
}