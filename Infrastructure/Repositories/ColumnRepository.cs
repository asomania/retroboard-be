using Microsoft.EntityFrameworkCore;
using Retroboard.Api.Application.Interfaces;
using Retroboard.Api.Domain.Entities;
using Retroboard.Api.Infrastructure.Data;

namespace Retroboard.Api.Infrastructure.Repositories;

public class ColumnRepository : IColumnRepository
{
    private readonly RetroboardDbContext _dbContext;

    public ColumnRepository(RetroboardDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<Column>> GetByBoardIdAsync(string boardId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Columns
            .Where(column => column.BoardId == boardId)
            .Include(column => column.Cards)
                .ThenInclude(card => card.Comments)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<Column?> GetByIdAsync(string boardId, string columnId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Columns
            .Include(column => column.Cards)
                .ThenInclude(card => card.Comments)
            .FirstOrDefaultAsync(column => column.BoardId == boardId && column.Id == columnId, cancellationToken);
    }

    public async Task AddAsync(Column column, CancellationToken cancellationToken = default)
    {
        _dbContext.Columns.Add(column);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Column column, CancellationToken cancellationToken = default)
    {
        _dbContext.Columns.Update(column);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Column column, CancellationToken cancellationToken = default)
    {
        _dbContext.Columns.Remove(column);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> BoardExistsAsync(string boardId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Boards.AnyAsync(board => board.Id == boardId, cancellationToken);
    }
}
