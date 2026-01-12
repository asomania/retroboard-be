using Retroboard.Api.Domain.Entities;

namespace Retroboard.Api.Application.Interfaces;

public interface IColumnRepository
{
    Task<List<Column>> GetByBoardIdAsync(string boardId, CancellationToken cancellationToken = default);
    Task<Column?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task AddAsync(Column column, CancellationToken cancellationToken = default);
    Task UpdateAsync(Column column, CancellationToken cancellationToken = default);
    Task DeleteAsync(Column column, CancellationToken cancellationToken = default);
    Task<bool> BoardExistsAsync(string boardId, CancellationToken cancellationToken = default);
}
