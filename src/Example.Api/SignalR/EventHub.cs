using Microsoft.AspNetCore.SignalR;

namespace Example.Api.SignalR;

public class EventHub : Hub
{
    // public async Task SendMessage(string user, string message)
    // {
    //     await Clients.All.SendAsync("ReceiveMessage", user, message);
    // }
}
