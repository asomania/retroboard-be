using Microsoft.AspNetCore.Mvc;
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
        var boards = await _boardService.GetBoardsAsync(cancellationToken);
        return Ok(boards);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BoardDetailResponse>> GetBoardById(string id, CancellationToken cancellationToken)
    {
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
        try
        {
            var board = await _boardService.CreateBoardAsync(request, cancellationToken);
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
        var updated = await _boardService.UpdateBoardAsync(id, request, cancellationToken);
        if (!updated)
        {
            return NotFound(new ApiErrorResponse("Board not found"));
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBoard(string id, CancellationToken cancellationToken)
    {
        var deleted = await _boardService.DeleteBoardAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new ApiErrorResponse("Board not found"));
        }

        return NoContent();
    }
}
