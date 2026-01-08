using Example.Api.SignalR;

namespace Example.Api;

static class SignalRModule
{
    public const string SignalRPath = "/events";
    
    // internal static void SetupServices(IServiceCollection services)
    // {
    // }

    internal static void SetupPipeline(IEndpointRouteBuilder app)
    {
        app.MapHub<EventHub>(SignalRPath, opts =>
        {
            // close whenever jwt-auth-token expires 
            // see https://learn.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz?view=aspnetcore-8.0#authenticate-users-connecting-to-a-signalr-hub
            opts.CloseOnAuthenticationExpiration = true;
        });
    }
}