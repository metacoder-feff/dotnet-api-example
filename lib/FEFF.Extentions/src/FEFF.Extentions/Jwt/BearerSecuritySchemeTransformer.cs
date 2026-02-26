using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace FEFF.Extentions.JWT;

// see also: MMonrad.OpenApi

public static class OpenApiExtentions
{
    public static OpenApiOptions AddBearerSecurity(this OpenApiOptions src, string schemeName = JwtBearerDefaults.AuthenticationScheme, string loginPath = "/login")
    {
        return src.AddBearerSecurityScheme(schemeName, loginPath);
    }

    internal static OpenApiOptions AddBearerSecurityScheme(this OpenApiOptions src, string schemeName, string loginPath)
    {
        src.AddDocumentTransformer( (document, context, cancellationToken) =>
        {
            var securitySchemes = new Dictionary<string, IOpenApiSecurityScheme>
            {
                [schemeName] = new OpenApiSecurityScheme
                {
                    Description  = 
                        $"Please enter token from '{loginPath}' output.\n" +
                        "Example: 'eyJh...mA' (without quotes).",
                    Name         = "Authorization",
                    In           = ParameterLocation.Header,
                    Type         = SecuritySchemeType.Http,
                    Scheme       = "Bearer", // "bearer" refers to the header name here (RFC 7235)
                    BearerFormat = "JWT",
                }
            };
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = securitySchemes;

            // Add Security Requirement gloablly
            foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations ?? []))
            {
                operation.Value.Security ??= [];
                operation.Value.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference(schemeName, document)] = []
                });
            }
            return Task.CompletedTask;
        });
        return src;
    }
}

// Transformer class allows inject services
// in this example injected service is used to disable transform when scheme is not present:
//
// public sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
// {
//     public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
//     {
//         var schemeName = JwtBearerDefaults.AuthenticationScheme;

//         var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
//         if (authenticationSchemes.Any(authScheme => authScheme.Name == schemeName) == false)
//             return;
//     }
// }