using System.Diagnostics;
using Lms.SharedKernel.Pagination;
using Lms.SharedKernel.Results;
using Microsoft.AspNetCore.Http;

namespace Lms.SharedKernel.Http;

/// <summary>
/// Turns a <see cref="Result"/> into an HTTP response.
/// </summary>
/// <remarks>
/// <b>This is the only place <see cref="ErrorType"/> maps to a status code.</b> If you find a
/// second mapping, delete it. Status codes are specified in
/// artifacts/design/03-api-design.md §1.2.
/// </remarks>
public static class HttpResults
{
    /// <summary>Maps a failure classification to its HTTP status. The single source of truth.</summary>
    public static int ToStatusCode(this ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthenticated => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Invariant => StatusCodes.Status422UnprocessableEntity,
        _ => StatusCodes.Status500InternalServerError
    };

    /// <summary>200 with the value, or a ProblemDetails carrying the error.</summary>
    public static IResult ToHttpResult<T>(this Result<T> result) =>
        result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : Problem(result.Error);

    /// <summary>Projects the value on success — used to convert a QueryResult into a PagedResult.</summary>
    public static IResult ToHttpResult<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> project)
    {
        ArgumentNullException.ThrowIfNull(project);

        return result.IsSuccess
            ? TypedResults.Ok(project(result.Value))
            : Problem(result.Error);
    }

    /// <summary>204 on success, ProblemDetails otherwise.</summary>
    public static IResult ToHttpResult(this Result result) =>
        result.IsSuccess
            ? TypedResults.NoContent()
            : Problem(result.Error);

    /// <summary>
    /// 204 on success — <see cref="Unit"/> means "nothing to return".
    /// </summary>
    /// <remarks>
    /// Overload resolution picks this over the generic <c>ToHttpResult&lt;T&gt;</c>, which would
    /// otherwise serialise <see cref="Unit"/> and answer <c>200 {}</c>. Every command that
    /// returns nothing goes through here, so getting it wrong would be wrong everywhere.
    /// </remarks>
    public static IResult ToHttpResult(this Result<Unit> result) =>
        result.IsSuccess
            ? TypedResults.NoContent()
            : Problem(result.Error);

    /// <summary>201 with a Location header, ProblemDetails otherwise.</summary>
    public static IResult ToCreatedResult<T>(this Result<T> result, Func<T, string> location)
    {
        ArgumentNullException.ThrowIfNull(location);

        return result.IsSuccess
            ? TypedResults.Created(location(result.Value), result.Value)
            : Problem(result.Error);
    }

    /// <summary>Converts a paged query result straight to the wire envelope.</summary>
    public static IResult ToPagedHttpResult<T>(this Result<QueryResult<T>> result, PageRequest page) =>
        result.ToHttpResult(queryResult => PagedResult<T>.From(queryResult, page));

    /// <summary>RFC 9457 problem response. Every 4xx in this API goes through here.</summary>
    public static IResult Problem(Error error)
    {
        var status = error.Type.ToStatusCode();

        return TypedResults.Problem(
            title: TitleFor(error.Type),
            detail: error.Message,
            statusCode: status,
            type: $"https://lms.example.com/errors/{error.Code}",
            extensions: new Dictionary<string, object?>
            {
                ["errorCode"] = error.Code,
                ["traceId"] = Activity.Current?.Id
            });
    }

    private static string TitleFor(ErrorType type) => type switch
    {
        ErrorType.Validation => "The request was not valid.",
        ErrorType.Unauthenticated => "Authentication is required.",
        ErrorType.Forbidden => "You are not permitted to perform this action.",
        ErrorType.NotFound => "The requested resource was not found.",
        ErrorType.Conflict => "The request conflicts with the current state.",
        ErrorType.Invariant => "The request violates a rule of the domain.",
        _ => "An unexpected error occurred."
    };
}
