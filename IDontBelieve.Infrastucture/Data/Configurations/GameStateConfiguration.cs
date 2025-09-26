using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IDontBelieve.Core.Models;

namespace IDontBelieve.Infrastructure.Data.Configurations;

public class GameStateConfiguration : IEntityTypeConfiguration<GameState>
{
    public void Configure(EntityTypeBuilder<GameState> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Phase)
            .HasConversion<int>();
            
        builder.Property(e => e.DeckJson)
            .HasColumnName("Deck");
            
        builder.Property(e => e.DiscardPileJson)
            .HasColumnName("DiscardPile");
            
        builder.HasIndex(e => e.GameRoomId).IsUnique();
        builder.HasIndex(e => e.LastMoveAt);
        
        builder.HasMany(e => e.MoveHistory)
            .WithOne(e => e.GameState)
            .HasForeignKey(e => e.GameStateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}