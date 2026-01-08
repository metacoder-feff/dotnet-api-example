namespace Example.Tests;
using Example.Api.SignalR;

public class ExampleApiTestSignalr : AuthorizedApiTestBase
{
    [Fact]
    public async Task Weatherforecast__should__send_event()
    {
        // PREPARE
        SignalrClient.Subscribe(EventSender.MethodName, 1);
        await SignalrClient.StartAsync();
        
        // ACT
        _ = await Client.TestGetStringAsync("/api/v1/public/weatherforecast");
        var resp = await SignalrClient.WaitForEvent(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        
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