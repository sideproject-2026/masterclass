using Lms.Modules.Catalog.Domain;
using Lms.SharedKernel.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lms.Modules.Catalog.Infrastructure.Configurations;

internal sealed class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("courses");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();   // UUIDv7 from application code

        builder.Property(c => c.Slug).HasMaxLength(Course.SlugMaxLength).IsRequired();
        builder.Property(c => c.Title).HasMaxLength(Course.TitleMaxLength).IsRequired();
        builder.Property(c => c.Subtitle).HasMaxLength(Course.SubtitleMaxLength);
        builder.Property(c => c.Description).HasMaxLength(Course.DescriptionMaxLength).IsRequired();
        builder.Property(c => c.ThumbnailBlobPath).HasMaxLength(Course.ThumbnailPathMaxLength);

        // Enums as int, per 02-domain-model.md §3. Storing the name would make renaming a member
        // a data migration.
        builder.Property(c => c.Level).HasConversion<int>().IsRequired();
        builder.Property(c => c.Status).HasConversion<int>().IsRequired();

        // text[], mapped straight to the backing field. The reasoning for an array over a join
        // table is 02 §3.5; the GIN index below is what makes it a real indexed lookup rather
        // than the LIKE scan a delimited string would force.
        builder.Property<string[]>("_tags")
            .HasColumnName("tags")
            .HasColumnType("text[]")
            .IsRequired();

        builder.Ignore(c => c.Tags);

        builder.Property(c => c.Version).IsRowVersion();

        // The public URL. Unique index rather than a pre-check: querying first and inserting
        // after still loses to a concurrent create — same call as A-6's instructor slug.
        builder.HasIndex(c => c.Slug).IsUnique();

        builder.HasIndex(c => c.InstructorId);

        // The catalogue's own query: published courses, newest first.
        builder.HasIndex(c => new { c.Status, c.PublishedAt });

        builder.HasIndex("_tags")
            .HasDatabaseName("ix_courses_tags")
            .HasMethod("gin");

        builder.HasMany(c => c.Chapters)
            .WithOne()
            .HasForeignKey(c => c.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Course.Chapters))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class ChapterConfiguration : IEntityTypeConfiguration<Chapter>
{
    public void Configure(EntityTypeBuilder<Chapter> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("chapters");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.Title).HasMaxLength(Chapter.TitleMaxLength).IsRequired();
        builder.Property(c => c.SortOrder).IsRequired();

        // Dense and unique per course (02 §3.4). The database enforces it so a reorder bug
        // surfaces as a failed write rather than as a curriculum that renders in the wrong order.
        builder.HasIndex(c => new { c.CourseId, c.SortOrder }).IsUnique();

        builder.HasMany(c => c.Lessons)
            .WithOne()
            .HasForeignKey(l => l.ChapterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Chapter.Lessons))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("lessons");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();

        builder.Property(l => l.Title).HasMaxLength(Lesson.TitleMaxLength).IsRequired();
        builder.Property(l => l.SortOrder).IsRequired();
        builder.Property(l => l.Type).HasConversion<int>().IsRequired();
        builder.Property(l => l.VideoProvider).HasConversion<int>();
        builder.Property(l => l.ExternalVideoId).HasMaxLength(Lesson.ExternalVideoIdMaxLength);

        // No length cap: these are the lesson body and its notes, and a limit here would be an
        // arbitrary editor constraint rather than a domain rule.
        builder.Property(l => l.ContentMarkdown);
        builder.Property(l => l.NotesMarkdown);

        builder.HasIndex(l => new { l.ChapterId, l.SortOrder }).IsUnique();
    }
}
