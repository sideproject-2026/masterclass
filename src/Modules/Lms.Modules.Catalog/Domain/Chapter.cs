using Lms.SharedKernel.Identifiers;
using Lms.SharedKernel.Results;

namespace Lms.Modules.Catalog.Domain;

/// <summary>
/// A grouping of lessons. Part of the <see cref="Course"/> aggregate.
/// </summary>
/// <remarks>
/// No description field, deliberately — 02-domain-model.md §3 calls a chapter "grouping, not
/// content". Add one when a screen actually needs it.
/// </remarks>
public sealed class Chapter
{
    public const int TitleMaxLength = 160;

    private readonly List<Lesson> _lessons = [];

    // EF materialisation.
    private Chapter() => Title = null!;

    private Chapter(CourseId courseId, string title, int sortOrder)
    {
        Id = ChapterId.New();
        CourseId = courseId;
        Title = title;
        SortOrder = sortOrder;
    }

    public ChapterId Id { get; private set; }

    public CourseId CourseId { get; private set; }

    public string Title { get; private set; }

    /// <summary>Dense and 0-based within the course.</summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// Exposed read-only so the only way to add a lesson is <see cref="AddLesson"/>, which
    /// keeps <see cref="Lesson.SortOrder"/> dense.
    /// </summary>
    public IReadOnlyList<Lesson> Lessons => _lessons;

    internal static Result<Chapter> Create(CourseId courseId, string title, int sortOrder)
    {
        var trimmed = (title ?? string.Empty).Trim();

        if (trimmed.Length is 0 or > TitleMaxLength)
        {
            return CatalogErrors.TitleRequired;
        }

        return new Chapter(courseId, trimmed, sortOrder);
    }

    internal Result Rename(string title)
    {
        var trimmed = (title ?? string.Empty).Trim();

        if (trimmed.Length is 0 or > TitleMaxLength)
        {
            return CatalogErrors.TitleRequired;
        }

        Title = trimmed;
        return Result.Success();
    }

    internal Result<Lesson> AddLesson(string title, DateTimeOffset now)
    {
        var lesson = Lesson.Create(Id, title, _lessons.Count, now);

        if (lesson.IsSuccess)
        {
            _lessons.Add(lesson.Value);
        }

        return lesson;
    }

    internal bool RemoveLesson(LessonId lessonId)
    {
        var lesson = _lessons.SingleOrDefault(l => l.Id == lessonId);

        if (lesson is null)
        {
            return false;
        }

        _lessons.Remove(lesson);
        Renumber();
        return true;
    }

    /// <summary>
    /// Rewrites every lesson's order from the complete ordered list of ids.
    /// </summary>
    /// <remarks>
    /// The whole list, never per-item deltas — 02-domain-model.md §3.4. A mismatched list is
    /// rejected outright rather than partially applied: applying half a reorder leaves gaps or
    /// duplicates that the next reorder inherits.
    /// </remarks>
    internal Result ReorderLessons(IReadOnlyList<LessonId> orderedIds)
    {
        if (orderedIds.Count != _lessons.Count || orderedIds.Distinct().Count() != orderedIds.Count)
        {
            return CatalogErrors.ReorderMismatch;
        }

        var byId = _lessons.ToDictionary(l => l.Id);

        if (!orderedIds.All(byId.ContainsKey))
        {
            return CatalogErrors.ReorderMismatch;
        }

        for (var index = 0; index < orderedIds.Count; index++)
        {
            byId[orderedIds[index]].SetSortOrder(index);
        }

        Sort();
        return Result.Success();
    }

    internal void SetSortOrder(int sortOrder) => SortOrder = sortOrder;

    internal Lesson? FindLesson(LessonId lessonId) => _lessons.SingleOrDefault(l => l.Id == lessonId);

    internal void AttachLesson(Lesson lesson)
    {
        lesson.MoveToChapter(Id, _lessons.Count, lesson.UpdatedAt);
        _lessons.Add(lesson);
    }

    internal void DetachLesson(Lesson lesson)
    {
        _lessons.Remove(lesson);
        Renumber();
    }

    private void Renumber()
    {
        Sort();

        for (var index = 0; index < _lessons.Count; index++)
        {
            _lessons[index].SetSortOrder(index);
        }
    }

    private void Sort() => _lessons.Sort((left, right) => left.SortOrder.CompareTo(right.SortOrder));
}
