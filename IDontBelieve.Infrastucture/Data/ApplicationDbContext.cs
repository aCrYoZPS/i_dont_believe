using Microsoft.EntityFrameworkCore;
using IDontBelieve.Core.Models;
using IDontBelieve.Infrastructure.Data.Configurations;

namespace IDontBelieve.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    
    public DbSet<GameRoom> GameRooms { get; set; }
    public DbSet<GameState> GameStates { get; set; }
    public DbSet<GameMove> GameMoves { get; set; }
    public DbSet<GamePlayer> GamePlayers { get; set; }
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new GameRoomConfiguration());
        modelBuilder.ApplyConfiguration(new GameStateConfiguration());
        
        modelBuilder.Entity<GamePlayer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<int>();
            
            entity.HasOne(e => e.User)
                  .WithMany(e => e.GameRoom)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => new { e.GameRoomId, e.UserId }).IsUnique();
        });

        modelBuilder.Entity<GameMove>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Outcome).HasConversion<int>();
            
            entity.HasOne(e => e.GameState)
                  .WithMany(e => e.MoveHistory)
                  .HasForeignKey(e => e.GameStateId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => new { e.GameStateId, e.MoveNumber }).IsUnique();
        });
    }
}