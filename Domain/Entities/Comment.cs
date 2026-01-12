namespace Retroboard.Api.Domain.Entities;

public class Comment
{
    public string Id { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CardId { get; set; } = string.Empty;
    public Card? Card { get; set; }
}
