namespace Lms.SharedKernel.Time;

/// <summary>
/// Injected time. Nothing in the domain calls <c>DateTimeOffset.UtcNow</c> directly —
/// a clock you cannot control is a test you cannot write.
/// </summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
