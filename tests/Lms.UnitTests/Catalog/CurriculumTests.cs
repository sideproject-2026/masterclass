using Lms.Modules.Catalog;
using Lms.Modules.Catalog.Domain;
using Lms.SharedKernel.Identifiers;

namespace Lms.UnitTests.Catalog;

public class LessonContentTests
{
    private static Lesson NewLesson()
    {
        var course = CourseFixture.Draft();
        var chapter = course.AddChapter("Chapter", CourseFixture.Now).Value;
        return course.AddLesson(chapter.Id, "Lesson", CourseFixture.Now).Value;
    }

    [Fact]
    public void A_new_lesson_is_an_empty_reading()
    {
        // Deliberately creatable without content: an instructor makes the lesson, then writes
        // it. PublishViolations is what stops an empty one reaching students.
        var lesson = NewLesson();

        lesson.Type.ShouldBe(LessonType.Reading);
        lesson.HasCompleteContent().ShouldBeFalse();
        lesson.IsRequired.ShouldBeTrue();
        lesson.IsPreview.ShouldBeFalse();
    }

    [Fact]
    public void Setting_video_content_clears_the_reading_body()
    {
        // Switching type clears the other side — 02-domain-model.md §3.3. Leaving it would keep
        // a body nothing renders and that the publish check would have to reason about.
        var lesson = NewLesson();
        lesson.SetReadingContent("A reading body.", 60, CourseFixture.Now);

        lesson.SetVideoContent(VideoProvider.YouTube, "dQw4w9WgXcQ", 540, "Notes.", CourseFixture.Now);

        lesson.Type.ShouldBe(LessonType.Video);
        lesson.ContentMarkdown.ShouldBeNull();
        lesson.ExternalVideoId.ShouldBe("dQw4w9WgXcQ");
        lesson.DurationSeconds.ShouldBe(540);
        lesson.NotesMarkdown.ShouldBe("Notes.");
        lesson.HasCompleteContent().ShouldBeTrue();
    }

    [Fact]
    public void Setting_reading_content_clears_the_video_fields()
    {
        var lesson = NewLesson();
        lesson.SetVideoContent(VideoProvider.YouTube, "dQw4w9WgXcQ", 540, "Notes.", CourseFixture.Now);

        lesson.SetReadingContent("Now it is a reading.", 90, CourseFixture.Now);

        lesson.Type.ShouldBe(LessonType.Reading);
        lesson.VideoProvider.ShouldBeNull();
        lesson.ExternalVideoId.ShouldBeNull();
        lesson.NotesMarkdown.ShouldBeNull();
        lesson.HasCompleteContent().ShouldBeTrue();
    }

    [Theory]
    [InlineData("", 540)]
    [InlineData("   ", 540)]
    [InlineData("dQw4w9WgXcQ", 0)]
    [InlineData("dQw4w9WgXcQ", -1)]
    public void Rejects_incomplete_video_content(string videoId, int duration) =>
        NewLesson()
            .SetVideoContent(VideoProvider.YouTube, videoId, duration, null, CourseFixture.Now)
            .Error.ShouldBe(CatalogErrors.VideoContentIncomplete);

    [Fact]
    public void Rejects_empty_reading_content() =>
        NewLesson().SetReadingContent("  ", 60, CourseFixture.Now)
            .Error.ShouldBe(CatalogErrors.ReadingContentIncomplete);

    [Fact]
    public void Rejects_a_negative_reading_time() =>
        NewLesson().SetReadingContent("Body.", -1, CourseFixture.Now)
            .Error.ShouldBe(CatalogErrors.InvalidDuration);

    [Fact]
    public void Blank_notes_become_null_rather_than_whitespace()
    {
        var lesson = NewLesson();

        lesson.SetVideoContent(VideoProvider.YouTube, "dQw4w9WgXcQ", 540, "   ", CourseFixture.Now);

        lesson.NotesMarkdown.ShouldBeNull();
    }
}

public class OrderingTests
{
    [Fact]
    public void Chapters_are_numbered_densely_from_zero()
    {
        var course = CourseFixture.Draft();

        course.AddChapter("One", CourseFixture.Now);
        course.AddChapter("Two", CourseFixture.Now);
        course.AddChapter("Three", CourseFixture.Now);

        course.Chapters.Select(c => c.SortOrder).ShouldBe([0, 1, 2]);
    }

