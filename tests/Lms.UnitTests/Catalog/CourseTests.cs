using Lms.Modules.Catalog;
using Lms.Modules.Catalog.Domain;
using Lms.SharedKernel.Identifiers;

namespace Lms.UnitTests.Catalog;

/// <summary>
/// Shared fixtures. A course is built through its own factory in every test — a mapper never
/// populates an entity, and nor does a test helper reaching for a constructor.
/// </summary>
public static class CourseFixture
{
    public static readonly DateTimeOffset Now = new(2026, 9, 28, 9, 0, 0, TimeSpan.Zero);

    public static Course Draft(string title = "Clean Architecture in .NET") =>
        Course.Create(UserId.New(), title, "A description.", CourseLevel.Intermediate, Now).Value;

    /// <summary>A course that satisfies every publish invariant.</summary>
    public static Course Publishable()
    {
        var course = Draft();
        course.SetThumbnail("course-assets/thumb.png", Now);

        var chapter = course.AddChapter("Getting started", Now).Value;
        var lesson = course.AddLesson(chapter.Id, "Why layers?", Now).Value;
        lesson.SetReadingContent("Some body text.", 120, Now);
        course.SetLessonContentChanged(Now);

        return course;
    }
}

public class CourseCreationTests
{
    [Fact]
    public void Creates_a_draft_with_a_slug_derived_from_the_title()
    {
        var course = CourseFixture.Draft();

        course.Status.ShouldBe(CourseStatus.Draft);
        course.Slug.ShouldBe("clean-architecture-in-net");
        course.PublishedAt.ShouldBeNull();
    }

    [Theory]
    [InlineData("Réactivité Avancée", "reactivite-avancee")]      // accents folded
    [InlineData("Größe und Straße", "grosse-und-strasse")]        // ß expands to ss
    [InlineData("Cœur de Métier", "coeur-de-metier")]             // ligature expands to oe
    [InlineData("C# 14 — What's New?", "c-14-what-s-new")]        // punctuation collapsed
    [InlineData("  Spaced   Out  ", "spaced-out")]                // runs collapse to one hyphen
    public void Slugifies_titles_into_url_safe_segments(string title, string expected) =>
        CourseFixture.Draft(title).Slug.ShouldBe(expected);

    [Fact]
    public void Folds_accents_even_though_the_solution_runs_invariant_globalization()
    {
        // Regression guard for a trap, not a feature. This project sets
        // InvariantGlobalization=true, which makes string.Normalize a *silent no-op* rather
        // than an error — so the textbook FormD-and-strip-combining-marks slugifier passes
        // every ASCII test and mangles the first accented title it meets.
        CourseFixture.Draft("Éléphant").Slug.ShouldBe("elephant");
    }

    [Fact]
    public void Refuses_a_title_that_slugifies_to_nothing()
    {
        // "???" has no alphanumerics at all. Inventing a fallback slug here would produce a
        // meaningless public URL; refusing lets the caller set one deliberately.
        var result = Course.Create(
            UserId.New(), "???", "A description.", CourseLevel.Beginner, CourseFixture.Now);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CatalogErrors.InvalidSlug);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Requires_a_title(string title) =>
        Course.Create(UserId.New(), title, "A description.", CourseLevel.Beginner, CourseFixture.Now)
            .Error.ShouldBe(CatalogErrors.TitleRequired);

    [Fact]
    public void Requires_a_title_within_the_column_width() =>
        Course.Create(
                UserId.New(),
                new string('x', Course.TitleMaxLength + 1),
                "A description.",
                CourseLevel.Beginner,
                CourseFixture.Now)
            .Error.ShouldBe(CatalogErrors.TitleRequired);

    [Fact]
    public void Requires_a_description() =>
        Course.Create(UserId.New(), "A title", "  ", CourseLevel.Beginner, CourseFixture.Now)
            .Error.ShouldBe(CatalogErrors.DescriptionRequired);

    [Fact]
    public void Requires_an_instructor() =>
        Course.Create(UserId.Empty, "A title", "A description.", CourseLevel.Beginner, CourseFixture.Now)
            .Error.ShouldBe(CatalogErrors.InstructorRequired);
}

public class CourseSlugTests
{
    [Fact]
    public void A_draft_slug_can_be_changed()
    {
        var course = CourseFixture.Draft();

        course.ChangeSlug("My-Custom-Slug", CourseFixture.Now).IsSuccess.ShouldBeTrue();
        course.Slug.ShouldBe("my-custom-slug");
    }

