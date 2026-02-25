using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;
using FEFF.Extentions.Jwt;

public static class ServiceCollectionExtentions
{
//TODO: better registry
//TODO: better doc
    /// <summary>
    /// Register "IJwtFactory" that can create JWTs.
    /// And also register system services that can validate these JWTs.
    /// Both are configured via same "JwtOptions":
    ///   "configSectionPath": {
    ///     "SecretKey"     : "***", // min 32 bytes, KEEP THIS IN SECRET!!!.
    ///     "Issuer"        : "***",
    ///     "Audience"      : "***",
    ///     "TokenLifeTime" : "d.HH:mm:ss "
    ///   }
    /// </summary>
    public static IServiceCollection AddJwtBearerAuthenticationServices(this IServiceCollection services, string configSectionPath)
    {
//TODO: validate
        // register app opts
        // ex. 'Jwt__SecretKey=.....'
        services.AddOptions<JwtOptions>()
                .BindConfiguration(configSectionPath)
                //.ValidationHelper().ValidateBy<JwtOptionsValidator>()
                .ValidateOnStart();

        //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        services.AddAuthentication()
                .AddJwtBearer(static o => 
                {
//TODO: const
                    // for SignalR
                    o.AddQueryStringAuthentication(x => x.StartsWithSegments("/events"));
                });

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>( (dst, src) =>
            {
                dst.TokenValidationParameters.ValidIssuer      = src.Value.Issuer;
                dst.TokenValidationParameters.ValidAudience    = src.Value.Audience;
                dst.TokenValidationParameters.IssuerSigningKey = src.Value.GetKey();

                if(src.Value.TimeProvider != null)
                    dst.TimeProvider = src.Value.TimeProvider;
            });

        services.TryAddTransient<IJwtFactory, JwtFactory>();

        return services;
    }
}