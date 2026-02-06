
using Example.Api;
using FEFF.Extentions.Web;

// *************************************
// MAIN
// *************************************
var app = TryCreateApp(args);

if(app == null)
    return -1;

return TryRunApp(app);


// *************************************
static WebApplication? TryCreateApp(string[] args)
{
    // create logger to use at app building stage
    using var logCtx = new SimpleLogContext(
        (b) => b.AddStdCloudLogging(),
        "TryCreateApp"
    );

    try
    {
        // setup ulimit otherwise
        // to fix linux exception:
        //   "The configured user limit (128) on the number of inotify..."
        //ReloadConfigHelper.DisableReloadConfigByDefault();

        // WebApplication.CreateBuilder uses constructor new WebApplicationBuilder(...)
        // which uses ConfigurationManager values to add config files to ConfigurationManager
        // At that moment ConfigurationManager is set only from Environment 
        // by: EnvironmentVariablesExtensions.AddEnvironmentVariables(this ..., string? prefix)
        var builder = WebApplication.CreateBuilder(args);
        //builder.Services.AddStdCloudLogging();

        InfrastructureModule.SetupConfiguration(builder.Configuration, builder.Environment);
        InfrastructureModule.SetupServices(builder.Services);
        ExampleApiModule.SetupServices(builder.Services);

        return builder.Build();
    }
    catch (Exception e)
    {
        logCtx.Logger.LogCritical(e, "Error at 'TryCreateApp' stage.");
        return null;
    }
}

// *************************************
static int TryRunApp(WebApplication app)
{
    try
    {
        InfrastructureModule.SetupPipeline(app);
        ExampleApiModule.SetupPipeline(app);

        app.Run();
        return 0;
    }
    catch (Exception e)
    {
        app.Logger.LogCritical(e, "Error at 'TryRunApp' stage.");
        return -1;
    }
}