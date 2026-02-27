using Microsoft.Extensions.DependencyInjection;

using FEFF.Extentions.Jwt;

namespace Example.Tests;
using Example.Api;
using Example.Api.SignalR;

public class ExampleApiTestSignalr : ApiTestBase
{
    [Fact]
    public async Task Weatherforecast__should__send_event()
    {
        // PREPARE
//TODO: fixture AuthorizedClient
        var jwt = AppFixture.LazyScopeServiceProvider.GetRequiredService<IJwtFactory>();
        var token = LoginApiModule.CreateToken(jwt, "testuser");
        Client.AddBearerHeader(token);

        await using var signalr = TestApplication.CreateSignalRClient("/api/v1/public/events", token);
        signalr.Subscribe(EventSender.MethodName, 1);
        await signalr.StartAsync();
        
        // ACT
        var rStr = await Client.TestGetStringAsync("/api/v1/public/weatherforecast");
        var resp = await signalr.WaitForEvent(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        
        // ASSERT
        resp.ToJToken()
            .Should().BeEquivalentTo("""
            {
              "Method": "finished_ok",
              "Args": [
                {
                  "result": "ok",
                  "finished_at": "2000-01-01T00:00:00Z"
                }
              ]
            }
            """.ParseJToken());

    }
}