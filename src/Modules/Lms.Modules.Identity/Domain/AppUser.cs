using Microsoft.AspNetCore.Identity;

namespace Lms.Modules.Identity.Domain;

/// <summary>
/// A person. Every registered user holds <c>Student</c>; <c>Instructor</c> and <c>Admin</c>
/// are granted, never self-assigned (artifacts/design/00-overview.md §5).
/// </summary>
/// <remarks>
/// <see cref="IdentityUser{TKey}"/> brings the password hash, security stamp, lockout and
/// 2FA columns. Only the fields below are domain concerns; the rest is framework plumbing
/// and nothing outside this module should read it.
/// <para>
/// <c>Id</c> is what every other module stores as <c>UserId</c>, and what appears as the
/// <c>sub</c> claim — see artifacts/design/04-adr-authentication.md §5.
/// </para>
/// </remarks>
public sealed class AppUser : IdentityUser<Guid>
{
    public const int DisplayNameMaxLength = 100;

    // EF materialisation.
    private AppUser() => DisplayName = null!;

    private AppUser(Guid id, string email, string displayName, DateTimeOffset createdAt)
    {
        Id = id;
        Email = email;
        UserName = email;              // email is the login identifier
        NormalizedEmail = email.ToUpperInvariant();
        NormalizedUserName = email.ToUpperInvariant();
        DisplayName = displayName;
        CreatedAt = createdAt;
    }

    /// <summary>Shown as the author name and in the nav bar.</summary>
    public string DisplayName { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>UUIDv7 generated here, never by the database (02-domain-model.md §8.2).</summary>
    public static AppUser Create(string email, string displayName, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        return new AppUser(Guid.CreateVersion7(), email.Trim(), displayName.Trim(), now);
    }

    public void Rename(string displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        DisplayName = displayName.Trim();
    }
}
