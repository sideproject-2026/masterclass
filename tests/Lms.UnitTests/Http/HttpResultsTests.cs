using Lms.SharedKernel.Http;
using Lms.SharedKernel.Pagination;
using Lms.SharedKernel.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Lms.UnitTests.Http;

/// <summary>
/// The ErrorType -> status mapping is the single source of truth for every 4xx this API
/// returns (artifacts/design/03-api-design.md §1.2). It is worth pinning exactly.
/// </summary>
public class ErrorTypeStatusMappingTests
{
    [Theory]
    [InlineData(ErrorType.Validation, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorType.Unauthenticated, StatusCodes.Status401Unauthorized)]
    [InlineData(ErrorType.Forbidden, StatusCodes.Status403Forbidden)]
    [InlineData(ErrorType.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ErrorType.Conflict, StatusCodes.Status409Conflict)]
    [InlineData(ErrorType.Invariant, StatusCodes.Status422UnprocessableEntity)]
    public void Each_error_type_maps_to_its_documented_status(ErrorType type, int expected) =>
        type.ToStatusCode().ShouldBe(expected);

    [Fact]
    public void Every_error_type_has_an_explicit_mapping()
    {
        // Guards against a new ErrorType silently falling through to 500.
        foreach (var type in Enum.GetValues<ErrorType>())
        {
            type.ToStatusCode().ShouldNotBe(
                StatusCodes.Status500InternalServerError,
                $"{type} has no explicit status mapping");
        }
    }
}

public class ToHttpResultTests
{
    private static readonly Error NotFound = Error.NotFound("course.not_found", "No such course.");

    [Fact]
    public void A_successful_value_becomes_200_with_the_value()
    {
        Result<string> result = "hello";

        var http = result.ToHttpResult();

        http.ShouldBeOfType<Ok<string>>().Value.ShouldBe("hello");
    }

    [Fact]
    public void A_successful_unit_result_becomes_204()
    {
        var http = Result.Success().ToHttpResult();

        http.ShouldBeOfType<NoContent>();
    }

    [Fact]
    public void A_failure_becomes_a_problem_result_with_the_mapped_status()
    {
        Result<string> result = NotFound;

        var problem = result.ToHttpResult().ShouldBeOfType<ProblemHttpResult>();

        problem.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
        problem.ProblemDetails.Detail.ShouldBe("No such course.");
    }

    [Fact]
    public void A_problem_carries_the_machine_readable_error_code()
    {
        var problem = HttpResults.Problem(NotFound).ShouldBeOfType<ProblemHttpResult>();

        problem.ProblemDetails.Extensions["errorCode"].ShouldBe("course.not_found");
        problem.ProblemDetails.Type.ShouldBe("https://lms.example.com/errors/course.not_found");
    }

    [Fact]
    public void The_projecting_overload_transforms_the_value()
    {
        Result<int> result = 21;

        var http = result.ToHttpResult(v => v * 2);

        http.ShouldBeOfType<Ok<int>>().Value.ShouldBe(42);
    }

    [Fact]
    public void The_projecting_overload_does_not_run_on_failure()
    {
        Result<int> result = NotFound;
        var projected = false;

        var http = result.ToHttpResult(v => { projected = true; return v; });

        projected.ShouldBeFalse();
        http.ShouldBeOfType<ProblemHttpResult>();
    }

    [Fact]
    public void Created_sets_the_location_header_from_the_value()
    {
        Result<int> result = 7;

        var created = result.ToCreatedResult(v => $"/api/courses/{v}")
            .ShouldBeOfType<Created<int>>();

        created.Location.ShouldBe("/api/courses/7");
        created.Value.ShouldBe(7);
    }

    [Fact]
    public void A_paged_query_result_becomes_a_paged_envelope()
    {
        Result<QueryResult<string>> result = new QueryResult<string>(["a", "b"], TotalCount: 41);

        var ok = result.ToPagedHttpResult(PageRequest.Of(2, 20))
            .ShouldBeOfType<Ok<PagedResult<string>>>();

        ok.Value.ShouldNotBeNull();
        ok.Value.Items.ShouldBe(["a", "b"]);
        ok.Value.Page.ShouldBe(2);
        ok.Value.TotalCount.ShouldBe(41);
        ok.Value.TotalPages.ShouldBe(3);
    }
}

public class PagingParamsTests
{
    [Fact]
    public void Defaults_apply_when_nothing_is_supplied()
    {
        var page = new PagingParams(null, null).ToPageRequest();

        page.Page.ShouldBe(1);
        page.PageSize.ShouldBe(PageRequest.DefaultPageSize);
    }

    [Fact]
    public void Values_are_clamped_rather_than_rejected()
    {
        var page = new PagingParams(-4, 9999).ToPageRequest();

        page.Page.ShouldBe(1);
        page.PageSize.ShouldBe(PageRequest.MaxPageSize);
    }

    [Fact]
    public async Task Binding_reads_the_query_string()
    {
        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString("?page=3&pageSize=25");

        var bound = await PagingParams.BindAsync(context);

        bound.Page.ShouldBe(3);
        bound.PageSize.ShouldBe(25);
    }

    [Fact]
    public async Task Binding_ignores_junk_and_falls_back_to_defaults()
    {
        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString("?page=banana&pageSize=");

        var bound = await PagingParams.BindAsync(context);

        bound.ToPageRequest().Page.ShouldBe(1);
        bound.ToPageRequest().PageSize.ShouldBe(PageRequest.DefaultPageSize);
    }
}