    [Fact]
    public void Removing_a_chapter_closes_the_gap()
    {
        // Dense means dense. A hole left behind would make the next reorder produce duplicates.
        var course = CourseFixture.Draft();
        course.AddChapter("One", CourseFixture.Now);
        var middle = course.AddChapter("Two", CourseFixture.Now).Value;
        course.AddChapter("Three", CourseFixture.Now);

        course.RemoveChapter(middle.Id, CourseFixture.Now).IsSuccess.ShouldBeTrue();

        course.Chapters.Select(c => c.SortOrder).ShouldBe([0, 1]);
        course.Chapters.Select(c => c.Title).ShouldBe(["One", "Three"]);
    }

    [Fact]
    public void Reordering_rewrites_every_row_from_the_complete_list()
    {
        var course = CourseFixture.Draft();
        var one = course.AddChapter("One", CourseFixture.Now).Value;
        var two = course.AddChapter("Two", CourseFixture.Now).Value;
        var three = course.AddChapter("Three", CourseFixture.Now).Value;

        course.ReorderChapters([three.Id, one.Id, two.Id], CourseFixture.Now).IsSuccess.ShouldBeTrue();

        course.Chapters.Select(c => c.Title).ShouldBe(["Three", "One", "Two"]);
        course.Chapters.Select(c => c.SortOrder).ShouldBe([0, 1, 2]);
    }

    [Fact]
    public void Refuses_a_partial_reorder()
    {
        // Applying half a reorder leaves gaps or duplicates that the next reorder inherits,
        // and the debugging is miserable — 02-domain-model.md §3.4.
        var course = CourseFixture.Draft();
        var one = course.AddChapter("One", CourseFixture.Now).Value;
        course.AddChapter("Two", CourseFixture.Now);

        course.ReorderChapters([one.Id], CourseFixture.Now)
            .Error.ShouldBe(CatalogErrors.ReorderMismatch);
    }

    [Fact]
    public void Refuses_a_reorder_naming_the_same_id_twice()
    {
        var course = CourseFixture.Draft();
        var one = course.AddChapter("One", CourseFixture.Now).Value;
        course.AddChapter("Two", CourseFixture.Now);

        course.ReorderChapters([one.Id, one.Id], CourseFixture.Now)
            .Error.ShouldBe(CatalogErrors.ReorderMismatch);
    }

    [Fact]
    public void Refuses_a_reorder_naming_an_unrelated_id()
    {
        var course = CourseFixture.Draft();
        course.AddChapter("One", CourseFixture.Now);

        course.ReorderChapters([ChapterId.New()], CourseFixture.Now)
            .Error.ShouldBe(CatalogErrors.ReorderMismatch);
    }

    [Fact]
    public void Lessons_reorder_within_their_chapter()
    {
        var course = CourseFixture.Draft();
        var chapter = course.AddChapter("Chapter", CourseFixture.Now).Value;
        var first = course.AddLesson(chapter.Id, "First", CourseFixture.Now).Value;
        var second = course.AddLesson(chapter.Id, "Second", CourseFixture.Now).Value;

        course.ReorderLessons(chapter.Id, [second.Id, first.Id], CourseFixture.Now)
            .IsSuccess.ShouldBeTrue();

        chapter.Lessons.Select(l => l.Title).ShouldBe(["Second", "First"]);
        chapter.Lessons.Select(l => l.SortOrder).ShouldBe([0, 1]);
    }

    [Fact]
    public void Moving_a_lesson_renumbers_both_chapters()
    {
        var course = CourseFixture.Draft();
        var source = course.AddChapter("Source", CourseFixture.Now).Value;
        var target = course.AddChapter("Target", CourseFixture.Now).Value;

        var stays = course.AddLesson(source.Id, "Stays", CourseFixture.Now).Value;
        var moves = course.AddLesson(source.Id, "Moves", CourseFixture.Now).Value;
        course.AddLesson(target.Id, "Already there", CourseFixture.Now);

        course.MoveLesson(moves.Id, target.Id, CourseFixture.Now).IsSuccess.ShouldBeTrue();

        source.Lessons.Select(l => l.Title).ShouldBe(["Stays"]);
        stays.SortOrder.ShouldBe(0);
        target.Lessons.Select(l => l.Title).ShouldBe(["Already there", "Moves"]);
        target.Lessons.Select(l => l.SortOrder).ShouldBe([0, 1]);
        moves.ChapterId.ShouldBe(target.Id);
    }

