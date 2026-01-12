using Retroboard.Api.Domain.Entities;

namespace Retroboard.Api.Application.Interfaces;

public interface ICardRepository
{
    Task<List<Card>> GetByColumnIdAsync(string columnId, CancellationToken cancellationToken = default);
    Task<Card?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task AddAsync(Card card, CancellationToken cancellationToken = default);
    Task UpdateAsync(Card card, CancellationToken cancellationToken = default);
    Task DeleteAsync(Card card, CancellationToken cancellationToken = default);
    Task<bool> ColumnExistsAsync(string columnId, CancellationToken cancellationToken = default);
}
