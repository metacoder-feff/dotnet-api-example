using Example.Tests.Fixures;

namespace Example.Tests;

public class AuthorizedApiTestBase: ApiTestBase //IAsyncLifetime
{
    #region props from fixtures for smart access
//TODO: memoize
    /// <summary>
    /// Build&Run TestApp, create, memoize and return HttpClient connected to TestApp.
    /// </summary>
    protected override HttpClient Client => GetFixture<AuthorizedClientFixture>().Client;
    protected SignalrTestClient SignalrClient => GetFixture<AuthorizedClientFixture>().SignalrClient;
    #endregion
}