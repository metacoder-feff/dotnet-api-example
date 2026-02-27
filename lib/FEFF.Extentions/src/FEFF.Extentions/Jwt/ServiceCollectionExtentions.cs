using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;
using FEFF.Extentions.Jwt;

public static class ServiceCollectionExtentions
{
    /// <summary>
    /// Register <see cref="IJwtFactory"/> that can create JWTs.<br/>
    /// Register <see cref="JwtBearerHandler"/> that validates JWTs.<br/>
    /// Both are configured via same <see cref="JwtOptions"/>:
    /// <code>
    ///   "configSectionPath": {
    ///     "SecretKey"     : "***", // min 32 bytes, KEEP THIS IN SECRET!!!.
    ///     "Issuer"        : "***",
    ///     "Audience"      : "***",
    ///     "TokenLifeTime" : "d.HH:mm:ss "
    ///   }
    /// </code>
    /// </summary>
    public static IServiceCollection AddSymmetricJwt(this AuthenticationBuilder builder, string configSectionPath, string authenticationScheme = JwtBearerDefaults.AuthenticationScheme, Action<JwtBearerOptions>? configure = null)
    {
        /// <remarks>
        /// We could configure ValidIssuer,ValidAudience and IssuerSigningKey via standard configuration and get them for token generation & validation.<br/>
        /// But we also need to configure TokenLifeTime.<br/>
        /// So decision is to place all options together into <see cref="JwtOptions"/>.
        /// </remarks>

        var services = builder.Services;

        // add JwtBearerHandler
        if(configure == null)
            builder.AddJwtBearer(authenticationScheme);
        else
            builder.AddJwtBearer(authenticationScheme, configure);

        // add IJwtFactory
        services.TryAddTransient<IJwtFactory, JwtFactory>();

        // add options for both IJwtFactory & JwtBearerHandler
        services.AddOptions<JwtOptions>()
                .BindConfiguration(configSectionPath)
                .ValidationHelper().ValidateBy<JwtOptionsValidator>()
                .ValidateOnStart();

        // configure JwtBearerHandler by JwtOptions
        services.AddOptions<JwtBearerOptions>(authenticationScheme)
            .Configure<IOptions<JwtOptions>>( (dst, src) =>
            {
                dst.TokenValidationParameters.ValidIssuer      = src.Value.Issuer;
                dst.TokenValidationParameters.ValidAudience    = src.Value.Audience;
                dst.TokenValidationParameters.IssuerSigningKey = src.Value.GetKey();

                if(src.Value.TimeProvider != null)
                    dst.TimeProvider = src.Value.TimeProvider;
            });

        return services;
    }
}