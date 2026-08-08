using Lms.Modules.Identity.Domain;
using Lms.SharedKernel.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Lms.Modules.Identity.Infrastructure;

/// <summary>
/// One DbContext per module, one schema per module (01-architecture.md §4).
/// </summary>
/// <remarks>
/// Named <c>IdentityModuleDbContext</c> rather than <c>IdentityDbContext</c> to avoid
/// colliding with the ASP.NET Core Identity base class it derives from.
/// </remarks>
public sealed class IdentityModuleDbContext(DbContextOptions<IdentityModuleDbContext> options)
    : IdentityDbContext<AppUser, AppRole, Guid>(options)
{
    public const string Schema = "identity";
    public const string MigrationsHistoryTable = "__ef_migrations_history_identity";

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Parameter is named 'builder' to match the base signature (CA1725).
    protected override void OnModelCreating(ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        base.OnModelCreating(builder);

        builder.HasDefaultSchema(Schema);
        builder.ApplyConfigurationsFromAssembly(typeof(IdentityModuleDbContext).Assembly);

        RenameFrameworkTables(builder);
    }

    /// <summary>
    /// Drops the <c>asp_net_</c> prefix from the Identity framework tables.
    /// </summary>
    /// <remarks>
    /// They are already namespaced by the <c>identity</c> schema, so the prefix is noise —
    /// and leaving it would mean half this schema reads <c>users</c> and half reads
    /// <c>asp_net_user_roles</c>. Free to do now, a rename migration later.
    /// </remarks>
    private static void RenameFrameworkTables(ModelBuilder builder)
    {
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("user_roles");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("role_claims");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens");
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder);

        configurationBuilder.ApplyStronglyTypedIdConventions();
    }
}
