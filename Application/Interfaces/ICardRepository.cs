using Retroboard.Api.Domain.Entities;

namespace Retroboard.Api.Application.Interfaces;

public interface ICardRepository
{
    Task<List<Card>> GetByColumnAsync(string boardId, string columnId, CancellationToken cancellationToken = default);
    Task<Card?> GetByIdAsync(string boardId, string columnId, string cardId, CancellationToken cancellationToken = default);
    Task AddAsync(Card card, CancellationToken cancellationToken = default);
    Task UpdateAsync(Card card, CancellationToken cancellationToken = default);
    Task DeleteAsync(Card card, CancellationToken cancellationToken = default);
    Task<bool> ColumnExistsAsync(string boardId, string columnId, CancellationToken cancellationToken = default);
}
