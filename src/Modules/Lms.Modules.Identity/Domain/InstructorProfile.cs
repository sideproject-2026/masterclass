using System.Text.RegularExpressions;
using Lms.SharedKernel.Results;

namespace Lms.Modules.Identity.Domain;

/// <summary>
/// The public face of an instructor, created when the <c>Instructor</c> role is granted.
/// </summary>
/// <remarks>
/// Separate from <see cref="AppUser"/> because it is published content, not credentials: the
/// slug and headline appear on a course page that anonymous visitors can read, while everything
/// on <c>AppUser</c> is either private or authentication plumbing. Keeping them apart means a
/// query that projects a public profile cannot accidentally reach a password hash.
/// <para>
/// One-to-one with <c>AppUser</c>: <see cref="UserId"/> is both primary key and foreign key.
/// A real FK is correct here — both tables live in the <c>identity</c> schema, so this is not
/// the cross-module case the no-FK rule in 01-architecture.md §4.1 is about.
/// </para>
/// </remarks>
public sealed partial class InstructorProfile
{
    public const int SlugMaxLength = 80;
    public const int HeadlineMaxLength = 160;
    public const int BioMaxLength = 2000;
    public const int UrlMaxLength = 300;

    // EF materialisation.
    private InstructorProfile()
    {
        Slug = null!;
        Headline = null!;
    }

    private InstructorProfile(Guid userId, string slug, string headline, DateTimeOffset createdAt)
    {
        UserId = userId;
        Slug = slug;
        Headline = headline;
        CreatedAt = createdAt;
    }

    /// <summary>Primary key and foreign key. One profile per user, never more.</summary>
    public Guid UserId { get; private set; }

    /// <summary>URL segment: <c>/instructors/scott-allen</c>. Unique across all instructors.</summary>
    public string Slug { get; private set; }

    public string Headline { get; private set; }

    /// <summary>Markdown, rendered through the sanitiser like all other authored content.</summary>
    public string? Bio { get; private set; }

    /// <summary>Path in the <c>course-assets</c> container, not a URL.</summary>
    public string? AvatarBlobPath { get; private set; }

    public string? WebsiteUrl { get; private set; }

    public string? GitHubUrl { get; private set; }

    public string? LinkedInUrl { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Creates a profile, validating the slug.
    /// </summary>
    /// <remarks>
    /// Returns <see cref="Result{T}"/> rather than throwing: a badly formed slug is an expected
    /// failure that the caller turns into a 422, not a programmer error.
    /// </remarks>
    public static Result<InstructorProfile> Create(
        Guid userId,
        string slug,
        string headline,
        DateTimeOffset now)
    {
        if (userId == Guid.Empty)
        {
            return IdentityErrors.UserNotFound;
        }

        var normalisedSlug = (slug ?? string.Empty).Trim().ToLowerInvariant();

        if (normalisedSlug.Length > SlugMaxLength || !SlugPattern().IsMatch(normalisedSlug))
        {
            return IdentityErrors.InvalidSlug;
        }

        var trimmedHeadline = (headline ?? string.Empty).Trim();

        if (trimmedHeadline.Length is 0 or > HeadlineMaxLength)
        {
            return IdentityErrors.InvalidHeadline;
        }

        return new InstructorProfile(userId, normalisedSlug, trimmedHeadline, now);
    }

    public void UpdateHeadline(string headline)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(headline);
        Headline = headline.Trim();
    }

    /// <summary>
    /// Lowercase letters, digits and single hyphens; no leading, trailing or doubled hyphen.
    /// </summary>
    /// <remarks>
    /// The slug goes straight into a public URL, so it is constrained at the domain boundary
    /// rather than trusted from an admin's request body. Length is checked separately, before
    /// the match, so the column width is never the thing that enforces it.
    /// </remarks>
    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.None, matchTimeoutMilliseconds: 200)]
    private static partial Regex SlugPattern();
}
