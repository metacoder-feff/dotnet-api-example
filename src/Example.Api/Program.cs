using Example.Api;

var builder = WebApplication.CreateBuilder(args);

//TODO: log errors until build completes?
InfrastructureModule.SetupServices(builder.Services);
ExampleApiModule.SetupServices(builder.Services);

var app = builder.Build();

try
{
    InfrastructureModule.SetupPipeline(app);
    ExampleApiModule.SetupPipeline(app);

    app.Run();
}
catch(Exception e)
{
    app.Logger.LogCritical(e, "Error at 'App Setup/Run' stage.");
}