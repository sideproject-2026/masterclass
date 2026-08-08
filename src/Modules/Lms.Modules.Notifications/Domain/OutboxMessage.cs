namespace Lms.Modules.Notifications.Domain;

/// <summary>
/// A queued outbound email.
/// </summary>
/// <remarks>
/// The point of the outbox is transactional safety: the row is written inside the business
/// transaction, so an email-provider outage can never roll back a student's course completion.
/// A background sender drains it with retry. See artifacts/design/02-domain-model.md §5.
/// </remarks>
public sealed class OutboxMessage
{
    // EF materialisation only.
    private OutboxMessage()
    {
        Type = null!;
        Payload = null!;
        RecipientEmail = null!;
    }

    private OutboxMessage(Guid id, string type, string payload, string recipientEmail, DateTimeOffset createdAt)
    {
        Id = id;
        Type = type;
        Payload = payload;
        RecipientEmail = recipientEmail;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    /// <summary>Discriminator for the sender, e.g. <c>CourseCompletedEmail</c>.</summary>
    public string Type { get; private set; }

    /// <summary>Serialised message body. Stored as <c>jsonb</c> so the backlog stays queryable.</summary>
    public string Payload { get; private set; }

    public string RecipientEmail { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Null while pending. Presence, not a bool, so the timestamp is kept.</summary>
    public DateTimeOffset? SentAt { get; private set; }

    public int AttemptCount { get; private set; }

    public string? LastError { get; private set; }

    /// <summary>Keys are UUIDv7 generated here, not by the database (02-domain-model.md §8.2).</summary>
    public static OutboxMessage Create(string type, string payload, string recipientEmail, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientEmail);

        return new OutboxMessage(Guid.CreateVersion7(), type, payload, recipientEmail, now);
    }

    /// <summary>Idempotent: re-marking an already-sent message does not move the timestamp.</summary>
    public void MarkSent(DateTimeOffset now)
    {
        if (SentAt is not null)
        {
            return;
        }

        SentAt = now;
        LastError = null;
    }

    public void MarkFailed(string error)
    {
        AttemptCount++;
        LastError = error;
    }
}
