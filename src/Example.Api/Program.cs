using Example.Api;

var builder = WebApplication.CreateBuilder(args);
//TODO: setup logger

//TODO: log errors?
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
//TODO: test??   
    app.Logger.LogCritical(e, "Error at 'App Setup/Run' stage.");
}