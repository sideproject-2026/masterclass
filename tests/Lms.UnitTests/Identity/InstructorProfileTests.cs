using Lms.Modules.Identity.Domain;

namespace Lms.UnitTests.Identity;

/// <summary>
/// The slug invariant.
/// </summary>
/// <remarks>
/// A slug becomes a public URL segment, so it is validated at the domain boundary rather than
/// escaped wherever it happens to be rendered. Getting that backwards means every future call
/// site has to remember to be careful.
/// </remarks>
public sealed class InstructorProfileTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 18, 10, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData("jane-doe")]
    [InlineData("scott-allen")]
    [InlineData("k8s-expert")]
    [InlineData("a")]
    public void Create_accepts_a_well_formed_slug(string slug)
    {
        var result = InstructorProfile.Create(Guid.CreateVersion7(), slug, "Principal engineer", Now);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Slug.ShouldBe(slug);
    }

    [Theory]
    [InlineData("", "empty")]
    [InlineData("   ", "whitespace only")]
    [InlineData("-jane", "leading hyphen")]
    [InlineData("jane-", "trailing hyphen")]
    [InlineData("jane--doe", "doubled hyphen")]
    [InlineData("jane doe", "space")]
    [InlineData("jane_doe", "underscore")]
    [InlineData("jane.doe", "dot")]
    [InlineData("jane/doe", "path separator")]
    [InlineData("jane?a=1", "query string")]
    [InlineData("../admin", "traversal")]
    [InlineData("<script>", "markup")]
    public void Create_rejects_a_malformed_slug(string slug, string why)
    {
        var result = InstructorProfile.Create(Guid.CreateVersion7(), slug, "Principal engineer", Now);

        result.IsFailure.ShouldBeTrue($"'{slug}' should be rejected — {why}");
        result.Error.Code.ShouldBe("admin.invalid_slug");
    }

    /// <summary>The cap is checked in code, so the column width is never what enforces it.</summary>
    [Fact]
    public void Create_rejects_a_slug_over_the_column_width()
    {
        var tooLong = new string('a', InstructorProfile.SlugMaxLength + 1);

        var result = InstructorProfile.Create(Guid.CreateVersion7(), tooLong, "Engineer", Now);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("admin.invalid_slug");
    }

    [Fact]
    public void Create_lowercases_and_trims_the_slug()
    {
        var result = InstructorProfile.Create(Guid.CreateVersion7(), "  Jane-DOE  ", "Engineer", Now);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Slug.ShouldBe("jane-doe");
    }

    [Fact]
    public void Create_requires_a_headline()
    {
        var result = InstructorProfile.Create(Guid.CreateVersion7(), "jane-doe", "   ", Now);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("admin.invalid_headline");
    }

    [Fact]
    public void Create_rejects_a_headline_over_the_column_width()
    {
        var tooLong = new string('x', InstructorProfile.HeadlineMaxLength + 1);

        var result = InstructorProfile.Create(Guid.CreateVersion7(), "jane-doe", tooLong, Now);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("admin.invalid_headline");
    }

    [Fact]
    public void Create_rejects_an_empty_user_id()
    {
        var result = InstructorProfile.Create(Guid.Empty, "jane-doe", "Engineer", Now);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("auth.user_not_found");
    }
}
