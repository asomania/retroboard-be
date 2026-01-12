using Microsoft.EntityFrameworkCore;
using Retroboard.Api.Application.Interfaces;
using Retroboard.Api.Domain.Entities;
using Retroboard.Api.Infrastructure.Data;

namespace Retroboard.Api.Infrastructure.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly RetroboardDbContext _dbContext;

    public CommentRepository(RetroboardDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<Comment>> GetByCardIdAsync(string cardId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Comments
            .Where(comment => comment.CardId == cardId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<Comment?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Comments
            .FirstOrDefaultAsync(comment => comment.Id == id, cancellationToken);
    }

    public async Task AddAsync(Comment comment, CancellationToken cancellationToken = default)
    {
        _dbContext.Comments.Add(comment);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Comment comment, CancellationToken cancellationToken = default)
    {
        _dbContext.Comments.Remove(comment);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> CardExistsAsync(string cardId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Cards.AnyAsync(card => card.Id == cardId, cancellationToken);
    }
}
