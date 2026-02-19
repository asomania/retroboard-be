using Retroboard.Api.Application.Interfaces;
using Retroboard.Api.Application.Models;
using Retroboard.Api.Domain.Entities;

namespace Retroboard.Api.Application.Services;

public class ColumnService : IColumnService
{
    private readonly IColumnRepository _columnRepository;

    public ColumnService(IColumnRepository columnRepository)
    {
        _columnRepository = columnRepository;
    }

    public async Task<IReadOnlyList<ColumnResponse>> GetColumnsAsync(string boardId, CancellationToken cancellationToken = default)
    {
        var columns = await _columnRepository.GetByBoardIdAsync(boardId, cancellationToken);
        return columns.Select(MapColumn).ToList();
    }

    public async Task<ColumnResponse?> CreateColumnAsync(string boardId, ColumnCreateRequest request, CancellationToken cancellationToken = default)
    {
        if (!await _columnRepository.BoardExistsAsync(boardId, cancellationToken))
        {
            return null;
        }

        var existing = await _columnRepository.GetByIdAsync(boardId, request.Id, cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException("Column already exists.");
        }

        var column = new Column
        {
            Id = request.Id,
            Title = request.Title,
            BoardId = boardId
        };

        await _columnRepository.AddAsync(column, cancellationToken);
        return MapColumn(column);
    }

    public async Task<bool> UpdateColumnAsync(string boardId, string columnId, ColumnUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var column = await _columnRepository.GetByIdAsync(boardId, columnId, cancellationToken);
        if (column is null)
        {
            return false;
        }

        column.Title = request.Title;
        await _columnRepository.UpdateAsync(column, cancellationToken);
        return true;
    }

    public async Task<bool> DeleteColumnAsync(string boardId, string columnId, CancellationToken cancellationToken = default)
    {
        var column = await _columnRepository.GetByIdAsync(boardId, columnId, cancellationToken);
        if (column is null)
        {
            return false;
        }

        await _columnRepository.DeleteAsync(column, cancellationToken);
        return true;
    }

    private static ColumnResponse MapColumn(Column column)
    {
        return new ColumnResponse
        {
            Id = column.Id,
            Title = column.Title,
            Cards = column.Cards.Select(card => new CardResponse
            {
                Id = card.Id,
                Text = card.Text,
                Votes = card.Votes,
                CreatedByUserId = card.CreatedByUserId,
                CreatedByDisplayName = card.CreatedByDisplayName,
                Comments = card.Comments.Select(comment => new CommentResponse
                {
                    Id = comment.Id,
                    CreatedByUserId = comment.CreatedByUserId,
                    CreatedByDisplayName = comment.CreatedByDisplayName,
                    Author = comment.Author,
                    Text = comment.Text,
                    CreatedAt = comment.CreatedAt
                }).ToList()
            }).ToList()
        };
    }
}
