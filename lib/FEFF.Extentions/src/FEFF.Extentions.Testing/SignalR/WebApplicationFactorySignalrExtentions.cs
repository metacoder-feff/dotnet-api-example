using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;

namespace FEFF.Extentions.Testing;

public static class WebApplicationFactorySignalrExtentions
{
    public static HubConnection CreateSignalRConnection<T>(this WebApplicationFactory<T> factory, string url, string token)
    where T : class
    {
        return new HubConnectionBuilder()
        .WithUrl(
            url,
            o =>
            {
                o.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
                o.AccessTokenProvider = () => Task.FromResult(token)!;
            }
        )
        .Build();
    }

    public static HubConnection CreateSignalRConnection<T>(this WebApplicationFactory<T> factory, string url)
    where T : class
    {
        return new HubConnectionBuilder()
        .WithUrl(
            url,
            o =>
            {
                o.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
            }
        )
        .Build();
    }
    
    public static SignalrTestClient CreateSignalRClient(this ITestApplication factory, string url)
    {
        var c =  new HubConnectionBuilder()
        .WithUrl(
            url,
            o =>
            {
                o.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
            }
        )
        .Build();

        return new SignalrTestClient(c);
    }
}