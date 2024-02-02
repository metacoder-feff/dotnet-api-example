using Microsoft.AspNetCore.Mvc.Testing;

namespace Example.Api.Tests.Fixtures;

public class ApiWebApplicationFactory: WebApplicationFactory<Program>
{
    // public IConfiguration Configuration { get; private set; }

    // protected override void ConfigureWebHost(IWebHostBuilder builder)
    // {
    //     builder.ConfigureAppConfiguration(config =>
    //     {
    //         Configuration = new ConfigurationBuilder()
    //           .AddJsonFile("integrationsettings.json")
    //           .Build();

    //         config.AddConfiguration(Configuration);
    //     });

    //     builder.ConfigureTestServices(services =>
    //     {
    //         services.AddTransient<IWeatherForecastConfigService, WeatherForecastConfigStub>();
    //     });
    // }
}

[Trait("Category", "Integration")]
public abstract class IntegrationTest: IClassFixture<ApiWebApplicationFactory>
{
    //private readonly Checkpoint _checkpoint = new Checkpoint
    //{
    //    SchemasToInclude = new[] {
    //    "Playground"
    //},
    //    WithReseed = true
    //};

    protected readonly ApiWebApplicationFactory _factory;
    protected readonly HttpClient _client;

    public IntegrationTest(ApiWebApplicationFactory fixture)
    {
        _factory = fixture;
        _client = _factory.CreateClient();

        // if needed, reset the DB
        //_checkpoint.Reset(_factory.Configuration.GetConnectionString("SQL")).Wait();
    }
}