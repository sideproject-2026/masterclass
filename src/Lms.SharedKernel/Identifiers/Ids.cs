using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lms.SharedKernel.Identifiers;

/// <summary>
/// Marker for a Guid-backed identifier, so generic converter machinery can find them.
/// </summary>
public interface IStronglyTypedId
{
    Guid Value { get; }
}

/// <summary>
/// Typed identifiers. <c>EnrollStudent(courseId, studentId)</c> compiles just as happily
/// with the arguments swapped when both are <see cref="Guid"/> — these close that bug class.
/// </summary>
/// <remarks>
/// Keys are UUIDv7 generated in application code: time-ordered, so index locality is good,
/// and no database round trip to obtain one. See artifacts/design/02-domain-model.md §8.2.
/// These live in SharedKernel rather than a module's Contracts because they are shared
/// vocabulary — <c>CourseId</c> is written by Catalog and referenced by Enrollment.
/// </remarks>
[JsonConverter(typeof(StronglyTypedIdJsonConverter<UserId>))]
public readonly record struct UserId(Guid Value) : IStronglyTypedId
{
    public static UserId New() => new(Guid.CreateVersion7());
    public static UserId Empty { get; }
    public override string ToString() => Value.ToString();
}

[JsonConverter(typeof(StronglyTypedIdJsonConverter<CourseId>))]
public readonly record struct CourseId(Guid Value) : IStronglyTypedId
{
    public static CourseId New() => new(Guid.CreateVersion7());
    public static CourseId Empty { get; }
    public override string ToString() => Value.ToString();
}

[JsonConverter(typeof(StronglyTypedIdJsonConverter<ChapterId>))]
public readonly record struct ChapterId(Guid Value) : IStronglyTypedId
{
    public static ChapterId New() => new(Guid.CreateVersion7());
    public static ChapterId Empty { get; }
    public override string ToString() => Value.ToString();
}

[JsonConverter(typeof(StronglyTypedIdJsonConverter<LessonId>))]
public readonly record struct LessonId(Guid Value) : IStronglyTypedId
{
    public static LessonId New() => new(Guid.CreateVersion7());
    public static LessonId Empty { get; }
    public override string ToString() => Value.ToString();
}

[JsonConverter(typeof(StronglyTypedIdJsonConverter<EnrollmentId>))]
public readonly record struct EnrollmentId(Guid Value) : IStronglyTypedId
{
    public static EnrollmentId New() => new(Guid.CreateVersion7());
    public static EnrollmentId Empty { get; }
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Serialises a typed id as a bare JSON string, so the wire format is identical to a raw Guid.
/// Written once here rather than per type — the boilerplate that makes typed ids worth having.
/// </summary>
public sealed class StronglyTypedIdJsonConverter<TId> : JsonConverter<TId>
    where TId : struct, IStronglyTypedId
{
    private static readonly Func<Guid, TId> Create = BuildFactory();

    public override TId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var raw = reader.GetString();
        return raw is null ? default : Create(Guid.Parse(raw));
    }

    public override void Write(Utf8JsonWriter writer, TId value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(value.Value);
    }

    private static Func<Guid, TId> BuildFactory()
    {
        var ctor = typeof(TId).GetConstructor([typeof(Guid)])
            ?? throw new InvalidOperationException(
                $"{typeof(TId).Name} must declare a constructor taking a single Guid.");

        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(Guid), "value");
        var lambda = System.Linq.Expressions.Expression.Lambda<Func<Guid, TId>>(
            System.Linq.Expressions.Expression.New(ctor, parameter), parameter);

        return lambda.Compile();
    }
}
