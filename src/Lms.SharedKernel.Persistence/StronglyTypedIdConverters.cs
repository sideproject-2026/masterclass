using Lms.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Lms.SharedKernel.Persistence;

/// <summary>
/// Teaches EF Core to store a typed identifier as a plain <see cref="Guid"/>.
/// </summary>
/// <remarks>
/// Written once here rather than a converter per type — this boilerplate is the whole cost
/// of typed identifiers, and paying it in one file is what makes them worth having
/// (artifacts/design/09-code-conventions.md §4).
/// </remarks>
public sealed class StronglyTypedIdConverter<TId>()
    : ValueConverter<TId, Guid>(id => id.Value, value => Factory(value))
    where TId : struct, IStronglyTypedId
{
    private static readonly Func<Guid, TId> Factory = BuildFactory();

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

public static class ModelConfigurationBuilderExtensions
{
    /// <summary>
    /// Registers every typed identifier so no entity configuration has to mention a converter.
    /// Call from <c>ConfigureConventions</c> in each module's DbContext.
    /// </summary>
    public static ModelConfigurationBuilder ApplyStronglyTypedIdConventions(
        this ModelConfigurationBuilder configurationBuilder)
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder);

        configurationBuilder.Properties<UserId>().HaveConversion<StronglyTypedIdConverter<UserId>>();
        configurationBuilder.Properties<CourseId>().HaveConversion<StronglyTypedIdConverter<CourseId>>();
        configurationBuilder.Properties<ChapterId>().HaveConversion<StronglyTypedIdConverter<ChapterId>>();
        configurationBuilder.Properties<LessonId>().HaveConversion<StronglyTypedIdConverter<LessonId>>();
        configurationBuilder.Properties<EnrollmentId>().HaveConversion<StronglyTypedIdConverter<EnrollmentId>>();

        return configurationBuilder;
    }
}
