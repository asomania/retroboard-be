using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Retroboard.Api.Application.Interfaces;
using Retroboard.Api.Application.Models;

namespace Retroboard.Api.Api.Realtime;

public class BoardEventStream : IBoardEventPublisher, IBoardEventStream
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, Channel<BoardEvent>>> _subscriptions = new();

    public IAsyncEnumerable<BoardEvent> SubscribeAsync(string boardId, CancellationToken cancellationToken = default)
    {
        var channel = Channel.CreateUnbounded<BoardEvent>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        var subscriptionId = Guid.NewGuid();
        var boardSubscriptions = _subscriptions.GetOrAdd(boardId, _ => new ConcurrentDictionary<Guid, Channel<BoardEvent>>());
        boardSubscriptions[subscriptionId] = channel;

        return ReadAllAsync(boardId, subscriptionId, channel, cancellationToken);
    }

    public Task PublishAsync(BoardEvent boardEvent, CancellationToken cancellationToken = default)
    {
        if (!_subscriptions.TryGetValue(boardEvent.BoardId, out var boardSubscriptions))
        {
            return Task.CompletedTask;
        }

        foreach (var subscription in boardSubscriptions.Values)
        {
            subscription.Writer.TryWrite(boardEvent);
        }

        return Task.CompletedTask;
    }

    private async IAsyncEnumerable<BoardEvent> ReadAllAsync(
        string boardId,
        Guid subscriptionId,
        Channel<BoardEvent> channel,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var boardEvent in channel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return boardEvent;
            }
        }
        finally
        {
            if (_subscriptions.TryGetValue(boardId, out var boardSubscriptions))
            {
                boardSubscriptions.TryRemove(subscriptionId, out _);
                if (boardSubscriptions.IsEmpty)
                {
                    _subscriptions.TryRemove(boardId, out _);
                }
            }

            channel.Writer.TryComplete();
        }
    }
}
