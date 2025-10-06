using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IDontBelieve.Core.Models;

namespace IDontBelieve.Infrastructure.Data.Configurations;

public class GameRoomConfiguration : IEntityTypeConfiguration<GameRoom>
{
    public void Configure(EntityTypeBuilder<GameRoom> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(e => e.DeckType)
            .HasConversion<int>();
            
        builder.Property(e => e.Status)
            .HasConversion<int>();
            
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.CreatedAt);
        
        builder.HasMany(e => e.Players)
            .WithOne(e => e.GameRoom)
            .HasForeignKey(e => e.GameRoomId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(e => e.CreatedBy)
            .WithMany()
            .HasForeignKey(e => e.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(e => e.GameState)
            .WithOne(e => e.GameRoom)
            .HasForeignKey<GameState>(e => e.GameRoomId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}