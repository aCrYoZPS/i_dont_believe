using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IDontBelieve.Core.Models;

namespace IDontBelieve.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.UserName)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(e => e.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.RefreshToken)
            .HasMaxLength(500);
            
        builder.Property(e => e.Coins)
            .HasPrecision(18, 2);
            
        builder.HasIndex(e => e.UserName).IsUnique();
        builder.HasIndex(e => e.Email).IsUnique();
        builder.HasIndex(e => e.Rating);
        
        /*builder.HasMany(e => e.GamePlayers)
            .WithOne(e => e.User)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);*/
    }
}