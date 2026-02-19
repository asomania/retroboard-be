using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
        if (!TryResolveCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new ApiErrorResponse("User identity is required"));
        }

        var cards = await _cardService.GetCardsAsync(boardId, columnId, currentUserId, cancellationToken);
        return Ok(cards);
    }

    [HttpPost("api/cards")]
    public async Task<ActionResult<CardResponse>> CreateCard([FromBody] CardCreateRequest request, CancellationToken cancellationToken)
    {
        if (!TryResolveCurrentUserId(out var currentUserId))
        {
            return Unauthorized(new ApiErrorResponse("User identity is required"));
        }

        try
        {
            var card = await _cardService.CreateCardAsync(request.BoardId, request.ColumnId, request, currentUserId, cancellationToken);
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
        if (!TryResolveCurrentUserId(out _))
        {
            return Unauthorized(new ApiErrorResponse("User identity is required"));
        }

        var updated = await _cardService.UpdateCardAsync(request.BoardId, cardId, request, cancellationToken);
        if (!updated)
        {
            return NotFound(new ApiErrorResponse("Card not found"));
        }

        return NoContent();
    }

    [HttpPost("api/cards/{cardId}/move")]
    public async Task<IActionResult> MoveCard(string cardId, [FromBody] CardMoveRequest request, CancellationToken cancellationToken)
    {
        if (!TryResolveCurrentUserId(out _))
        {
            return Unauthorized(new ApiErrorResponse("User identity is required"));
        }

        var result = await _cardService.MoveCardAsync(cardId, request, cancellationToken);
        return result.Status switch
        {
            CardMoveStatus.Moved => NoContent(),
            CardMoveStatus.NoChange => NoContent(),
            CardMoveStatus.CardNotFound => NotFound(new ApiErrorResponse("Card not found")),
            CardMoveStatus.TargetColumnNotFound => NotFound(new ApiErrorResponse("Target column not found")),
            CardMoveStatus.Conflict => Conflict(new ApiErrorResponse("Card already exists in target column")),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ApiErrorResponse("Unexpected move status"))
        };
    }

    [HttpPost("api/cards/{cardId}/like")]
    public async Task<ActionResult<CardLikeResponse>> LikeCard(string cardId, [FromBody] CardLikeRequest request, CancellationToken cancellationToken)
    {
        if (!TryResolveCurrentUserId(out var userId))
        {
            return Unauthorized(new ApiErrorResponse("User identity is required"));
        }

        var result = await _cardService.LikeCardAsync(cardId, request, userId, cancellationToken);
        return result.Status switch
        {
            CardLikeStatus.Liked => Ok(new CardLikeResponse
            {
                CardId = cardId,
                Votes = result.Votes
            }),
            CardLikeStatus.CardNotFound => NotFound(new ApiErrorResponse("Card not found")),
            CardLikeStatus.AlreadyLiked => Conflict(new ApiErrorResponse("Already liked")),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ApiErrorResponse("Unexpected like status"))
        };
    }

    [HttpDelete("api/cards/{cardId}")]
    public async Task<IActionResult> DeleteCard(string cardId, [FromQuery] string boardId, CancellationToken cancellationToken)
    {
        if (!TryResolveCurrentUserId(out _))
        {
            return Unauthorized(new ApiErrorResponse("User identity is required"));
        }

        var deleted = await _cardService.DeleteCardAsync(boardId, cardId, cancellationToken);
        if (!deleted)
        {
            return NotFound(new ApiErrorResponse("Card not found"));
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
