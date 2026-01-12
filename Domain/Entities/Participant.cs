namespace Retroboard.Api.Domain.Entities;

public class Participant
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BoardId { get; set; } = string.Empty;
    public Board? Board { get; set; }
}
