using Retroboard.Api.Application.Models;

namespace Retroboard.Api.Application.Interfaces;

public interface IBoardEventPublisher
{
    Task PublishAsync(BoardEvent boardEvent, CancellationToken cancellationToken = default);
}
