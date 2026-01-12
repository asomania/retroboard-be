namespace Retroboard.Api.Domain.Entities;

public class Card
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public int Votes { get; set; }
    public string ColumnId { get; set; } = string.Empty;
    public Column? Column { get; set; }
    public List<Comment> Comments { get; set; } = new();
}
