namespace Retroboard.Api.Application.Models;

public enum CardLikeStatus
{
    Liked = 0,
    CardNotFound = 1,
    AlreadyLiked = 2
}

public class CardLikeResult
{
    public CardLikeStatus Status { get; set; }
    public string ColumnId { get; set; } = string.Empty;
    public int Votes { get; set; }
}
