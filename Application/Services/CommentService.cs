using Retroboard.Api.Application.Interfaces;
using Retroboard.Api.Application.Models;
using Retroboard.Api.Domain.Entities;

namespace Retroboard.Api.Application.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;

    public CommentService(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<IReadOnlyList<CommentResponse>> GetCommentsAsync(string cardId, CancellationToken cancellationToken = default)
    {
        var comments = await _commentRepository.GetByCardIdAsync(cardId, cancellationToken);
        return comments.Select(comment => new CommentResponse
        {
            Id = comment.Id,
            Author = comment.Author,
            Text = comment.Text,
            CreatedAt = comment.CreatedAt
        }).ToList();
    }

    public async Task<CommentResponse?> CreateCommentAsync(string cardId, CommentCreateRequest request, CancellationToken cancellationToken = default)
    {
        if (!await _commentRepository.CardExistsAsync(cardId, cancellationToken))
        {
            return null;
        }

        var existing = await _commentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException("Comment already exists.");
        }

        var comment = new Comment
        {
            Id = request.Id,
            Author = request.Author,
            Text = request.Text,
            CreatedAt = request.CreatedAt,
            CardId = cardId
        };

        await _commentRepository.AddAsync(comment, cancellationToken);
        return new CommentResponse
        {
            Id = comment.Id,
            Author = comment.Author,
            Text = comment.Text,
            CreatedAt = comment.CreatedAt
        };
    }

    public async Task<bool> DeleteCommentAsync(string id, CancellationToken cancellationToken = default)
    {
        var comment = await _commentRepository.GetByIdAsync(id, cancellationToken);
        if (comment is null)
        {
            return false;
        }

        await _commentRepository.DeleteAsync(comment, cancellationToken);
        return true;
    }
}
