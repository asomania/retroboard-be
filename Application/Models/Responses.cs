namespace Retroboard.Api.Application.Models;

public class BoardSummaryResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public bool InviteRequired { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public int ParticipantsCount { get; set; }
}

public class BoardDetailResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public bool InviteRequired { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public List<string> Participants { get; set; } = new();
    public List<ColumnResponse> Columns { get; set; } = new();
}

public class ColumnResponse
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public List<CardResponse> Cards { get; set; } = new();
}

public class CardResponse
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public int Votes { get; set; }
    public bool LikedByMe { get; set; }
    public List<CommentResponse> Comments { get; set; } = new();
}

public class CardLikeResponse
{
    public string CardId { get; set; } = string.Empty;
    public int Votes { get; set; }
}

public class CommentResponse
{
    public string Id { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
