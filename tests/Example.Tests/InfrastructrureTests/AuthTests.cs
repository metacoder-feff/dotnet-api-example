namespace Example.Tests.InfrastructrureTests;

public class AuthTests : ApiTestBase
{
    [Theory]
    [InlineData(true, HttpStatusCode.OK)]
    [InlineData(false, HttpStatusCode.Unauthorized)]
    public async Task SignalR_HttpStatus_should_depend_on_authorization(bool authorize, HttpStatusCode expected)
    {
        string? token = null;
        if(authorize == true)
        {
            token = await LoginAsync();
        }

//TODO: DRY
//TODO: fixture SignalRClient??
        await using var signalr = TestApplication.CreateSignalRClient("/api/v1/public/events", token);

        // Act
        var act = () => signalr.StartAsync();

        // Assert
        if(authorize)
        {
            await act.Should()
                    .NotThrowAsync();
        }
        else
        {
            await act.Should()
                    .ThrowAsync<HttpRequestException>()
                    .Where(e => e.StatusCode == expected);
        }
    }

    [Theory]
    [InlineData(true, HttpStatusCode.OK)]
    [InlineData(false, HttpStatusCode.Unauthorized)]
    public async Task Api_HttpStatus_should_depend_on_authorization(bool authorize, HttpStatusCode expected)
    {
        if(authorize == true)
        {
            var token = await LoginAsync();
            Client.AddBearerHeader(token);
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