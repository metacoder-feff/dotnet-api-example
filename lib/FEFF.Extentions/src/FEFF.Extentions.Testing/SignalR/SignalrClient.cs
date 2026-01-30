using System.Text.Json.Nodes;
using System.Threading.Channels;
using Microsoft.AspNetCore.SignalR.Client;

namespace FEFF.Extentions.Testing;

public static class ChannelEx
{
    public static async Task<T?> TryReadAsync<T>(this ChannelReader<T> src, TimeSpan timeout, CancellationToken cancellationToken)
    where T : notnull
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout);

        var timeoutToken = timeoutCts.Token;
        
        try
        {
            return await src.ReadAsync(timeoutToken);
        }
        catch (OperationCanceledException e) 
        when (e.CancellationToken == timeoutToken 
            && timeoutCts.IsCancellationRequested == true
            && cancellationToken.IsCancellationRequested == false)
        {
            return default;
        }
    }
}

public sealed class SignalrClient
{
    private readonly Channel<ServerEvent> _eventsQueue = Channel.CreateUnbounded<ServerEvent>();
    private readonly HubConnection _connection;

    public SignalrClient(HubConnection connection)
    {
        _connection = connection;
    }

    public void Subscribe(string methodName)
    {
//TODO: return valuse auto dispose?
//TODO: better handler?
        _ = _connection.On(methodName, (JsonNode x) => SignalRHandler(methodName, x));
        //_ = _connection.On(methodName, [typeof(JsonNode)], SignalRHandler1);
    }

    private async Task SignalRHandler(string methodName, JsonNode arg)
    {
        var e = new ServerEvent(
            Method: methodName,
            Body:arg.ToString()
        );
        await _eventsQueue.Writer.WriteAsync(e);
    }

    public async Task<ServerEvent?> WaitForEvent(TimeSpan timeout, CancellationToken cancellationToken)
    {
        return await _eventsQueue.Reader.TryReadAsync(timeout, cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        return _connection.DisposeAsync();
    }

    public async Task StartAsync()
    {
        await _connection.StartAsync();
    }
}

//TODO: JsonNode Body
public record ServerEvent(string Method, string Body);