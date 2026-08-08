using Lms.Modules.Notifications.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lms.Modules.Notifications.Infrastructure.Configurations;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("outbox_messages");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();   // UUIDv7 comes from application code

        builder.Property(m => m.Type).HasMaxLength(100).IsRequired();
        builder.Property(m => m.RecipientEmail).HasMaxLength(256).IsRequired();

        // jsonb, not text: same storage cost, but the backlog stays queryable and indexable.
        builder.Property(m => m.Payload).HasColumnType("jsonb").IsRequired();

        builder.Property(m => m.CreatedAt).IsRequired();
        builder.Property(m => m.SentAt);
        builder.Property(m => m.AttemptCount).IsRequired();
        builder.Property(m => m.LastError).HasMaxLength(2000);

        // Filtered index — the drain only ever asks for pending messages, and this keeps the
        // index small no matter how much history accumulates.
        builder.HasIndex(m => m.SentAt)
            .HasFilter("sent_at IS NULL")
            .HasDatabaseName("ix_outbox_messages_pending");
    }
}
