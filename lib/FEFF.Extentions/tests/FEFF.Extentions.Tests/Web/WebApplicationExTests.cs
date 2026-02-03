using Microsoft.AspNetCore.Builder;

namespace FEFF.Extentions.Tests.Web;

using FEFF.Extentions.Web;

public class WebApplicationExTests
{
    //DisableReloadConfigByDefault__should__change_WebApplicationBuilder_when__env_is_not_set
    [Theory]
    // When Env is set the action changes nothing
    [InlineData("true" , false , true)]
    [InlineData("true" , true  , true)]
    [InlineData("false", false , false)]
    [InlineData("false", true  , false)]
    // Initial WebApplication behavior if Env is not set (action NOT applied)
    [InlineData(null   , false , true)]
    // NEW WebApplication behavior if Env is not set (action IS applied)
    [InlineData(null   , true  , false)]
    [RestoreProcessEnvironmentAfterTest]
    public void DisableReloadConfigByDefault(string? initialEnvValue, bool actionIsApplied, bool expected)
    {
        // PREPARE
        Environment.SetEnvironmentVariable("DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE", initialEnvValue);

        // ACT
        if(actionIsApplied)
            ReloadConfigHelper.DisableReloadConfigByDefault();

        var builder = WebApplication.CreateBuilder();

        // Assert
        var reloadValue = builder.Configuration.GetReloadConfigOnChangeValue();

        reloadValue.Should().Be(expected);
    }
}