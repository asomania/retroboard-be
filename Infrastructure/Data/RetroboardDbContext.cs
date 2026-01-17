using Microsoft.EntityFrameworkCore;
using Retroboard.Api.Domain.Entities;

namespace Retroboard.Api.Infrastructure.Data;

public class RetroboardDbContext : DbContext
{
    public RetroboardDbContext(DbContextOptions<RetroboardDbContext> options)
        : base(options)
    {
    }

    public DbSet<Board> Boards => Set<Board>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Column> Columns => Set<Column>();
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Board>(entity =>
        {
            entity.HasKey(board => board.Id);
            entity.Property(board => board.Id).ValueGeneratedNever();
            entity.Property(board => board.Name).IsRequired();
            entity.HasMany(board => board.Participants)
                .WithOne(participant => participant.Board)
                .HasForeignKey(participant => participant.BoardId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(board => board.Columns)
                .WithOne(column => column.Board)
                .HasForeignKey(column => column.BoardId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Participant>(entity =>
        {
            entity.HasKey(participant => participant.Id);
            entity.Property(participant => participant.Name).IsRequired();
        });

        modelBuilder.Entity<Column>(entity =>
        {
            entity.HasKey(column => new { column.BoardId, column.Id });
            entity.Property(column => column.Id).ValueGeneratedNever();
            entity.Property(column => column.Title).IsRequired();
            entity.Property(column => column.BoardId).IsRequired();
            entity.HasMany(column => column.Cards)
                .WithOne(card => card.Column)
                .HasForeignKey(card => new { card.BoardId, card.ColumnId })
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Card>(entity =>
        {
            entity.HasKey(card => new { card.BoardId, card.ColumnId, card.Id });
            entity.Property(card => card.Id).ValueGeneratedNever();
            entity.Property(card => card.Text).IsRequired();
            entity.Property(card => card.BoardId).IsRequired();
            entity.Property(card => card.ColumnId).IsRequired();
            entity.HasMany(card => card.Comments)
                .WithOne(comment => comment.Card)
                .HasForeignKey(comment => new { comment.BoardId, comment.ColumnId, comment.CardId })
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(comment => new { comment.BoardId, comment.ColumnId, comment.CardId, comment.Id });
            entity.Property(comment => comment.Id).ValueGeneratedNever();
            entity.Property(comment => comment.Author).IsRequired();
            entity.Property(comment => comment.Text).IsRequired();
            entity.Property(comment => comment.CreatedAt).IsRequired();
            entity.Property(comment => comment.BoardId).IsRequired();
            entity.Property(comment => comment.ColumnId).IsRequired();
            entity.Property(comment => comment.CardId).IsRequired();
        });
    }
}
