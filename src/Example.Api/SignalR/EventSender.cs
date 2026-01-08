using Microsoft.AspNetCore.SignalR;
using NodaTime.Extensions;

namespace Example.Api.SignalR;

//TODO: sync via internal queue
public interface IEventSender
{
    Task SendFinishedOkAsync(string userId);
}

public class EventSender : IEventSender
{
    public const string MethodName = "finished_ok";
    private readonly IHubContext<EventHub> _hubCtx;
    private readonly TimeProvider _time;

    public EventSender(IHubContext<EventHub> hubCtx, TimeProvider time)
    {
        _hubCtx = hubCtx;
        _time = time;
    }

    // public async Task SendFinishedOkAsync(string userId)
    // {
    //     await _hubCtx.Clients.User(userId).SendAsync(MethodName);
    // }

    public async Task SendFinishedOkAsync(string userId)
    {
        var d = new {Result = "ok", FinishedAt = _time.GetCurrentInstant() };
        await _hubCtx.Clients
            .User(userId)
            .SendAsync(MethodName, d);
    }
}