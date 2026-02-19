using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Retroboard.Api.Api.Models;
using Retroboard.Api.Application.Interfaces;
using Retroboard.Api.Application.Models;

namespace Retroboard.Api.Api.Controllers;

[ApiController]
[Route("api/boards")]
public class BoardsController : ControllerBase
{
    private readonly IBoardService _boardService;
    private readonly ILogger<BoardsController> _logger;

    public BoardsController(IBoardService boardService, ILogger<BoardsController> logger)
    {
        _boardService = boardService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BoardSummaryResponse>>> GetBoards(CancellationToken cancellationToken)
    {
        if (!TryResolveCurrentUserId(out _))
        {
            return Unauthorized(new ApiErrorResponse("User identity is required"));
        }

        var boards = await _boardService.GetBoardsAsync(cancellationToken);
        return Ok(boards);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BoardDetailResponse>> GetBoardById(string id, CancellationToken cancellationToken)
    {
        if (!TryResolveCurrentUserId(out _))
        {
            return Unauthorized(new ApiErrorResponse("User identity is required"));
        }

        var board = await _boardService.GetBoardByIdAsync(id, cancellationToken);
        if (board is null)
        {
            return NotFound(new ApiErrorResponse("Board not found"));
        }

        return Ok(board);
    }

    [HttpPost]
    public async Task<ActionResult<BoardDetailResponse>> CreateBoard([FromBody] BoardCreateRequest request, CancellationToken cancellationToken)
    {
        if (!TryResolveCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new ApiErrorResponse("User identity is required"));
        }

        try
        {
            var board = await _boardService.CreateBoardAsync(request, currentUserId, cancellationToken);
            return CreatedAtAction(nameof(GetBoardById), new { id = board.Id }, board);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Board already exists. Id: {BoardId}", request.Id);
            return Conflict(new ApiErrorResponse("Board already exists"));
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBoard(string id, [FromBody] BoardUpdateRequest request, CancellationToken cancellationToken)
    {
        if (!TryResolveCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new ApiErrorResponse("User identity is required"));
        }

        try
        {
            var updated = await _boardService.UpdateBoardAsync(id, request, currentUserId, cancellationToken);
            if (!updated)
            {
                return NotFound(new ApiErrorResponse("Board not found"));
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized board update attempt. BoardId: {BoardId}, UserId: {UserId}", id, currentUserId);
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse("Bu board ayarlarını sadece oluşturan kişi değiştirebilir."));
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBoard(string id, CancellationToken cancellationToken)
    {
        if (!TryResolveCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new ApiErrorResponse("User identity is required"));
        }

        try
        {
            var deleted = await _boardService.DeleteBoardAsync(id, currentUserId, cancellationToken);
            if (!deleted)
            {
                return NotFound(new ApiErrorResponse("Board not found"));
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized board delete attempt. BoardId: {BoardId}, UserId: {UserId}", id, currentUserId);
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse("Bu board ayarlarını sadece oluşturan kişi değiştirebilir."));
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
