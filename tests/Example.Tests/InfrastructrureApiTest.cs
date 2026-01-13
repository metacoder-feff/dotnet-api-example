using System.Net;

namespace Example.Tests;

public class InfrastructrureApiTest : ApiTestBase
{
    [Theory]
    [InlineData(AspEnvironment.Development, HttpStatusCode.OK)]
    [InlineData(AspEnvironment.Production , HttpStatusCode.NotFound)]
    public async Task Test_swagger_ui_enabled(AspEnvironment env, HttpStatusCode res)
    {
        _appFactory.SetAspEnvironment(env);

        var body = await Client.GetStringAsync("/swagger", res);

        if (env == AspEnvironment.Development)
            body.Should().Contain("""
            <div id="swagger-ui">
            """);
    }

    [Theory]
    [InlineData(AspEnvironment.Development, HttpStatusCode.OK)]
    [InlineData(AspEnvironment.Production , HttpStatusCode.NotFound)]
    public async Task Test_openapi_enabled(AspEnvironment env, HttpStatusCode res)
    {
        _appFactory.SetAspEnvironment(env);

        var body = await Client.GetStringAsync("/openapi/v1.json", res);

        if (env == AspEnvironment.Development)
            body.Should().Contain("openapi");
    }
    
    /// <summary>
    /// If intended to change API, run this test locally and push updated "tests/Files/openapi.json" to repo.
    /// </summary>
    [Fact]
    public async Task OpenAPI_json__update()
    {
//TODO: attribute, see tests/tmp/Ext.Exa/SupportedOS.cs        
        Assert.SkipWhen(IsCI, "Only for local-dev");
        await AssertOrUpdateOpenAPI(false);
    }

    /// <summary>
    /// This check aims to avoid accidentally changing of public API. Runs on CI only.
    /// If intended to change API, then run test "OpenAPI_json__update" locally and push updated "tests/Files/openapi.json" to repo.
    /// This patch would be reviewed at MR/PR.
    /// </summary>
    [Fact]
    public async Task OpenAPI_json__should_not_change()
    {
//TODO: attribute
        Assert.SkipUnless(IsCI, "Only for CI");
        await AssertOrUpdateOpenAPI(true);
    }

    private async Task AssertOrUpdateOpenAPI(bool isCI)
    {
        var body = await Client.GetStringAsync("/openapi/v1.json");

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

    [Fact]
    public async Task Test_Metrics()
    {
        var body = await Client.GetStringAsync("/metrics");

        body.Should().Contain("dotnet_collection_count_total counter");
    }
}