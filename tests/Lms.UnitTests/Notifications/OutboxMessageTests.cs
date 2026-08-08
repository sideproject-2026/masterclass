using Lms.Modules.Notifications.Domain;

namespace Lms.UnitTests.Notifications;

public class OutboxMessageTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 8, 12, 0, 0, TimeSpan.Zero);

    private static OutboxMessage AnUnsentMessage() =>
        OutboxMessage.Create("CourseCompletedEmail", """{"courseId":"abc"}""", "sam@example.com", Now);

    [Fact]
    public void Create_populates_the_message_and_leaves_it_pending()
    {
        var message = AnUnsentMessage();

        message.Type.ShouldBe("CourseCompletedEmail");
        message.RecipientEmail.ShouldBe("sam@example.com");
        message.CreatedAt.ShouldBe(Now);
        message.SentAt.ShouldBeNull("a new message is pending until the drain sends it");
        message.AttemptCount.ShouldBe(0);
        message.LastError.ShouldBeNull();
    }

    [Fact]
    public void Create_generates_a_version_7_key_in_application_code()
    {
        var message = AnUnsentMessage();

        message.Id.ShouldNotBe(Guid.Empty);
        message.Id.Version.ShouldBe(7, "keys are UUIDv7, never a database default");
    }

    [Theory]
    [InlineData("", "payload", "a@b.com")]
    [InlineData("  ", "payload", "a@b.com")]
    [InlineData("Type", "", "a@b.com")]
    [InlineData("Type", "payload", "")]
    [InlineData("Type", "payload", "   ")]
    public void Create_rejects_blank_arguments(string type, string payload, string recipient) =>
        Should.Throw<ArgumentException>(() => OutboxMessage.Create(type, payload, recipient, Now));

    [Fact]
    public void MarkSent_records_when_it_was_sent()
    {
        var message = AnUnsentMessage();
        var sentAt = Now.AddMinutes(5);

        message.MarkSent(sentAt);

        message.SentAt.ShouldBe(sentAt);
    }

    [Fact]
    public void MarkSent_is_idempotent_and_never_moves_the_timestamp()
    {
        // The drain may retry after a partial failure. Re-sending must not rewrite history,
        // or "when was this delivered?" stops being answerable.
        var message = AnUnsentMessage();
        var firstSend = Now.AddMinutes(5);

        message.MarkSent(firstSend);
        message.MarkSent(Now.AddHours(3));

        message.SentAt.ShouldBe(firstSend);
    }

    [Fact]
    public void MarkSent_clears_a_previous_error()
    {
        var message = AnUnsentMessage();
        message.MarkFailed("SMTP timeout");

        message.MarkSent(Now.AddMinutes(1));

        message.LastError.ShouldBeNull();
        message.AttemptCount.ShouldBe(1, "the attempt still happened and stays on the record");
    }

    [Fact]
    public void MarkFailed_accumulates_attempts_and_keeps_the_latest_error()
    {
        var message = AnUnsentMessage();

        message.MarkFailed("first failure");
        message.MarkFailed("second failure");

        message.AttemptCount.ShouldBe(2);
        message.LastError.ShouldBe("second failure");
        message.SentAt.ShouldBeNull();
    }
}
