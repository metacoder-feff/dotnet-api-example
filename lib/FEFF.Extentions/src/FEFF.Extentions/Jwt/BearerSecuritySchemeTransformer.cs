using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace FEFF.Extentions.JWT;

// see also: MMonrad.OpenApi

public static class OpenApiExtentions
{
    public static OpenApiOptions AddBearerSecurity(this OpenApiOptions src, string schemeName = JwtBearerDefaults.AuthenticationScheme, string loginPath = "/login")
    {
        return src
            .AddBearerSecurityScheme(schemeName, loginPath)
            .AddOperations(schemeName)
            ;
    }

    internal static OpenApiOptions AddOperations(this OpenApiOptions src, string schemeName)
    {
        src.AddOperationTransformer((operation, context, cancellationToken) =>
        {
            // Check if the endpoint has authorization/anonymous metadata
            if (IsAuthorizationRequired(context) == false)
                return Task.CompletedTask;
            if (IsAnonymousAllowed(context) == true)
                return Task.CompletedTask;

            operation.Security ??= [];
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(schemeName, context.Document)] = []
            });

            operation.Responses ??= [];
            operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
            operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });

            return Task.CompletedTask;
        });
        return src;
    }

    private static bool IsAnonymousAllowed(OpenApiOperationTransformerContext context)
    {
        return context.Description.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any();
    }

    private static bool IsAuthorizationRequired(OpenApiOperationTransformerContext context)
    {
        return context.Description.ActionDescriptor.EndpointMetadata.OfType<IAuthorizeData>().Any();
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
            // foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations ?? []))
            // {
            //     operation.Value.Security ??= [];
            //     operation.Value.Security.Add(new OpenApiSecurityRequirement
            //     {
            //         [new OpenApiSecuritySchemeReference(schemeName, document)] = []
            //     });
            // }

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