namespace Retroboard.Api.Domain.Entities;

public class Board
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public bool InviteRequired { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public List<Participant> Participants { get; set; } = new();
    public List<Column> Columns { get; set; } = new();
}
