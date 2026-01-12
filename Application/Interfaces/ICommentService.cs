using Retroboard.Api.Application.Models;

namespace Retroboard.Api.Application.Interfaces;

public interface ICommentService
{
    Task<IReadOnlyList<CommentResponse>> GetCommentsAsync(string cardId, CancellationToken cancellationToken = default);
    Task<CommentResponse?> CreateCommentAsync(string cardId, CommentCreateRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteCommentAsync(string id, CancellationToken cancellationToken = default);
}
