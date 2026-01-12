using Retroboard.Api.Application.Models;

namespace Retroboard.Api.Application.Interfaces;

public interface IColumnService
{
    Task<IReadOnlyList<ColumnResponse>> GetColumnsAsync(string boardId, CancellationToken cancellationToken = default);
    Task<ColumnResponse?> CreateColumnAsync(string boardId, ColumnCreateRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateColumnAsync(string id, ColumnUpdateRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteColumnAsync(string id, CancellationToken cancellationToken = default);
}
