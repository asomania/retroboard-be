namespace Retroboard.Api.Application.Models;

public enum CardMoveStatus
{
    Moved,
    NoChange,
    CardNotFound,
    TargetColumnNotFound,
    Conflict
}

public class CardMoveResult
{
    public CardMoveStatus Status { get; init; }
    public string FromColumnId { get; init; } = string.Empty;
}
