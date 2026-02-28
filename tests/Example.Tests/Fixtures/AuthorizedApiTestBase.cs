using Example.Tests.Fixures;

namespace Example.Tests;

public class AuthorizedApiTestBase: ApiTestBase //IAsyncLifetime
{
    private readonly string DbName = $"Weather-test-{Guid.NewGuid()}";

    #region Stored fixtures
    protected AuthorizedClientFixture AuthorizedClientFixture {get; }
    #endregion

    #region props from fixtures for smart access
    /// <summary>
    /// Build&Run TestApp, create, memoize and return HttpClient connected to TestApp.
    /// </summary>
    protected override HttpClient Client => AuthorizedClientFixture.Client;
    protected SignalrTestClient SignalrClient => AuthorizedClientFixture.SignalrClient;
    #endregion

    public AuthorizedApiTestBase()
    {
        AuthorizedClientFixture = new(AppFixture, ScopeFixture);
    }

    protected override async Task DisposeAsyncCore()
    {
        await AuthorizedClientFixture.DisposeAsync();
        await base.DisposeAsyncCore();
    }
}