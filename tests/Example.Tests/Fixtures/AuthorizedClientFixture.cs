using Example.Api;
using FEFF.Extentions.Jwt;

namespace Example.Tests.Fixures;

[Fixture]
public sealed class AuthorizedClientFixture : IAsyncDisposable
{
    private readonly Lazy<HttpClient> _client;
    private readonly Lazy<SignalrTestClient> _signal;

    /// <summary>
    /// Runs AppFactory, creates, memoizes and returns Client.
    /// </summary>
    public HttpClient Client => _client.Value;
    public SignalrTestClient SignalrClient => _signal.Value;

    public AuthorizedClientFixture(TestApplicationFixture app, AppServiceScopeFixture scope)
    {
        // cannot remove lambda expression because access to 'app.LazyTestApplication' finishes app building
        // but we only need to register callback
        _client = new(() => CreateClient(app, scope));
        _signal = new(() => CreateSignal(app, scope));
    }

    private static SignalrTestClient CreateSignal(TestApplicationFixture app, AppServiceScopeFixture scope)
    {
        var jwt = scope.GetRequiredService<IJwtFactory>();
        var token = LoginApiModule.CreateToken(jwt, "testuser");

        return app.LazyTestApplication.CreateSignalRClient("/api/v1/public/events", token);
    }

    private static HttpClient CreateClient(TestApplicationFixture app, AppServiceScopeFixture scope)
    {
        var jwt = scope.GetRequiredService<IJwtFactory>();
        var token = LoginApiModule.CreateToken(jwt, "testuser");

        var res = app.LazyTestApplication.CreateClient();
        res.AddBearerHeader(token);
        return res;
    }

    public ValueTask DisposeAsync()
    {
        if(_client.IsValueCreated)
            _client.Value.Dispose();
            
        if(_signal.IsValueCreated)
            return _signal.Value.DisposeAsync();

        return ValueTask.CompletedTask;
    }
}