    [Fact]
    public void Moving_a_lesson_to_its_own_chapter_is_a_no_op()
    {
        var course = CourseFixture.Draft();
        var chapter = course.AddChapter("Chapter", CourseFixture.Now).Value;
        var lesson = course.AddLesson(chapter.Id, "Lesson", CourseFixture.Now).Value;

        course.MoveLesson(lesson.Id, chapter.Id, CourseFixture.Now).IsSuccess.ShouldBeTrue();

        chapter.Lessons.Count.ShouldBe(1);
    }

    [Fact]
    public void Adding_a_lesson_to_an_unknown_chapter_fails() =>
        CourseFixture.Draft().AddLesson(ChapterId.New(), "Lesson", CourseFixture.Now)
            .Error.ShouldBe(CatalogErrors.ChapterNotFound);
}

public class DerivedTotalsTests
{
    [Fact]
    public void Lesson_count_tracks_the_curriculum()
    {
        var course = CourseFixture.Draft();
        var chapter = course.AddChapter("Chapter", CourseFixture.Now).Value;

        course.AddLesson(chapter.Id, "One", CourseFixture.Now);
        course.AddLesson(chapter.Id, "Two", CourseFixture.Now);

        course.LessonCount.ShouldBe(2);
    }

    [Fact]
    public void Removing_a_lesson_updates_the_totals()
    {
        var course = CourseFixture.Draft();
        var chapter = course.AddChapter("Chapter", CourseFixture.Now).Value;
        var lesson = course.AddLesson(chapter.Id, "One", CourseFixture.Now).Value;
        lesson.SetReadingContent("Body.", 600, CourseFixture.Now);
        course.SetLessonContentChanged(CourseFixture.Now);

        course.RemoveLesson(lesson.Id, CourseFixture.Now).IsSuccess.ShouldBeTrue();

        course.LessonCount.ShouldBe(0);
        course.EstimatedMinutes.ShouldBe(0);
    }

    [Fact]
    public void Estimated_minutes_rounds_up()
    {
        // A 90-second lesson reading as "0 minutes" on a catalogue card is worse than "1 minute".
        var course = CourseFixture.Draft();
        var chapter = course.AddChapter("Chapter", CourseFixture.Now).Value;
        var lesson = course.AddLesson(chapter.Id, "Short", CourseFixture.Now).Value;

        lesson.SetReadingContent("Body.", 90, CourseFixture.Now);
        course.SetLessonContentChanged(CourseFixture.Now);

        course.EstimatedMinutes.ShouldBe(2);
    }

    [Fact]
    public void Estimated_minutes_sums_across_chapters()
    {
        var course = CourseFixture.Draft();
        var one = course.AddChapter("One", CourseFixture.Now).Value;
        var two = course.AddChapter("Two", CourseFixture.Now).Value;

        course.AddLesson(one.Id, "A", CourseFixture.Now).Value
            .SetVideoContent(VideoProvider.YouTube, "aaaaaaaaaaa", 600, null, CourseFixture.Now);
        course.AddLesson(two.Id, "B", CourseFixture.Now).Value
            .SetVideoContent(VideoProvider.YouTube, "bbbbbbbbbbb", 600, null, CourseFixture.Now);
        course.SetLessonContentChanged(CourseFixture.Now);

        course.EstimatedMinutes.ShouldBe(20);
        course.LessonCount.ShouldBe(2);
    }

    [Fact]
    public void Find_lesson_reaches_across_chapters()
    {
        var course = CourseFixture.Draft();
        course.AddChapter("One", CourseFixture.Now);
        var two = course.AddChapter("Two", CourseFixture.Now).Value;
        var lesson = course.AddLesson(two.Id, "Buried", CourseFixture.Now).Value;

        course.FindLesson(lesson.Id).ShouldBe(lesson);
        course.FindLesson(LessonId.New()).ShouldBeNull();
    }
}
