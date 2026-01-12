using Microsoft.AspNetCore.Mvc;
using Retroboard.Api.Api.Models;
using Retroboard.Api.Application.Interfaces;
using Retroboard.Api.Application.Models;

namespace Retroboard.Api.Api.Controllers;

[ApiController]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;
    private readonly ILogger<CommentsController> _logger;

    public CommentsController(ICommentService commentService, ILogger<CommentsController> logger)
    {
        _commentService = commentService;
        _logger = logger;
    }

    [HttpGet("api/cards/{cardId}/comments")]
    public async Task<ActionResult<IReadOnlyList<CommentResponse>>> GetComments(string cardId, CancellationToken cancellationToken)
    {
        var comments = await _commentService.GetCommentsAsync(cardId, cancellationToken);
        return Ok(comments);
    }

    [HttpPost("api/cards/{cardId}/comments")]
    public async Task<ActionResult<CommentResponse>> CreateComment(string cardId, [FromBody] CommentCreateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var comment = await _commentService.CreateCommentAsync(cardId, request, cancellationToken);
            if (comment is null)
            {
                return NotFound(new ApiErrorResponse("Card not found"));
            }

            return CreatedAtAction(nameof(GetComments), new { cardId }, comment);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Comment already exists. Id: {CommentId}", request.Id);
            return Conflict(new ApiErrorResponse("Comment already exists"));
        }
    }

    [HttpDelete("api/comments/{id}")]
    public async Task<IActionResult> DeleteComment(string id, CancellationToken cancellationToken)
    {
        var deleted = await _commentService.DeleteCommentAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new ApiErrorResponse("Comment not found"));
        }

        return NoContent();
    }
}
