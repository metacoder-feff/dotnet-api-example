using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using FEFF.TestFixtures.AspNetCore.EF;
using FEFF.TestFixtures.AspNetCore.Randomness;

namespace Example.Tests;
using Example.Api;
using Example.Tests.Fixures;

public class ApiTestBase
{
    protected FixtureSet FixtureSet { get; } = TestContext.Current.GetFeffFixture<FixtureSet>();

    #region smart access to most used fixtures

    // This only fixture may be accessed only before test application is built otherwise thows
    protected IAppConfigurator AppBuilder => FixtureSet.TestApplication.ConfigurationBuilder;

    protected FakeRandom FakeRandom => FixtureSet.FakeRandom.Value;
    protected FakeTimeProvider FakeTime => FixtureSet.FakeTime.Value;

    /// <summary>
    /// Build and return ITestApplication.
    /// </summary>
    protected ITestApplication TestApplication => FixtureSet.TestApplication.LazyApplication;

    /// <summary>
    /// Build&Run TestApp and return HttpClient connected to TestApp.
    /// </summary>
    protected virtual HttpClient Client => FixtureSet.ClientFx.LazyValue;

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
    public TService GetAppService<TService>() where TService : notnull =>
        FixtureSet.ServiceFx.LazyServiceProvider.GetRequiredService<TService>();

    public IDatabaseLifecycleFixture EnsureDbFx => FixtureSet.EnsureDb;
    #endregion
}