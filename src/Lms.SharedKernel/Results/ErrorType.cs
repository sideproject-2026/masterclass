namespace Lms.SharedKernel.Results;

/// <summary>
/// Classifies a failure. Maps to an HTTP status code in exactly one place
/// (the endpoint result extension) — see artifacts/design/03-api-design.md §1.2.
/// </summary>
public enum ErrorType
{
    /// <summary>Malformed request or a validator rejected the shape. → 400</summary>
    Validation = 0,

    /// <summary>Authenticated but not permitted: wrong role, not the owner, not enrolled. → 403</summary>
    Forbidden = 1,

    /// <summary>
    /// No credentials, or credentials that are not valid. → 401
    /// </summary>
    /// <remarks>
    /// Distinct from <see cref="Forbidden"/>: 401 means "we do not know who you are",
    /// 403 means "we know, and you may not". A failed login is 401, not 403.
    /// </remarks>
    Unauthenticated = 5,

    /// <summary>Does not exist, or must not be revealed to exist. → 404</summary>
    NotFound = 2,

    /// <summary>State conflict, including optimistic concurrency. → 409</summary>
    Conflict = 3,

    /// <summary>Well-formed but violates a domain invariant. → 422</summary>
    Invariant = 4
}
