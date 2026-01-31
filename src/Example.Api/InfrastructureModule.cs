using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NodaTime.Serialization.SystemTextJson;
using Npgsql;
using Prometheus;

using FEFF.Extentions.EntityFrameworkCore;
using FEFF.Extentions.HealthChecks;
using FEFF.Extentions.OpenApi.NodaTime;
using FEFF.Extentions.Redis;

using Example.Api.SignalR;

namespace Example.Api;

static class InfrastructureModule
{
    public const string PgConnectionStringName = "PgDb";

    internal static void SetupConfiguration(ConfigurationManager configuration, IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            var reloadOnChange = configuration.GetReloadConfigOnChangeValue();
            configuration
                .AddJsonFile("appsettings.secrets.json", optional: true, reloadOnChange: reloadOnChange);
        }
    }

    public static void SetupServices(IServiceCollection services)
    {
        services.AddStdCloudLogging();

        services.ConfigureHttpJsonOptions(o => 
            ConfigureJsonSerializer(o.SerializerOptions)
        );

        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        services.AddOpenApi(ConfigureOpenApi);

        /*------------------------------------------------*/
        // Health
        /*------------------------------------------------*/
        services.AddHealthChecks()
                .AddSimpleLivenessCheck()
                // readiness
                .AddDbContextCheck<WeatherContext>(tags: [HealthCheckTag.Readiness])
                // overview
                .AddCheck<RedisHealthCheck>("Redis");
                ;

        /*------------------------------------------------*/
        // Redis
        /*------------------------------------------------*/
        services.AddRedisConnectrionManager();

        /*------------------------------------------------*/
        // SignalR
        /*------------------------------------------------*/
        services.AddSignalR()
            .AddJsonProtocol(o => 
                ConfigureJsonSerializer(o.PayloadSerializerOptions)
            );
        /*------------------------------------------------*/
        // DB
        /*------------------------------------------------*/
        services.AddTimeProvider(); // for CreatedAtUpdatedAtInterceptor
        services.AddTransient<CreatedAtUpdatedAtInterceptor>();
        services.AddDbContext<WeatherContext>((sp, opt) =>
        {
            //opt.SetupContextOptions(pgConnectionStringName, sp, "primary");
            //opt.SetupContextOptions(pgConnectionStringName, sp, null);

            var connstr = sp.GetRequiredConnectionString(PgConnectionStringName);
            opt.UseNpgsql(
                connstr,
                o => o
                    .UseNodaTime()
                    //.ConfigureWith(WeatherContext.MapEnums)
            );

            var i = sp.GetRequiredService<CreatedAtUpdatedAtInterceptor>();
            opt.AddInterceptors(i);
        });
    }

    private static void ConfigureOpenApi(OpenApiOptions o)
    {
        o.ConfigureNodaTime();
    }

    public static void SetupPipeline(WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            app.UseSwaggerUI(options =>
            {
                options.EnableDeepLinking();
                options.SwaggerEndpoint("/openapi/v1.json", "v1");
            });
        }

        app.UseHttpsRedirection();

        // Enable the /metrics page to export Prometheus metrics.
        // Metrics published in this sample:
        // * built-in process metrics giving basic information about the .NET runtime (enabled by default)
        // * metrics from .NET Event Counters (enabled by default, updated every 10 seconds)
        // * metrics from .NET Meters (enabled by default)
        // ref: https://github.com/prometheus-net/prometheus-net/blob/master/Sample.Web/Program.cs
        app.MapMetrics();

        app.MapStdHealthChecks();

        app.MapHub<EventHub>("/events", opts =>
        {
            // close whenever jwt-auth-token expires 
            // see https://learn.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz?view=aspnetcore-8.0#authenticate-users-connecting-to-a-signalr-hub
            opts.CloseOnAuthenticationExpiration = true;
        });
    }

    internal static void ConfigureJsonSerializer(JsonSerializerOptions o)
    {
        o.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        o.Converters.Add(
            new JsonStringEnumConverter(namingPolicy: JsonNamingPolicy.SnakeCaseLower)
        );
        o.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        o.AllowTrailingCommas    = true;
        o.ReadCommentHandling    = JsonCommentHandling.Skip;

        o.ConfigureForNodaTime(NodaTime.DateTimeZoneProviders.Tzdb);
    }
}