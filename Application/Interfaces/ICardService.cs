using Retroboard.Api.Application.Models;

namespace Retroboard.Api.Application.Interfaces;

public interface ICardService
{
    Task<IReadOnlyList<CardResponse>> GetCardsAsync(string columnId, CancellationToken cancellationToken = default);
    Task<CardResponse?> CreateCardAsync(string columnId, CardCreateRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateCardAsync(string id, CardUpdateRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteCardAsync(string id, CancellationToken cancellationToken = default);
}
