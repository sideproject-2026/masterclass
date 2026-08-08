using Microsoft.AspNetCore.Identity;

namespace Lms.Modules.Identity.Domain;

/// <summary>
/// One of exactly three roles — <c>Student</c>, <c>Instructor</c>, <c>Admin</c>.
/// The names live in <c>Lms.SharedKernel.Authorization.Roles</c>; do not redeclare them here.
/// </summary>
/// <remarks>
/// Deliberately flat: no per-course grants, no organisations, no teams
/// (artifacts/design/00-overview.md §6). Course ownership is a column on the course, not a role.
/// </remarks>
public sealed class AppRole : IdentityRole<Guid>
{
    private AppRole() { }

    private AppRole(Guid id, string name)
    {
        Id = id;
        Name = name;
        NormalizedName = name.ToUpperInvariant();
    }

    public static AppRole Create(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new AppRole(Guid.CreateVersion7(), name);
    }
}
