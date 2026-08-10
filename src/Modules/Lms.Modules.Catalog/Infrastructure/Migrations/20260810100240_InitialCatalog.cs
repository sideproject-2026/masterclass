using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lms.Modules.Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "catalog");

            migrationBuilder.CreateTable(
                name: "courses",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    subtitle = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    description = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    instructor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    thumbnail_blob_path = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    estimated_minutes = table.Column<int>(type: "integer", nullable: false),
                    lesson_count = table.Column<int>(type: "integer", nullable: false),
                    enrollment_count = table.Column<int>(type: "integer", nullable: false),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    tags = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_courses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "chapters",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    course_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_chapters", x => x.id);
                    table.ForeignKey(
                        name: "fk_chapters_courses_course_id",
                        column: x => x.course_id,
                        principalSchema: "catalog",
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lessons",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    chapter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    is_preview = table.Column<bool>(type: "boolean", nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false),
                    video_provider = table.Column<int>(type: "integer", nullable: true),
                    external_video_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    duration_seconds = table.Column<int>(type: "integer", nullable: false),
                    content_markdown = table.Column<string>(type: "text", nullable: true),
                    notes_markdown = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lessons", x => x.id);
                    table.ForeignKey(
                        name: "fk_lessons_chapters_chapter_id",
                        column: x => x.chapter_id,
                        principalSchema: "catalog",
                        principalTable: "chapters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_chapters_course_id_sort_order",
                schema: "catalog",
                table: "chapters",
                columns: new[] { "course_id", "sort_order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_courses_instructor_id",
                schema: "catalog",
                table: "courses",
                column: "instructor_id");

            migrationBuilder.CreateIndex(
                name: "ix_courses_slug",
                schema: "catalog",
                table: "courses",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_courses_status_published_at",
                schema: "catalog",
                table: "courses",
                columns: new[] { "status", "published_at" });

            migrationBuilder.CreateIndex(
                name: "ix_courses_tags",
                schema: "catalog",
                table: "courses",
                column: "tags")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_lessons_chapter_id_sort_order",
                schema: "catalog",
                table: "lessons",
                columns: new[] { "chapter_id", "sort_order" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lessons",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "chapters",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "courses",
                schema: "catalog");
        }
    }
}
