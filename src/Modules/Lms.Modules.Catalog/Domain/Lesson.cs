using Lms.SharedKernel.Identifiers;
using Lms.SharedKernel.Results;

namespace Lms.Modules.Catalog.Domain;

/// <summary>
/// A single unit of content — a video or a reading. Part of the <see cref="Course"/> aggregate;
/// reached through its <see cref="Chapter"/>, never saved on its own.
/// </summary>
/// <remarks>
/// The content invariant (02-domain-model.md §3.3) is the whole reason this type has no public
/// setters. Both content columns are nullable at the schema level and the *combination* is what
/// is constrained, so a setter per field would let a caller leave a Video lesson with reading
/// markdown and no video id — valid to the database, meaningless to the player.
/// <see cref="SetVideoContent"/> and <see cref="SetReadingContent"/> are the only ways in, and
/// each clears the other type's fields.
/// </remarks>
public sealed class Lesson
{
    public const int TitleMaxLength = 160;
    public const int ExternalVideoIdMaxLength = 64;

    // EF materialisation.
    private Lesson() => Title = null!;

    private Lesson(ChapterId chapterId, string title, int sortOrder, DateTimeOffset now)
    {
        Id = LessonId.New();
        ChapterId = chapterId;
        Title = title;
        SortOrder = sortOrder;
        Type = LessonType.Reading;
        IsRequired = true;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public LessonId Id { get; private set; }

    public ChapterId ChapterId { get; private set; }

    public string Title { get; private set; }

    /// <summary>Dense and 0-based within the chapter. Rewritten wholesale on reorder.</summary>
    public int SortOrder { get; private set; }

    public LessonType Type { get; private set; }

    /// <summary>When true, readable without enrolling. The API's 403 is the gate, not the UI.</summary>
    public bool IsPreview { get; private set; }

    /// <summary>False excludes the lesson from the completion calculation.</summary>
    public bool IsRequired { get; private set; }

    public VideoProvider? VideoProvider { get; private set; }

    /// <summary>The bare id (<c>dQw4w9WgXcQ</c>), never a URL — 05-adr-video-and-storage.md §2.1.</summary>
    public string? ExternalVideoId { get; private set; }

    /// <summary>Video runtime, or estimated reading time. Feeds <see cref="Course.EstimatedMinutes"/>.</summary>
    public int DurationSeconds { get; private set; }

    public string? ContentMarkdown { get; private set; }

    public string? NotesMarkdown { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    /// A new lesson is a <see cref="LessonType.Reading"/> with no content yet.
    /// </summary>
    /// <remarks>
    /// Deliberately *not* publishable on creation: an empty lesson is a legitimate intermediate
    /// state while authoring, and <see cref="Course.PublishViolations"/> is what stops it
    /// reaching students. Requiring content up front would mean the Studio could not create a
    /// lesson before the instructor had written it.
    /// </remarks>
    internal static Result<Lesson> Create(
        ChapterId chapterId,
        string title,
        int sortOrder,
        DateTimeOffset now)
    {
        var trimmed = (title ?? string.Empty).Trim();

        if (trimmed.Length is 0 or > TitleMaxLength)
        {
            return CatalogErrors.TitleRequired;
        }

        return new Lesson(chapterId, trimmed, sortOrder, now);
    }

    public Result Rename(string title, DateTimeOffset now)
    {
        var trimmed = (title ?? string.Empty).Trim();

        if (trimmed.Length is 0 or > TitleMaxLength)
        {
            return CatalogErrors.TitleRequired;
        }

        Title = trimmed;
        UpdatedAt = now;
        return Result.Success();
    }

    /// <summary>
    /// Makes this a video lesson, clearing any reading content.
    /// </summary>
    public Result SetVideoContent(
        VideoProvider provider,
        string externalVideoId,
        int durationSeconds,
        string? notesMarkdown,
        DateTimeOffset now)
    {
        var id = (externalVideoId ?? string.Empty).Trim();

        if (id.Length is 0 or > ExternalVideoIdMaxLength || durationSeconds <= 0)
        {
            return CatalogErrors.VideoContentIncomplete;
        }

        Type = LessonType.Video;
        VideoProvider = provider;
        ExternalVideoId = id;
        DurationSeconds = durationSeconds;
        NotesMarkdown = string.IsNullOrWhiteSpace(notesMarkdown) ? null : notesMarkdown;

        // Switching type clears the other side. Leaving it would keep a reading body that
        // nothing renders and that the next publish check would have to reason about.
        ContentMarkdown = null;

        UpdatedAt = now;
        return Result.Success();
    }

    /// <summary>
    /// Makes this a reading lesson, clearing any video fields.
    /// </summary>
    public Result SetReadingContent(string contentMarkdown, int estimatedReadSeconds, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(contentMarkdown))
        {
            return CatalogErrors.ReadingContentIncomplete;
        }

        if (estimatedReadSeconds < 0)
        {
            return CatalogErrors.InvalidDuration;
        }

        Type = LessonType.Reading;
        ContentMarkdown = contentMarkdown;
        DurationSeconds = estimatedReadSeconds;

        VideoProvider = null;
        ExternalVideoId = null;
        NotesMarkdown = null;

        UpdatedAt = now;
        return Result.Success();
    }

    public void SetPreview(bool isPreview, DateTimeOffset now)
    {
        IsPreview = isPreview;
        UpdatedAt = now;
    }

    public void SetRequired(bool isRequired, DateTimeOffset now)
    {
        IsRequired = isRequired;
        UpdatedAt = now;
    }

    internal void SetSortOrder(int sortOrder) => SortOrder = sortOrder;

    internal void MoveToChapter(ChapterId chapterId, int sortOrder, DateTimeOffset now)
    {
        ChapterId = chapterId;
        SortOrder = sortOrder;
        UpdatedAt = now;
    }

    /// <summary>
    /// Whether the lesson satisfies the content invariant — 02-domain-model.md §3.3.
    /// </summary>
    /// <remarks>
    /// Read by <see cref="Course.PublishViolations"/>. It is a query rather than a guard because
    /// an incomplete lesson is a normal state while authoring; it only becomes a problem at the
    /// moment somebody tries to publish.
    /// </remarks>
    public bool HasCompleteContent() => Type switch
    {
        LessonType.Video => VideoProvider is not null
            && !string.IsNullOrWhiteSpace(ExternalVideoId)
            && DurationSeconds > 0,
        LessonType.Reading => !string.IsNullOrWhiteSpace(ContentMarkdown),
        _ => false,
    };
}
