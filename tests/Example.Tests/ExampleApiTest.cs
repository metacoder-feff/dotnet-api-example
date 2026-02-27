using FEFF.Extentions.Jwt;

namespace Example.Tests;
using Example.Api;

public class ExampleApiTest : ApiTestBase
{
    [Fact]
    public async Task Weatherforecast_should_contain_data()
    {
        // PREPARE
        FakeRandom.IntStrategy = FakeRandom.ConstStrategy(2);
        FakeTime.SetNow("2005-05-05T15:15:15Z");

        AuthorizeClient();

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
                "temperature_c": -18,
                "summary": "Chilly",
                "temperature_f": 0,
              }
            ]
            """.ParseJToken());
    }

    private void AuthorizeClient()
    {
//TODO: DRY
//TODO: fixture AuthorizedClient
        var jwt = GetRequiredService<IJwtFactory>();
        var token = LoginApiModule.CreateToken(jwt, "testuser");
        Client.AddBearerHeader(token);
    }
}