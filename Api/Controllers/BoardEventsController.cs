using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
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
}
