namespace Lms.Modules.Catalog.Domain;

/// <summary>
/// Stored as <c>int</c>, not string — see artifacts/design/02-domain-model.md §3.
/// Never reorder or renumber these; the numbers are in the database.
/// </summary>
public enum CourseLevel
{
    Beginner = 0,
    Intermediate = 1,
    Advanced = 2,
}

/// <summary>Lifecycle per 02-domain-model.md §3.1.</summary>
public enum CourseStatus
{
    Draft = 0,
    Published = 1,
    Archived = 2,
}

/// <summary>
/// Determines which content fields must be populated — the invariant in 02 §3.3.
/// </summary>
public enum LessonType
{
    Video = 0,
    Reading = 1,
}

/// <summary>
/// The swap point for artifacts/design/05-adr-video-and-storage.md. One member in MVP;
/// the enum exists so that adding a provider is a migration of nothing.
/// </summary>
public enum VideoProvider
{
    YouTube = 0,
}
