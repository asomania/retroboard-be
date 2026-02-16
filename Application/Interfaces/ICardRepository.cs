using Retroboard.Api.Domain.Entities;
using Retroboard.Api.Application.Models;

namespace Retroboard.Api.Application.Interfaces;

public interface ICardRepository
{
    Task<List<Card>> GetByColumnAsync(string boardId, string columnId, CancellationToken cancellationToken = default);
    Task<Card?> GetByIdAsync(string boardId, string cardId, CancellationToken cancellationToken = default);
    Task<HashSet<string>> GetLikedCardIdsAsync(string boardId, string userId, IEnumerable<string> cardIds, CancellationToken cancellationToken = default);
    Task AddAsync(Card card, CancellationToken cancellationToken = default);
    Task UpdateAsync(Card card, CancellationToken cancellationToken = default);
    Task DeleteAsync(Card card, CancellationToken cancellationToken = default);
    Task<CardLikeResult> LikeAsync(string boardId, string cardId, string userId, CancellationToken cancellationToken = default);
    Task<bool> ColumnExistsAsync(string boardId, string columnId, CancellationToken cancellationToken = default);
    Task<CardMoveResult> MoveAsync(string boardId, string cardId, string toColumnId, CancellationToken cancellationToken = default);
}
