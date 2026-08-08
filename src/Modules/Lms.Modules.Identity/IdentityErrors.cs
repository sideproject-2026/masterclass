using Lms.SharedKernel.Results;

namespace Lms.Modules.Identity;

/// <summary>
/// The module's error catalogue. Never an inline error string at a call site —
/// artifacts/design/09-code-conventions.md §3.
/// </summary>
public static class IdentityErrors
{
    /// <summary>
    /// The single failure for every bad-login case.
    /// </summary>
    /// <remarks>
    /// Unknown email, wrong password, locked out and unconfirmed all return <b>this exact
    /// error</b>. Distinguishing them turns login into a user-enumeration oracle: an attacker
    /// learns which addresses are registered by reading the response. One error, one message,
    /// one status — see artifacts/design/04-adr-authentication.md §7.
    /// </remarks>
    public static Error InvalidCredentials { get; } =
        Error.Unauthenticated("auth.invalid_credentials", "Email or password is incorrect.");

    public static Error EmailAlreadyRegistered { get; } =
        Error.Conflict("auth.email_taken", "That email address is already registered.");

    public static Error RegistrationFailed(string detail) =>
        Error.Validation("auth.registration_failed", detail);

    public static Error InvalidRefreshToken { get; } =
        Error.Unauthenticated("auth.invalid_refresh_token", "The refresh token is invalid or has expired.");

    public static Error UserNotFound { get; } =
        Error.NotFound("auth.user_not_found", "No such user.");
}
