using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Retroboard.Api.Application.Interfaces;
using Retroboard.Api.Application.Models;

namespace Retroboard.Api.Api.Controllers;

[ApiController]
public class BoardEventsController : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IBoardEventStream _eventStream;

    public BoardEventsController(IBoardEventStream eventStream)
    {
        _eventStream = eventStream;
    }

    [HttpGet("api/boards/{boardId}/stream")]
    public async Task GetBoardStream(string boardId, CancellationToken cancellationToken)
    {
        if (!TryResolveCurrentUserId(out _))
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("X-Accel-Buffering", "no");

        await foreach (var boardEvent in _eventStream.SubscribeAsync(boardId, cancellationToken))
        {
            await WriteEventAsync(boardEvent, cancellationToken);
        }
    }

    private async Task WriteEventAsync(BoardEvent boardEvent, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(boardEvent, JsonOptions);
        await Response.WriteAsync($"event: {boardEvent.Type}\n", cancellationToken);
        await Response.WriteAsync($"data: {payload}\n\n", cancellationToken);
        await Response.Body.FlushAsync(cancellationToken);
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
