using Retroboard.Api.Application.Models;

namespace Retroboard.Api.Application.Interfaces;

public interface IBoardService
{
    Task<IReadOnlyList<BoardSummaryResponse>> GetBoardsAsync(CancellationToken cancellationToken = default);
    Task<BoardDetailResponse?> GetBoardByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<BoardDetailResponse> CreateBoardAsync(BoardCreateRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateBoardAsync(string id, BoardUpdateRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteBoardAsync(string id, CancellationToken cancellationToken = default);
}
