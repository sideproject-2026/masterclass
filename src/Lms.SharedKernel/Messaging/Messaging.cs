using Lms.SharedKernel.Results;

namespace Lms.SharedKernel.Messaging;

/// <summary>A write operation. Returns <see cref="Unit"/> when there is nothing to return.</summary>
/// <remarks>
/// Commands and queries are separate interfaces for one concrete reason: the transaction
/// decorator wraps <see cref="ICommandHandler{TCommand,TResponse}"/> and must not wrap queries.
/// A write registered as a query silently loses its transaction.
/// See artifacts/design/09-code-conventions.md §2.
/// </remarks>
public interface ICommand<TResponse>;

/// <summary>A read operation. Never mutates state.</summary>
public interface IQuery<TResponse>;

public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<Result<TResponse>> HandleAsync(TCommand command, CancellationToken ct);
}

public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<Result<TResponse>> HandleAsync(TQuery query, CancellationToken ct);
}

/// <summary>
/// Marks a command whose repeated execution must be a no-op rather than an error.
/// Only enrolment needs this today; the behaviour that consumes it arrives with the
/// second case (rule of two).
/// </summary>
public interface IIdempotent
{
    string IdempotencyKey { get; }
}
