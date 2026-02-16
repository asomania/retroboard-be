using System.ComponentModel.DataAnnotations;

namespace Retroboard.Api.Application.Models;

public class BoardCreateRequest
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; }

    public bool InviteRequired { get; set; }

    public List<string> Participants { get; set; } = new();
}

public class BoardUpdateRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; }

    public bool InviteRequired { get; set; }

    public List<string> Participants { get; set; } = new();
}

public class ColumnCreateRequest
{
    [Required]
    public string BoardId { get; set; } = string.Empty;

    [Required]
    public string Id { get; set; } = string.Empty;

    [Required]
    public string Title { get; set; } = string.Empty;
}

public class ColumnUpdateRequest
{
    [Required]
    public string BoardId { get; set; } = string.Empty;

    [Required]
    public string Title { get; set; } = string.Empty;
}

public class CardCreateRequest
{
    [Required]
    public string BoardId { get; set; } = string.Empty;

    [Required]
    public string ColumnId { get; set; } = string.Empty;

    [Required]
    public string Id { get; set; } = string.Empty;

    [Required]
    public string Text { get; set; } = string.Empty;

    public int Votes { get; set; }
}

public class CardUpdateRequest
{
    [Required]
    public string BoardId { get; set; } = string.Empty;

    [Required]
    public string ColumnId { get; set; } = string.Empty;

    [Required]
    public string Text { get; set; } = string.Empty;
}

public class CardMoveRequest
{
    [Required]
    public string BoardId { get; set; } = string.Empty;

    [Required]
    public string ToColumnId { get; set; } = string.Empty;
}

public class CardLikeRequest
{
    [Required]
    public string BoardId { get; set; } = string.Empty;
}

public class CommentCreateRequest
{
    [Required]
    public string BoardId { get; set; } = string.Empty;

    [Required]
    public string ColumnId { get; set; } = string.Empty;

    [Required]
    public string CardId { get; set; } = string.Empty;

    [Required]
    public string Id { get; set; } = string.Empty;

    [Required]
    public string Author { get; set; } = string.Empty;

    [Required]
    public string Text { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; }
}
