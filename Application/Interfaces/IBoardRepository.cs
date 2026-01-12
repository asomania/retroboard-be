using Retroboard.Api.Domain.Entities;

namespace Retroboard.Api.Application.Interfaces;

public interface IBoardRepository
{
    Task<List<Board>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Board?> GetByIdAsync(string id, bool includeDetails, CancellationToken cancellationToken = default);
    Task AddAsync(Board board, CancellationToken cancellationToken = default);
    Task UpdateAsync(Board board, CancellationToken cancellationToken = default);
    Task DeleteAsync(Board board, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);
}
