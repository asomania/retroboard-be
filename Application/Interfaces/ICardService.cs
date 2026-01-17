using Retroboard.Api.Application.Models;

namespace Retroboard.Api.Application.Interfaces;

public interface ICardService
{
    Task<IReadOnlyList<CardResponse>> GetCardsAsync(string boardId, string columnId, CancellationToken cancellationToken = default);
    Task<CardResponse?> CreateCardAsync(string boardId, string columnId, CardCreateRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateCardAsync(string boardId, string columnId, string cardId, CardUpdateRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteCardAsync(string boardId, string columnId, string cardId, CancellationToken cancellationToken = default);
}
