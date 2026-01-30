using Microsoft.AspNetCore.SignalR;

namespace Example.Api.SignalR;

//TODO: sync via internal queue
public interface IEventSender
{
    //Task SendFinishedOkAsync(string userId);
    Task SendFinishedOkAsync();
}

public class EventSender : IEventSender
{
    public const string MethodName = "finished_ok";
    private readonly IHubContext<EventHub> _hubCtx;

    public EventSender(IHubContext<EventHub> hubCtx)
    {
        _hubCtx = hubCtx;
    }

    // public async Task SendFinishedOkAsync(string userId)
    // {
    //     await _hubCtx.Clients.User(userId).SendAsync(MethodName);
    // }

    public async Task SendFinishedOkAsync()
    {
        var d = new {Result = "ok" };
        await _hubCtx.Clients.All.SendAsync(MethodName, d);
    }
}