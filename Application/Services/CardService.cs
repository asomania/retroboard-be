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

    public async Task<IReadOnlyList<CardResponse>> GetCardsAsync(string boardId, string columnId, CancellationToken cancellationToken = default)
    {
        var cards = await _cardRepository.GetByColumnAsync(boardId, columnId, cancellationToken);
        return cards.Select(MapCard).ToList();
    }

    public async Task<CardResponse?> CreateCardAsync(string boardId, string columnId, CardCreateRequest request, CancellationToken cancellationToken = default)
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
                Votes = card.Votes
            },
            Ts = DateTime.UtcNow
        }, cancellationToken);

        return MapCard(card);
    }

    public async Task<bool> UpdateCardAsync(string boardId, string cardId, CardUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var card = await _cardRepository.GetByIdAsync(boardId, cardId, cancellationToken);
        if (card is null)
        {
            return false;
        }

        var previousColumnId = card.ColumnId;
        var previousVotes = card.Votes;
        var previousText = card.Text;

        card.Text = request.Text;
        card.Votes = request.Votes;
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

        if (card.Votes > previousVotes)
        {
            await _eventPublisher.PublishAsync(new BoardEvent
            {
                Type = "card.liked",
                BoardId = boardId,
                Data = new CardLikedEventData
                {
                    CardId = card.Id,
                    ColumnId = card.ColumnId,
                    Votes = card.Votes,
                    Delta = card.Votes - previousVotes
                },
                Ts = DateTime.UtcNow
            }, cancellationToken);
        }

        if (!string.Equals(previousText, card.Text, StringComparison.Ordinal) || card.Votes != previousVotes)
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
                    Votes = card.Votes
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
        return new CardResponse
        {
            Id = card.Id,
            Text = card.Text,
            Votes = card.Votes,
            Comments = card.Comments.Select(comment => new CommentResponse
            {
                Id = comment.Id,
                Author = comment.Author,
                Text = comment.Text,
                CreatedAt = comment.CreatedAt
            }).ToList()
        };
    }
}
