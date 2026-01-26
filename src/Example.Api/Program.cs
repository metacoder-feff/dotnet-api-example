using Example.Api;

// setup ulimit otherwise
// to fix linux exception:
//   "The configured user limit (128) on the number of inotify..."
//EnvironmentHelper.DisableReloadConfigByDefault();

// WebApplication.CreateBuilder uses constructor new WebApplicationBuilder(...)
// which uses ConfigurationManager values to add config files to ConfigurationManager
// At that moment ConfigurationManager is set only from Environment 
// by: EnvironmentVariablesExtensions.AddEnvironmentVariables(this ..., string? prefix)
var builder = WebApplication.CreateBuilder(args);

//TODO: log errors until build completes?
InfrastructureModule.SetupConfiguration(builder.Configuration, builder.Environment);
InfrastructureModule.SetupServices(builder.Services);
ExampleApiModule.SetupServices(builder.Services);

var app = builder.Build();

try
{
    InfrastructureModule.SetupPipeline(app);
    ExampleApiModule.SetupPipeline(app);

    app.Run();
}
catch (Exception e)
{
    app.Logger.LogCritical(e, "Error at 'App Setup/Run' stage.");
}