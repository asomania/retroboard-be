using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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

    [HttpGet("api/comments")]
    public async Task<ActionResult<IReadOnlyList<CommentResponse>>> GetComments([FromQuery] string boardId, [FromQuery] string columnId, [FromQuery] string cardId, CancellationToken cancellationToken)
    {
        if (!TryResolveCurrentUserId(out _))
        {
            return Unauthorized(new ApiErrorResponse("User identity is required"));
        }

        var comments = await _commentService.GetCommentsAsync(boardId, columnId, cardId, cancellationToken);
        return Ok(comments);
    }

    [HttpPost("api/comments")]
    public async Task<ActionResult<CommentResponse>> CreateComment([FromBody] CommentCreateRequest request, CancellationToken cancellationToken)
    {
        if (!TryResolveCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new ApiErrorResponse("User identity is required"));
        }

        try
        {
            var comment = await _commentService.CreateCommentAsync(request.BoardId, request.ColumnId, request.CardId, request, currentUserId, cancellationToken);
            if (comment is null)
            {
                return NotFound(new ApiErrorResponse("Card not found"));
            }

            return CreatedAtAction(nameof(GetComments), new { boardId = request.BoardId, columnId = request.ColumnId, cardId = request.CardId }, comment);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Comment already exists. Id: {CommentId}", request.Id);
            return Conflict(new ApiErrorResponse("Comment already exists"));
        }
    }

    [HttpDelete("api/comments/{id}")]
    public async Task<IActionResult> DeleteComment(string id, [FromQuery] string boardId, [FromQuery] string columnId, [FromQuery] string cardId, CancellationToken cancellationToken)
    {
        if (!TryResolveCurrentUserId(out _))
        {
            return Unauthorized(new ApiErrorResponse("User identity is required"));
        }

        var deleted = await _commentService.DeleteCommentAsync(boardId, columnId, cardId, id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new ApiErrorResponse("Comment not found"));
        }

        return NoContent();
    }

    private bool TryResolveCurrentUserId(out string userId)
    {
        var claimUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!string.IsNullOrWhiteSpace(claimUserId))
        {
            userId = claimUserId;
            return true;
        }

        if (Request.Headers.TryGetValue("X-User-Id", out var headerUserId) && !string.IsNullOrWhiteSpace(headerUserId))
        {
            userId = headerUserId.ToString();
            return true;
        }

        userId = string.Empty;
        return false;
    }
}
