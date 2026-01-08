using FEFF.TestFixtures.AspNetCore.Randomness;

namespace Example.Tests;

public class ExampleApiTest : AuthorizedApiTestBase
{
    [Fact]
    public async Task Weatherforecast_should_contain_data()
    {
        // PREPARE
        FakeRandom.Int32Next = FixedNextStrategy.From(2);
        FakeTime.SetNow("2005-05-05T15:15:15Z");

        // ACT
        var rStr = await Client.TestGetStringAsync("/api/v1/public/weatherforecast");

        // ASSERT
        rStr
            .ParseJToken()
            .Should()
            .ContainSubtree("""
            [
              {
                "timestamp": "2005-05-05T15:15:15Z",
                "date": "2005-05-06",
                "temperature_c": 2,
                "summary": "Chilly",
                "temperature_f": 35,
              }
            ]
            """.ParseJToken());
    }
}