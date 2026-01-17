using Retroboard.Api.Application.Models;

namespace Retroboard.Api.Application.Interfaces;

public interface IColumnService
{
    Task<IReadOnlyList<ColumnResponse>> GetColumnsAsync(string boardId, CancellationToken cancellationToken = default);
    Task<ColumnResponse?> CreateColumnAsync(string boardId, ColumnCreateRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateColumnAsync(string boardId, string columnId, ColumnUpdateRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteColumnAsync(string boardId, string columnId, CancellationToken cancellationToken = default);
}
