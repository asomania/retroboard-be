using Retroboard.Api.Application.Models;

namespace Retroboard.Api.Application.Interfaces;

public interface IBoardEventStream
{
    IAsyncEnumerable<BoardEvent> SubscribeAsync(string boardId, CancellationToken cancellationToken = default);
}
