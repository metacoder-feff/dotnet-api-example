using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;

namespace FEFF.Extentions.Testing.SignalR;

public static class TestServerSignalrExtentions
{
    public static SignalrTestClient CreateSignalRClient(this TestServer server, string url, string? token = null)
    {
        var c =  new HubConnectionBuilder()
        .WithUrl(
            url,
            o =>
            {
                o.HttpMessageHandlerFactory = _ => server.CreateHandler();
                if(token != null)
                    o.AccessTokenProvider = () => Task.FromResult(token)!;
            }
        )
        .Build();

        return new SignalrTestClient(c);
    }
}