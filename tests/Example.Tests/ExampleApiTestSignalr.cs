using Example.Api.SignalR;

namespace Example.Tests;

public class ExampleApiTestSignalr : ApiTestBase
{
    [Fact]
    public async Task Weatherforecast__should__send_event()
    {
        // PREPARE
        await using var signalr = TestApplication.CreateSignalRClient("/events");
        signalr.Subscribe(EventSender.MethodName, 1);
        await signalr.StartAsync();
        
        // ACT
        var rStr = await Client.TestGetStringAsync("/weatherforecast");
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