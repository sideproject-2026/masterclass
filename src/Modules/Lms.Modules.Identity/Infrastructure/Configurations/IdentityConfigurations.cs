using Lms.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lms.Modules.Identity.Infrastructure.Configurations;

internal sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("users");

        builder.Property(u => u.Id).ValueGeneratedNever();   // UUIDv7 from application code
        builder.Property(u => u.DisplayName)
            .HasMaxLength(AppUser.DisplayNameMaxLength)
            .IsRequired();
        builder.Property(u => u.CreatedAt).IsRequired();
    }
}

internal sealed class AppRoleConfiguration : IEntityTypeConfiguration<AppRole>
{
    public void Configure(EntityTypeBuilder<AppRole> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("roles");
        builder.Property(r => r.Id).ValueGeneratedNever();
    }
}

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("refresh_tokens");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        // SHA-256 hex is always 64 chars. The raw token is never persisted.
        builder.Property(t => t.TokenHash).HasMaxLength(64).IsRequired();
        builder.Property(t => t.ReplacedByTokenHash).HasMaxLength(64);

        builder.Property(t => t.UserId).IsRequired();
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.ExpiresAt).IsRequired();

        // Refresh looks tokens up by hash, so this must be unique and indexed.
        builder.HasIndex(t => t.TokenHash)
            .IsUnique()
            .HasDatabaseName("ux_refresh_tokens_token_hash");

        // Revoking a user's whole chain on reuse detection scans by user.
        builder.HasIndex(t => new { t.UserId, t.RevokedAt })
            .HasDatabaseName("ix_refresh_tokens_user_active");

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
