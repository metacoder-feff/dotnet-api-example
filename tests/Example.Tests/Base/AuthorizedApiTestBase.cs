using Example.Tests.Fixures;
using FEFF.TestFixtures.AspNetCore.SignalR;

namespace Example.Tests;

public class AuthorizedApiTestBase: ApiTestBase
{
    #region props from fixtures for smart access

    protected AuthorizedFixtureSet AuthorizedFx{ get; } = TestContext.Current.GetFeffFixture<AuthorizedFixtureSet>();
    
//TODO: memoize
    /// <summary>
    /// Build&Run TestApp, create, memoize and return HttpClient connected to TestApp.
    /// </summary>
    protected override HttpClient Client => AuthorizedFx.ClientFx.LazyValue;
    protected SignalrTestClient SignalrClient => AuthorizedFx.SignalR.LazyValue;
    #endregion
}