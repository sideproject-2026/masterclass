namespace Lms.SharedKernel.Results;

/// <summary>
/// The four combinators, and deliberately no more (artifacts/design/09-code-conventions.md §3).
/// <list type="bullet">
///   <item><c>Map</c>    — transform the value.</item>
///   <item><c>Bind</c>   — chain another operation that can itself fail.</item>
///   <item><c>Tap</c>    — run a side effect on success, keep the value.</item>
///   <item><c>Ensure</c> — fail if a predicate does not hold.</item>
/// </list>
/// Resist growing this into a monad library.
/// </summary>
public static class ResultExtensions
{
    public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> map) =>
        result.IsSuccess ? Result<TOut>.Success(map(result.Value)) : Result<TOut>.Failure(result.Error);

    public static Result<TOut> Bind<TIn, TOut>(this Result<TIn> result, Func<TIn, Result<TOut>> bind) =>
        result.IsSuccess ? bind(result.Value) : Result<TOut>.Failure(result.Error);

    public static Result Bind<TIn>(this Result<TIn> result, Func<TIn, Result> bind) =>
        result.IsSuccess ? bind(result.Value) : Result.Failure(result.Error);

    public static Result<T> Tap<T>(this Result<T> result, Action<T> action)
    {
        if (result.IsSuccess)
        {
            action(result.Value);
        }

        return result;
    }

    public static Result<T> Ensure<T>(this Result<T> result, Func<T, bool> predicate, Error error) =>
        result.IsFailure || predicate(result.Value) ? result : Result<T>.Failure(error);

    // --- async variants -------------------------------------------------------
    // Only the shapes handlers actually need: awaiting a side effect (SaveChangesAsync)
    // and chaining an async operation. Add more when a second case appears.

    public static async Task<Result<T>> TapAsync<T>(
        this Result<T> result,
        Func<T, Task> action)
    {
        if (result.IsSuccess)
        {
            await action(result.Value);
        }

        return result;
    }

    public static async Task<Result> TapAsync(
        this Result result,
        Func<Task> action)
    {
        if (result.IsSuccess)
        {
            await action();
        }

        return result;
    }

    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<Result<TOut>>> bind) =>
        result.IsSuccess ? await bind(result.Value) : Result<TOut>.Failure(result.Error);

    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, TOut> map) =>
        (await resultTask).Map(map);
}
