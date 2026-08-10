using Lms.SharedKernel.Results;

namespace Lms.Modules.Catalog;

/// <summary>
/// The module's error catalogue. Never an inline error string at a call site —
/// artifacts/design/09-code-conventions.md §3.
/// </summary>
public static class CatalogErrors
{
    public static Error CourseNotFound { get; } =
        Error.NotFound("course.not_found", "No such course.");

    public static Error ChapterNotFound { get; } =
        Error.NotFound("chapter.not_found", "No such chapter.");

    public static Error LessonNotFound { get; } =
        Error.NotFound("lesson.not_found", "No such lesson.");

    public static Error TitleRequired { get; } =
        Error.Validation("course.title_required", "A title is required and must be 160 characters or fewer.");

    public static Error DescriptionRequired { get; } =
        Error.Validation("course.description_required", "A description is required and must be 8000 characters or fewer.");

    public static Error SubtitleTooLong { get; } =
        Error.Validation("course.subtitle_too_long", "A subtitle must be 300 characters or fewer.");

    public static Error InstructorRequired { get; } =
        Error.Validation("course.instructor_required", "A course must belong to an instructor.");

    public static Error InvalidSlug { get; } =
        Error.Validation(
            "course.invalid_slug",
            "A slug must be lowercase letters, digits and single hyphens — for example 'clean-architecture'.");

    /// <summary>
    /// The slug is frozen once published, because it is in URLs people have shared.
    /// </summary>
    public static Error SlugFrozen { get; } =
        Error.Invariant(
            "course.slug_frozen",
            "A published course keeps its slug. Unpublish it first if the URL really must change.");

    public static Error TooManyTags { get; } =
        Error.Validation("course.too_many_tags", $"A course may carry at most {Domain.Course.MaxTags} tags.");

    public static Error ThumbnailRequired { get; } =
        Error.Invariant("course.thumbnail_required", "A course needs a thumbnail before it can be published.");

    public static Error NoChapters { get; } =
        Error.Invariant("course.no_chapters", "A course needs at least one chapter before it can be published.");

    public static Error EmptyChapter(string chapterTitle) =>
        Error.Invariant("course.empty_chapter", $"Chapter '{chapterTitle}' has no lessons.");

    public static Error NoRequiredLesson { get; } =
        Error.Invariant(
            "course.no_required_lesson",
            "At least one lesson must be required, otherwise completion means nothing.");

    /// <summary>
    /// The summary failure. The individual violations come from
    /// <see cref="Domain.Course.PublishViolations"/> — see the remarks there.
    /// </summary>
    public static Error PublishInvariantsUnmet(int count) =>
        Error.Invariant(
            "course.not_ready_to_publish",
            count == 1
                ? "The course is not ready to publish: 1 problem must be fixed first."
                : $"The course is not ready to publish: {count} problems must be fixed first.");

    public static Error CannotPublishArchived { get; } =
        Error.Invariant("course.archived", "An archived course must be restored before it can be published.");

    public static Error CannotEditArchived { get; } =
        Error.Invariant("course.archived_readonly", "An archived course cannot be edited.");

    public static Error CannotArchiveDraft { get; } =
        Error.Invariant("course.not_published", "Only a published course can be archived.");

    public static Error CannotRestoreUnarchived { get; } =
        Error.Invariant("course.not_archived", "Only an archived course can be restored.");

    public static Error VideoContentIncomplete { get; } =
        Error.Validation(
            "lesson.video_incomplete",
            "A video lesson needs a provider, an external video id, and a positive duration.");

    public static Error ReadingContentIncomplete { get; } =
        Error.Validation("lesson.reading_incomplete", "A reading lesson needs content.");

    public static Error LessonContentMissing(string lessonTitle) =>
        Error.Invariant("lesson.content_missing", $"Lesson '{lessonTitle}' has no content.");

    public static Error InvalidDuration { get; } =
        Error.Validation("lesson.invalid_duration", "A duration must be zero or more seconds.");

    public static Error ReorderMismatch { get; } =
        Error.Validation(
            "catalog.reorder_mismatch",
            "A reorder must list every child exactly once — see 02-domain-model.md §3.4.");
}
