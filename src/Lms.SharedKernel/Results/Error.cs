namespace Lms.SharedKernel.Results;

/// <summary>
/// An expected failure, returned as a value rather than thrown.
/// Build these from a per-module error catalogue (e.g. <c>CatalogErrors.CourseNotFound</c>),
/// never inline at a call site — see artifacts/design/09-code-conventions.md §3.
/// </summary>
/// <param name="Code">Stable machine-readable identifier, e.g. <c>course.not_found</c>.</param>
/// <param name="Message">Human-readable description. Safe to return to a caller.</param>
/// <param name="Type">Classification that determines the HTTP status.</param>
public readonly record struct Error(string Code, string Message, ErrorType Type)
{
    /// <summary>The absence of an error. Never returned to a caller.</summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Validation);

    public bool IsNone => string.IsNullOrEmpty(Code);

    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);
    public static Error Forbidden(string code, string message) => new(code, message, ErrorType.Forbidden);
    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);
    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);
    public static Error Invariant(string code, string message) => new(code, message, ErrorType.Invariant);

    public override string ToString() => $"{Type}:{Code}";
}
