using Retroboard.Api.Application.Interfaces;
using Retroboard.Api.Application.Models;
using Retroboard.Api.Domain.Entities;

namespace Retroboard.Api.Application.Services;

public class CardService : ICardService
{
    private readonly ICardRepository _cardRepository;
    private readonly IBoardEventPublisher _eventPublisher;

    public CardService(ICardRepository cardRepository, IBoardEventPublisher eventPublisher)
    {
        _cardRepository = cardRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<IReadOnlyList<CardResponse>> GetCardsAsync(string boardId, string columnId, string currentUserId, CancellationToken cancellationToken = default)
    {
        var cards = await _cardRepository.GetByColumnAsync(boardId, columnId, cancellationToken);
        var likedCardIds = await _cardRepository.GetLikedCardIdsAsync(boardId, currentUserId, cards.Select(card => card.Id), cancellationToken);

        return cards.Select(card => MapCard(card, likedCardIds.Contains(card.Id))).ToList();
    }

    public async Task<CardResponse?> CreateCardAsync(string boardId, string columnId, CardCreateRequest request, string currentUserId, CancellationToken cancellationToken = default)
    {
        if (!await _cardRepository.ColumnExistsAsync(boardId, columnId, cancellationToken))
        {
            return null;
        }

        var existing = await _cardRepository.GetByIdAsync(boardId, request.Id, cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException("Card already exists.");
        }

        var card = new Card
        {
            Id = request.Id,
            Text = request.Text,
            Votes = request.Votes,
            CreatedByUserId = currentUserId,
            CreatedByDisplayName = ResolveCardDisplayName(request),
            BoardId = boardId,
            ColumnId = columnId
        };

        await _cardRepository.AddAsync(card, cancellationToken);

        await _eventPublisher.PublishAsync(new BoardEvent
        {
            Type = "card.created",
            BoardId = boardId,
            Data = new CardCreatedEventData
            {
                CardId = card.Id,
                ColumnId = card.ColumnId,
                Text = card.Text,
                Votes = card.Votes,
                CreatedByUserId = card.CreatedByUserId,
                CreatedByDisplayName = card.CreatedByDisplayName
            },
            Ts = DateTime.UtcNow
        }, cancellationToken);

        return MapCard(card);
    }

    public async Task<CardMoveResult> MoveCardAsync(string cardId, CardMoveRequest request, CancellationToken cancellationToken = default)
    {
        var moveResult = await _cardRepository.MoveAsync(request.BoardId, cardId, request.ToColumnId, cancellationToken);
        if (moveResult.Status != CardMoveStatus.Moved)
        {
            return moveResult;
        }

        await _eventPublisher.PublishAsync(new BoardEvent
        {
            Type = "card.moved",
            BoardId = request.BoardId,
            Data = new CardMovedEventData
            {
                CardId = cardId,
                FromColumnId = moveResult.FromColumnId,
                ToColumnId = request.ToColumnId
            },
            Ts = DateTime.UtcNow
        }, cancellationToken);

        return moveResult;
    }

    public async Task<CardLikeResult> LikeCardAsync(string cardId, CardLikeRequest request, string userId, CancellationToken cancellationToken = default)
    {
        var likeResult = await _cardRepository.LikeAsync(request.BoardId, cardId, userId, cancellationToken);
        if (likeResult.Status != CardLikeStatus.Liked)
        {
            return likeResult;
        }

        await _eventPublisher.PublishAsync(new BoardEvent
        {
            Type = "card.liked",
            BoardId = request.BoardId,
            Data = new CardLikedEventData
            {
                CardId = cardId,
                ColumnId = likeResult.ColumnId,
                Votes = likeResult.Votes,
                Delta = 1
            },
            Ts = DateTime.UtcNow
        }, cancellationToken);

        return likeResult;
    }

    public async Task<bool> UpdateCardAsync(string boardId, string cardId, CardUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var card = await _cardRepository.GetByIdAsync(boardId, cardId, cancellationToken);
        if (card is null)
        {
            return false;
        }

        var previousColumnId = card.ColumnId;
        var previousText = card.Text;

        card.Text = request.Text;
        card.ColumnId = request.ColumnId;
        await _cardRepository.UpdateAsync(card, cancellationToken);

        if (!string.Equals(previousColumnId, card.ColumnId, StringComparison.Ordinal))
        {
            await _eventPublisher.PublishAsync(new BoardEvent
            {
                Type = "card.moved",
                BoardId = boardId,
                Data = new CardMovedEventData
                {
                    CardId = card.Id,
                    FromColumnId = previousColumnId,
                    ToColumnId = card.ColumnId
                },
                Ts = DateTime.UtcNow
            }, cancellationToken);
        }

        if (!string.Equals(previousText, card.Text, StringComparison.Ordinal))
        {
            await _eventPublisher.PublishAsync(new BoardEvent
            {
                Type = "card.updated",
                BoardId = boardId,
                Data = new CardUpdatedEventData
                {
                    CardId = card.Id,
                    ColumnId = card.ColumnId,
                    Text = card.Text,
                    Votes = card.Votes,
                    CreatedByUserId = card.CreatedByUserId,
                    CreatedByDisplayName = card.CreatedByDisplayName
                },
                Ts = DateTime.UtcNow
            }, cancellationToken);
        }

        return true;
    }

    public async Task<bool> DeleteCardAsync(string boardId, string cardId, CancellationToken cancellationToken = default)
    {
        var card = await _cardRepository.GetByIdAsync(boardId, cardId, cancellationToken);
        if (card is null)
        {
            return false;
        }

        await _cardRepository.DeleteAsync(card, cancellationToken);

        await _eventPublisher.PublishAsync(new BoardEvent
        {
            Type = "card.deleted",
            BoardId = boardId,
            Data = new CardDeletedEventData
            {
                CardId = card.Id,
                ColumnId = card.ColumnId
            },
            Ts = DateTime.UtcNow
        }, cancellationToken);

        return true;
    }

    private static CardResponse MapCard(Card card)
    {
        return MapCard(card, likedByMe: false);
    }

    private static CardResponse MapCard(Card card, bool likedByMe)
    {
        return new CardResponse
        {
            Id = card.Id,
            Text = card.Text,
            Votes = card.Votes,
            CreatedByUserId = card.CreatedByUserId,
            CreatedByDisplayName = card.CreatedByDisplayName,
            LikedByMe = likedByMe,
            Comments = card.Comments.Select(comment => new CommentResponse
            {
                Id = comment.Id,
                CreatedByUserId = comment.CreatedByUserId,
                CreatedByDisplayName = comment.CreatedByDisplayName,
                Author = comment.Author,
                Text = comment.Text,
                CreatedAt = comment.CreatedAt
            }).ToList()
        };
    }

    private static string ResolveCardDisplayName(CardCreateRequest request)
    {
        return FirstNonEmpty(request.CreatedByDisplayName, request.CreatedByName, request.Author) ?? "Anonymous";
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return null;
    }
}
