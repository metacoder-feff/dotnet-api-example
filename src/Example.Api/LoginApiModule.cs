using System.Security.Claims;
using FEFF.Extentions.Jwt;

namespace Example.Api;

internal record LoginRequest(string Username, string Password);

static class LoginApiModule
{
    // SignalR uses ClaimTypes.NameIdentifier
    public const string ClaimTypeForUserId = ClaimTypes.NameIdentifier;
    public const string UserRoleName = "user";
    
    // internal static void SetupServices(IServiceCollection services)
    // {
    //     services.AddTimeProvider();
    //     services.AddRandom();
    //     services.AddTransient<IEventSender, EventSender>();
    // }

    internal static void SetupPipeline(IEndpointRouteBuilder app)
    {
        app
            .MapPost("/login", Login)
            .AllowAnonymous(); // Allow unauthenticated access to the login endpoint
    }

    private static IResult Login(LoginRequest request, IJwtFactory jwt)
    {
//TODO: log user scope
//TODO: 400 validator
//TODO: auth validator // Mock user validation (replace with actual user validation logic)
//TODO: openapi results
//TODO: signalR

        var res = Authorize(request);
        if (res == false)
            return Results.Unauthorized();
        
        var tokenString = CreateToken(jwt, request.Username);

        return Results.Ok(new { Token = tokenString });
    }

    private static bool Authorize(LoginRequest request)
    {
        // if (request.Username == "admin" && request.Password == "password")
        //     return true;
        // return false;
        return true;
    }

    internal static string CreateToken(IJwtFactory jwt, string userId)
    {
        // Define claims for the token
        // var claims = new[]
        // {
        //     new System.Security.Claims.Claim(System.Security.Claims.JwtRegisteredClaimNames.Sub, request.Username),
        //     new System.Security.Claims.Claim(System.Security.Claims.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        // };

        var claims = new List<Claim>
        {
            new(ClaimTypeForUserId, userId),
            new(ClaimTypes.Role, UserRoleName),
        };
        
        return jwt.CreateToken(claims);
    }
}