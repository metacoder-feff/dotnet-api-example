using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace FEFF.Extentions.JWT;

//TODO: multiple SecurityScheme ??
//TODO: automate AuthenticationScheme loookup ?? + map AuthenticationScheme.Name -> SecurityScheme.Name
public static class OpenApiExtentions
{
    /// <summary>
    /// Add BearerSecurityScheme (JWT) to OpenApi document.<br/>
    /// Add SecurityRequirements of this scheme to operations.<br/>
    /// Add 401,403 responses to operations.<br/>
    /// Operations are filtered by authorization/anonymous metadata.
    /// </summary>
    /// <param name="openApiSecuritySchemeName">A name of a new security scheme in the OpenApi document. Also shown in swagger UI.</param>
    /// <param name="loginPathHint">A hint shown in a description of the security scheme.</param>
    /// <returns></returns>
    public static OpenApiOptions AddJwtBearerSecurity(this OpenApiOptions src, string openApiSecuritySchemeName = "Bearer-JWT", string? loginPathHint = null)
    //public static OpenApiOptions AddJwtBearerSecurity(this OpenApiOptions src, string openApiSecuritySchemeName = JwtBearerDefaults.AuthenticationScheme, string? loginPathHint = null)
    {
        return src
            .AddBearerSecurityScheme(openApiSecuritySchemeName, loginPathHint)
            .AddOperationsSecurityRequirements(openApiSecuritySchemeName)
            ;
    }

//TODO: automate SecurityScheme loookup ??
//TODO: multiple SecurityScheme ??
    internal static OpenApiOptions AddOperationsSecurityRequirements(this OpenApiOptions src, string schemeName)
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
        var xx = context.Description.ActionDescriptor.EndpointMetadata.OfType<IAuthorizeData>().ToList();
        return context.Description.ActionDescriptor.EndpointMetadata.OfType<IAuthorizeData>().Any();
    }

    internal static OpenApiOptions AddBearerSecurityScheme(this OpenApiOptions src, string schemeName, string? loginPath)
    {
        var loginDescr = "";
        if(loginPath.IsNullOrEmpty() == false)
            loginDescr = $" from '{loginPath}' output";

        src.AddDocumentTransformer( (document, context, cancellationToken) =>
        {
            var securitySchemes = new Dictionary<string, IOpenApiSecurityScheme>
            {
                [schemeName] = new OpenApiSecurityScheme
                {
                    Description  = 
                        $"Please enter token{loginDescr}.\n" +
                        "Example: 'eyJh...mA' (without quotes).",
                    Name         = "Authorization",
                    Type         = SecuritySchemeType.Http,
                    In           = ParameterLocation.Header,
                    Scheme       = HttpClientExtentions.BearerAuthHeader, // "bearer" refers to the header name here (RFC 7235)
                    BearerFormat = "Json Web Token (JWT)",
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

// Transformer class allows to inject services
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