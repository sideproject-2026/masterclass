namespace Lms.SharedKernel.Authorization;

/// <summary>
/// Policy names, in one place. Grep for a role string and you should find exactly this file —
/// that is what makes a future identity-provider swap cheap (artifacts/design/04-adr-authentication.md §5).
/// </summary>
/// <remarks>
/// A role check is necessary but never sufficient. Resource ownership (a course belongs to the
/// caller) and the enrolment gate are guard clauses inside handlers, not policies.
/// </remarks>
public static class AuthPolicies
{
    public const string Student = nameof(Student);
    public const string Instructor = nameof(Instructor);
    public const string Admin = nameof(Admin);
}

/// <summary>
/// Rate-limiting policy names. Here rather than in the host because modules attach them to
/// their own route groups, and a module must never reference <c>Lms.Api</c>.
/// </summary>
public static class RateLimitPolicies
{
    /// <summary>Login, register and refresh. See artifacts/design/03-api-design.md §8.</summary>
    public const string Auth = "auth";
}

/// <summary>Role names as persisted by ASP.NET Core Identity. Exactly three.</summary>
public static class Roles
{
    public const string Student = nameof(Student);
    public const string Instructor = nameof(Instructor);
    public const string Admin = nameof(Admin);

    public static IReadOnlyList<string> All { get; } = [Student, Instructor, Admin];
}
