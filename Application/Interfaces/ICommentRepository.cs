using Retroboard.Api.Domain.Entities;

namespace Retroboard.Api.Application.Interfaces;

public interface ICommentRepository
{
    Task<List<Comment>> GetByCardIdAsync(string cardId, CancellationToken cancellationToken = default);
    Task<Comment?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task AddAsync(Comment comment, CancellationToken cancellationToken = default);
    Task DeleteAsync(Comment comment, CancellationToken cancellationToken = default);
    Task<bool> CardExistsAsync(string cardId, CancellationToken cancellationToken = default);
}
