using Lms.SharedKernel.Identifiers;
using Lms.SharedKernel.Persistence;

namespace Lms.UnitTests.Persistence;

/// <summary>
/// The converters are what let typed ids reach the database as plain <c>uuid</c> columns.
/// Written once generically (artifacts/design/09-code-conventions.md §4), so they are worth
/// pinning once generically too.
/// </summary>
public class StronglyTypedIdConverterTests
{
    [Fact]
    public void CourseId_converts_to_its_underlying_guid()
    {
        var converter = new StronglyTypedIdConverter<CourseId>();
        var raw = Guid.CreateVersion7();

        converter.ConvertToProvider(new CourseId(raw)).ShouldBe(raw);
    }

    [Fact]
    public void CourseId_converts_back_from_a_guid()
    {
        var converter = new StronglyTypedIdConverter<CourseId>();
        var raw = Guid.CreateVersion7();

        converter.ConvertFromProvider(raw).ShouldBe(new CourseId(raw));
    }

    [Fact]
    public void Every_id_type_round_trips()
    {
        AssertRoundTrip<UserId>();
        AssertRoundTrip<CourseId>();
        AssertRoundTrip<ChapterId>();
        AssertRoundTrip<LessonId>();
        AssertRoundTrip<EnrollmentId>();
    }

    [Fact]
    public void The_default_id_round_trips_as_an_empty_guid()
    {
        var converter = new StronglyTypedIdConverter<EnrollmentId>();

        converter.ConvertToProvider(default(EnrollmentId)).ShouldBe(Guid.Empty);
        converter.ConvertFromProvider(Guid.Empty).ShouldBe(default(EnrollmentId));
    }

    private static void AssertRoundTrip<TId>() where TId : struct, IStronglyTypedId
    {
        var converter = new StronglyTypedIdConverter<TId>();
        var raw = Guid.CreateVersion7();
        var typed = (TId)Activator.CreateInstance(typeof(TId), raw)!;

        converter.ConvertToProvider(typed)
            .ShouldBe(raw, $"{typeof(TId).Name} must store as its underlying Guid");

        ((TId)converter.ConvertFromProvider(raw)!).Value
            .ShouldBe(raw, $"{typeof(TId).Name} must restore from its underlying Guid");
    }
}
