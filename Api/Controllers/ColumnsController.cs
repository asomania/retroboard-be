using Microsoft.AspNetCore.Mvc;
using Retroboard.Api.Api.Models;
using Retroboard.Api.Application.Interfaces;
using Retroboard.Api.Application.Models;

namespace Retroboard.Api.Api.Controllers;

[ApiController]
public class ColumnsController : ControllerBase
{
    private readonly IColumnService _columnService;
    private readonly ILogger<ColumnsController> _logger;

    public ColumnsController(IColumnService columnService, ILogger<ColumnsController> logger)
    {
        _columnService = columnService;
        _logger = logger;
    }

    [HttpGet("api/columns")]
    public async Task<ActionResult<IReadOnlyList<ColumnResponse>>> GetColumns([FromQuery] string boardId, CancellationToken cancellationToken)
    {
        var columns = await _columnService.GetColumnsAsync(boardId, cancellationToken);
        return Ok(columns);
    }

    [HttpPost("api/columns")]
    public async Task<ActionResult<ColumnResponse>> CreateColumn([FromBody] ColumnCreateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var column = await _columnService.CreateColumnAsync(request.BoardId, request, cancellationToken);
            if (column is null)
            {
                return NotFound(new ApiErrorResponse("Board not found"));
            }

            return CreatedAtAction(nameof(GetColumns), new { boardId = request.BoardId }, column);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Column already exists. Id: {ColumnId}", request.Id);
            return Conflict(new ApiErrorResponse("Column already exists"));
        }
    }

    [HttpPut("api/columns/{columnId}")]
    public async Task<IActionResult> UpdateColumn(string columnId, [FromBody] ColumnUpdateRequest request, CancellationToken cancellationToken)
    {
        var updated = await _columnService.UpdateColumnAsync(request.BoardId, columnId, request, cancellationToken);
        if (!updated)
        {
            return NotFound(new ApiErrorResponse("Column not found"));
        }

        return NoContent();
    }

    [HttpDelete("api/columns/{columnId}")]
    public async Task<IActionResult> DeleteColumn(string columnId, [FromQuery] string boardId, CancellationToken cancellationToken)
    {
        var deleted = await _columnService.DeleteColumnAsync(boardId, columnId, cancellationToken);
        if (!deleted)
        {
            return NotFound(new ApiErrorResponse("Column not found"));
        }

        return NoContent();
    }
}
