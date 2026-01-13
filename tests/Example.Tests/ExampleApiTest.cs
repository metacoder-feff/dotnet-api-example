namespace Example.Tests;

public class ExampleApiTest : ApiTestBase
{
    [Fact]
    public async Task Weatherforecast_should_return()
    {
        // PREPARE
        _appFactory.FakeRandom.FakeNextInt = 2;

        // ACT
        var rStr = await Client.GetStringAsync("/weatherforecast");
        
        // ASSERT
        rStr
            .ParseJToken()
            .Should()
            .BeEquivalentTo("""
            [
            {
                "date":"2000-01-02",
                "temperatureC":-18,
                "summary":"Chilly",
                "temperatureF":0
            }
            ]
            """);
    }
}