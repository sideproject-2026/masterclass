using System.Diagnostics.CodeAnalysis;

namespace Lms.SharedKernel.Results;

/// <summary>
/// Success, or an <see cref="Error"/>. Expected failures are values; exceptions are
/// reserved for programmer errors and infrastructure faults.
/// See artifacts/design/09-code-conventions.md §3.
/// </summary>
public readonly struct Result
{
    private Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) => Result<TValue>.Success(value);
    public static Result<TValue> Failure<TValue>(Error error) => Result<TValue>.Failure(error);

    public static implicit operator Result(Error error) => Failure(error);

    /// <summary>Returns the first failure, or success if all succeeded.</summary>
    public static Result FirstFailureOr(params ReadOnlySpan<Result> results)
    {
        foreach (var result in results)
        {
            if (result.IsFailure)
            {
                return result;
            }
        }

        return Success();
    }
}

/// <summary>A value on success, or an <see cref="Error"/>.</summary>
public readonly struct Result<TValue>
{
    private readonly TValue? _value;

    private Result(bool isSuccess, TValue? value, Error error)
    {
        IsSuccess = isSuccess;
        _value = value;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    /// <summary>The value. Throws if the result is a failure — check <see cref="IsSuccess"/> first.</summary>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException($"Cannot read the value of a failed result ({Error}).");

    public static Result<TValue> Success(TValue value) => new(true, value, Error.None);
    public static Result<TValue> Failure(Error error) => new(false, default, error);

    public static implicit operator Result<TValue>(TValue value) => Success(value);
    public static implicit operator Result<TValue>(Error error) => Failure(error);

    /// <summary>Widens to a non-generic <see cref="Result"/>, discarding the value.</summary>
    public Result AsResult() => IsSuccess ? Result.Success() : Result.Failure(Error);

    public bool TryGetValue([NotNullWhen(true)] out TValue? value)
    {
        value = _value;
        return IsSuccess;
    }

    /// <summary>Collapses both branches into a single value.</summary>
    public TOut Match<TOut>(Func<TValue, TOut> onSuccess, Func<Error, TOut> onFailure) =>
        IsSuccess ? onSuccess(_value!) : onFailure(Error);
}
