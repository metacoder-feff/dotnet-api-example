using FEFF.Extentions.Jwt;

namespace Example.Tests.InfrastructrureTests;

public class AuthTests : ApiTestBase
{
    [Theory]
    [InlineData(true, HttpStatusCode.OK)]
    [InlineData(false, HttpStatusCode.Unauthorized)]
    public async Task Api_HttpStatus_should_depend_on_authorization(bool authorize, HttpStatusCode expected)
    {
        if(authorize == true)
        {
            var token = await LoginAsync();
            Client.AddJwtHeader(token);
        }

        // ACT & Assert
        _ = await Client.TestGetStringAsync("/api/v1/public/weatherforecast", expected);
    }

    private async Task<string> LoginAsync()
    {
        var msg = """
        {
            "username" : "uuuu",
            "password" : "12345"
        }
        """;
        var rStr = await Client.TestPostAsync("/api/v1/public/login", msg);
        var r = (string?)rStr.ParseJToken()["token"];
        
        r.Should().NotBeNullOrEmpty();
        return r;
    }
}