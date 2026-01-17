using Retroboard.Api.Application.Models;

namespace Retroboard.Api.Application.Interfaces;

public interface ICommentService
{
    Task<IReadOnlyList<CommentResponse>> GetCommentsAsync(string boardId, string columnId, string cardId, CancellationToken cancellationToken = default);
    Task<CommentResponse?> CreateCommentAsync(string boardId, string columnId, string cardId, CommentCreateRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteCommentAsync(string boardId, string columnId, string cardId, string id, CancellationToken cancellationToken = default);
}
