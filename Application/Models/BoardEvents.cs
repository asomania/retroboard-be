namespace Retroboard.Api.Application.Models;

public class BoardEvent
{
    public string Type { get; set; } = string.Empty;
    public string BoardId { get; set; } = string.Empty;
    public object Data { get; set; } = new();
    public DateTime Ts { get; set; }
}

public class CardMovedEventData
{
    public string CardId { get; set; } = string.Empty;
    public string FromColumnId { get; set; } = string.Empty;
    public string ToColumnId { get; set; } = string.Empty;
}

public class CardLikedEventData
{
    public string CardId { get; set; } = string.Empty;
    public string ColumnId { get; set; } = string.Empty;
    public int Votes { get; set; }
    public int Delta { get; set; }
}

public class CardCreatedEventData
{
    public string CardId { get; set; } = string.Empty;
    public string ColumnId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public int Votes { get; set; }
}

public class CardDeletedEventData
{
    public string CardId { get; set; } = string.Empty;
    public string ColumnId { get; set; } = string.Empty;
}

public class CommentCreatedEventData
{
    public string CommentId { get; set; } = string.Empty;
    public string CardId { get; set; } = string.Empty;
    public string ColumnId { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CardUpdatedEventData
{
    public string CardId { get; set; } = string.Empty;
    public string ColumnId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public int Votes { get; set; }
}

public class CommentDeletedEventData
{
    public string CommentId { get; set; } = string.Empty;
    public string CardId { get; set; } = string.Empty;
    public string ColumnId { get; set; } = string.Empty;
}