    [Fact]
    public void A_published_slug_is_frozen()
    {
        // The URL is somebody's bookmark by now — 02-domain-model.md §3.
        var course = CourseFixture.Publishable();
        course.Publish(CourseFixture.Now);

        var result = course.ChangeSlug("something-else", CourseFixture.Now);

        result.Error.ShouldBe(CatalogErrors.SlugFrozen);
        course.Slug.ShouldBe("clean-architecture-in-net");
    }

    [Theory]
    [InlineData("-leading")]
    [InlineData("trailing-")]
    [InlineData("double--hyphen")]
    [InlineData("has spaces")]
    [InlineData("has_underscore")]
    public void Rejects_a_malformed_slug(string slug) =>
        CourseFixture.Draft().ChangeSlug(slug, CourseFixture.Now)
            .Error.ShouldBe(CatalogErrors.InvalidSlug);

    [Fact]
    public void Retitling_does_not_move_the_url()
    {
        // Silently changing a URL because someone fixed a typo in the title is surprising even
        // on a draft, and it is exactly the kind of thing nobody notices until a link breaks.
        var course = CourseFixture.Draft();

        course.UpdateDetails("A Completely Different Title", null, "Still described.",
            CourseLevel.Advanced, CourseFixture.Now);

        course.Slug.ShouldBe("clean-architecture-in-net");
    }
}

public class CourseTagTests
{
    [Fact]
    public void Lowercases_and_deduplicates()
    {
        var course = CourseFixture.Draft();

        course.SetTags([" DotNet ", "dotnet", "EF-Core"], CourseFixture.Now).IsSuccess.ShouldBeTrue();

        course.Tags.ShouldBe(["dotnet", "ef-core"]);
    }

    [Fact]
    public void Drops_blank_tags() =>
        CourseFixture.Draft().Tap(c => c.SetTags(["dotnet", "  ", ""], CourseFixture.Now))
            .Tags.Count.ShouldBe(1);

    [Fact]
    public void Caps_the_tag_count()
    {
        var tooMany = Enumerable.Range(0, Course.MaxTags + 1).Select(i => $"tag{i}").ToArray();

        CourseFixture.Draft().SetTags(tooMany, CourseFixture.Now)
            .Error.ShouldBe(CatalogErrors.TooManyTags);
    }

    [Fact]
    public void Counts_the_cap_after_deduplication()
    {
        // Nine tags, but only eight distinct once case is normalised — that is within the cap.
        var course = CourseFixture.Draft();
        string[] tags = ["a", "b", "c", "d", "e", "f", "g", "h", "H"];

        course.SetTags(tags, CourseFixture.Now).IsSuccess.ShouldBeTrue();
        course.Tags.Count.ShouldBe(Course.MaxTags);
    }
}

public class CoursePublishTests
{
    [Fact]
    public void Publishes_when_every_invariant_holds()
    {
        var course = CourseFixture.Publishable();

        course.Publish(CourseFixture.Now).IsSuccess.ShouldBeTrue();

        course.Status.ShouldBe(CourseStatus.Published);
        course.PublishedAt.ShouldBe(CourseFixture.Now);
    }

    [Fact]
    public void Reports_every_violation_at_once_rather_than_the_first()
    {
        // The whole point of the list: telling an author about one missing thing at a time
        // turns publishing into a guessing game — 02-domain-model.md §3.2.
        var course = CourseFixture.Draft();

        var violations = course.PublishViolations();

        violations.ShouldContain(CatalogErrors.ThumbnailRequired);
        violations.ShouldContain(CatalogErrors.NoChapters);
        violations.Count.ShouldBe(2);
    }

