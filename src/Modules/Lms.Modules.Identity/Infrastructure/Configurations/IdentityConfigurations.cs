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

internal sealed class InstructorProfileConfiguration : IEntityTypeConfiguration<InstructorProfile>
{
    public void Configure(EntityTypeBuilder<InstructorProfile> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("instructor_profiles");

        // UserId is both PK and FK — one profile per user, enforced by the key itself rather
        // than by a unique index that someone could later drop.
        builder.HasKey(p => p.UserId);
        builder.Property(p => p.UserId).ValueGeneratedNever();

        builder.Property(p => p.Slug).HasMaxLength(InstructorProfile.SlugMaxLength).IsRequired();
        builder.Property(p => p.Headline)
            .HasMaxLength(InstructorProfile.HeadlineMaxLength)
            .IsRequired();
        builder.Property(p => p.Bio).HasMaxLength(InstructorProfile.BioMaxLength);
        builder.Property(p => p.AvatarBlobPath).HasMaxLength(InstructorProfile.UrlMaxLength + 100);
        builder.Property(p => p.WebsiteUrl).HasMaxLength(InstructorProfile.UrlMaxLength);

        // Named explicitly: the snake_case convention splits the internal capitals and produces
        // git_hub_url and linked_in_url. pgweb exists so the database can be read by hand, which
        // is the whole reason for snake_case — these two are worth the two extra lines.
        builder.Property(p => p.GitHubUrl)
            .HasColumnName("github_url")
            .HasMaxLength(InstructorProfile.UrlMaxLength);
        builder.Property(p => p.LinkedInUrl)
            .HasColumnName("linkedin_url")
            .HasMaxLength(InstructorProfile.UrlMaxLength);
        builder.Property(p => p.CreatedAt).IsRequired();

        // The public catalog filters by instructor slug, and grant checks it for collisions.
        // Unique in the database, not just in the handler — two admins can grant at once.
        builder.HasIndex(p => p.Slug)
            .IsUnique()
            .HasDatabaseName("ux_instructor_profiles_slug");

        builder.HasOne<AppUser>()
            .WithOne()
            .HasForeignKey<InstructorProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
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
