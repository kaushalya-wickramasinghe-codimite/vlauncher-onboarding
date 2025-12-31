using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VLauncher.Domain.Entities;

namespace VLauncher.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.GoogleEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(u => u.GoogleEmail)
            .IsUnique();

        builder.Property(u => u.AdUserPrincipalName)
            .HasMaxLength(256);

        builder.Property(u => u.Status)
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .IsRequired();
    }
}
