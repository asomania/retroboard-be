using Retroboard.Api.Application.Interfaces;
using Retroboard.Api.Application.Models;
using Retroboard.Api.Domain.Entities;

namespace Retroboard.Api.Application.Services;

public class BoardService : IBoardService
{
    private readonly IBoardRepository _boardRepository;

    public BoardService(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<IReadOnlyList<BoardSummaryResponse>> GetBoardsAsync(CancellationToken cancellationToken = default)
    {
        var boards = await _boardRepository.GetAllAsync(cancellationToken);
        return boards
            .Select(board => new BoardSummaryResponse
            {
                Id = board.Id,
                Name = board.Name,
                Date = board.Date,
                InviteRequired = board.InviteRequired,
                CreatedByUserId = board.CreatedByUserId,
                ParticipantsCount = board.Participants.Count
            })
            .ToList();
    }

    public async Task<BoardDetailResponse?> GetBoardByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var board = await _boardRepository.GetByIdAsync(id, includeDetails: true, cancellationToken);
        return board is null ? null : MapBoardDetail(board);
    }

    public async Task<BoardDetailResponse> CreateBoardAsync(BoardCreateRequest request, string currentUserId, CancellationToken cancellationToken = default)
    {
        if (await _boardRepository.ExistsAsync(request.Id, cancellationToken))
        {
            throw new InvalidOperationException("Board already exists.");
        }

        var board = new Board
        {
            Id = request.Id,
            Name = request.Name,
            Date = request.Date,
            InviteRequired = request.InviteRequired,
            CreatedByUserId = currentUserId,
            Participants = request.Participants.Select(name => new Participant
            {
                Name = name
            }).ToList(),
            Columns = new List<Column>
            {
                new() { Id = "start", Title = "Start", BoardId = request.Id },
                new() { Id = "stop", Title = "Stop", BoardId = request.Id },
                new() { Id = "continue", Title = "Continue", BoardId = request.Id }
            }
        };

        await _boardRepository.AddAsync(board, cancellationToken);
        return MapBoardDetail(board);
    }

    public async Task<bool> UpdateBoardAsync(string id, BoardUpdateRequest request, string currentUserId, CancellationToken cancellationToken = default)
    {
        var board = await _boardRepository.GetByIdAsync(id, includeDetails: false, cancellationToken);
        if (board is null)
        {
            return false;
        }

        if (board.CreatedByUserId != currentUserId)
        {
            throw new UnauthorizedAccessException("Only board owner can update board settings.");
        }

        board.Name = request.Name;
        board.Date = request.Date;
        board.InviteRequired = request.InviteRequired;
        board.Participants = request.Participants.Select(name => new Participant
        {
            Name = name,
            BoardId = board.Id
        }).ToList();

        await _boardRepository.UpdateAsync(board, cancellationToken);
        return true;
    }

    public async Task<bool> DeleteBoardAsync(string id, string currentUserId, CancellationToken cancellationToken = default)
    {
        var board = await _boardRepository.GetByIdAsync(id, includeDetails: false, cancellationToken);
        if (board is null)
        {
            return false;
        }

        if (board.CreatedByUserId != currentUserId)
        {
            throw new UnauthorizedAccessException("Only board owner can delete board.");
        }

        await _boardRepository.DeleteAsync(board, cancellationToken);
        return true;
    }

    private static BoardDetailResponse MapBoardDetail(Board board)
    {
        return new BoardDetailResponse
        {
            Id = board.Id,
            Name = board.Name,
            Date = board.Date,
            InviteRequired = board.InviteRequired,
            CreatedByUserId = board.CreatedByUserId,
            Participants = board.Participants.Select(participant => participant.Name).ToList(),
            Columns = board.Columns.Select(column => new ColumnResponse
            {
                Id = column.Id,
                Title = column.Title,
                Cards = column.Cards.Select(card => new CardResponse
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
                }).ToList()
            }).ToList()
        };
    }
}
