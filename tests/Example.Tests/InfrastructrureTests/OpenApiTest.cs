namespace Example.Tests.InfrastructrureTests;

public class OpenApiTest : ApiTestBase
{
    [Theory]
    [InlineData(AspEnvironment.Development, HttpStatusCode.OK)]
    [InlineData(AspEnvironment.Production , HttpStatusCode.NotFound)]
    public async Task Swagger_ui_should_be_enabled_if(AspEnvironment env, HttpStatusCode res)
    {
        Factory.BuilderOverrider.UseAspEnvironment(env);

        var body = await Client.TestGetStringAsync("/swagger", res);

        if (env == AspEnvironment.Development)
            body.Should().Contain("""
            <div id="swagger-ui">
            """);
    }

    [Theory]
    [InlineData(AspEnvironment.Development, HttpStatusCode.OK)]
    [InlineData(AspEnvironment.Production , HttpStatusCode.NotFound)]
    public async Task OpenAPI_should_be_enabled_if(AspEnvironment env, HttpStatusCode res)
    {
        Factory.BuilderOverrider.UseAspEnvironment(env);

        var body = await Client.TestGetStringAsync("/openapi/v1.json", res);

        if (env == AspEnvironment.Development)
            body.Should().Contain("openapi");
    }
    
    /// <summary>
    /// If intended to change API, run this test locally and push updated "tests/Files/openapi.json" to repo.
    /// </summary>
    [Fact]
    [SkipWhenCI("Only for local-dev")]
    public async Task OpenAPI_json__update()
    {
        await AssertOrUpdateOpenAPI(false);
    }

    /// <summary>
    /// This check aims to avoid accidentally changing of public API. Runs on CI only.
    /// If intended to change API, then run test "OpenAPI_json__update" locally and push updated "tests/Files/openapi.json" to repo.
    /// This patch would be reviewed at MR/PR.
    /// </summary>
    [Fact]
    [SkipUnlessCI]
    public async Task OpenAPI_json__should_not_be_changed()
    {
        await AssertOrUpdateOpenAPI(true);
    }

    private async Task AssertOrUpdateOpenAPI(bool isCI)
    {
        var body = await Client.TestGetStringAsync("/openapi/v1.json");

        var targetFile = "../../../../Files/openapi.json";
        var changesFile = targetFile + ".modified.json";

        // for local dev
        if(isCI == false)
        {
            File.WriteAllText(targetFile, body);
            return;
        }

        // for CI
        var stored = File.ReadAllText(targetFile);
        if (stored == body)
            return;

        File.WriteAllText(changesFile, body);
        body
            .ParseJToken()
            .Should()
            .BeEquivalentTo(stored);
            //.BeEquivalentToEX(stored);
    }
}