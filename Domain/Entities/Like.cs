namespace Retroboard.Api.Domain.Entities;

public class Like
{
    public string BoardId { get; set; } = string.Empty;
    public string CardId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
