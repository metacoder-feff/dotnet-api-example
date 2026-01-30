//using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Example.Api.SignalR;

//[Authorize]
public class EventHub : Hub
{
    // public async Task SendMessage(string user, string message)
    // {
    //     await Clients.All.SendAsync("ReceiveMessage", user, message);
    // }
}
