using System.Text;
using System.Text.RegularExpressions;
using Lms.SharedKernel.Identifiers;
using Lms.SharedKernel.Results;

namespace Lms.Modules.Catalog.Domain;

/// <summary>
/// The aggregate root for authoring — 02-domain-model.md §3.
/// </summary>
/// <remarks>
/// Chapters and lessons are reached through this type and never saved independently. That is
/// what lets <see cref="Publish"/> answer for the whole curriculum in one place: the publish
/// invariants span all three levels, and an aggregate that did not own its children would have
/// to query for them and hope nothing changed in between.
/// </remarks>
public sealed partial class Course
{
    public const int SlugMaxLength = 120;
    public const int TitleMaxLength = 160;
    public const int SubtitleMaxLength = 300;
    public const int DescriptionMaxLength = 8000;
    public const int ThumbnailPathMaxLength = 400;
    public const int MaxTags = 8;

    private readonly List<Chapter> _chapters = [];
    private string[] _tags = [];

    // EF materialisation.
    private Course()
    {
        Slug = null!;
        Title = null!;
        Description = null!;
    }

    private Course(
        UserId instructorId,
        string slug,
        string title,
        string description,
        CourseLevel level,
        DateTimeOffset now)
    {
        Id = CourseId.New();
        InstructorId = instructorId;
        Slug = slug;
        Title = title;
        Description = description;
        Level = level;
        Status = CourseStatus.Draft;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public CourseId Id { get; private set; }

    /// <summary>
    /// Unique. Editable while <see cref="CourseStatus.Draft"/>, frozen once published —
    /// a published URL is somebody else's bookmark.
    /// </summary>
    public string Slug { get; private set; }

    public string Title { get; private set; }

    public string? Subtitle { get; private set; }

    /// <summary>Markdown. Rendered through the sanitiser, like all authored content.</summary>
    public string Description { get; private set; }

    /// <summary>
    /// Logical reference to <c>identity.users</c> — an indexed <see cref="UserId"/>, never a
    /// foreign key. Cross-module FKs are what turn a modular monolith back into a monolith
    /// (01-architecture.md §4.1).
    /// </summary>
    public UserId InstructorId { get; private set; }

    public CourseLevel Level { get; private set; }

    public CourseStatus Status { get; private set; }

    /// <summary>Path in the public asset bucket, not a URL. Required to publish.</summary>
    public string? ThumbnailBlobPath { get; private set; }

    /// <summary>
    /// PostgreSQL <c>text[]</c>, lowercased, capped at <see cref="MaxTags"/>. Not a join table —
    /// the reasoning is in 02-domain-model.md §3.5.
    /// </summary>
    public IReadOnlyList<string> Tags => _tags;

    /// <summary>Derived from lesson durations. Recomputed whenever the curriculum changes.</summary>
    public int EstimatedMinutes { get; private set; }

    /// <summary>Derived. A column rather than a count, so catalogue cards are not an N+1.</summary>
    public int LessonCount { get; private set; }

    /// <summary>
    /// Denormalised from the Enrollment module via <c>StudentEnrolled</c>/<c>StudentUnenrolled</c>.
    /// Eventually consistent by design — Catalog cannot query enrollment tables.
    /// </summary>
    public int EnrollmentCount { get; private set; }

    public DateTimeOffset? PublishedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    /// Postgres <c>xmin</c>. A <c>uint</c> with <c>IsRowVersion()</c> — no extra column.
    /// </summary>
    public uint Version { get; private set; }

    public IReadOnlyList<Chapter> Chapters => _chapters;

    public static Result<Course> Create(
        UserId instructorId,
        string title,
        string description,
        CourseLevel level,
        DateTimeOffset now)
    {
        if (instructorId == UserId.Empty)
        {
            return CatalogErrors.InstructorRequired;
        }

        var trimmedTitle = (title ?? string.Empty).Trim();

        if (trimmedTitle.Length is 0 or > TitleMaxLength)
        {
            return CatalogErrors.TitleRequired;
        }

        var trimmedDescription = (description ?? string.Empty).Trim();

        if (trimmedDescription.Length is 0 or > DescriptionMaxLength)
        {
            return CatalogErrors.DescriptionRequired;
        }

        var slug = Slugify(trimmedTitle);

        // A title of nothing but punctuation slugifies to an empty string. Rather than invent a
        // fallback here, refuse it — the caller can set a slug explicitly.
        if (slug.Length == 0)
        {
            return CatalogErrors.InvalidSlug;
        }

        return new Course(instructorId, slug, trimmedTitle, trimmedDescription, level, now);
    }

    public Result UpdateDetails(
        string title,
        string? subtitle,
        string description,
        CourseLevel level,
        DateTimeOffset now)
    {
        if (Status == CourseStatus.Archived)
        {
            return CatalogErrors.CannotEditArchived;
        }

        var trimmedTitle = (title ?? string.Empty).Trim();

        if (trimmedTitle.Length is 0 or > TitleMaxLength)
        {
            return CatalogErrors.TitleRequired;
        }

        var trimmedDescription = (description ?? string.Empty).Trim();

        if (trimmedDescription.Length is 0 or > DescriptionMaxLength)
        {
            return CatalogErrors.DescriptionRequired;
        }

        var trimmedSubtitle = subtitle?.Trim();

        if (trimmedSubtitle?.Length > SubtitleMaxLength)
        {
            return CatalogErrors.SubtitleTooLong;
        }

        Title = trimmedTitle;
        Subtitle = string.IsNullOrWhiteSpace(trimmedSubtitle) ? null : trimmedSubtitle;
        Description = trimmedDescription;
        Level = level;
        Touch(now);

        // Note what does NOT happen here: the slug is not regenerated from the new title.
        // Retitling a draft and silently moving its URL is surprising even before publication.
        return Result.Success();
    }

    public Result ChangeSlug(string slug, DateTimeOffset now)
    {
        if (Status != CourseStatus.Draft)
        {
            return CatalogErrors.SlugFrozen;
        }

        var normalised = (slug ?? string.Empty).Trim().ToLowerInvariant();

        if (normalised.Length is 0 or > SlugMaxLength || !SlugPattern().IsMatch(normalised))
        {
            return CatalogErrors.InvalidSlug;
        }

        Slug = normalised;
        Touch(now);
        return Result.Success();
    }

    public Result SetThumbnail(string? blobPath, DateTimeOffset now)
    {
        if (Status == CourseStatus.Archived)
        {
            return CatalogErrors.CannotEditArchived;
        }

        var trimmed = blobPath?.Trim();

        if (trimmed?.Length > ThumbnailPathMaxLength)
        {
            return CatalogErrors.ThumbnailRequired;
        }

        ThumbnailBlobPath = string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
        Touch(now);
        return Result.Success();
    }

    /// <summary>
    /// Replaces the tag set, lowercased and de-duplicated.
    /// </summary>
    public Result SetTags(IEnumerable<string> tags, DateTimeOffset now)
    {
        if (Status == CourseStatus.Archived)
        {
            return CatalogErrors.CannotEditArchived;
        }

        var normalised = (tags ?? [])
            .Select(tag => (tag ?? string.Empty).Trim().ToLowerInvariant())
            .Where(tag => tag.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        // The cap is checked after de-duplication, so ["dotnet","DotNet"] counts as one.
        if (normalised.Length > MaxTags)
        {
            return CatalogErrors.TooManyTags;
        }

        _tags = normalised;
        Touch(now);
        return Result.Success();
    }

    public Result<Chapter> AddChapter(string title, DateTimeOffset now)
    {
        if (Status == CourseStatus.Archived)
        {
            return CatalogErrors.CannotEditArchived;
        }

        var chapter = Chapter.Create(Id, title, _chapters.Count);

        if (chapter.IsFailure)
        {
            return chapter;
        }

        _chapters.Add(chapter.Value);
        Touch(now);
        return chapter;
    }

    public Result RemoveChapter(ChapterId chapterId, DateTimeOffset now)
    {
        var chapter = _chapters.SingleOrDefault(c => c.Id == chapterId);

        if (chapter is null)
        {
            return CatalogErrors.ChapterNotFound;
        }

        _chapters.Remove(chapter);
        RenumberChapters();
        RecomputeDerived();
        Touch(now);
        return Result.Success();
    }

    public Result<Lesson> AddLesson(ChapterId chapterId, string title, DateTimeOffset now)
    {
        if (Status == CourseStatus.Archived)
        {
            return CatalogErrors.CannotEditArchived;
        }

        var chapter = _chapters.SingleOrDefault(c => c.Id == chapterId);

        if (chapter is null)
        {
            return CatalogErrors.ChapterNotFound;
        }

        var lesson = chapter.AddLesson(title, now);

        if (lesson.IsSuccess)
        {
            RecomputeDerived();
            Touch(now);
        }

        return lesson;
    }

    /// <summary>
    /// Finds a lesson anywhere in the curriculum.
    /// </summary>
    /// <remarks>
    /// Content changes go through the lesson itself, so callers need a way to reach one without
    /// knowing its chapter. <see cref="RecomputeDerived"/> must be called afterwards if the
    /// duration changed — <see cref="SetLessonContentChanged"/> exists for exactly that.
    /// </remarks>
    public Lesson? FindLesson(LessonId lessonId)
    {
        foreach (var chapter in _chapters)
        {
            var lesson = chapter.FindLesson(lessonId);

            if (lesson is not null)
            {
                return lesson;
            }
        }

        return null;
    }

    /// <summary>
    /// Call after changing a lesson's duration, so the derived totals stay true.
    /// </summary>
    public void SetLessonContentChanged(DateTimeOffset now)
    {
        RecomputeDerived();
        Touch(now);
    }

    public Result RemoveLesson(LessonId lessonId, DateTimeOffset now)
    {
        var chapter = _chapters.SingleOrDefault(c => c.FindLesson(lessonId) is not null);

        if (chapter is null)
        {
            return CatalogErrors.LessonNotFound;
        }

        chapter.RemoveLesson(lessonId);
        RecomputeDerived();
        Touch(now);
        return Result.Success();
    }

    public Result ReorderChapters(IReadOnlyList<ChapterId> orderedIds, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(orderedIds);

        if (orderedIds.Count != _chapters.Count || orderedIds.Distinct().Count() != orderedIds.Count)
        {
            return CatalogErrors.ReorderMismatch;
        }

        var byId = _chapters.ToDictionary(c => c.Id);

        if (!orderedIds.All(byId.ContainsKey))
        {
            return CatalogErrors.ReorderMismatch;
        }

        for (var index = 0; index < orderedIds.Count; index++)
        {
            byId[orderedIds[index]].SetSortOrder(index);
        }

        SortChapters();
        Touch(now);
        return Result.Success();
    }

    public Result ReorderLessons(ChapterId chapterId, IReadOnlyList<LessonId> orderedIds, DateTimeOffset now)
    {
        var chapter = _chapters.SingleOrDefault(c => c.Id == chapterId);

        if (chapter is null)
        {
            return CatalogErrors.ChapterNotFound;
        }

        var result = chapter.ReorderLessons(orderedIds);

        if (result.IsSuccess)
        {
            Touch(now);
        }

        return result;
    }

    /// <summary>
    /// Moves a lesson to another chapter, appending it at the end.
    /// </summary>
    public Result MoveLesson(LessonId lessonId, ChapterId targetChapterId, DateTimeOffset now)
    {
        var source = _chapters.SingleOrDefault(c => c.FindLesson(lessonId) is not null);

        if (source is null)
        {
            return CatalogErrors.LessonNotFound;
        }

        var target = _chapters.SingleOrDefault(c => c.Id == targetChapterId);

        if (target is null)
        {
            return CatalogErrors.ChapterNotFound;
        }

        if (source.Id == target.Id)
        {
            return Result.Success();
        }

        var lesson = source.FindLesson(lessonId)!;
        source.DetachLesson(lesson);
        target.AttachLesson(lesson);
        Touch(now);
        return Result.Success();
    }

    /// <summary>
    /// Every reason this course cannot be published, in the order an author would fix them.
    /// </summary>
    /// <remarks>
    /// A list rather than a first-failure, because 02-domain-model.md §3.2 requires the 422 to
    /// name every problem: telling an author about one missing thing at a time turns publishing
    /// into a guessing game. <see cref="Publish"/> summarises this into a single
    /// <see cref="Error"/> because that is all <see cref="Result"/> carries — `S-6` reads this
    /// method directly to build the structured response. A multi-error Result is not being
    /// introduced for one caller (rule of two).
    /// </remarks>
    public IReadOnlyList<Error> PublishViolations()
    {
        var violations = new List<Error>();

        if (string.IsNullOrWhiteSpace(Title))
        {
            violations.Add(CatalogErrors.TitleRequired);
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            violations.Add(CatalogErrors.DescriptionRequired);
        }

        if (string.IsNullOrWhiteSpace(ThumbnailBlobPath))
        {
            violations.Add(CatalogErrors.ThumbnailRequired);
        }

        if (_chapters.Count == 0)
        {
            violations.Add(CatalogErrors.NoChapters);
        }

        foreach (var chapter in _chapters.Where(chapter => chapter.Lessons.Count == 0))
        {
            violations.Add(CatalogErrors.EmptyChapter(chapter.Title));
        }

        foreach (var lesson in _chapters.SelectMany(c => c.Lessons).Where(l => !l.HasCompleteContent()))
        {
            violations.Add(CatalogErrors.LessonContentMissing(lesson.Title));
        }

        // Checked only when lessons exist: "no required lesson" on a course with no lessons at
        // all is noise on top of the errors that already say so.
        if (_chapters.Any(c => c.Lessons.Count > 0)
            && !_chapters.SelectMany(c => c.Lessons).Any(l => l.IsRequired))
        {
            violations.Add(CatalogErrors.NoRequiredLesson);
        }

        return violations;
    }

    public Result Publish(DateTimeOffset now)
    {
        if (Status == CourseStatus.Archived)
        {
            return CatalogErrors.CannotPublishArchived;
        }

        var violations = PublishViolations();

        if (violations.Count > 0)
        {
            return CatalogErrors.PublishInvariantsUnmet(violations.Count);
        }

        // Re-publishing keeps the original PublishedAt: the catalogue sorts on it, and a brief
        // unpublish to fix a typo should not send a year-old course back to the top of "new".
        PublishedAt ??= now;
        Status = CourseStatus.Published;
        Touch(now);
        return Result.Success();
    }

    public Result Unpublish(DateTimeOffset now)
    {
        if (Status != CourseStatus.Published)
        {
            return Result.Success();
        }

        Status = CourseStatus.Draft;
        Touch(now);
        return Result.Success();
    }

    public Result Archive(DateTimeOffset now)
    {
        if (Status != CourseStatus.Published)
        {
            return CatalogErrors.CannotArchiveDraft;
        }

        // Archiving hides the course from the catalogue. It deliberately does not touch
        // enrollments — 02 §3.1: students who already enrolled keep full access.
        Status = CourseStatus.Archived;
        Touch(now);
        return Result.Success();
    }

    public Result Restore(DateTimeOffset now)
    {
        if (Status != CourseStatus.Archived)
        {
            return CatalogErrors.CannotRestoreUnarchived;
        }

        Status = CourseStatus.Published;
        Touch(now);
        return Result.Success();
    }

    /// <summary>Applied by the handler subscribing to Enrollment's events.</summary>
    public void AdjustEnrollmentCount(int delta) =>
        EnrollmentCount = Math.Max(0, EnrollmentCount + delta);

    private void RecomputeDerived()
    {
        var lessons = _chapters.SelectMany(c => c.Lessons).ToList();

        LessonCount = lessons.Count;

        // Rounded up: a 90-second lesson reading as "0 minutes" on a catalogue card is worse
        // than reading as "1 minute".
        EstimatedMinutes = (int)Math.Ceiling(lessons.Sum(l => l.DurationSeconds) / 60.0);
    }

    private void RenumberChapters()
    {
        SortChapters();

        for (var index = 0; index < _chapters.Count; index++)
        {
            _chapters[index].SetSortOrder(index);
        }
    }

    private void SortChapters() =>
        _chapters.Sort((left, right) => left.SortOrder.CompareTo(right.SortOrder));

    private void Touch(DateTimeOffset now) => UpdatedAt = now;

    /// <summary>
    /// Latin-1 letters folded to ASCII, by index. The two strings are parallel and must stay
    /// the same length.
    /// </summary>
    private const string AccentedLetters =
        "ÀÁÂÃÄÅàáâãäåÇçÈÉÊËèéêëÌÍÎÏìíîïÑñÒÓÔÕÖØòóôõöøÙÚÛÜùúûüÝýÿ";

    private const string FoldedLetters =
        "AAAAAAaaaaaaCcEEEEeeeeIIIIiiiiNnOOOOOOooooooUUUUuuuuYyy";

    /// <summary>
    /// Title → URL segment: accents folded to ASCII, lowercased, runs of anything else
    /// collapsed to a single hyphen.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The folding is an explicit table rather than the usual
    /// <c>Normalize(FormD)</c>-and-drop-the-combining-marks trick, because this solution builds
    /// with <c>InvariantGlobalization=true</c> and **that makes <c>string.Normalize</c> a silent
    /// no-op** — it does not throw, it returns the string unchanged. The idiomatic version
    /// therefore compiles, passes on ASCII titles, and quietly turns "Réactivité" into
    /// "r-activit-". A public URL is the wrong place to discover that.
    /// </para>
    /// <para>
    /// Anything outside the table collapses to a hyphen, which is predictable rather than
    /// clever: a title in a non-Latin script produces an empty slug, <see cref="Create"/>
    /// rejects it, and the author sets one explicitly.
    /// </para>
    /// <para>
    /// Uniqueness is the database's job — the unique index on <c>slug</c> is the arbiter, the
    /// same call made for the instructor slug in <c>A-6</c>. Checking first and inserting after
    /// still loses to a concurrent create.
    /// </para>
    /// </remarks>
    private static string Slugify(string title)
    {
        var builder = new StringBuilder(title.Length);

        foreach (var character in title)
        {
            var index = AccentedLetters.IndexOf(character, StringComparison.Ordinal);

            if (index >= 0)
            {
                builder.Append(FoldedLetters[index]);
                continue;
            }

            // Ligatures and the sharp s expand to two letters, so they cannot live in the
            // parallel table above.
            switch (character)
            {
                case 'ß':
                    builder.Append("ss");
                    break;
                case 'Æ' or 'æ':
                    builder.Append("ae");
                    break;
                case 'Œ' or 'œ':
                    builder.Append("oe");
                    break;
                default:
                    builder.Append(character);
                    break;
            }
        }

        var folded = builder.ToString().ToLowerInvariant();
        var slug = NonSlugCharacters().Replace(folded, "-").Trim('-');

        return slug.Length > SlugMaxLength ? slug[..SlugMaxLength].Trim('-') : slug;
    }

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.None, matchTimeoutMilliseconds: 200)]
    private static partial Regex SlugPattern();

    [GeneratedRegex("[^a-z0-9]+", RegexOptions.None, matchTimeoutMilliseconds: 200)]
    private static partial Regex NonSlugCharacters();
}
