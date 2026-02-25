using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NodaTime.Serialization.SystemTextJson;
using Npgsql;
using Prometheus;
using StackExchange.Redis.Configuration;

using FEFF.Extentions.EntityFrameworkCore;
using FEFF.Extentions.HealthChecks;
using FEFF.Extentions.Jwt;
using FEFF.Extentions.OpenApi.NodaTime;
using FEFF.Extentions.Redis;

namespace Example.Api;
using SignalR;

static class InfrastructureModule
{
    public const string PgConnectionStringName = "PgDb";
    public const string JwtOptionsSection = "JWT";

    public const string UserAuthPolicyName = "User";

    public static void SetupServices(IServiceCollection services)
    {
        /*------------------------------------------------*/
        // Configure serialization for minimal API
        /*------------------------------------------------*/
        services.ConfigureHttpJsonOptions(o => 
            ConfigureJsonSerializer(o.SerializerOptions)
        );

        /*------------------------------------------------*/
        // JWT Authentication & Authorization policy
        /*------------------------------------------------*/
        services.AddAuthentication()
            .AddSymmetricJwt(JwtOptionsSection, configure: static opt =>
            {
//TODO: const
                // for SignalR
                opt.AddQueryStringAuthentication(x => x.StartsWithSegments("/events"));
                
            });
        //services.AddAuthorization();
        services.AddAuthorizationBuilder()
            .AddPolicy(UserAuthPolicyName, policy => policy
                .RequireRole("user")
                //.RequireClaim("scope", "greetings_api")
            );


        /*------------------------------------------------*/
        // OpenApi
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        /*------------------------------------------------*/
        services.AddOpenApi(ConfigureOpenApi);
        
        // openapi compile error:
        // The 'interceptors' feature is not enabled in this namespace. Add '<InterceptorsNamespaces>$(InterceptorsNamespaces);Microsoft.AspNetCore.OpenApi.Generated</InterceptorsNamespaces>' to your project.
        // 
        // services.AddOpenApi(o =>
        //     o.ConfigureNodaTime()
        // );
        // workaround - add to csproj
        // <InterceptorsNamespaces>$(InterceptorsNamespaces);Microsoft.AspNetCore.OpenApi.Generated</InterceptorsNamespaces>

        /*------------------------------------------------*/
        // Redis connections (self managed)
        // the connection is managed (created and disposed) by RedisConnectionManager singleton service
        /*------------------------------------------------*/
        services.AddRedis<RedisConnectionManager>(ConfigureRedis);
        // we can register multiple subclases of 'RedisConnectionManager' but here we use only one
        // Add interfaces to registred above services
        // for easier use
        services.AddRedisInterfacesFor<RedisConnectionManager>();

        /*------------------------------------------------*/
        // SignalR
        /*------------------------------------------------*/
        services.AddSignalR()
            .AddJsonProtocol(o => 
                ConfigureJsonSerializer(o.PayloadSerializerOptions)
            )
            // provides a connection for SignalR via ConnectionFactory
            // and exports the connection to the healthcheck
            // the connection is managed (requested and disposed) by SignalR singleton service
            .AddRedisWithHealthCheckProxy(ConfigureRedis)
            ;

        /*------------------------------------------------*/
        // DB (Postgres)
        /*------------------------------------------------*/
        services.AddTimeProvider(); // for CreatedAtUpdatedAtInterceptor
        services.AddTransient<CreatedAtUpdatedAtInterceptor>();
        services.AddDbContext<WeatherContext>((sp, opt) =>
        {
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

        /*------------------------------------------------*/
        // Health
        /*------------------------------------------------*/
        services.AddHealthChecks()
            // liveness:
            .AddSimpleLivenessCheck()
            // readiness:
            .AddDbContextCheck<WeatherContext>(tags: [HealthCheckTag.Readiness])
            // overview - all above plus:
            .AddRedisConnectionForSignalRCheck()
            .AddRedisConnectionCheck<RedisConnectionManager>()
            ;
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
//TODO: api/v1/public & jwt
        app.MapHub<EventHub>("/events", opts =>
        {
            // close whenever jwt-auth-token expires 
            // see https://learn.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz?view=aspnetcore-8.0#authenticate-users-connecting-to-a-signalr-hub
            opts.CloseOnAuthenticationExpiration = true;
        });
        
        // Apps typically don't need to call UseRouting or UseEndpoints. 
        // WebApplicationBuilder configures a middleware pipeline that wraps middleware added in Program.cs with UseRouting and UseEndpoints.
        // Calling UseAuthentication and UseAuthorization adds the authentication and authorization middleware. 
        // These middleware are placed between UseRouting and UseEndpoints.
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-10.0

        // By default, the WebApplication automatically registers the authentication and authorization middlewares if certain authentication and authorization services are enabled. 
        // In the following sample, it's not necessary to invoke UseAuthentication or UseAuthorization to register the middlewares because WebApplication does this automatically after AddAuthentication or AddAuthorization are called.
        // In some cases, such as controlling middleware order, it's necessary to explicitly register authentication and authorization.
        // In the following sample, the authentication middleware runs after the CORS middleware has run.
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/security?view=aspnetcore-10.0
        
        // app.UseCors();
        // app.UseAuthentication();
        // app.UseAuthorization();
    }

    private static void ConfigureOpenApi(OpenApiOptions o)
    {
        o.ConfigureNodaTime();
//TODO: add JWT        
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

        o.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
    }

    // use same redis configurations for both connections: 
    // RedisConnectionManager & SignalR
    private static void ConfigureRedis(IRedisConfigurationFactoryBuilder builder)
    {
        builder
            .ReadConnectionString("Redis")
            .SetLoggerFactory()
            .Configure(options =>
            {
                // Enable reconnecting by default
                options.AbortOnConnectFail = false;

                // FROM 'SignalR.Connect()'
                // suffix SignalR onto the declared library name
                var provider = DefaultOptionsProvider.GetProvider(options.EndPoints);
                options.LibraryName = $"{provider.LibraryName} FEFF+SignalR";

                // trust all cerificates
                options.CertificateValidation += delegate { return true; };
//TODO: test with self-signed CA
                //options.CertificateSelection  += SelectLocalCertificate;
                //options.TrustIssuer("CA-path"); // or X509-obj (overload)

                // also we can set ENV_variable: "SERedis_IssuerCertPath" targeting at "CA-path"
                // see: https://github.com/StackExchange/StackExchange.Redis/blob/main/src/StackExchange.Redis/PhysicalConnection.cs#L1470C70-L1470C92
            });
    }
}