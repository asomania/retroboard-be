using Microsoft.EntityFrameworkCore;
using Retroboard.Api.Application.Interfaces;
using Retroboard.Api.Domain.Entities;
using Retroboard.Api.Infrastructure.Data;

namespace Retroboard.Api.Infrastructure.Repositories;

public class CardRepository : ICardRepository
{
    private readonly RetroboardDbContext _dbContext;

    public CardRepository(RetroboardDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<Card>> GetByColumnAsync(string boardId, string columnId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Cards
            .Where(card => card.BoardId == boardId && card.ColumnId == columnId)
            .Include(card => card.Comments)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<Card?> GetByIdAsync(string boardId, string cardId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Cards
            .Include(card => card.Comments)
            .FirstOrDefaultAsync(card => card.BoardId == boardId && card.Id == cardId, cancellationToken);
    }

    public async Task AddAsync(Card card, CancellationToken cancellationToken = default)
    {
        _dbContext.Cards.Add(card);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Card card, CancellationToken cancellationToken = default)
    {
        _dbContext.Cards.Update(card);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Card card, CancellationToken cancellationToken = default)
    {
        _dbContext.Cards.Remove(card);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> ColumnExistsAsync(string boardId, string columnId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Columns.AnyAsync(column => column.BoardId == boardId && column.Id == columnId, cancellationToken);
    }
}
