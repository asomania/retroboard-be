namespace Retroboard.Api.Domain.Entities;

public class Column
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string BoardId { get; set; } = string.Empty;
    public Board? Board { get; set; }
    public List<Card> Cards { get; set; } = new();
}
