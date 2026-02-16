using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Retroboard.Api.Application.Interfaces;
using Retroboard.Api.Application.Models;
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

    public async Task<HashSet<string>> GetLikedCardIdsAsync(string boardId, string userId, IEnumerable<string> cardIds, CancellationToken cancellationToken = default)
    {
        var cardIdList = cardIds.Distinct().ToList();
        if (cardIdList.Count == 0)
        {
            return new HashSet<string>(StringComparer.Ordinal);
        }

        var likedCardIds = await _dbContext.Likes
            .Where(like => like.BoardId == boardId && like.UserId == userId && cardIdList.Contains(like.CardId))
            .Select(like => like.CardId)
            .ToListAsync(cancellationToken);

        return likedCardIds.ToHashSet(StringComparer.Ordinal);
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
        var likes = await _dbContext.Likes
            .Where(like => like.BoardId == card.BoardId && like.CardId == card.Id)
            .ToListAsync(cancellationToken);

        if (likes.Count > 0)
        {
            _dbContext.Likes.RemoveRange(likes);
        }

        _dbContext.Cards.Remove(card);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<CardLikeResult> LikeAsync(string boardId, string cardId, string userId, CancellationToken cancellationToken = default)
    {
        await using var tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var card = await _dbContext.Cards
            .FirstOrDefaultAsync(existingCard => existingCard.BoardId == boardId && existingCard.Id == cardId, cancellationToken);

        if (card is null)
        {
            await tx.RollbackAsync(cancellationToken);
            return new CardLikeResult
            {
                Status = CardLikeStatus.CardNotFound
            };
        }

        _dbContext.Likes.Add(new Like
        {
            BoardId = boardId,
            CardId = cardId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        });

        card.Votes += 1;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            await tx.RollbackAsync(cancellationToken);
            return new CardLikeResult
            {
                Status = CardLikeStatus.AlreadyLiked,
                ColumnId = card.ColumnId,
                Votes = card.Votes - 1
            };
        }

        return new CardLikeResult
        {
            Status = CardLikeStatus.Liked,
            ColumnId = card.ColumnId,
            Votes = card.Votes
        };
    }

    public Task<bool> ColumnExistsAsync(string boardId, string columnId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Columns.AnyAsync(column => column.BoardId == boardId && column.Id == columnId, cancellationToken);
    }

    public async Task<CardMoveResult> MoveAsync(string boardId, string cardId, string toColumnId, CancellationToken cancellationToken = default)
    {
        if (!await ColumnExistsAsync(boardId, toColumnId, cancellationToken))
        {
            return new CardMoveResult
            {
                Status = CardMoveStatus.TargetColumnNotFound
            };
        }

        var sourceCard = await _dbContext.Cards
            .Include(card => card.Comments)
            .FirstOrDefaultAsync(card => card.BoardId == boardId && card.Id == cardId, cancellationToken);

        if (sourceCard is null)
        {
            return new CardMoveResult
            {
                Status = CardMoveStatus.CardNotFound
            };
        }

        if (string.Equals(sourceCard.ColumnId, toColumnId, StringComparison.Ordinal))
        {
            return new CardMoveResult
            {
                Status = CardMoveStatus.NoChange,
                FromColumnId = sourceCard.ColumnId
            };
        }

        var targetConflict = await _dbContext.Cards.AnyAsync(
            card => card.BoardId == boardId && card.ColumnId == toColumnId && card.Id == cardId,
            cancellationToken);

        if (targetConflict)
        {
            return new CardMoveResult
            {
                Status = CardMoveStatus.Conflict,
                FromColumnId = sourceCard.ColumnId
            };
        }

        await using var tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var movedCard = new Card
        {
            Id = sourceCard.Id,
            Text = sourceCard.Text,
            Votes = sourceCard.Votes,
            BoardId = sourceCard.BoardId,
            ColumnId = toColumnId
        };
        _dbContext.Cards.Add(movedCard);

        if (sourceCard.Comments.Count > 0)
        {
            var movedComments = sourceCard.Comments.Select(comment => new Comment
            {
                Id = comment.Id,
                Author = comment.Author,
                Text = comment.Text,
                CreatedAt = comment.CreatedAt,
                BoardId = comment.BoardId,
                ColumnId = toColumnId,
                CardId = cardId
            });

            _dbContext.Comments.AddRange(movedComments);
        }

        _dbContext.Cards.Remove(sourceCard);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        return new CardMoveResult
        {
            Status = CardMoveStatus.Moved,
            FromColumnId = sourceCard.ColumnId
        };
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlException &&
            (sqlException.Number == 2601 || sqlException.Number == 2627);
    }
}
