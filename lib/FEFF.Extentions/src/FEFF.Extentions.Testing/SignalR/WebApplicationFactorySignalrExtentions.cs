using Microsoft.AspNetCore.SignalR.Client;

namespace FEFF.Extentions.Testing;

public static class WebApplicationFactorySignalrExtentions
{
    public static SignalrTestClient CreateSignalRClient(this ITestApplication factory, string url, string? token = null)
    {
        var c =  new HubConnectionBuilder()
        .WithUrl(
            url,
            o =>
            {
                o.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
                if(token != null)
                    o.AccessTokenProvider = () => Task.FromResult(token)!;
            }
        )
        .Build();

        return new SignalrTestClient(c);
    }
}