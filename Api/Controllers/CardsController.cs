using Microsoft.AspNetCore.Mvc;
using Retroboard.Api.Api.Models;
using Retroboard.Api.Application.Interfaces;
using Retroboard.Api.Application.Models;

namespace Retroboard.Api.Api.Controllers;

[ApiController]
public class CardsController : ControllerBase
{
    private readonly ICardService _cardService;
    private readonly ILogger<CardsController> _logger;

    public CardsController(ICardService cardService, ILogger<CardsController> logger)
    {
        _cardService = cardService;
        _logger = logger;
    }

    [HttpGet("api/cards")]
    public async Task<ActionResult<IReadOnlyList<CardResponse>>> GetCards([FromQuery] string boardId, [FromQuery] string columnId, CancellationToken cancellationToken)
    {
        var cards = await _cardService.GetCardsAsync(boardId, columnId, cancellationToken);
        return Ok(cards);
    }

    [HttpPost("api/cards")]
    public async Task<ActionResult<CardResponse>> CreateCard([FromBody] CardCreateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var card = await _cardService.CreateCardAsync(request.BoardId, request.ColumnId, request, cancellationToken);
            if (card is null)
            {
                return NotFound(new ApiErrorResponse("Column not found"));
            }

            return CreatedAtAction(nameof(GetCards), new { boardId = request.BoardId, columnId = request.ColumnId }, card);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Card already exists. Id: {CardId}", request.Id);
            return Conflict(new ApiErrorResponse("Card already exists"));
        }
    }

    [HttpPut("api/cards/{cardId}")]
    public async Task<IActionResult> UpdateCard(string cardId, [FromBody] CardUpdateRequest request, CancellationToken cancellationToken)
    {
        var updated = await _cardService.UpdateCardAsync(request.BoardId, cardId, request, cancellationToken);
        if (!updated)
        {
            return NotFound(new ApiErrorResponse("Card not found"));
        }

        return NoContent();
    }

    [HttpDelete("api/cards/{cardId}")]
    public async Task<IActionResult> DeleteCard(string cardId, [FromQuery] string boardId, CancellationToken cancellationToken)
    {
        var deleted = await _cardService.DeleteCardAsync(boardId, cardId, cancellationToken);
        if (!deleted)
        {
            return NotFound(new ApiErrorResponse("Card not found"));
        }

        return NoContent();
    }
}
