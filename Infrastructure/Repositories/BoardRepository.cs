using Microsoft.EntityFrameworkCore;
using Retroboard.Api.Application.Interfaces;
using Retroboard.Api.Domain.Entities;
using Retroboard.Api.Infrastructure.Data;

namespace Retroboard.Api.Infrastructure.Repositories;

public class BoardRepository : IBoardRepository
{
    private readonly RetroboardDbContext _dbContext;

    public BoardRepository(RetroboardDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<Board>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.Boards
            .Include(board => board.Participants)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<Board?> GetByIdAsync(string id, bool includeDetails, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Boards.AsQueryable();

        if (includeDetails)
        {
            query = query
                .Include(board => board.Participants)
                .Include(board => board.Columns)
                    .ThenInclude(column => column.Cards)
                        .ThenInclude(card => card.Comments);
        }
        else
        {
            query = query.Include(board => board.Participants);
        }

        return query.AsNoTracking().FirstOrDefaultAsync(board => board.Id == id, cancellationToken);
    }

    public async Task AddAsync(Board board, CancellationToken cancellationToken = default)
    {
        _dbContext.Boards.Add(board);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Board board, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Boards
            .Include(existingBoard => existingBoard.Participants)
            .FirstOrDefaultAsync(existingBoard => existingBoard.Id == board.Id, cancellationToken);

        if (existing is null)
        {
            return;
        }

        existing.Name = board.Name;
        existing.Date = board.Date;
        existing.InviteRequired = board.InviteRequired;

        _dbContext.Participants.RemoveRange(existing.Participants);
        existing.Participants = board.Participants;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Board board, CancellationToken cancellationToken = default)
    {
        _dbContext.Boards.Remove(board);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Boards.AnyAsync(board => board.Id == id, cancellationToken);
    }
}
