using Retroboard.Api.Application.Models;

namespace Retroboard.Api.Application.Interfaces;

public interface ICardService
{
    Task<IReadOnlyList<CardResponse>> GetCardsAsync(string boardId, string columnId, string? currentUserId, CancellationToken cancellationToken = default);
    Task<CardResponse?> CreateCardAsync(string boardId, string columnId, CardCreateRequest request, CancellationToken cancellationToken = default);
    Task<CardMoveResult> MoveCardAsync(string cardId, CardMoveRequest request, CancellationToken cancellationToken = default);
    Task<CardLikeResult> LikeCardAsync(string cardId, CardLikeRequest request, string userId, CancellationToken cancellationToken = default);
    Task<bool> UpdateCardAsync(string boardId, string cardId, CardUpdateRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteCardAsync(string boardId, string cardId, CancellationToken cancellationToken = default);
}