    [Fact]
    public void Requires_a_thumbnail()
    {
        var course = CourseFixture.Publishable();
        course.SetThumbnail(null, CourseFixture.Now);

        course.PublishViolations().ShouldContain(CatalogErrors.ThumbnailRequired);
        course.Publish(CourseFixture.Now).IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void Requires_at_least_one_chapter() =>
        CourseFixture.Draft().Tap(c => c.SetThumbnail("t.png", CourseFixture.Now))
            .PublishViolations().ShouldContain(CatalogErrors.NoChapters);

    [Fact]
    public void Requires_every_chapter_to_hold_a_lesson()
    {
        var course = CourseFixture.Publishable();
        course.AddChapter("Empty on purpose", CourseFixture.Now);

        course.PublishViolations().ShouldContain(CatalogErrors.EmptyChapter("Empty on purpose"));
    }

    [Fact]
    public void Requires_every_lesson_to_have_content()
    {
        var course = CourseFixture.Publishable();
        var chapter = course.Chapters[0];
        course.AddLesson(chapter.Id, "Written later", CourseFixture.Now);

        course.PublishViolations().ShouldContain(CatalogErrors.LessonContentMissing("Written later"));
    }

    [Fact]
    public void Requires_at_least_one_required_lesson()
    {
        // Otherwise completion is vacuous — every student is instantly finished.
        var course = CourseFixture.Publishable();
        course.Chapters[0].Lessons[0].SetRequired(false, CourseFixture.Now);

        course.PublishViolations().ShouldContain(CatalogErrors.NoRequiredLesson);
    }

    [Fact]
    public void Does_not_complain_about_required_lessons_when_there_are_no_lessons()
    {
        // "No required lesson" on a course with no lessons is noise on top of the errors that
        // already say so.
        var course = CourseFixture.Draft();
        course.SetThumbnail("t.png", CourseFixture.Now);
        course.AddChapter("Empty", CourseFixture.Now);

        course.PublishViolations().ShouldNotContain(CatalogErrors.NoRequiredLesson);
    }

    [Fact]
    public void Republishing_keeps_the_original_publication_date()
    {
        // A brief unpublish to fix a typo must not send a year-old course back to the top of
        // whatever sorts on PublishedAt.
        var course = CourseFixture.Publishable();
        course.Publish(CourseFixture.Now);

        var later = CourseFixture.Now.AddMonths(6);
        course.Unpublish(later);
        course.Publish(later);

        course.PublishedAt.ShouldBe(CourseFixture.Now);
    }

    [Fact]
    public void Summarises_the_violation_count_in_the_failure()
    {
        var course = CourseFixture.Draft();

        course.Publish(CourseFixture.Now).Error
            .ShouldBe(CatalogErrors.PublishInvariantsUnmet(course.PublishViolations().Count));
    }
}

public class CourseLifecycleTests
{
    [Fact]
    public void Archives_a_published_course()
    {
        var course = CourseFixture.Publishable();
        course.Publish(CourseFixture.Now);

        course.Archive(CourseFixture.Now).IsSuccess.ShouldBeTrue();
        course.Status.ShouldBe(CourseStatus.Archived);
    }

    [Fact]
    public void Refuses_to_archive_a_draft() =>
        CourseFixture.Draft().Archive(CourseFixture.Now)
            .Error.ShouldBe(CatalogErrors.CannotArchiveDraft);

    [Fact]
    public void Restores_an_archived_course_to_published()
    {
        var course = CourseFixture.Publishable();
        course.Publish(CourseFixture.Now);
        course.Archive(CourseFixture.Now);

        course.Restore(CourseFixture.Now).IsSuccess.ShouldBeTrue();
        course.Status.ShouldBe(CourseStatus.Published);
    }

    [Fact]
    public void Refuses_to_restore_something_that_is_not_archived() =>
        CourseFixture.Draft().Restore(CourseFixture.Now)
            .Error.ShouldBe(CatalogErrors.CannotRestoreUnarchived);

    [Fact]
    public void Unpublishing_is_idempotent() =>
        CourseFixture.Draft().Unpublish(CourseFixture.Now).IsSuccess.ShouldBeTrue();

    [Fact]
    public void An_archived_course_cannot_be_edited()
    {
        var course = CourseFixture.Publishable();
        course.Publish(CourseFixture.Now);
        course.Archive(CourseFixture.Now);

        course.UpdateDetails("New", null, "New.", CourseLevel.Beginner, CourseFixture.Now)
            .Error.ShouldBe(CatalogErrors.CannotEditArchived);
        course.AddChapter("New", CourseFixture.Now).Error.ShouldBe(CatalogErrors.CannotEditArchived);
        course.SetTags(["x"], CourseFixture.Now).Error.ShouldBe(CatalogErrors.CannotEditArchived);
    }

    [Fact]
    public void An_archived_course_cannot_be_published_without_being_restored()
    {
        var course = CourseFixture.Publishable();
        course.Publish(CourseFixture.Now);
        course.Archive(CourseFixture.Now);

        course.Publish(CourseFixture.Now).Error.ShouldBe(CatalogErrors.CannotPublishArchived);
    }

    [Fact]
    public void Enrollment_count_never_goes_negative()
    {
        // The count is denormalised from another module's events, so it is eventually
        // consistent by design. A duplicate or out-of-order unenrol must not produce -1.
        var course = CourseFixture.Draft();

        course.AdjustEnrollmentCount(-1);

        course.EnrollmentCount.ShouldBe(0);
    }
}

/// <summary>Small helper so a fixture can be configured inline without a local variable.</summary>
internal static class TapExtensions
{
    public static T Tap<T>(this T value, Action<T> action)
    {
        action(value);
        return value;
    }
}
