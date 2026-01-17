using Retroboard.Api.Application.Interfaces;
using Retroboard.Api.Application.Models;
using Retroboard.Api.Domain.Entities;

namespace Retroboard.Api.Application.Services;

public class CardService : ICardService
{
    private readonly ICardRepository _cardRepository;

    public CardService(ICardRepository cardRepository)
    {
        _cardRepository = cardRepository;
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

        var existing = await _cardRepository.GetByIdAsync(boardId, columnId, request.Id, cancellationToken);
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
        return MapCard(card);
    }

    public async Task<bool> UpdateCardAsync(string boardId, string columnId, string cardId, CardUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var card = await _cardRepository.GetByIdAsync(boardId, columnId, cardId, cancellationToken);
        if (card is null)
        {
            return false;
        }

        card.Text = request.Text;
        card.Votes = request.Votes;
        await _cardRepository.UpdateAsync(card, cancellationToken);
        return true;
    }

    public async Task<bool> DeleteCardAsync(string boardId, string columnId, string cardId, CancellationToken cancellationToken = default)
    {
        var card = await _cardRepository.GetByIdAsync(boardId, columnId, cardId, cancellationToken);
        if (card is null)
        {
            return false;
        }

        await _cardRepository.DeleteAsync(card, cancellationToken);
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
