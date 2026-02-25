using System.Security.Claims;

namespace Microsoft.Extensions.DependencyInjection;
using FEFF.Extentions.Jwt;

/*
You can also pass the token in as a paramater in the query string instead of as a header or a cookie (ex: /protected?jwt=<TOKEN>). 
However, in almost all cases it is recomended that you do not do this, as it comes with some security issues. 
If you perform a GET request with a JWT in the query param, it is possible that the browser will save the URL, which could lead to a leaked token. 
It is also very likely that your backend (such as nginx or uwsgi) could log the full url paths, which is obviously not ideal from a security standpoint.
*/


// 'IJwtProvider' is used by 'AuthController'
public interface IJwtFactory
{
    string CreateToken(IEnumerable<Claim> claims);
}

public static class ServiceCollectionExtentions
{
    /// <summary>
    /// Register "IJwtProvider" that can create JWTs.
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
        // register app opts
        // ex. 'Jwt__SecretKey=.....'
        services.AddOptions<JwtOptions>()
                .BindConfiguration(configSectionPath)
                //.ValidationHelper().ValidateBy<JwtOptionsValidator>()
                .ValidateOnStart();

        // register 'IConfigureNamedOptions<JwtBearerOptions>' realization
        // to cofigure 'JwtBearerOptions' on demand, 
        // is used in 'AddJwtBearer'
        services.ConfigureOptions<SymmetricJwtService>();

        // add regular JwtAuthentication (from manuals)
        // is used in request pipeline
        //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        services.AddAuthentication()
                .AddJwtBearer(static o => 
                /* also is configured by JwtService.Configure(...)*/
                {
//TODO: const
                    // for SignalR
                    o.AddQueryStringAuthentication(x => x.StartsWithSegments("/events"));
                });

        services.AddScoped<IJwtFactory, SymmetricJwtService>();

        return services;
    }
}