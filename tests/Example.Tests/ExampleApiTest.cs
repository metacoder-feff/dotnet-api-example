namespace Example.Tests;

public class ExampleApiTest : ApiTestBase
{
    [Fact]
    public async Task Weatherforecast_should_contain_data()
    {
        // PREPARE
        _appFactory.FakeRandom.IntStrategy = FakeRandom.ConstStrategy(2);
        _appFactory.FakeTime.SetNow("2005-05-05T15:15:15Z");

        // ACT
        var rStr = await Client.GetStringAsync("/weatherforecast");
        
        // ASSERT
        rStr
            .ParseJToken()
            .Should()
            .ContainSubtree("""
            [
              {
                "timestamp": "2005-05-05T15:15:15Z",
                "date": "2005-05-06",
                "temperature_c": -18,
                "summary": "Chilly",
                "temperature_f": 0,
              }
            ]
            """);
    }
}