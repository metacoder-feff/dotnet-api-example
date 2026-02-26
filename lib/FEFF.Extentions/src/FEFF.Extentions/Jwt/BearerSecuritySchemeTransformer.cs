using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace FEFF.Extentions.JWT;

// see also: MMonrad.OpenApi
// internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
// {
//     public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
//     {
//         var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
//         if (authenticationSchemes.Any(authScheme => authScheme.Name == JwtBearerDefaults.AuthenticationScheme))
//         {
//             document.Components ??= new OpenApiComponents();
//             document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

//             var scheme = new OpenApiSecurityScheme
//             {
//                 Type = SecuritySchemeType.Http,
//                 BearerFormat = "JSON Web Token",
//                 Description = "JWT Authorization header using the Bearer scheme.",
//                 Scheme = JwtBearerDefaults.AuthenticationScheme
//             };
//             document.Components.SecuritySchemes[JwtBearerDefaults.AuthenticationScheme] = scheme;

//             var requirement = new OpenApiSecurityRequirement
//             {
//                 [scheme] = new List<string>()
//             };
//             document.SecurityRequirements ??= new List<OpenApiSecurityRequirement>();
//             document.SecurityRequirements.Add(requirement);
//         }
//     }
// }

public sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
//TODO: select/multiple schemeName
        var schemeName = JwtBearerDefaults.AuthenticationScheme;
//TODO: remove authenticationSchemeProvider??
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (authenticationSchemes.Any(authScheme => authScheme.Name == schemeName) == false)
            return;

        var securitySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            [schemeName] = new OpenApiSecurityScheme
            {
                Description  = """
                    Please enter token from '/login' output. 
                    Example: 'eyJh...mA' (without quotes).
                """.Replace("\r\n", "\n"), // for windows
                Name         = "Authorization",
                In           = ParameterLocation.Header,
                Type         = SecuritySchemeType.Http,
                Scheme       = "Bearer", // "bearer" refers to the header name here
                BearerFormat = "JWT",
            }
        };
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = securitySchemes;
    }
}