using System.Text.Json;
using Lms.SharedKernel.Identifiers;

namespace Lms.UnitTests.Identifiers;

public class StronglyTypedIdTests
{
    [Fact]
    public void New_produces_a_version_7_uuid()
    {
        var id = CourseId.New();

        id.Value.ShouldNotBe(Guid.Empty);
        id.Value.Version.ShouldBe(7, "keys must be time-ordered for index locality");
    }

    [Fact]
    public void New_ids_are_time_ordered()
    {
        var first = CourseId.New();
        Thread.Sleep(2);
        var second = CourseId.New();

        // UUIDv7 sorts lexicographically by creation time.
        string.CompareOrdinal(first.ToString(), second.ToString()).ShouldBeLessThan(0);
    }

    [Fact]
    public void Ids_of_different_types_are_not_interchangeable()
    {
        // The whole point: this is a compile-time guarantee, so the test documents it
        // rather than proving it. Distinct types cannot be assigned to one another.
        typeof(CourseId).ShouldNotBe(typeof(LessonId));
    }

    [Fact]
    public void Equality_is_by_value()
    {
        var raw = Guid.CreateVersion7();

        new CourseId(raw).ShouldBe(new CourseId(raw));
        new CourseId(raw).ShouldNotBe(CourseId.New());
    }

    [Fact]
    public void Serialises_as_a_bare_string_identical_to_a_raw_guid()
    {
        var raw = Guid.CreateVersion7();

        var json = JsonSerializer.Serialize(new CourseId(raw));

        json.ShouldBe($"\"{raw}\"");
    }

    [Fact]
    public void Round_trips_through_json()
    {
        var original = LessonId.New();

        var restored = JsonSerializer.Deserialize<LessonId>(JsonSerializer.Serialize(original));

        restored.ShouldBe(original);
    }

    [Fact]
    public void Serialises_inside_a_containing_object()
    {
        var id = EnrollmentId.New();

        var json = JsonSerializer.Serialize(new { enrollmentId = id });

        json.ShouldBe($$"""{"enrollmentId":"{{id.Value}}"}""");
    }

    [Fact]
    public void Empty_is_the_default_value()
    {
        UserId.Empty.Value.ShouldBe(Guid.Empty);
        default(UserId).ShouldBe(UserId.Empty);
    }
}
